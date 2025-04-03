using AutoMapper;
using Fish_Manage.Models;
using Fish_Manage.Models.DTO.Order;
using Fish_Manage.Repository.DTO;
using Fish_Manage.Repository.IRepository;
using Fish_Manage.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace Fish_Manage.Controllers
{
    [Route("api/PaymentAPI")]
    [ApiController]
    public class PaymentAPIController : ControllerBase
    {
        private readonly FishManageContext _db;
        protected APIResponse _response;
        private readonly IMomoService _momoService;
        private readonly IProductRepository _productRepository;
        private readonly IPaymentCODService _paymentCODService;
        private readonly IMapper _mapper;

        public PaymentAPIController(FishManageContext db, APIResponse response, IMomoService momoService, IProductRepository productRepository, IPaymentCODService paymentCODService, IMapper mapper)
        {
            _db = db;
            _response = new APIResponse();
            _momoService = momoService;
            _productRepository = productRepository;
            _paymentCODService = paymentCODService;
            _mapper = mapper;
        }

        [HttpPost("CreatePaymentMomo")]
        public async Task<IActionResult> CreatePaymentMomo([FromBody] OrderCreateDTO model)
        {
            Console.WriteLine($"[INFO] Received MoMo Payment Request: {JsonConvert.SerializeObject(model)}");

            if (model == null)
            {
                Console.WriteLine("[ERROR] Request body is missing or invalid.");
                return BadRequest(new { message = "Request body is missing or invalid." });
            }

            if (model.TotalAmount == "")
            {
                Console.WriteLine("[ERROR] Invalid payment amount. Amount must be greater than zero.");
                return BadRequest(new { message = "Invalid payment amount" });
            }

            try
            {
                var response = await _momoService.CreatePaymentMomo(model);

                if (response == null)
                {
                    Console.WriteLine("[ERROR] Failed to create MoMo payment.");
                    return BadRequest(new { message = "Failed to create MoMo payment." });
                }

                Console.WriteLine($"[SUCCESS] MoMo payment created successfully: {JsonConvert.SerializeObject(response)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPTION] Error creating MoMo payment: {ex.Message}");
                return StatusCode(500, new { message = "An internal server error occurred.", error = ex.Message });
            }
        }
        [HttpPost("CreatePaymentCOD")]
        public async Task<ActionResult<APIResponse>> CreatePaymentCOD(OrderCreateDTO createDTO)
        {
            try
            {
                if (createDTO == null)
                {
                    return BadRequest(new APIResponse { IsSuccess = false, ErrorMessages = new List<string> { "Invalid order data" } });
                }

                var order = new Order
                {
                    OrderId = createDTO.OrderId,
                    UserId = createDTO.UserId,
                    OrderDate = createDTO.OrderDate,
                    TotalAmount = createDTO.TotalAmount,
                    PaymentMethod = createDTO.PaymentMethod,
                    Address = createDTO.Address,
                    Name = createDTO.Name,
                    Email = createDTO.Email,
                    PhoneNumber = createDTO.PhoneNumber,
                    OrderProducts = createDTO.Products.Select(p => new OrderProduct
                    {
                        ProductId = p.ProductId,
                        OrderId = createDTO.OrderId,
                        Quantity = p.Quantity
                    }).ToList()
                };



                await _paymentCODService.CreateAsync(order);

                // Fetch all product details from DB in one call
                var productIds = createDTO.Products.Select(p => p.ProductId).ToList();
                var productsInDb = await _productRepository.GetByIdsAsync(productIds);

                // Check stock availability & update quantity
                foreach (var orderProduct in createDTO.Products)
                {
                    var product = productsInDb.FirstOrDefault(p => p.ProductId == orderProduct.ProductId);
                    if (product == null || product.Quantity < orderProduct.Quantity)
                    {
                        return BadRequest(new APIResponse
                        {
                            IsSuccess = false,
                            ErrorMessages = new List<string> { $"Insufficient stock for product {orderProduct.ProductId}" }
                        });
                    }
                    product.Quantity -= orderProduct.Quantity;
                }

                // Batch update product stock
                await _productRepository.UpdateRangeAsync(productsInDb);

                // Return structured response
                return CreatedAtAction(nameof(CreatePaymentCOD), new APIResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.Created,
                    Result = new
                    {
                        order.OrderId,
                        order.UserId,
                        order.OrderDate,
                        order.TotalAmount,
                        order.PaymentMethod,
                        Products = order.OrderProducts.Select(op => new
                        {
                            op.Product.ProductId,
                            op.OrderId,
                            op.Product.ProductName,
                            op.Product.Price,
                            op.Product.Category,
                            op.Product.Description,
                            op.Product.Supplier,
                            op.Product.ImageURl,
                            Quantity = op.Quantity
                        }).ToList()
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse
                {
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message },
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
        }


        [HttpGet]
        [Route("api/[controller]/callback")]
        public async Task<IActionResult> PaymentCallback()
        {
            var requestQuery = HttpContext.Request.Query;
            Console.WriteLine($"Request Query: {requestQuery}");

            var response = await _momoService.PaymentExecuteAsync(requestQuery);

            // Ensure response is valid
            if (response == null)
            {
                Console.WriteLine("Error: MoMo payment response is null.");
                return BadRequest("Invalid MoMo payment response.");
            }

            Console.WriteLine($"Response Amount: {response.Amount}");

            var newOrder = new Order
            {
                OrderId = response.OrderId,
                UserId = DateTime.Now.Ticks.ToString(),
                OrderDate = DateTime.Now,
                TotalAmount = response.Amount,
                PaymentMethod = "MoMo",
            };

            Console.WriteLine("Order details: " + Newtonsoft.Json.JsonConvert.SerializeObject(newOrder));

            await _momoService.CreateAsync(newOrder);

            return Redirect($"http://localhost:5173/CallBack?resultCode={requestQuery["resultCode"]}&orderId={response.OrderId}");
        }




    }
}

