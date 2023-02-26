namespace dotnetAPI.Dtos
{
    public class UpdateProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public int SizeId { get; set; }
        public List<int> ColorIds { get; set; }
    }
}
