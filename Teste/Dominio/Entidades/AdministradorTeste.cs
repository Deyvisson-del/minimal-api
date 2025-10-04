using minimal_api.Dominio.Entidades;
namespace Teste.Dominio.Entidades;


[TestClass]
public class AdministradorTeste
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var adm = new Administrador();

        //Act

        adm.Id = 1;
        adm.Email = "administrador@teste.com";
        adm.Senha = "123456";
        adm.Perfil = "Adm";

        //Assert

        Assert.AreEqual(1, adm.Id);
        Assert.AreEqual("administrador@teste.com", adm.Email);
        Assert.AreEqual("123456", adm.Senha);
        Assert.AreEqual("Adm", adm.Perfil);

    }

}