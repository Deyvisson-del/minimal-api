#region Usings
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Dtos;
#endregion

namespace minimal_api.Dominio.Interfaces
{
    public interface IAdministradorServico
    {
        Administrador? Login(LoginDTO loginDTO);
        void IncluirAdministrador(Administrador administrador);
        Administrador? BuscarIdAdministrador(int id);
        List<Administrador> TodosAdministradores(int? pagina, string? email);
    }
}