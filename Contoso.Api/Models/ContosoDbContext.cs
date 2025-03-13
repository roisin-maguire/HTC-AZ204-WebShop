using Microsoft.EntityFrameworkCore;

namespace Contoso.Api.Models
{
    public class ContosoDbContext : DbContext
    {
        public ContosoDbContext(DbContextOptions<ContosoDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();


            // modelBuilder.Entity<User>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>()
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.Email)
                .ToContainer("Users");

            // modelBuilder.Entity<Order>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Order>().HasKey(x => x.Id);
            modelBuilder.Entity<Order>()
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.Id)
                .ToContainer("Orders");

            modelBuilder.Entity<Product>().HasKey(x => x.Id);//.Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Product>()
                .HasNoDiscriminator()
                .HasPartitionKey(x => x.Category)
                .ToContainer("Products");
                

            // modelBuilder.HasDefaultContainer("Products");

 
        }
    }
}