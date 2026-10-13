using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using MyRecipeBook.Domain.Entities;

// Allow the test project to access internal members of this assembly
[assembly: InternalsVisibleTo("WebApi.Tests")]
namespace MyRecipeBook.Infrastructure.DataAccess;

internal class MyRecipeBookDbContext : DbContext {
  public DbSet<User> Users { get; set; }

  public MyRecipeBookDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }
}
