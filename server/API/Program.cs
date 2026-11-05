using System.Text;
using System.Text.Json.Serialization;
using API;
using API.Nswag;
using Infrastructure;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Service;

var builder = WebApplication.CreateBuilder(args);

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

// Swagger
builder.Services.AddOpenApiDocument(settings =>
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

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDatabase>();
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