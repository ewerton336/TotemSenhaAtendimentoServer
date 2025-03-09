using TotemSenhaAtendimentoServer.Domain.Bases;
using TotemSenhaAtendimentoServer.Domain.Filas.Dtos;
using TotemSenhaAtendimentoServer.Domain.Senhas.Dtos;
using TotemSenhaAtendimentoServer.Domain.Senhas.Entities;

namespace TotemSenhaAtendimentoServer.Domain.Senhas.Services
{
    public interface ISenhaService : IServiceBase
    {
        Task<Senha> GerarSenha(SenhaRequest request, string queueName);
        Task<FilaSenhasResponse> ObterFila();
        public Task<Senha?> ChamarProximaSenha(string queueName);


    }
}
