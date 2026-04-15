using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text;
using Clients.Services;
using Clients.Services.Myikea;
using Clients.Middleware;
using Clients.Data;
using Clients.Repositories.Myikea;
using Clients.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

// Database - Primary (Clients)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Database - Secondary (MyIkea)
builder.Services.AddDbContext<MyikeaDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MyikeaConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MyikeaConnection"))
    )
);

// Authentication Services
// Usamos un esquema personalizado "jwt" que será manejado por nuestro middleware personalizado
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "jwt";
    options.DefaultChallengeScheme = "jwt";
})
.AddScheme<AuthenticationSchemeOptions, NoOpAuthenticationHandler>("jwt", _ => { });

// Register custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Token Blacklist Service - Singleton porque se comparte entre todos los requests
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();

// Register MyIkea services
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", corsBuilder =>
    {
        // En desarrollo: aceptar cualquier localhost (http://localhost, http://localhost:3000, etc.)
        // En producción: usar la URL configurada
        if (builder.Environment.IsDevelopment())
        {
            corsBuilder
                .SetIsOriginAllowed(origin => 
                    origin.StartsWith("http://localhost") || 
                    origin.StartsWith("http://127.0.0.1") ||
                    origin == "http://localhost" ||
                    origin.StartsWith("http://localhost:"))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
        else
        {
            var frontendUrl = builder.Configuration["AppUrls:FrontendUrl"];
            corsBuilder
                .WithOrigins(frontendUrl!)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Inicializar estructura de carpetas de uploads (dentro de wwwroot)
var uploadDir = app.Configuration["FileUpload:UploadDirectory"];
var absoluteUploadDir = Path.IsPathRooted(uploadDir) ? uploadDir : Path.Combine(Directory.GetCurrentDirectory(), uploadDir);

// Asegurar que el directorio base existe
var uploadBaseDir = Path.GetDirectoryName(absoluteUploadDir);
if (!Directory.Exists(uploadBaseDir))
{
    Directory.CreateDirectory(uploadBaseDir);
    app.Logger.LogInformation($"📁 Carpeta base creada: {uploadBaseDir}");
}

if (!Directory.Exists(absoluteUploadDir))
{
    Directory.CreateDirectory(absoluteUploadDir);
    app.Logger.LogInformation($"📁 Carpeta de uploads creada: {absoluteUploadDir}");
}

// Crear carpeta default para imágenes por defecto
var defaultImageDir = Path.Combine(absoluteUploadDir, "default");
if (!Directory.Exists(defaultImageDir))
{
    Directory.CreateDirectory(defaultImageDir);
    app.Logger.LogInformation($"📁 Carpeta de imágenes por defecto creada: {defaultImageDir}");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Redirect root to Scalar UI
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/scalar/v1", permanent: false);
            return;
        }
        await next();
    });

    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Auth API");
    });
}
else
{
    // Solo usar HTTPS redirection en producción
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

// Configurar acceso a los archivos estáticos con serializacion de caché apropiada
// Esto permite que el navegador acceda directamente a /uploads/* sin ir por un controlador
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        absoluteUploadDir),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        // Desabilitar caché para desarrollo, permitir caché de 1 día en producción
        if (!app.Environment.IsDevelopment())
        {
            const int maxAgeDays = 1;
            ctx.Context.Response.Headers.CacheControl = $"public, max-age={maxAgeDays * 24 * 60 * 60}";
        }
        else
        {
            ctx.Context.Response.Headers.CacheControl = "public, max-age=0, no-cache";
        }
        // Agregar headers CORS para imágenes
        ctx.Context.Response.Headers.AccessControlAllowOrigin = "*";
    }
});

// Middleware personalizado para validar y enriquecer JWT (equivalente a JwtAuthenticationFilter de Java)
// DEBE estar ANTES de app.UseAuthentication() para reemplazar el comportamiento por defecto
app.UseMiddleware<JwtAuthenticationMiddleware>();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();