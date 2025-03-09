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

        public async Task<Senha> GerarSenha(SenhaRequest request, string queueName)
        {
            var codigo = request.Prioritario ? $"P-{_contadorSenhas:D3}" : $"N-{_contadorSenhas:D3}";
            _contadorSenhas++;

            var senha = new Senha
            {
                Id = _contadorSenhas,
                Codigo = codigo,
                Prioritaria = request.Prioritario
            };

            // Publica a senha no RabbitMQ
            await _rabbitMqService.PublicarMensagemAsync(senha, queueName);

            return senha;
        }

        public async Task<List<Senha>> ObterFila(string queueName)
        {
            if (queueName.Contains("prioridade"))
                queueName += "_prioridade";

            var mensagens = await _rabbitMqService.ConsumirMensagens(queueName);
            return mensagens.Select(m => new Senha { Codigo = m, Prioritaria = queueName.Contains("prioridade") }).ToList();
        }

        public async Task<Senha?> ChamarProximaSenha(string queueName)
        {
            if (queueName.Contains("prioridade"))
                queueName += "_prioridade";

            var mensagem = await _rabbitMqService.ConsumirProximaMensagem(queueName);
            if (mensagem == null) return null;

            return new Senha { Codigo = mensagem, Prioritaria = queueName.Contains("prioridade") };
        }

    }
}

