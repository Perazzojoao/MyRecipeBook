using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace MyRecipeBook.Infrastructure.Migrations;

// Migrate database to the latest version
public class DatabaseMigration {
  public static void ExecuteMigrations(IServiceProvider serviceProvider) {
    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
  }
}
