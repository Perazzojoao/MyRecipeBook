using MyRecipeBook.Infrastructure.DataAccess.Repositories;

namespace MyRecipeBook.Infrastructure.DataAccess;

internal class UnitOfWork(MyRecipeBookDbContext dbContext) : IUnitOfWork {
  private readonly MyRecipeBookDbContext _dbContext = dbContext;

  public async Task Commit() => await _dbContext.SaveChangesAsync();
}
