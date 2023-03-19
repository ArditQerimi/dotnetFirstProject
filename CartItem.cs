namespace dotnetAPI
{
    public class CartItem
    {
        public int Id { get; set; }
        public Product? Product { get; set; }
        public double? Total { get; set; }
        public int? Quantity { get; set; }

        public int? UserId { get; set; }
    }
}
