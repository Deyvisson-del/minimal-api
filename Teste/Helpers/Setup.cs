namespace Api.Teste.Helpers;

public class Setup
{
    public const string Port = "5197";
    public static TesteContexto testeContexto = default!;
    public static WebApplicationFactory<Startup> http = default!;
    public static HttpClient client = default!;

    public static async Task ExecutaComandoSqlAsync(string sql)
    {
        await new DbContexto().Database.ExecuteSqlRawAsync(sql);
    }

    public static async Task<int> ExecutaEntityCountAsync(int id, string nome)
    {
        return aswait new DbContexto().Clientes.Where(c=> c.Id == id && c.Nome)
    }
}