using ATMChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MediatR;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using ATMChallenge.Application.Interfaces;
using ATMChallenge.Infrastructure.Repositories;
using ATMChallenge.Application.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ATMChallenge.Application.ApplicationMarker>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token válido."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost,1433;Database=ATMChallengeDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

// Configurar MediatR con Behaviors (Pipeline Pattern)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(ATMChallenge.Application.ApplicationMarker).Assembly);
    
    // Registrar behaviors en orden de ejecución (de afuera hacia adentro)
    // 1. Logging (primero) - registra entrada/salida y performance
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    
    // 2. Validation (segundo) - valida antes de ejecutar la lógica de negocio
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var key = jwtSection["Key"] ?? "ThisIsASecureLongKeyForJWT_ChangeInProduction_0123456789ABCDEF";
var issuer = jwtSection["Issuer"] ?? "ATMChallengeIssuer";
var audience = jwtSection["Audience"] ?? "ATMChallengeAudience";

var signingKey = Encoding.UTF8.GetBytes(key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(signingKey)
    };

    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Aplicar migraciones con reintentos (para Docker)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var db = services.GetRequiredService<AppDbContext>();
        
        logger.LogInformation("Iniciando aplicación de migraciones...");
        
        if (db.Database.IsRelational())
        {
            // Reintentar hasta 10 veces (100 segundos total)
            var maxRetries = 10;
            var retryCount = 0;
            
            while (retryCount < maxRetries)
            {
                try
                {
                    logger.LogInformation("Intento {Retry} de {MaxRetries} para aplicar migraciones...", retryCount + 1, maxRetries);
                    
                    db.Database.Migrate();
                    
                    logger.LogInformation("? Migraciones aplicadas correctamente.");
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount >= maxRetries)
                    {
                        logger.LogError(ex, "? Error fatal al aplicar migraciones después de {Retries} intentos.", maxRetries);
                        throw;
                    }
                    
                    logger.LogWarning("?? Error al aplicar migraciones (intento {Retry}): {Message}. Reintentando en 10 segundos...", retryCount, ex.Message);
                    Thread.Sleep(10000); // Esperar 10 segundos antes de reintentar
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? Error crítico al inicializar la base de datos.");
        throw;
    }
}

// Habilitar Swagger en todos los ambientes (Development y Production)
// En producción real, esto debería estar detrás de autenticación
app.UseSwagger();
app.UseSwaggerUI();

// Comentar solo para Docker dev, en prod con reverse proxy reactivar
// app.UseHttpsRedirection();

app.UseMiddleware<ATMChallenge.API.Middleware.ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
