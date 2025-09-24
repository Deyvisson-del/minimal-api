using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.Servicos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
using Microsoft.AspNetCore.Mvc;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddOpenApi(); dotnet 9

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<DbContexto>(options =>
    {
        options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
        );
    });


var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores 
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Json("Login com Sucesso");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");
#endregion

#region Veiculos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Modelo = veiculoDTO.Modelo,
        Ano = veiculoDTO.Ano,
    };
    veiculoServico.IncluirVeiculo(veiculo);

    return Results.Created($"/veiculo{veiculo.Id}", veiculo);
}).WithTags("Veiculos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaIdVeiculo(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

#endregion

#region App
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi(); dotnet 9
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();
#endregion