using dotnetAPI.Dictionaries;
using dotnetAPI.Enums;
using System.Text.Json.Serialization;

namespace dotnetAPI
{

    public class Size
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [JsonIgnore]
        public virtual Product Product { get; set; }

        public string? SizeType { get; set; }
        //public int? ProductId { get; set; }
    }
}
