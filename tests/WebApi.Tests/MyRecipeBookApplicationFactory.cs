using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MySql;

namespace WebApi.Tests;

public class MyRecipeBookApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime {
  private readonly MySqlContainer _mySqlContainer;

  public MyRecipeBookApplicationFactory() {
    _mySqlContainer = new MySqlBuilder("mysql:8.4")
      .WithDatabase("myrecipebook")
      .Build();
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder) {
    builder.UseEnvironment("Tests")
      .ConfigureAppConfiguration((_, config) => {
        var parameters = new Dictionary<string, string?> {
          ["ConnectionStrings:DbConnection"] = _mySqlContainer.GetConnectionString()
        };
        config.AddInMemoryCollection(parameters);
      });
  }

  // Used to start the MySQL container before running tests
  public async Task InitializeAsync() {
    await _mySqlContainer.StartAsync();
  }

  // Used to stop the MySQL container after running all tests
  Task IAsyncLifetime.DisposeAsync() => _mySqlContainer.StopAsync();

}
