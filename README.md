# MyRecipeBook

API backend em .NET 10 organizada em seis projetos. Este documento descreve a configuração que existe hoje e os passos para recriá-la do zero; ainda não documenta funcionalidades de negócio.

## Requisitos

- .NET SDK 10. O SDK encontrado neste ambiente é `10.0.401`.
- Docker com o plugin Docker Compose (`docker compose`).
- `make` para usar o atalho de desenvolvimento.

Todos os projetos usam `TargetFramework` `net10.0`. Não há `global.json`, então a versão do SDK não está fixada pelo repositório.

Os exemplos de terminal usam a sintaxe Bash/Zsh, disponível em Linux, macOS ou WSL.

## Estrutura

```text
src/
├── Backend/
│   ├── MyRecipeBook.Api/             # ASP.NET Core Web API
│   ├── MyRecipeBook.Application/     # Casos de uso e aplicação
│   ├── MyRecipeBook.Domain/          # Domínio
│   └── MyRecipeBook.Infrastructure/  # Infraestrutura
└── Shared/
    ├── MyRecipeBook.Communication/   # Tipos compartilhados de comunicação
    └── MyRecipeBook.Exception/       # Tipos compartilhados de exceção
```

A solution `MyRecipeBook.slnx` agrupa os quatro projetos de Backend em `src/Backend` e os dois projetos compartilhados em `src/Shared`. Há também uma pasta `tests/` vazia na solution.

### Referências entre projetos

| Projeto | Referencia |
| --- | --- |
| `MyRecipeBook.Api` | `MyRecipeBook.Communication`, `MyRecipeBook.Exception`, `MyRecipeBook.Application`, `MyRecipeBook.Infrastructure` |
| `MyRecipeBook.Application` | `MyRecipeBook.Communication`, `MyRecipeBook.Domain`, `MyRecipeBook.Exception` |
| `MyRecipeBook.Infrastructure` | `MyRecipeBook.Domain` |
| `MyRecipeBook.Domain` | Nenhum projeto da solução |
| `MyRecipeBook.Communication` | Nenhum projeto da solução |
| `MyRecipeBook.Exception` | Nenhum projeto da solução |

### FluentValidation

O projeto `MyRecipeBook.Application` referencia o pacote [FluentValidation](https://www.nuget.org/packages/FluentValidation/12.1.1) na versão `12.1.1`, para definir regras de validação dos dados recebidos pelos casos de uso. Essa versão é compatível com o `net10.0` utilizado na solução.

Para adicionar a mesma dependência a partir da raiz do repositório:

```bash
dotnet add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj package FluentValidation --version 12.1.1
```

O `RegisterUserAccountValidator` valida nome, e-mail e senha usando as mensagens de `ResourceMessagesException`. As mensagens são consultadas durante a validação, conforme a `CurrentUICulture`: inglês é o padrão e `pt-BR` tem traduções próprias. A chamada do validador no caso de uso ainda precisa ser implementada.

A classe pública `ResourceMessagesException` é gerada pelo MSBuild em `obj/`, a partir do `.resx`. O projeto `MyRecipeBook.Exception` também executa a geração na compilação de análise do editor (`DesignTimeBuild`), para disponibilizar a classe no autocomplete. Se o editor ainda exibir diagnósticos antigos após atualizar o `.csproj`, recarregue a solução ou reinicie o servidor de linguagem C#.

## Executar a configuração atual

Na raiz do repositório, restaure e compile:

```bash
dotnet restore MyRecipeBook.slnx
dotnet build MyRecipeBook.slnx
```

O MySQL é opcional para executar a API atual, pois a API ainda não o utiliza. Para iniciar o container local:

```bash
docker compose up -d
```

Para iniciar a API usando o perfil HTTP configurado:

```bash
make dev
```

O atalho define `DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=1` para contornar um problema de manifesto de assets do SDK 10 ao iniciar `dotnet watch`. Isso preserva o Hot Reload de C#. Se executar `dotnet watch` diretamente, use o mesmo ajuste:

```bash
DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=1 dotnet watch --project src/Backend/MyRecipeBook.Api run --launch-profile http
```

Com o perfil `http`, a API escuta em `http://localhost:5000`. Em Development, o Swagger UI fica em `http://localhost:5000/swagger` e o documento JSON em `http://localhost:5000/swagger/v1/swagger.json`. O perfil `https` também escuta em `https://localhost:7009`; o redirecionamento HTTPS configurado pode enviar requisições HTTP para HTTPS quando esse perfil estiver ativo. Os dois perfis têm `launchBrowser` desabilitado.

O Swagger é registrado pelo pacote `Swashbuckle.AspNetCore` na versão `10.2.3` e os middlewares Swagger só são ativados quando `ASPNETCORE_ENVIRONMENT` é `Development`.

## MySQL local

O `docker-compose.yml` inicia somente `mysql:8.4`, publica a porta `3306` do container em `localhost:3306` e persiste os dados no volume nomeado `mysql_data`. A configuração inicial é:

| Variável | Padrão | Uso |
| --- | --- | --- |
| `MYSQL_ROOT_PASSWORD` | `root` | Senha do usuário `root` |
| `MYSQL_DATABASE` | `myrecipebook` | Banco criado na inicialização |
| `MYSQL_USER` | `myrecipebook` | Usuário da aplicação criado na inicialização |
| `MYSQL_PASSWORD` | `myrecipebook` | Senha do usuário da aplicação |

`MYSQL_DATABASE` é fixo no compose; as outras credenciais podem ser substituídas por variáveis de ambiente exportadas antes de executar `docker compose up -d`. Por exemplo:

```bash
export MYSQL_ROOT_PASSWORD='uma-senha-root'
export MYSQL_USER='outro-usuario'
export MYSQL_PASSWORD='outra-senha'
docker compose up -d
```

As variáveis de inicialização do container MySQL só são aplicadas quando o diretório de dados está vazio. Alterá-las com o volume `mysql_data` já inicializado não troca automaticamente as credenciais nem recria o banco. Para apenas parar o serviço e manter os dados:

```bash
docker compose stop
```

Para parar e remover o container e a rede do compose, preservando o volume:

```bash
docker compose down
```

O backend ainda não tem Entity Framework, conector MySQL, `ConnectionStrings` ou integração com o banco. Subir o container apenas disponibiliza o MySQL local; não conecta a API a ele.

## Recriar a configuração do zero

Os comandos abaixo são executados em um diretório vazio com os requisitos instalados. `--format slnx` é a opção confirmada pelo CLI instalado para criar XML Solution Files. Os comandos criam a solution, os seis projetos, as referências, os pacotes Swagger e FluentValidation e os arquivos de configuração locais.

```bash
mkdir MyRecipeBook
cd MyRecipeBook

dotnet new sln --name MyRecipeBook --format slnx

dotnet new webapi --use-controllers -n MyRecipeBook.Api -o src/Backend/MyRecipeBook.Api -f net10.0
dotnet new classlib -n MyRecipeBook.Application -o src/Backend/MyRecipeBook.Application -f net10.0
dotnet new classlib -n MyRecipeBook.Domain -o src/Backend/MyRecipeBook.Domain -f net10.0
dotnet new classlib -n MyRecipeBook.Infrastructure -o src/Backend/MyRecipeBook.Infrastructure -f net10.0
dotnet new classlib -n MyRecipeBook.Communication -o src/Shared/MyRecipeBook.Communication -f net10.0
dotnet new classlib -n MyRecipeBook.Exception -o src/Shared/MyRecipeBook.Exception -f net10.0

dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Api/MyRecipeBook.Api.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Domain/MyRecipeBook.Domain.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Shared/MyRecipeBook.Communication/MyRecipeBook.Communication.csproj --solution-folder src/Shared
dotnet sln MyRecipeBook.slnx add src/Shared/MyRecipeBook.Exception/MyRecipeBook.Exception.csproj --solution-folder src/Shared

dotnet reference add \
  src/Shared/MyRecipeBook.Communication/MyRecipeBook.Communication.csproj \
  src/Shared/MyRecipeBook.Exception/MyRecipeBook.Exception.csproj \
  src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj \
  src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj \
  --project src/Backend/MyRecipeBook.Api/MyRecipeBook.Api.csproj

dotnet reference add \
  src/Shared/MyRecipeBook.Communication/MyRecipeBook.Communication.csproj \
  src/Backend/MyRecipeBook.Domain/MyRecipeBook.Domain.csproj \
  src/Shared/MyRecipeBook.Exception/MyRecipeBook.Exception.csproj \
  --project src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj

dotnet reference add \
  src/Backend/MyRecipeBook.Domain/MyRecipeBook.Domain.csproj \
  --project src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj

dotnet remove src/Backend/MyRecipeBook.Api/MyRecipeBook.Api.csproj package Microsoft.AspNetCore.OpenApi
dotnet add src/Backend/MyRecipeBook.Api/MyRecipeBook.Api.csproj package Swashbuckle.AspNetCore --version 10.2.3
dotnet add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj package FluentValidation --version 12.1.1
```

`dotnet sln add` registra e organiza projetos na solution; as chamadas separadas a `dotnet reference add` criam as dependências `ProjectReference` entre eles.

Para reproduzir também a pasta organizacional vazia de testes, acrescente `<Folder Name="/tests/" />` antes de `</Solution>` no `MyRecipeBook.slnx`. Essa pasta ainda não contém projetos de teste.

Substitua o conteúdo de `src/Backend/MyRecipeBook.Api/Program.cs` pelo seguinte para reproduzir o pipeline e o Swagger atuais:

```csharp
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyRecipeBook API",
        Version = "v1"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("v1/swagger.json", "MyRecipeBook API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Substitua `src/Backend/MyRecipeBook.Api/Properties/launchSettings.json` por:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7009;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Crie `docker-compose.yml` na raiz com este conteúdo:

```yaml
services:
  mysql:
    image: mysql:8.4
    environment:
      MYSQL_ROOT_PASSWORD: ${MYSQL_ROOT_PASSWORD:-root}
      MYSQL_DATABASE: myrecipebook
      MYSQL_USER: ${MYSQL_USER:-myrecipebook}
      MYSQL_PASSWORD: ${MYSQL_PASSWORD:-myrecipebook}
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

volumes:
  mysql_data:
```

Crie `Makefile` na raiz. As linhas dos comandos precisam começar com um caractere TAB real, não espaços:

```makefile
.PHONY: dev

dev:
	# Evita o erro de manifesto de assets no SDK 10; preserva o Hot Reload de C#.
	DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=1 dotnet watch --project src/Backend/MyRecipeBook.Api run --launch-profile http
```

Finalize com restore e build:

```bash
dotnet restore MyRecipeBook.slnx
dotnet build MyRecipeBook.slnx
```
