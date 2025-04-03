using Fish_Manage.Models;
using Fish_Manage.Models.DTO.Order;
using Fish_Manage.Models.Momo;
using Fish_Manage.Repository;
using Fish_Manage.Repository.IRepository;
using Fish_Manage.Service.IService;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;

namespace Fish_Manage.Service.Momo
{
    public class MomoService : Repository<Order>, IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly IOrderRepository _orderRepository;
        private readonly FishManageContext _context;
        private readonly IProductRepository _productRepository;

        public MomoService(IOptions<MomoOptionModel> options, IOrderRepository orderRepository, FishManageContext context, IProductRepository productRepository) : base(context)
        {
            _options = options;
            _orderRepository = orderRepository;
            _context = context;
            _productRepository = productRepository;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderCreateDTO model)
        {
            if (_options == null || _options.Value == null)
            {
                throw new NullReferenceException("Momo settings (_options) is not configured properly.");
            }

            var order = new Order
            {
                UserId = model.UserId,
                TotalAmount = model.TotalAmount,
                OrderDate = model.OrderDate,
                PaymentMethod = model.PaymentMethod,
                Address = model.Address,
                Name = model.Name,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                OrderProducts = model.Products.Select(p => new OrderProduct
                {
                    ProductId = p.ProductId,
                    OrderId = model.OrderId,
                    Quantity = p.Quantity
                }).ToList()
            };
            // Fetch all product details from DB in one call
            var productIds = model.Products.Select(p => p.ProductId).ToList();
            var productsInDb = await _productRepository.GetByIdsAsync(productIds);
            foreach (var orderProduct in model.Products)
            {
                var product = productsInDb.FirstOrDefault(p => p.ProductId == orderProduct.ProductId);
                if (product == null || product.Quantity < orderProduct.Quantity)
                {
                    throw new NullReferenceException("Product quantity return below 0");
                }
                product.Quantity -= orderProduct.Quantity;
            }
            await _productRepository.UpdateRangeAsync(productsInDb);
            await _orderRepository.CreateAsync(order);

            // Generate order info for all products
            var formattedOrderInfo = string.Join("|", model.Products.Select(product =>
                $"{model.OrderDate:yyyyMMddHHmmss}-{model.PaymentMethod}-{product.ProductId}-{product.Quantity}"
            ));

            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.Accesskey}" +
                $"&requestId={model.UserId}" +
                $"&amount={model.TotalAmount}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={formattedOrderInfo}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData=";

            var signature = ComputeHmacSha256(rawData, _options.Value.Secretkey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };

            request.AddHeader("Content-Type", "application/json");

            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                accessKey = _options.Value.Accesskey,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = model.OrderId,
                amount = model.TotalAmount,
                orderInfo = formattedOrderInfo,
                requestId = model.UserId,
                extraData = "",
                lang = "en",
                signature = signature,
            };

            string jsonData = JsonConvert.SerializeObject(requestData);
            request.AddStringBody(jsonData, RestSharp.ContentType.Json);

            RestResponse response = await client.ExecuteAsync(request);

            var paymentResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);

            await _orderRepository.UpdateAsync(order);

            return paymentResponse;
        }
        public async Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
        {

            // Extract values safely
            string amount = collection.ContainsKey("amount") ? collection["amount"].ToString() : null;
            string orderId = collection.ContainsKey("orderId") ? collection["orderId"].ToString() : null;
            string orderInfo = collection.ContainsKey("orderInfo") ? collection["orderInfo"].ToString() : null;


            return await Task.FromResult(new MomoExecuteResponseModel
            {
                Amount = amount,
                OrderId = orderId,
                OrderInfo = orderInfo
            });
        }
        private string ComputeHmacSha256(string message, string secretKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


    }
}
