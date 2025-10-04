using minimal_api.Dominio.Entidades;
namespace Teste.Dominio.Entidades;

[TestClass]
public class VeiculosTeste
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var veiculo = new Veiculo();

        //Act
        veiculo.Id = 1;
        veiculo.Nome = "Lettie Crispin";
        veiculo.Marca = "Lincoln";
        veiculo.Modelo = "Mark VIII";
        veiculo.Ano = 1995;

        //Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("Lettie Crispin", veiculo.Nome);
        Assert.AreEqual("Lincoln", veiculo.Marca);
        Assert.AreEqual("Mark VIII", veiculo.Modelo);
        Assert.AreEqual(1995, veiculo.Ano);
    }
}