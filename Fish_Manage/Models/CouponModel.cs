using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fish_Manage.Models
{
    public class CouponModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
