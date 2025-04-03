namespace Fish_Manage.Models.DTO.Order
{
    public class OrderCreateDTO
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime OrderDate { get; set; }
        public string TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public List<ProductOrderDTO> Products { get; set; }
    }

    public class ProductOrderDTO
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
