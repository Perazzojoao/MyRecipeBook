.PHONY: dev test test-unit test-integration

dev:
	# Evita o erro de manifesto de assets no SDK 10; preserva o Hot Reload de C#.
	DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=1 dotnet watch --project src/Backend/MyRecipeBook.Api run --launch-profile http

test:
	dotnet test MyRecipeBook.slnx

test-unit:
	dotnet test tests/Validators.Tests/Validators.Tests.csproj
	dotnet test tests/UseCases.Tests/UseCases.Tests.csproj

test-integration:
	dotnet test tests/WebApi.Tests/WebApi.Tests.csproj
