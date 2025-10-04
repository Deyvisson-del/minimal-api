namespace minimal_api.Dominio.Dtos
{
    public record VeiculoDTO
    {
        public int Id { get; set; }
     
        public string Nome { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;
      
        public string Modelo { get; set; } = string.Empty;

        public int Ano { get; set; } = 0;
    }
}