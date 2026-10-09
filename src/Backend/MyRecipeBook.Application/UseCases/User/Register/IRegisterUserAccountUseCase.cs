using MyRecipeBook.Communication.Requests;
using MyRecipeBook.Communication.Responses;

namespace MyRecipeBook.Application.UseCases.User.Register;

public interface IRegisterUserAccountUseCase {
  public Task<ResponseRegisterUserJson> Execute(RequestRegisterUserAccountJson request);
}
