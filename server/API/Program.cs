
using System.Text.Json.Serialization;
using API.Nswag;
using Infrastructure;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Service;

var builder = WebApplication.CreateBuilder(args);

// Database
var options = new DataOptions()
    .UseSQLite(
        Environment.GetEnvironmentVariable("DB")
        ?? "Data Source=dev.db"
    );

builder.Services.AddSingleton(
    new DataOptions<UserDatabase>(options)
);

builder.Services.AddScoped<UserDatabase>();

builder.Services.AddScoped<DataConnection>(
    sp => sp.GetRequiredService<UserDatabase>()
);

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Controllers and JSON
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

// Cookie authentication
builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme
    )
    .AddCookie(options =>
    {
        options.Cookie.Name = "KvistAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3001")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger
builder.Services.AddOpenApiDocument(settings =>
    settings.SchemaSettings.SchemaProcessors.Add(
        new RequireNotNullableSchemaProcessor()
    )
);

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDatabase>();

    var passwordHasher =
        scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    Seeder.Seed(db, passwordHasher);
}

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenApi();
app.UseSwaggerUi();

app.MapControllers();

app.Run();
