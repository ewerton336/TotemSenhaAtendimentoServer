using TotemSenhaAtendimentoServer.Domain.Bases;
using TotemSenhaAtendimentoServer.Domain.Senhas.Dtos;
using TotemSenhaAtendimentoServer.Domain.Senhas.Entities;
using TotemSenhaAtendimentoServer.Domain.Senhas.Services;
using TotemSenhaAtendimentoServer.Infrastructure.Messaging;

namespace TotemSenhaAtendimentoServer.Application.Senhas.Services
{

    public class SenhaService : ISenhaService
    {
        private static int _contadorSenhas = 1;
        private readonly RabbitMqService _rabbitMqService;

        public SenhaService(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public Senha GerarSenha(SenhaRequest request, string queueName)
        {
            var codigo = request.Prioritario ? $"P-{_contadorSenhas:D3}" : $"N-{_contadorSenhas:D3}";
            _contadorSenhas++;

            var senha = new Senha
            {
                Id = _contadorSenhas,
                Codigo = codigo,
                Prioritaria = request.Prioritario
            };

            if (senha.Prioritaria)
                queueName = queueName + "_prioridade";

            // Publica a senha no RabbitMQ
            _rabbitMqService.PublicarMensagemAsync(senha, queueName).Wait();

            return senha;
        }
    }
}

