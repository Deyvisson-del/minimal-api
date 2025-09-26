using minimal_api.Dominio.Entidades;
using minimal_api.Dominio.Dtos;
using minimal_api.Infraestrutura.Db;
using minimal_api.Dominio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace minimal_api.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;

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
    }
}