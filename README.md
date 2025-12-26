# Timecrax Backend

API REST desenvolvida em .NET 8.0 para o sistema Timecrax - uma solução de controle de ponto e gerenciamento de tempo.

## Tecnologias

- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- BCrypt para hashing de senhas
- Swagger/OpenAPI para documentação
- ImageSharp para processamento de imagens

## Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL instalado e rodando
- Editor de código (Visual Studio, VS Code, Rider, etc.)

## Configuração

1. Clone o repositório:
```bash
git clone <url-do-repositorio>
cd timecrax-backend
```

2. Configure a string de conexão do banco de dados no arquivo `src/Timecrax.Api/appsettings.Development.json`

3. Execute as migrations para criar o banco de dados:
```bash
cd src/Timecrax.Api
dotnet ef database update
```

## Como rodar

### Desenvolvimento

Na raiz do projeto backend:

```bash
cd src/Timecrax.Api
dotnet run
```

A API estará disponível em:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

### Com Docker

Se preferir usar Docker:

```bash
docker build -t timecrax-api .
docker run -p 5000:80 timecrax-api
```

## Estrutura do Projeto

```
src/Timecrax.Api/
├── Controllers/     # Endpoints da API
├── Services/        # Lógica de negócio
├── Domain/          # Modelos de domínio
├── Data/            # Contexto do EF Core
├── Dtos/            # Data Transfer Objects
├── Middlewares/     # Middlewares customizados
├── Extensions/      # Métodos de extensão
├── Migrations/      # Migrations do EF Core
└── wwwroot/         # Arquivos estáticos
```

## Migrations

Para criar uma nova migration:

```bash
cd src/Timecrax.Api
dotnet ef migrations add NomeDaMigration
dotnet ef database update
```

## Documentação da API

Após rodar o projeto, acesse a documentação interativa do Swagger em:
- `https://localhost:5001/swagger`
