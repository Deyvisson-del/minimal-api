using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using minimal_api.Dominio.Entidades;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Servicos;
using Microsoft.OpenApi.Models;
using Test.Domain.Entidades;
using System.IO;

namespace Test.Domain.Servicos;

[TestClass]
public class AdministradorServicoTeste
{
    private DbContexto CriarContextoDeTeste()
    {
        var basePath = AppContext.BaseDirectory;
        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);
    }

    [TestMethod]
    public void TestarCadastroAdministrador()
    {
        // Arrange 
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Administradores");

        var adm = new Administrador();
        adm.Email = "administrador@teste.com";
        adm.Senha = "123456";
        adm.Perfil = "Adm";
        context = CriarContextoDeTeste();
        var administradorServico = new AdministradorServico(context);

        //Act
        administradorServico.IncluirAdministrador(adm);

        //Assert
        Assert.AreEqual(1, administradorServico.TodosAdministradores(1).Count());
    }
}