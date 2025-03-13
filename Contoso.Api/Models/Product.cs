using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Contoso.Api.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; } 

        public string? Name { get; set; }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public Product() {
            Id = Math.Abs(Guid.NewGuid().GetHashCode());
            
        }


    }
}