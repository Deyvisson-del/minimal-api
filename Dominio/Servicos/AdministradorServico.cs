# region Usings
using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Dtos;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;
#endregion

namespace minimal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;

        public Administrador? BuscarIdAdministrador(int id)
        {
            return _contexto.Administradores.Where(adm => adm.Id == id).FirstOrDefault();
        }

        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public void IncluirAdministrador(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public List<Administrador> TodosAdministradores(int? pagina = 1, string? email = null)
        {
            var query = _contexto.Administradores.AsQueryable();

            if (!string.IsNullOrEmpty(email)) query = query.Where(adm => EF.Functions.Like(adm.Email.ToLower(), $"%{email}"));
            int itensPagina = 10;
            
            if (pagina != null)
            {
                query = query.Skip(((int)pagina - 1) * itensPagina).Take(itensPagina);
            }
            return query.ToList();
        }
    }
}