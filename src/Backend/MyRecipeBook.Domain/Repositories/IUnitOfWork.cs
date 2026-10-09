namespace MyRecipeBook.Infrastructure.DataAccess.Repositories;

public interface IUnitOfWork {
  public Task Commit();
}
