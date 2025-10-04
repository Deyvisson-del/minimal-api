using minimal_api.Dominio.Enuns;
namespace minimal_api.Dominio.Dtos;

    public class AdministradorDTO
{

    public string Email { get; set; } = string.Empty;

    public string Senha { get; set; } = string.Empty;

    public Perfil? Perfil { get; set; }

}