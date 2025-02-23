using TotemSenhaAtendimentoServer.Domain.Bases;
using TotemSenhaAtendimentoServer.Domain.Senhas.Dtos;
using TotemSenhaAtendimentoServer.Domain.Senhas.Entities;

namespace TotemSenhaAtendimentoServer.Domain.Senhas.Services
{
    public interface ISenhaService : IServiceBase
    {
        Senha GerarSenha(SenhaRequest request);
    }
}
