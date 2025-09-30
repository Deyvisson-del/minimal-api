#region Usings
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.Servicos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Enuns;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
#endregion

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


builder.Services.AddAuthorization();


builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
          new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
            }, 
            new string[]{ }
         }

    });
});


//builder.Services.AddOpenApi(); dotnet 9

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
    builder.Configuration.GetConnectionString("MySql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql"))
    );
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
#endregion

#region Administradores 

string GerarTokenJwt(Administrador administrador)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email ", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
    };


    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    var adm = administradorServico.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm);

        return Results.Ok(new AdministradorLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administradores");

app.MapPost("/administradores/CadastroAdministrador", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = validaAdministradorDTO(administradorDTO);
    if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.IncluirAdministrador(administrador);

    return Results.Created($"/administrador{administrador.Id}", new AdministradorModelViews
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores/ListaAdministradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelViews>();
    var administradores = administradorServico.TodosAdministradores(pagina, null);
    foreach (var adm in administradores)
    {
        adms.Add(new AdministradorModelViews
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(adms);

}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores/ConsultaPorID{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    var administrador = administradorServico.BuscarIdAdministrador(id);
    if (administrador == null) return Results.NotFound();
    return Results.Ok(new AdministradorModelViews
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    });

}).RequireAuthorization().WithTags("Administradores");

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

    if (veiculoDTO.Ano <= 1949) validacao.Mensagens.Add("O ano do veículo inválido, Ano tem que ser maior que 1949");

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

    if (AdministradorDTO.Perfil == null) validacao.Mensagens.Add("O perfil deve ser vázio");

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
}).RequireAuthorization().WithTags("Veiculos");

app.MapGet("/veiculos/ConsultaPagina", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.TodosVeiculos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");

app.MapGet("/veiculos/ConsultaPorID{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaIdVeiculo(id);
    if (veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veiculos");

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
}).RequireAuthorization().WithTags("Veiculos");

app.MapDelete("/veiculos/DeletarVeiculo{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaIdVeiculo(id);
    if (veiculo == null) return Results.NotFound();
    veiculoServico.DeletarVeiculo(veiculo);
    return Results.NoContent();
}).RequireAuthorization().WithTags("Veiculos");

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

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion