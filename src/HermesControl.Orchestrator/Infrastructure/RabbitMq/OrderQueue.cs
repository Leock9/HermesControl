using HermesControl.Orchestrator.Domain;
using HermesControl.Orchestrator.Domain.UseCases;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HermesControl.Orchestrator.Infrastructure.RabbitMq;

public class OrderQueue : IOrderQueue, IDisposable
{
    private readonly IModel _channel;
    private readonly IConnection _connection;

    public OrderQueue(IRabbitMqSettings rabbitMqSettings)
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.HostName,
            UserName = rabbitMqSettings.UserName,
            Password = rabbitMqSettings.Password,
            VirtualHost = "/"
        };

        // Criar conexão e canal persistente
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish(Order order)
    {
        // Declara a fila com durabilidade e persistência de mensagem
        _channel.QueueDeclare(
            queue: order.Status.ToString(),
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order));

        // Propriedades para persistir a mensagem
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        // Publica a mensagem na fila
        _channel.BasicPublish(
            exchange: "",
            routingKey: order.Status.ToString(),
            basicProperties: properties,
            body: body
        );
    }

    public async Task<Order> ConsumeAsync(string status)
    {
        // Declarar a fila se ela não existir
        _channel.QueueDeclare(
            queue: status,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var tcs = new TaskCompletionSource<Order>();
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = JsonSerializer.Deserialize<Order>(message);

                // Confirmar o processamento da mensagem
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
                tcs.SetResult(order);
            }
            catch (Exception)
            {
                // Em caso de falha, reencaminhar a mensagem para a fila
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        // Consumir a mensagem
        _channel.BasicConsume(
            queue: status,
            autoAck: false,  // Confirmação manual habilitada
            consumer: consumer
        );

        // Esperar até que a mensagem seja recebida ou o tempo de espera exceda
        return await tcs.Task;
    }

    // Descartar conexão e canal ao finalizar
    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}
