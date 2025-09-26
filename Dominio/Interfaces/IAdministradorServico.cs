using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Dtos;

namespace minimal_api.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        void IncluirAdministrador(Administrador administrador);
        List<Administrador> TodosAdministradores(int? pagina, string? email);
    }
}