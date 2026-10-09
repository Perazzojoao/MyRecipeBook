namespace MyRecipeBook.Domain.Repositories;

public interface IUserWriteOnlyRepository {
  public Task Add(Entities.User user);
}
