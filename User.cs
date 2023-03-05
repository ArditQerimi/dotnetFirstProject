using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace dotnetAPI
{
    public class User:IdentityUser
    {
        public int? Id { get; set; }
        public string? FirstName { get; set; } 
        public string? LastName { get; set; } 
        //public string? Email { get; set; } 
        //public string? Password { get; set; } 
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public virtual List<Product>? Products { get; set; }
    }

}
