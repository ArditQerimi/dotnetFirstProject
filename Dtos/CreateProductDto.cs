namespace dotnetAPI.Dtos
{
    public class CreateProductDto
    {
        public string Name { get; set; }

        public double Price { get; set; }


        public int CategoryId { get; set; }

        public int SizeId { get; set; }

        public List<int> ColorIds { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SizeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ColorDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}

