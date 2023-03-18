using System.Collections.Generic;

namespace dotnetAPI.Interfaces
{
    public class IProductFilter
    {
       public string? Name { get; set; }
        public double[]? Price { get; set; }

        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? Category { get; set; }
    }
}
