using System.Text.Json;
using TotemSenhaAtendimentoServer.Domain.Filas.Dtos;
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

        public async Task<FilaSenhasResponse> ObterFila()
        {
            var mensagensNormais = await _rabbitMqService.ConsultarMensgens("fila_senhas_normal");
            var mensagensPrioritarias = await _rabbitMqService.ConsultarMensgens("fila_senhas_prioritaria");

            var totalNormais = await _rabbitMqService.ContarMensagens("fila_senhas_normal");
            var totalPrioritarias = await _rabbitMqService.ContarMensagens("fila_senhas_prioritaria");

            var senhasNormais = mensagensNormais.Select(m =>
            {
                try
                {
                    return JsonSerializer.Deserialize<Senha>(m) ?? new Senha { Codigo = "Erro de conversão" };
                }
                catch
                {
                    return new Senha { Codigo = "Erro de conversão" };
                }
            }).ToList();

            var senhasPrioritarias = mensagensPrioritarias.Select(m =>
            {
                try
                {
                    return JsonSerializer.Deserialize<Senha>(m) ?? new Senha { Codigo = "Erro de conversão" };
                }
                catch
                {
                    return new Senha { Codigo = "Erro de conversão" };
                }
            }).ToList();

            return new FilaSenhasResponse
            {
                SenhaNormal = senhasNormais,
                SenhaNormalCount = totalNormais,
                SenhaPrioritaria = senhasPrioritarias,
                SenhaPrioritariaCount = totalPrioritarias
            };
        }


        public async Task<Senha?> ChamarProximaSenha(string queueName)
        {
            var mensagem = await _rabbitMqService.ConsumirProximaMensagem(queueName);
            if (string.IsNullOrWhiteSpace(mensagem)) return null;

            try
            {
                var senha = JsonSerializer.Deserialize<Senha>(mensagem, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true, // Ignora maiúsculas e minúsculas nos nomes das propriedades
                    IncludeFields = true, // Garante que campos também sejam incluídos na serialização
                    WriteIndented = true // Apenas para depuração, melhora a formatação do JSON
                });

                return senha;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro ao desserializar a mensagem: {mensagem}. Erro: {ex.Message}");
                return null;
            }
        }


    }
}

