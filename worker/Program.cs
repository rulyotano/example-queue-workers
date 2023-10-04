using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const int DelayTimeInMs = 10*1000;

Console.WriteLine("Started new worker!");

var rabbitHost = Environment.GetEnvironmentVariable("Rabbit__Host");
var rabbitUser = Environment.GetEnvironmentVariable("Rabbit__User");
var rabbitPassword = Environment.GetEnvironmentVariable("Rabbit__Password");

var factory = new ConnectionFactory { HostName = rabbitHost, UserName = rabbitUser, Password = rabbitPassword };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

const string workerQueueName = "worker_queue";
channel.QueueDeclare(queue: workerQueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

Console.WriteLine("Worker listening for messages...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, deliverArgs) =>
{
    byte[] body = deliverArgs.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Received {message}");

    Thread.Sleep(DelayTimeInMs);

    Console.WriteLine($"Message Processed {message}");

    // here channel could also be accessed as ((EventingBasicConsumer)sender).Model
    channel.BasicAck(deliveryTag: deliverArgs.DeliveryTag, multiple: false);
};

channel.BasicConsume(queue: workerQueueName, autoAck: false, consumer: consumer);
while (true)
{
    await Task.Delay(1000);
}