# MyRecipeBook

API backend em .NET 10 organizada em seis projetos de produção, três projetos de testes xUnit e uma biblioteca de utilitários para testes. O fluxo atual cadastra usuários, valida os dados e a duplicidade de e-mail, gera o hash da senha com Argon2 e persiste no MySQL. As alterações de schema são executadas pelo FluentMigrator na inicialização da API.

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
tests/
├── CommonTestUtilities/             # Class library de utilitários para testes
├── UseCases.Tests/                   # Testes xUnit de casos de uso
├── Validators.Tests/                # Testes xUnit
└── WebApi.Tests/                    # Testes xUnit da API
```

A solution `MyRecipeBook.slnx` agrupa os quatro projetos de Backend em `src/Backend`, os dois projetos compartilhados em `src/Shared` e os projetos `Validators.Tests`, `UseCases.Tests`, `WebApi.Tests` e `CommonTestUtilities` em `tests`.

### Referências entre projetos

| Projeto | Referencia |
| --- | --- |
| `MyRecipeBook.Api` | `MyRecipeBook.Communication`, `MyRecipeBook.Exception`, `MyRecipeBook.Application`, `MyRecipeBook.Infrastructure` |
| `MyRecipeBook.Application` | `MyRecipeBook.Communication`, `MyRecipeBook.Domain`, `MyRecipeBook.Exception` |
| `MyRecipeBook.Infrastructure` | `MyRecipeBook.Domain` |
| `MyRecipeBook.Domain` | Nenhum projeto da solução |
| `MyRecipeBook.Communication` | Nenhum projeto da solução |
| `MyRecipeBook.Exception` | Nenhum projeto da solução |
| `Validators.Tests` | `MyRecipeBook.Application`, `CommonTestUtilities` |
| `CommonTestUtilities` | `MyRecipeBook.Communication`, `MyRecipeBook.Domain` |
| `UseCases.Tests` | `CommonTestUtilities`, `MyRecipeBook.Application` |
| `WebApi.Tests` | `CommonTestUtilities`, `MyRecipeBook.Api` |

### FluentValidation

O projeto `MyRecipeBook.Application` referencia o pacote [FluentValidation](https://www.nuget.org/packages/FluentValidation/12.1.1) na versão `12.1.1`, para definir regras de validação dos dados recebidos pelos casos de uso. Essa versão é compatível com o `net10.0` utilizado na solução.

Para adicionar a mesma dependência a partir da raiz do repositório:

```bash
dotnet add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj package FluentValidation --version 12.1.1
```

O `RegisterUserAccountValidator` exige nome, e-mail e senha; limita o nome a 100 caracteres, verifica o formato do e-mail e exige pelo menos seis caracteres na senha. O caso de uso executa essa validação e consulta se existe um usuário ativo com o mesmo e-mail antes de persistir.

As mensagens vêm de `ResourceMessagesException`: inglês é o padrão e `pt-BR` tem traduções próprias. A API seleciona o idioma pelo header `Accept-Language`. As chaves de validação usam o prefixo `VALIDATION_`, incluindo `VALIDATION_EMAIL_ALREADY_EXISTS`; a mensagem genérica mantém a chave `UNKNOWN_ERROR`.

A classe pública `ResourceMessagesException` é gerada pelo MSBuild em `obj/`, a partir do `.resx`. O projeto `MyRecipeBook.Exception` também executa a geração na compilação de análise do editor (`DesignTimeBuild`), para disponibilizar a classe no autocomplete. Se o editor ainda exibir diagnósticos antigos após atualizar o `.csproj`, recarregue a solução ou reinicie o servidor de linguagem C#.

### Dependências diretas

| Projeto | Pacote | Versão |
| --- | --- | --- |
| Api | Swashbuckle.AspNetCore | 10.2.3 |
| Application | FluentValidation | 12.1.1 |
| Application | Mapster | 10.0.13 |
| Application | Microsoft.Extensions.DependencyInjection | 10.0.12 |
| Infrastructure | Konscious.Security.Cryptography.Argon2 | 1.3.1 |
| Infrastructure | Microsoft.EntityFrameworkCore | 10.0.12 |
| Infrastructure | MySql.EntityFrameworkCore | 10.0.9 |
| Infrastructure | Microsoft.Extensions.DependencyInjection | 10.0.12 |
| Infrastructure | FluentMigrator | 8.0.1 |
| Infrastructure | FluentMigrator.Runner | 8.0.1 |
| CommonTestUtilities | Bogus | 35.6.5 |
| CommonTestUtilities | Moq | 4.21.0 |
| Validators.Tests | Shouldly | 4.3.0 |
| UseCases.Tests | Shouldly | 4.3.0 |
| WebApi.Tests | Shouldly | 4.3.0 |
| WebApi.Tests | Microsoft.AspNetCore.Mvc.Testing | 10.0.12 |
| WebApi.Tests | Testcontainers.MySql | 4.16.0 |

Mapster converte o request para a entidade `User`. Entity Framework Core realiza as consultas e a persistência; FluentMigrator controla o schema. `Microsoft.EntityFrameworkCore.Design` não é utilizado.

## Executar a configuração atual

Na raiz do repositório, restaure e compile:

```bash
dotnet restore MyRecipeBook.slnx
dotnet build MyRecipeBook.slnx
```

Para executar os testes:

```bash
make test
```

O comando executa `dotnet test MyRecipeBook.slnx`.

Para executar os testes por categoria:

```bash
make test-unit
make test-integration
```

`make test-unit` executa os projetos `Validators.Tests` e `UseCases.Tests`. `make test-integration` executa apenas `WebApi.Tests` e precisa do Docker disponível para iniciar o MySQL temporário.

O projeto `Validators.Tests` referencia `MyRecipeBook.Application` e `CommonTestUtilities` e cobre o sucesso da validação e os erros de nome, e-mail e senha vazios. A regra de senha usa `CascadeMode.Stop` para retornar apenas o erro de campo obrigatório quando a senha está vazia. A class library `CommonTestUtilities` centraliza a geração de requests com Bogus e os mocks de repositórios, unidade de trabalho e hash de senha com Moq.

O projeto `UseCases.Tests` referencia `CommonTestUtilities` e `MyRecipeBook.Application` e cobre o cadastro com sucesso, a rejeição de nome vazio e a rejeição de e-mail já cadastrado.

O projeto `WebApi.Tests` referencia `CommonTestUtilities` e `MyRecipeBook.Api` e inclui `Shouldly`, `Microsoft.AspNetCore.Mvc.Testing` e `Testcontainers.MySql`. Os testes verificam o cadastro via `POST /users`, a persistência do usuário e a resposta de erro para nome vazio em `en-US` e `pt-BR`. Para executá-los, o Docker precisa estar disponível: a factory inicia um container temporário `mysql:8.4`, usa o ambiente `Tests` e sobrescreve a connection string em memória. As migrations da API criam o schema nesse banco; a connection string do FluentMigrator é resolvida por callback para considerar essa configuração. O arquivo `appsettings.Tests.json` está vazio porque a conexão é fornecida pela factory. A classe `Program` é pública e parcial para permitir o uso de `WebApplicationFactory`, e `InternalsVisibleTo("WebApi.Tests")` permite que os testes consultem o `MyRecipeBookDbContext` interno.

### Integração contínua

Os workflows `.github/workflows/lint.yml` e `.github/workflows/tests.yml` executam separadamente ao abrir, atualizar ou reabrir um PR com destino às branches `dev` ou `main`, e quando um PR passa a estar pronto para revisão.

O workflow **Lint** restaura as dependências, compila toda a solução em Release e executa `dotnet format --verify-no-changes`, que verifica formatação e diagnósticos corrigíveis de estilo e análise estática no nível padrão de warning ou superior. O `.editorconfig` versionado mantém o estilo C# com dois espaços e a chave de abertura na mesma linha. O `global.json` seleciona um SDK estável da série .NET 10, permitindo atualizações dentro dessa série.

O workflow **Tests** compila em Release e executa todos os testes da solução usando `tests/coverlet.runsettings` e o coletor `XPlat Code Coverage`. Ele usa `ubuntu-24.04`, com os testes executados diretamente no runner e acesso ao Docker pré-instalado, conforme a [documentação do Testcontainers para GitHub Actions](https://dotnet.testcontainers.org/cicd/#github-actions). A factory gerencia o MySQL temporário e o Ryuk permanece habilitado para a limpeza dos recursos. O logger TRX gera os resultados dos testes, e o runsettings configura a cobertura em Cobertura e OpenCover apenas para as assemblies `MyRecipeBook.*`, excluindo os utilitários de testes e todos os arquivos sob `Migrations/`, inclusive subpastas. Após os testes passarem, o workflow exige a presença dos três formatos de relatório. Os arquivos `.trx`, `coverage.cobertura.xml` e `coverage.opencover.xml` gerados são disponibilizados como artefato `test-results` por dois dias, inclusive quando há falhas nos testes.

O [ReportGenerator](https://github.com/danielpalme/ReportGenerator-GitHub-Action) 5.5.11 consolida os relatórios Cobertura dos três projetos de testes e gera um resumo Markdown, um relatório HTML e um XML Cobertura consolidado. A entrada `TestResults/*/coverage.cobertura.xml` seleciona os arquivos originais e evita incluir novamente as cópias dos anexos TRX. A geração também ocorre quando os testes falham, desde que existam arquivos de cobertura; nesse caso, o relatório contém os resultados coletados até a falha e o workflow continua marcado como falho.

Para visualizar a cobertura no GitHub, abra **Actions → Tests → execução → Summary**. O resumo mostra a cobertura de linhas e branches e os resultados por assembly/classe. Para navegar pelo código e ver as linhas cobertas e descobertas, baixe o artefato **coverage-report**, extraia o ZIP e abra `index.html` no navegador. O HTML, `SummaryGithub.md` e o `Cobertura.xml` consolidado ficam disponíveis por dois dias. O artefato `test-results` continua contendo os relatórios originais.

Para reproduzir as verificações localmente, com Docker disponível:

```bash
dotnet restore MyRecipeBook.slnx
dotnet build MyRecipeBook.slnx --configuration Release --no-restore
dotnet format MyRecipeBook.slnx --verify-no-changes --no-restore
docker info
dotnet test MyRecipeBook.slnx --configuration Release --no-build --no-restore --settings tests/coverlet.runsettings --collect "XPlat Code Coverage" --logger trx --results-directory TestResults
```

Para gerar o relatório visual localmente na raiz do repositório:

```bash
dotnet tool install dotnet-reportgenerator-globaltool --tool-path .tools/reportgenerator --version 5.5.11
.tools/reportgenerator/reportgenerator \
  "-reports:TestResults/*/coverage.cobertura.xml" \
  "-targetdir:CoverageReport" \
  "-reporttypes:Html;MarkdownSummaryGithub;Cobertura" \
  "-sourcedirs:." \
  "-title:MyRecipeBook coverage"
```

Abra `CoverageReport/index.html` no navegador. O diretório do relatório e o da ferramenta local são ignorados pelo Git.

### Banco local

Para iniciar o MySQL local e a interface phpMyAdmin:

```bash
docker compose up -d
```

Confirme que o MySQL terminou de inicializar com `docker compose logs mysql`: o log deve indicar `ready for connections`. A API executa as migrations antes de começar a atender requisições, portanto o banco precisa estar acessível nesse momento.

Em Development, a conexão está em `src/Backend/MyRecipeBook.Api/appsettings.Development.json`, na chave `ConnectionStrings:DbConnection`. Para as credenciais padrão do Compose:

```json
"ConnectionStrings": {
  "DbConnection": "Server=localhost;Port=3306;Database=myrecipebook;Uid=myrecipebook;Pwd=myrecipebook;"
}
```

Também é possível substituir a conexão por variável de ambiente antes de iniciar a API:

```bash
export ConnectionStrings__DbConnection='Server=localhost;Port=3306;Database=myrecipebook;Uid=myrecipebook;Pwd=myrecipebook;'
```

Fora de Development, configure essa variável ou uma fonte equivalente, pois `appsettings.json` não contém a conexão. Se a API também rodar em um container na rede do Compose, use `Server=mysql`; `localhost` é o endereço para a API executada no host.

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

## Cadastro de usuários

O endpoint é `POST /users`. Exemplo, com a API HTTP em execução:

```bash
curl -i http://localhost:5000/users \
  -H 'Content-Type: application/json' \
  -H 'Accept-Language: pt-BR' \
  -d '{"name":"João Silva","email":"joao@example.com","password":"uma-senha-local"}'
```

Em caso de sucesso, retorna HTTP `201` com:

```json
{
  "name": "João Silva",
  "tokens": {
    "accessToken": "",
    "refreshToken": ""
  }
}
```

Os campos de token ainda são retornados vazios: a geração de tokens não foi implementada. Os dados do usuário são gravados por `UsersRepository` e confirmados por `IUnitOfWork.Commit()`. A senha é armazenada como hash Argon2id com salt aleatório.

Erros de validação retornam HTTP `400` com uma lista `errors`. Para um e-mail já cadastrado em um usuário ativo, por exemplo:

```json
{"errors":["Este e-mail já está em uso."]}
```

Exceções não tratadas retornam HTTP `500` com a mensagem genérica localizada. Sem `Accept-Language`, o idioma é inglês. O `StringConverter` remove espaços nas extremidades e substitui sequências de espaços em branco por um espaço na leitura de todas as strings JSON, inclusive `password`.

## Schema e migrations

As migrations ficam em `src/Backend/MyRecipeBook.Infrastructure/Migrations/Versions`. A API registra o runner com `AddMySql5()` e chama `DatabaseMigration.ExecuteMigrations()` antes de `app.Run()`. Esse é o nome do processador configurado para o serviço MySQL 8.4 do Compose.

O FluentMigrator registra as versões aplicadas na tabela `VersionInfo`. A migration `Version0000001`, versão `1`, cria `Users`:

| Coluna | Definição da migration |
| --- | --- |
| Id | GUID, chave primária, obrigatório; gerado pela aplicação com `Guid.CreateVersion7()` |
| Name | String de até 250 caracteres, obrigatória; a validação da API limita a 100 |
| Email | String de até 250 caracteres, obrigatória e única |
| Password | String de até 2000 caracteres, obrigatória; contém o hash |
| Active | Boolean obrigatório, padrão `true` |
| DeletedAt | Data/hora opcional, padrão `null` |

O contexto EF Core expõe `DbSet<User> Users` e utiliza o mapeamento por convenção. As migrations são `ForwardOnlyMigration`, sem operação `Down`. Para evoluir o schema, adicione uma nova migration com versão diferente e reinicie a API; não altere uma migration já aplicada.

Em um banco novo, deixe o FluentMigrator criar `Users`. Se essa tabela já foi criada manualmente sem o registro correspondente em `VersionInfo`, a migration `1` tentará criá-la novamente e a inicialização falhará. Confira o schema e o histórico e reconcilie-os antes de iniciar a API nesse banco.

## MySQL local

O `docker-compose.yml` inicia `mysql:8.4` e `phpmyadmin:5.2.3-apache`. O MySQL publica a porta `3306` do container em `localhost:3306` e persiste os dados no volume nomeado `mysql_data`. A configuração inicial é:

| Variável | Padrão | Uso |
| --- | --- | --- |
| `MYSQL_ROOT_PASSWORD` | `root` | Senha do usuário `root` |
| `MYSQL_DATABASE` | `myrecipebook` | Banco criado na inicialização |
| `MYSQL_USER` | `myrecipebook` | Usuário da aplicação criado na inicialização |
| `MYSQL_PASSWORD` | `myrecipebook` | Senha do usuário da aplicação |

O phpMyAdmin fica em [http://localhost:8080](http://localhost:8080), com acesso restrito ao host local. Ele se conecta ao serviço `mysql` pela rede do Compose. Entre com o usuário `myrecipebook` e a senha `myrecipebook` (ou as credenciais efetivas do seu banco), selecione o banco `myrecipebook` e abra uma tabela para visualizar os registros. O login é solicitado pela interface.

Para iniciar somente a interface quando o MySQL já estiver em execução:

```bash
docker compose up -d --no-deps phpmyadmin
```

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

O backend usa Entity Framework Core com o provedor MySQL. A conexão da API é definida em `ConnectionStrings:DbConnection`; quando a API roda no host, use `Server=localhost;Port=3306`.

Para consultar o schema pelo terminal:

```bash
docker compose exec mysql mysql -u myrecipebook -p myrecipebook
```

Digite a senha efetiva do usuário e execute:

```sql
SHOW TABLES;
DESCRIBE Users;
SELECT * FROM VersionInfo;
```

Um erro MySQL `1045` indica falha de autenticação: compare a connection string com as credenciais efetivas do volume. A senha definida no Compose só é aplicada na primeira inicialização.

## Recriar a estrutura inicial

Os comandos abaixo criam a solution, os seis projetos de produção, os três projetos de testes, a biblioteca de utilitários para testes, as referências e as dependências diretas em um diretório vazio. `--format slnx` cria XML Solution Files. Esses comandos não geram o código de negócio, os recursos ou as migrations: use os arquivos versionados em `src/` para reproduzir o comportamento atual.

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
dotnet new xunit -n Validators.Tests -o tests/Validators.Tests -f net10.0
dotnet new xunit -n UseCases.Tests -o tests/UseCases.Tests -f net10.0
dotnet new xunit -n WebApi.Tests -o tests/WebApi.Tests -f net10.0
dotnet new classlib -n CommonTestUtilities -o tests/CommonTestUtilities -f net10.0

dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Api/MyRecipeBook.Api.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Domain/MyRecipeBook.Domain.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj --solution-folder src/Backend
dotnet sln MyRecipeBook.slnx add src/Shared/MyRecipeBook.Communication/MyRecipeBook.Communication.csproj --solution-folder src/Shared
dotnet sln MyRecipeBook.slnx add src/Shared/MyRecipeBook.Exception/MyRecipeBook.Exception.csproj --solution-folder src/Shared
dotnet sln MyRecipeBook.slnx add tests/Validators.Tests/Validators.Tests.csproj --solution-folder tests
dotnet sln MyRecipeBook.slnx add tests/UseCases.Tests/UseCases.Tests.csproj --solution-folder tests
dotnet sln MyRecipeBook.slnx add tests/WebApi.Tests/WebApi.Tests.csproj --solution-folder tests
dotnet sln MyRecipeBook.slnx add tests/CommonTestUtilities/CommonTestUtilities.csproj --solution-folder tests

dotnet reference add \
  src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj \
  tests/CommonTestUtilities/CommonTestUtilities.csproj \
  --project tests/Validators.Tests/Validators.Tests.csproj

dotnet reference add \
  tests/CommonTestUtilities/CommonTestUtilities.csproj \
  src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj \
  --project tests/UseCases.Tests/UseCases.Tests.csproj

dotnet reference add \
  tests/CommonTestUtilities/CommonTestUtilities.csproj \
  src/Backend/MyRecipeBook.Api/MyRecipeBook.Api.csproj \
  --project tests/WebApi.Tests/WebApi.Tests.csproj

dotnet reference add \
  src/Shared/MyRecipeBook.Communication/MyRecipeBook.Communication.csproj \
  src/Backend/MyRecipeBook.Domain/MyRecipeBook.Domain.csproj \
  --project tests/CommonTestUtilities/CommonTestUtilities.csproj

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
dotnet add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj package Mapster --version 10.0.13
dotnet add src/Backend/MyRecipeBook.Application/MyRecipeBook.Application.csproj package Microsoft.Extensions.DependencyInjection --version 10.0.12
dotnet add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj package Konscious.Security.Cryptography.Argon2 --version 1.3.1
dotnet add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 10.0.12
dotnet add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj package MySql.EntityFrameworkCore --version 10.0.9
dotnet add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection --version 10.0.12
dotnet add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj package FluentMigrator --version 8.0.1
dotnet add src/Backend/MyRecipeBook.Infrastructure/MyRecipeBook.Infrastructure.csproj package FluentMigrator.Runner --version 8.0.1
dotnet add tests/CommonTestUtilities/CommonTestUtilities.csproj package Bogus --version 35.6.5
dotnet add tests/CommonTestUtilities/CommonTestUtilities.csproj package Moq --version 4.21.0
dotnet add tests/Validators.Tests/Validators.Tests.csproj package Shouldly --version 4.3.0
dotnet add tests/UseCases.Tests/UseCases.Tests.csproj package Shouldly --version 4.3.0
dotnet add tests/WebApi.Tests/WebApi.Tests.csproj package Shouldly --version 4.3.0
dotnet add tests/WebApi.Tests/WebApi.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 10.0.12
dotnet add tests/WebApi.Tests/WebApi.Tests.csproj package Testcontainers.MySql --version 4.16.0
```

`dotnet sln add` registra e organiza projetos na solution; as chamadas separadas a `dotnet reference add` criam as dependências `ProjectReference` entre eles.

Use o `src/Backend/MyRecipeBook.Api/Program.cs` versionado neste repositório, junto com os controllers, converters, filtros, casos de uso e migrations. O pipeline registra a injeção de dependências, o Swagger, a normalização das strings JSON, o idioma pelo `Accept-Language`, o filtro de exceções e a execução das migrations na inicialização.

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

  phpmyadmin:
    image: phpmyadmin:5.2.3-apache
    environment:
      PMA_HOST: mysql
      PMA_PORT: "3306"
    ports:
      - "127.0.0.1:8080:80"
    depends_on:
      - mysql

volumes:
  mysql_data:
```

Crie `Makefile` na raiz. As linhas dos comandos precisam começar com um caractere TAB real, não espaços:

```makefile
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
```

Finalize com restore e build:

```bash
dotnet restore MyRecipeBook.slnx
dotnet build MyRecipeBook.slnx
```
