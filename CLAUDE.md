# Claude Instructions - Timecrax Backend

## Project Overview
Timecrax is an educational history game platform. This is the ASP.NET Core backend API.

## Tech Stack
- **Framework:** ASP.NET Core 8.0
- **Database:** PostgreSQL 16
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer tokens
- **Container:** Docker + Docker Compose

## Project Structure
```
src/Timecrax.Api/
├── Controllers/     # API endpoints
├── Data/            # DbContext and database configuration
├── Domain/
│   └── Entities/    # Entity models (User, Theme, EventCard, etc.)
├── Migrations/      # EF Core migrations
├── Services/        # Business logic services (TokenService, PasswordService, etc.)
└── Middleware/      # Custom middleware (ExceptionHandlingMiddleware)

infra/
├── docker-compose.yml  # Docker services (db, pgadmin, api)
├── .env                # Environment variables (not in git)
└── seed-gdpr.sql       # GDPR terms seed script
```

## Database Schema
- Schema: `app`
- Main entities:
  - `users` - User accounts
  - `themes` - Game themes created by users
  - `event_cards` - Cards within themes
  - `gdpr` - GDPR terms by language
  - `refresh_tokens` - JWT refresh tokens

## Conventions

### Entity Configuration
- All entities configured in `AppDbContext.OnModelCreating()`
- Use snake_case for database column names
- PostgreSQL column names are case-sensitive when quoted

### Controllers
- Route prefix: `[Route("endpoint")]`
- Use `[Authorize]` for protected endpoints
- Return appropriate HTTP status codes
- Use anonymous types for responses when simple

### Environment Variables
- JWT key: `${JWT_SECRET_KEY}` (resolved by `TokenService.ResolveEnvVariable()`)
- Database: `DB_HOST`, `DB_PORT`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`
- SMTP: `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM_EMAIL`
- Frontend URL: `FRONTEND_URL`

### Paginated Responses
Standard format for list endpoints:
```csharp
new {
    items = [...],
    page = 1,
    pageSize = 10,
    totalCount = 100,
    totalPages = 10
}
```

## Entity Relationships
- User -> Themes (one-to-many)
- Theme -> EventCards (one-to-many, navigation: `EventCards`)
- EventCard has: ImageQuiz, TextQuiz, TrueFalseQuiz, CorrelationQuiz

## Common Patterns

### Adding New Endpoints
1. Add method to appropriate controller
2. Use `[HttpGet]`, `[HttpPost]`, `[HttpPut]`, `[HttpDelete]`
3. Get user ID: `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`
4. Return `Ok()`, `NotFound()`, `BadRequest()`, etc.

### Database Queries
- Use async/await with `ToListAsync()`, `FirstOrDefaultAsync()`
- Include related entities with `.Include()`
- For card count: `theme.EventCards.Count`

### Era Values
- Stored as string: "BC" or "AD"
- Type in entity: `string? Era`

## Docker Commands
```bash
# Start all services
docker-compose up -d

# Rebuild API after changes
docker-compose up -d --build api

# View logs
docker logs timecrax_api

# Run SQL script
docker exec -i timecrax_postgres psql -U timecrax -d timecrax -f - < script.sql
```

## Database Migrations
```bash
# Add migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

## Configuration Files
- `appsettings.json` - Base settings with `${VAR}` placeholders
- `appsettings.Development.json` - Development overrides (optional)
- Environment variables resolved at runtime by custom `ResolveEnvVariable()` method

## API Endpoints Overview
- `POST /auth/register` - User registration
- `POST /auth/login` - User login
- `GET /me` - Current user info
- `GET /themes/my-themes` - User's themes
- `GET /themes/storage` - Public themes storage
- `GET /themes/{id}` - Theme details with cards
- `POST /themes` - Create theme
- `PUT /themes/{id}` - Update theme
- `DELETE /themes/{id}` - Delete theme
- `GET /me/ranking` - User ranking
- `GET /legal/gdpr/{language}` - GDPR terms
