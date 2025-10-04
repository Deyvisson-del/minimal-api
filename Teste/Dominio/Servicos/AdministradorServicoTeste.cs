#region Usings
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Dtos;
using minimal_api.Dominio.Servico;
using minimal_api.Infraestrutura.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
#endregion

namespace Teste.Dominio.Servico;


[TestClass]
public class AdministradorServicoTeste
{
    private DbContexto CriarContextoTeste()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloandOnChange: true)
            .AddEnvironmentVariables();

        var configuration = builder.build();



        var options = new DbContextOptionsBuilder<DbContexto>()
           .UseInMemoryDatabase("TestDatabase")
           .Options;
        return new DbContexto(options);
    }



    [TestMethod]
    public void TesteCadastroAdministrador()
    {
        //Arrange
        var adm = new Administrador();
        adm.Id = 1;
        adm.Email = "administrador@teste.com";
        adm.Senha = "123456";
        adm.Perfil = "Adm";

        //Act
        var context = new CriarContextoTeste();


        //Assert
        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("administrador@teste.com", adm.Email);
        Assert.AreEqual("123456", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);

    }

}