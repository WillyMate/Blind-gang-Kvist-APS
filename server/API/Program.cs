using Infrastructure;
using LinqToDB;
using LinqToDB.Data;
using Service;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

var options = new DataOptions().UseSQLite(Environment.GetEnvironmentVariable("DB") ??"Data Source=dev.db");
builder.Services.AddSingleton(new DataOptions<UserDatabase>(options));
builder.Services.AddScoped<UserDatabase>();
builder.Services.AddScoped<DataConnection>(sp => sp.GetRequiredService<UserDatabase>());
builder.Services.AddControllers();

builder.Services.AddCors();




var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDatabase>();
    Seeder.seed(db);
}

app.UseCors(_ => _.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().SetIsOriginAllowed(_ => true));
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();
app.Run();
