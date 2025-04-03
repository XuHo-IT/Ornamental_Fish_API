using AutoMapper;
using Fish_Manage.Models;
using Fish_Manage.Models.DTO.Coupon;
using Fish_Manage.Repository.DTO;
using Fish_Manage.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Fish_Manage.Controllers
{
    [Route("api/CouponModel")]
    [ApiController]
    public class CouponModelAPIController : ControllerBase
    {

        private readonly FishManageContext _context;
        private readonly ICouponModelRepository _couponModelRepository;
        private readonly APIResponse _response;
        private readonly IMapper _mapper;
        private readonly IOrderRepository _dbOrder;
        private readonly IEmailSender _emailSender;
        private readonly IUserRepository _userRepository;

        public CouponModelAPIController(FishManageContext context, ICouponModelRepository couponModelRepository, IMapper mapper, IOrderRepository dbOrder, IEmailSender emailSender, IUserRepository userRepository)
        {
            _context = context;
            _couponModelRepository = couponModelRepository;
            _dbOrder = dbOrder;
            _mapper = mapper;
            _response = new APIResponse();
            _emailSender = emailSender;
            _userRepository = userRepository;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetCoupons()
        {
            IEnumerable<CouponModel> couponList;
            couponList = await _couponModelRepository.GetAllAsync();
            _response.Result = _mapper.Map<List<CouponModelDTO>>(couponList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        [HttpGet("{id}", Name = "GetCoupon")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetCoupon(string id)
        {
            try
            {
                if (id == "")
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var coupon = await _couponModelRepository.GetAsync(u => u.CouponId == id);
                if (coupon == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<List<CouponModelDTO>>(coupon);
                _response.StatusCode = HttpStatusCode.OK;
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
        [HttpPost("CreateCoupon")]
        public async Task<ActionResult<APIResponse>> CreateCoupon(CouponModelCreateDTO createCoupon)
        {
            try
            {
                if (createCoupon == null)
                {
                    return BadRequest(new APIResponse { IsSuccess = false, ErrorMessages = new List<string> { "Invalid coupon data" } });
                }

                CouponModel coupon = _mapper.Map<CouponModel>(createCoupon);
                await _couponModelRepository.CreateAsync(coupon);

                _response.Result = _mapper.Map<CouponModelDTO>(coupon);
                _response.StatusCode = HttpStatusCode.Created;

                return StatusCode(201, _response);


            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;

                return StatusCode(500, _response);
            }
        }
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("{id}", Name = "DeleteCoupon")]
        public async Task<ActionResult<APIResponse>> DeleteOrder(string id)
        {
            try
            {
                if (id == "")
                {
                    return BadRequest();
                }
                var order = await _couponModelRepository.GetAsync(u => u.CouponId == id);
                if (order == null)
                {
                    return NotFound();
                }
                await _couponModelRepository.RemoveAsync(order);
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
        [Authorize(Roles = "admin")]
        [HttpPut("{id}", Name = "UpdateCoupon")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdateOrder(string id, [FromBody] CouponModelUpdateDTO updateCoupon)
        {
            try
            {
                if (updateCoupon == null || id != updateCoupon.CouponId)
                {
                    return BadRequest();
                }

                CouponModel model = _mapper.Map<CouponModel>(updateCoupon);

                await _couponModelRepository.UpdateAsync(model);
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
        [HttpPost("ApplyDiscount")]
        public async Task<IActionResult> ApplyDiscount([FromBody] ApplyDiscountRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.CouponId))
            {
                return BadRequest(new { message = "The couponId field is required." });
            }

            var result = await _couponModelRepository.ApplyDiscount(request.Money, request.CouponId);

            return Ok(result);
        }


        [HttpPost("FirstCoupon")]
        public async Task<IActionResult> SendMailFirstCoupon([FromBody] CouponRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { message = "Invalid request. Email is required." });
            }

            var user = await _userRepository.GetAsync(u => u.Email == request.Email);
            if (user != null)
            {
                return BadRequest(new { message = "User has already received the first coupon." });
            }

            try
            {
                await _emailSender.SendEmailAsync(request.Email, request.Subject, request.Message);
                return Ok(new { message = "Coupon sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send email.", error = ex.Message });
            }
        }

    }
}
