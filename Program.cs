using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.Servicos;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();


var app = builder.Build();
builder.Services.AddDbContext<DbContexto>(
    options =>
    {
        options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
        );
    });

builder.Services.AddOpenApi();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Deyvisson", "William", "Guilherme", "Eduardo", "Cleber", "Carla", "Lucas", "Kauan", "Cauan", "Luis"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 20).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");


app.MapPost("/login", ([FromBody]LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Json("Login com Sucesso");
    }
    //else if (loginDTO.Email == "" && loginDTO.Senha == "")
    //{
    //    return Results.BadRequest(new { erro = "Email e senha vázios" });
    //}
    else { 
        return Results.Unauthorized();
    }
});

app.Run();



record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}