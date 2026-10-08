using MyRecipeBook.Communication.Requests;

namespace MyRecipeBook.Application.UseCases.User.Register;

public interface IRegisterUserAccountUseCase {
  public void Execute(RequestRegisterUserAccountJson request);
}
