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

app.MapPost("/administradores/CadastroAdministrador", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
    {
        var validacao = validaAdministradorDTO(administradorDTO);
        if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

        var administrador = new Administrador
        {
            Email = administradorDTO.Email,
            Senha = administradorDTO.Senha,
            Perfil = administradorDTO.Perfil,
        };

        administradorServico.IncluirAdministrador(administrador);

        return Results.Created($"/administrador{administrador.Id}", administrador);

    }).WithTags("Administradores");


#endregion

#region ErrosValidação
ErrosDeValidacao validaVeiculoDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Nome)) validacao.Mensagens.Add("O nome não pode ser vazio");

    if (string.IsNullOrEmpty(veiculoDTO.Marca)) validacao.Mensagens.Add("A marca do veículo não informada");

    if (string.IsNullOrEmpty(veiculoDTO.Modelo)) validacao.Mensagens.Add("O modelo do veículo não foi informada");

    if (veiculoDTO.Ano <= 1950) validacao.Mensagens.Add("O ano do veículo inválido, Ano tem que ser maior que 1949");

    return validacao;
}

ErrosDeValidacao validaAdministradorDTO(AdministradorDTO AdministradorDTO)
{
    var validacao = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(AdministradorDTO.Email)) validacao.Mensagens.Add("O Email não pode ser vazio");

    if (string.IsNullOrEmpty(AdministradorDTO.Senha)) validacao.Mensagens.Add("A senha não pode ser vazia");

    if (AdministradorDTO.Perfil != "Adm") validacao.Mensagens.Add("O perfil deve ser: Adm");

    return validacao;
}
#endregion

#region Veiculos

app.MapPost("/veiculos/CadastrarVeiculo", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var validacao = validaVeiculoDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

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

app.MapGet("/veiculos/ConsultaPagina", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/ConsultaPorID{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaIdVeiculo(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");

app.MapPut("/veiculos/AtualizacaoVeiculo{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var veiculo = veiculoServico.BuscaIdVeiculo(id);
    if (veiculo == null) return Results.NotFound();

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Modelo = veiculoDTO.Modelo;
    veiculo.Ano = veiculoDTO.Ano;

    var validacao = validaVeiculoDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

    veiculoServico.AtualizarVeiculo(veiculo);
    var mensagem = new { mensagem = "Atualização concluída", veiculo };

    return Results.Ok(mensagem);
}).WithTags("Veiculos");

app.MapDelete("/veiculos/DeletarVeiculo{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaIdVeiculo(id);
    if (veiculo == null) return Results.NotFound();
    veiculoServico.DeletarVeiculo(veiculo);
    return Results.NoContent();
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