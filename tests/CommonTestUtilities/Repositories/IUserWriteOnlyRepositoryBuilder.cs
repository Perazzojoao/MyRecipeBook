using Moq;
using MyRecipeBook.Domain.Repositories;

namespace CommonTestUtilities.Repositories;

public class IUserWriteOnlyRepositoryBuilder {
  public static IUserWriteOnlyRepository Build() {
    var mock = new Mock<IUserWriteOnlyRepository>();
    return mock.Object;
  }
}
