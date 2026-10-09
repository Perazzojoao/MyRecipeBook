using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;

namespace MyRecipeBook.Infrastructure.DataAccess;

internal class MyRecipeBookDbContext : DbContext {
  public DbSet<User> Users { get; set; }

  public MyRecipeBookDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
}
