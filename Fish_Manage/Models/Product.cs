using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fish_Manage.Models
{
    public partial class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProductId { get; set; }

        public string? ProductName { get; set; }

        public decimal? Price { get; set; }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public string? Supplier { get; set; }

        public string ImageURl { get; set; }
        public int? Quantity { get; set; }

    }
}
