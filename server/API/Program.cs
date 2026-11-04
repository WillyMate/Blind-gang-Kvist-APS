<<<<<<< HEAD

using System.Text.Json.Serialization;
=======
using System.Text;
using System.Text.Json.Serialization;
using API;
>>>>>>> 0a9f370 (AAA)
using API.Nswag;
using Infrastructure;
using LinqToDB;
using LinqToDB.Data;
<<<<<<< HEAD
using Microsoft.AspNetCore.Authentication.Cookies;
=======
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
>>>>>>> 0a9f370 (AAA)
using Service;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
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
=======
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

var options = new DataOptions().UseSQLite(Environment.GetEnvironmentVariable("DB") ?? "Data Source=dev.db");
builder.Services.AddSingleton(new DataOptions<UserDatabase>(options));
builder.Services.AddScoped<UserDatabase>();
builder.Services.AddScoped<DataConnection>(sp => sp.GetRequiredService<UserDatabase>());
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "KvistBjaelke",
            ValidateAudience = true,
            ValidAudience = "KvistBjaelke",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = "role",
            NameClaimType = "name",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
>>>>>>> 0a9f370 (AAA)

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
<<<<<<< HEAD
    settings.SchemaSettings.SchemaProcessors.Add(
        new RequireNotNullableSchemaProcessor()
    )
);
=======
{
    settings.SchemaSettings.SchemaProcessors.Add(new RequireNotNullableSchemaProcessor());
    settings.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Type: Bearer {your token}"
    });
    settings.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});
>>>>>>> 0a9f370 (AAA)

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDatabase>();
<<<<<<< HEAD

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
=======
    Seeder.seed(db);


    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    if (!db.GetTable<UsersDB>().Any(u => u.Role == UsersDB.UserRole.Admin))
    {
        var adminName = builder.Configuration["Seed:AdminUser"] ?? "admin";
        var adminPass = builder.Configuration["Seed:AdminPassword"]
            ?? throw new InvalidOperationException("Seed:AdminPassword is not configured");

        db.Insert(new UsersDB(
            Guid.NewGuid().ToString(),
            adminName,
            hasher.Hash(adminPass),
            UsersDB.UserRole.Admin));
    }
}

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
>>>>>>> 0a9f370 (AAA)
