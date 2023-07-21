using JwtAuthentication.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthentication.Server.Db;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions dbContextOptions)
        : base(dbContextOptions)
    {
    }

    public DbSet<LoginModel>? LoginModels { get; set; }
    
    // Seeding database
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginModel>().HasData(new LoginModel
        {
            Id = 1,
            UserName = "johndoe",
            Password = "def@123"
        });
    }
}