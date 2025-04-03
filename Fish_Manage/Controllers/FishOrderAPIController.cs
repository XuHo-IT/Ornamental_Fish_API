using AutoMapper;
using Fish_Manage.Models;
using Fish_Manage.Repository.DTO;
using Fish_Manage.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Fish_Manage.Controllers
{
    [Route("api/FishOrderAPI")]
    [ApiController]
    public class FishOrderAPIController : ControllerBase
    {
        protected APIResponse _response;
        private readonly FishManageContext _context;
        private readonly IOrderRepository _dbOrder;
        private readonly IMapper _mapper;

        public FishOrderAPIController(APIResponse response, FishManageContext context, IOrderRepository dbOrder, IMapper mapper)
        {
            _response = response;
            _context = context;
            _dbOrder = dbOrder;
            _mapper = mapper;
        }

        [HttpGet("GetOrderList")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> GetOrderList()
        {
            try
            {
                var orders = await _context.Orders
     .Include(o => o.OrderProducts)
     .ThenInclude(op => op.Product)
     .ToListAsync();


                var orderDTOs = orders.Select(order => new
                {
                    order.OrderId,
                    order.UserId,
                    order.Name,
                    order.PhoneNumber,
                    order.Address,
                    order.Email,
                    order.OrderDate,
                    order.TotalAmount,
                    order.PaymentMethod,
                    Products = order.OrderProducts.Select(op => new
                    {
                        op.Product.ProductId,
                        op.Product.ProductName,
                        op.Product.Price,
                        op.Product.Category,
                        op.Product.Description,
                        op.Product.Supplier,
                        op.Product.ImageURl,
                        Quantity = op.Quantity
                    }).ToList()
                }).ToList();

                return Ok(new APIResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = orderDTOs
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

        [HttpGet("{id}", Name = "GetOrder")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetOrder(string id)
        {
            try
            {
                if (id == "")
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                //var order = await _dbOrder.GetAsync(u => u.OrderId == id);
                var order = await _context.Orders
    .Include(o => o.OrderProducts)
    .ThenInclude(op => op.Product)
    .FirstOrDefaultAsync(u => u.OrderId == id);

                if (order == null)
                {
                    return NotFound(new APIResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Order not found" },
                        StatusCode = HttpStatusCode.NotFound
                    });
                }

                var orderDTO = new
                {
                    order.OrderId,
                    order.UserId,
                    order.Name,
                    order.PhoneNumber,
                    order.Address,
                    order.Email,
                    order.OrderDate,
                    order.TotalAmount,
                    order.PaymentMethod,
                    Products = order.OrderProducts.Select(op => new
                    {
                        op.Product.ProductId,
                        op.Product.ProductName,
                        op.Product.Price,
                        op.Product.Category,
                        op.Product.Description,
                        op.Product.Supplier,
                        op.Product.ImageURl,
                        Quantity = op.Quantity
                    }).ToList()
                };

                return Ok(new APIResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = orderDTO
                });
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("user/{userId}", Name = "GetOrderByUserId")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetOrderByUserId(string userId)
        {
            try
            {
                if (userId == "")
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var order = await _context.Orders
    .Include(o => o.OrderProducts)
    .ThenInclude(op => op.Product)
    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (order == null)
                {
                    return NotFound(new APIResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = new List<string> { "Order not found" },
                        StatusCode = HttpStatusCode.NotFound
                    });
                }

                var orderDTO = new
                {
                    order.OrderId,
                    order.UserId,
                    order.Name,
                    order.PhoneNumber,
                    order.Address,
                    order.Email,
                    order.OrderDate,
                    order.TotalAmount,
                    order.PaymentMethod,
                    Products = order.OrderProducts.Select(op => new
                    {
                        op.Product.ProductId,
                        op.Product.ProductName,
                        op.Product.Price,
                        op.Product.Category,
                        op.Product.Description,
                        op.Product.Supplier,
                        op.Product.ImageURl,
                        Quantity = op.Quantity
                    }).ToList()
                };

                return Ok(new APIResponse
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Result = orderDTO
                });
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id}", Name = "DeleteOrder")]
        public async Task<ActionResult<APIResponse>> DeleteOrder(string id)
        {
            try
            {
                if (id == "")
                {
                    return BadRequest();
                }
                var order = await _dbOrder.GetAsync(u => u.OrderId == id);
                if (order == null)
                {
                    return NotFound();
                }
                await _dbOrder.RemoveAsync(order);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = new List<string>() { ex.ToString() };
            }
            return _response;
        }
        [HttpPost("GetMoneyPerYear")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public decimal GetMoneyPerYear(int term)
        {
            switch (term)
            {
                case 1:
                    return _dbOrder.GetMoneyPerTerm(1);
                case 2:
                    return _dbOrder.GetMoneyPerTerm(2);
                case 3:
                    return _dbOrder.GetMoneyPerTerm(3);
                default:
                    return default(decimal);

            }
        }

        [HttpPost("GetMostMoneyBill")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ApplicationUser GetMostMoneyBill(int term)
        {
            switch (term)
            {
                case 1:
                    return _dbOrder.UserBuyMost(1);
                case 2:
                    return _dbOrder.UserBuyMost(2);
                case 3:
                    return _dbOrder.UserBuyMost(3);
                default:
                    return null;

            }
        }
    }
}
