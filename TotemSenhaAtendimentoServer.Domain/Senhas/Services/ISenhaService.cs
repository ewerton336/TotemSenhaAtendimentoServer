using TotemSenhaAtendimentoServer.Domain.Bases;
using TotemSenhaAtendimentoServer.Domain.Senhas.Dtos;
using TotemSenhaAtendimentoServer.Domain.Senhas.Entities;

namespace TotemSenhaAtendimentoServer.Domain.Senhas.Services
{
    public interface ISenhaService : IServiceBase
    {
        Task<Senha> GerarSenha(SenhaRequest request, string queueName);
        Task<List<Senha>> ObterFila(string queueName);
        public Task<Senha?> ChamarProximaSenha(string queueName);


    }
}
