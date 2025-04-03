using System.ComponentModel.DataAnnotations;

namespace Fish_Manage.Models.DTO.Order
{
    public class OrderUpdateDTO
    {
        [Required]
        public string OrderId { get; set; }
        [Required]
        public string UserId { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public string TotalAmount { get; set; }
        [Required]
        public string PaymentMethod { get; set; }

        public ICollection<OrderProductDTO> Products { get; set; } = new List<OrderProductDTO>();

    }
}
