using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace TotemSenhaAtendimentoServer.Infrastructure.Messaging
{
    public class RabbitMqService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName;

        public RabbitMqService(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = configuration["RabbitMQ:UserName"] ?? "admin",  // Adicionado
                Password = configuration["RabbitMQ:Password"] ?? "admin",  // Adicionado
                VirtualHost = "/",
                AutomaticRecoveryEnabled = true, // Para reconectar automaticamente
                RequestedHeartbeat = TimeSpan.FromSeconds(30) // Evita desconexões inesperadas
            };

        }

        public async Task PublicarMensagemAsync<T>(T mensagem, string queueName)
        {
            try
            {
                await using var connection = await _factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensagem));
                var properties = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent
                };

                await channel.BasicPublishAsync(
                      exchange: "",
                      routingKey: queueName,
                      mandatory: false,
                      basicProperties: properties,
                      body: body);
            }
            catch (BrokerUnreachableException ex)
            {
                Console.WriteLine($"Erro ao conectar ao RabbitMQ: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao publicar mensagem: {ex.Message}");
            }
        }

        public async Task<List<string>> ConsumirMensagens(string queueName)
        {
            var mensagens = new List<string>();

            try
            {
                await using var connection = await _factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                     queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var result = await channel.BasicGetAsync(queueName, false);
                while (result != null)
                {
                    var mensagem = Encoding.UTF8.GetString(result.Body.ToArray());
                    mensagens.Add(mensagem);
                    result = await channel.BasicGetAsync(queueName, false);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consumir mensagens: {ex.Message}");
            }

            return mensagens;
        }

        public async Task<string?> ConsumirProximaMensagem(string queueName)
        {
            try
            {
                await using var connection = await _factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                     queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                var result = await channel.BasicGetAsync(queueName, true);
                if (result == null) return null;

                return Encoding.UTF8.GetString(result.Body.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consumir próxima mensagem: {ex.Message}");
                return null;
            }
        }

    }
}