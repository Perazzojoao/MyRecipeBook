using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using MyRecipeBook.Api.Filters;
using MyRecipeBook.Infrastructure;
using MyRecipeBook.Application;
using MyRecipeBook.Api.Converters;
using MyRecipeBook.Infrastructure.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new StringConverter()));
builder.Services.AddSwaggerGen(options => {
  options.SwaggerDoc("v1", new OpenApiInfo {
    Title = "MyRecipeBook API",
    Version = "v1"
  });
});

// Add application and infrastructure services on dependency injection
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configure localization options
builder.Services.Configure<RequestLocalizationOptions>(options => {
  var supportedCultures = new List<CultureInfo> {
    new("en"),
    new("pt-BR")
  };

  options.DefaultRequestCulture = new("en");
  options.SupportedCultures = supportedCultures;
  options.SupportedUICultures = supportedCultures;

  // Use Accept-Language header to determine the culture (e.g., "en", "pt-BR")
  options.RequestCultureProviders = new List<IRequestCultureProvider> {
    new AcceptLanguageHeaderRequestCultureProvider()
  };
});

// Register the exception filter globally'
builder.Services.AddMvc(options => options.Filters.Add<ExceptionFilter>());

// Configure routing to use lowercase URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);

var app = builder.Build();

// Configure localization for dependency injection
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(localizationOptions.Value);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
  app.UseSwagger();
  app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("v1/swagger.json", "MyRecipeBook API v1");
  });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await ExecuteMigrations();

app.Run();

// Migrate database on application startup
async Task ExecuteMigrations() {
  await using var scope = app.Services.CreateAsyncScope();

  DatabaseMigration.ExecuteMigrations(scope.ServiceProvider);
}
