using Fish_Manage.Models.DTO.Order;
using Fish_Manage.Models.DTO.Product;

namespace Fish_Manage.Models.DTO
{
    public class OrderProductDTO
    {
        public string OrderId { get; set; }
        public OrderDTO Order { get; set; }

        public string ProductId { get; set; }
        public ProductDTO Product { get; set; }

        public int Quantity { get; set; }
    }
}
