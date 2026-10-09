using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyRecipeBook.Domain.Repositories;
using MyRecipeBook.Domain.Repositories.User;
using MyRecipeBook.Domain.Securiry.PasswordHashing;
using MyRecipeBook.Infrastructure.DataAccess;
using MyRecipeBook.Infrastructure.DataAccess.Repositories;

namespace MyRecipeBook.Infrastructure;

public static class DependencyInjectionExtension {
  extension(IServiceCollection services) {
    public void AddInfrastructure(IConfiguration configuration) {
      services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();

      services.AddScoped<IUserWriteOnlyRepository, UsersRepository>();
      services.AddScoped<IUserReadOnlyRepository, UsersRepository>();

      services.AddScoped<IUnitOfWork, UnitOfWork>();

      services.AddDbContext<MyRecipeBookDbContext>(config => {
        config.UseMySQL(configuration.GetConnectionString("DbConnection")!);
      });

      services.AddFluentMigratorCore().ConfigureRunner(config => {
        config
        .AddMySql5()
        .WithGlobalConnectionString(configuration.GetConnectionString("DbConnection")!)
        .ScanIn(Assembly.Load("MyRecipeBook.Infrastructure"))
        .For.All();
      });
    }
  }
}
