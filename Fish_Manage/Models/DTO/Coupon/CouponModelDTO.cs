using System.ComponentModel.DataAnnotations;

namespace Fish_Manage.Models.DTO.Coupon
{
    public class CouponModelDTO
    {
        [Required]
        public string CouponId { get; set; }
        [Required]
        public string CouponCode { get; set; }
        [Required]
        public string CouponDescription { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateExpired { get; set; }
        [Required]
        public int Quantity { get; set; }
    }
}
