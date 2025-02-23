namespace TotemSenhaAtendimentoServer.Domain.Senhas.Entities
{
    public class Senha
    {
        public int Id { get; set; }
        public string? Codigo { get; set; }
        public bool Prioritaria { get; set; }
    }
}
