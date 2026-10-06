.PHONY: dev

dev:
	# Evita o erro de manifesto de assets no SDK 10; preserva o Hot Reload de C#.
	DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=1 dotnet watch --project src/Backend/MyRecipeBook.Api run --launch-profile http
