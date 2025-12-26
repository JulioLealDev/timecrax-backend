using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Timecrax.Api.Data;
using Timecrax.Api.Services;
using Timecrax.Api.Middlewares;
using Timecrax.Api.Data.Seed;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<DbSeedHostedService>();
builder.Services.AddScoped<StorageImageService>();


builder.Services.AddCors(opt =>
{
    opt.AddPolicy("web", p =>
        p.WithOrigins("http://localhost:5173")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", "app")
    );
});

builder.Services.AddScoped<TokenService>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseStaticFiles();

var storageRoot = builder.Configuration["Storage:RootPath"];
var publicBasePath = builder.Configuration["Storage:PublicBasePath"] ?? "/media";

if (!string.IsNullOrWhiteSpace(storageRoot))
{
    Directory.CreateDirectory(storageRoot);

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(storageRoot),
        RequestPath = publicBasePath
    });
}

app.UseHttpsRedirection();
app.UseCors("web");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Timecrax API is running");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
