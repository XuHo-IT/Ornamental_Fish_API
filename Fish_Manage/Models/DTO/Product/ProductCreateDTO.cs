using System.ComponentModel.DataAnnotations;

namespace Fish_Manage.Models.DTO.Product
{
    public class ProductCreateDTO
    {
        [Required]
        public string ProductName { get; set; }
        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; }
        [Required]
        public string Description { get; set; }

        [Required]
        public string Supplier { get; set; }

        [Required]
        public int Quantity { get; set; }


    }
}
