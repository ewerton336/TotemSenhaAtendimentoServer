﻿using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                HostName = configuration["RabbitMQ:HostName"] ?? "localhost"
            };
            _queueName = configuration["RabbitMQ:QueueName"] ?? "fila_senhas";
        }

        public async Task PublicarMensagemAsync<T>(T mensagem)
        {
            try
            {
                await using var connection = await _factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: _queueName,
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
                      routingKey: _queueName,
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
    }
}