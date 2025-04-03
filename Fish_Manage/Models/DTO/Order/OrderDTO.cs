using Fish_Manage.Models.DTO.Product;
using System.ComponentModel.DataAnnotations;

namespace Fish_Manage.Models.DTO.Order
{
    public class OrderDTO
    {
        public string OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime OrderDate { get; set; }
        [Required]
        public string TotalAmount { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
        public ICollection<ProductDTO> Products { get; set; } = new List<ProductDTO>();
    }
}
