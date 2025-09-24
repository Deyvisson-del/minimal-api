using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Dtos;

namespace minimal_api.Dominio.Interfaces
{
    public interface IVeiculoServico
    {

        List<Veiculo> Todos(int? pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BuscaIdVeiculo(int id);

        void IncluirVeiculo(Veiculo veiculo);

        void AtualizarVeiculo(Veiculo veiculo);

        void DeletarVeiculo(Veiculo veiculo);

    }
}