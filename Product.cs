using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace dotnetAPI
{
    //[Index(nameof(SizeId), IsUnique = true), Index(nameof(CategoryId), IsUnique = true)]
    [Index(nameof(SizeId), IsUnique = false),
        Index(nameof(UserId), IsUnique = false)
        ]
    public class Product
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public Category Category { get; set; }

        public int CategoryId { get; set; }
        public virtual Size Size { get; set; }
        public int SizeId { get; set; }
        public List<Color> Colors { get; set; }

        public int UserId { get; set; }

    
    }
}
