using minimal_api;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Interfaces;
using minimal_api.Dominio.Servicos;
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.ModelViews;
using minimal_api.Dominio.Enuns;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
       
            Configuration = configuration;
            key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
        
    }

    private string key = "";

    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigurationServices(IServiceCollection services)
    {

        services.AddAuthentication(option =>
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

        services.AddAuthorization();


        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
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

        services.AddDbContext<DbContexto>(options =>
          {
              options.UseMySql(
            Configuration.GetConnectionString("MySql"),
              ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
              );
          });

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndPoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
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
                     new Claim(ClaimTypes.Role, administrador.Perfil),
                  };


                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
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

            endpoints.MapPost("/administradores/CadastroAdministrador", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
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

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/administradores/ListaAdministradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
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

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/administradores/ConsultaPorID{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
            {
                var administrador = administradorServico.BuscarIdAdministrador(id);
                if (administrador == null) return Results.NotFound();
                return Results.Ok(new AdministradorModelViews
                {
                    Id = administrador.Id,
                    Email = administrador.Email,
                    Perfil = administrador.Perfil
                });

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

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
            endpoints.MapPost("/veiculos/CadastrarVeiculo", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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
            })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                .WithTags("Veiculos");



            endpoints.MapGet("/veiculos/ConsultaPagina", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.TodosVeiculos(pagina);

                return Results.Ok(veiculos);
            })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                .WithTags("Veiculos");

            endpoints.MapGet("/veiculos/ConsultaPorID{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaIdVeiculo(id);
                if (veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);
            })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                .WithTags("Veiculos");

            endpoints.MapPut("/veiculos/AtualizacaoVeiculo{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
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
            })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
                .WithTags("Veiculos");

            endpoints.MapDelete("/veiculos/DeletarVeiculo{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscaIdVeiculo(id);
                if (veiculo == null) return Results.NotFound();
                veiculoServico.DeletarVeiculo(veiculo);
                return Results.NoContent();
            })
                .RequireAuthorization()
                .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                .WithTags("Veiculos");
            #endregion

        });
    }
}