using TotemSenhaAtendimentoServer.Domain.Senhas.Entities;

namespace TotemSenhaAtendimentoServer.Domain.Filas.Dtos
{
    public class FilaSenhasResponse
    {
        public int SenhaNormalCount { get; set; }
        public int SenhaPrioritariaCount { get; set; }
        public List<Senha> SenhaNormal { get; set; }
        public List<Senha> SenhaPrioritaria { get; set; }

    }
}
