using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Application.UseCases.User.Register;
using MyRecipeBook.Domain.Securiry.PasswordHashing;

namespace MyRecipeBook.Application;

public static class DependencyInjectionExtension {
  extension(IServiceCollection services) {
    public void AddApplication() {
      services.AddScoped<IRegisterUserAccountUseCase, RegisterUserAccountUseCase>();
    }
  }
}
