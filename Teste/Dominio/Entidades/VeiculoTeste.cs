
using minimal_api.Dominio.Entidades;

namespace Test.Domain.Entidades;

[TestClass]
public class VeiculoTeste
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        //Arrange
        var veiculosTeste = new Veiculo();

        //Act
        veiculosTeste.Id = 104;
        veiculosTeste.Nome = "Luiz Guilherme";
        veiculosTeste.Marca = "Nissan";
        veiculosTeste.Modelo = "GTR 35";
        veiculosTeste.Ano = 2011;

        //Assert
        Assert.AreEqual(104, veiculosTeste.Id);
        Assert.AreEqual("Luiz Guilherme", veiculosTeste.Nome);
        Assert.AreEqual("Nissan", veiculosTeste.Marca);
        Assert.AreEqual("GTR 35", veiculosTeste.Modelo);
        Assert.AreEqual(2011, veiculosTeste.Ano);
    }
}
