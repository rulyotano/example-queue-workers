using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace producer.Controllers;

[ApiController]
[Route("api/big-task")]
public class BigTaskController : ControllerBase
{
    private readonly ILogger<BigTaskController> _logger;
    private readonly RabbitConfig _configuration;

    public BigTaskController(ILogger<BigTaskController> logger, IOptions<RabbitConfig> rabbitConfig)
    {
        _logger = logger;
        _configuration = rabbitConfig.Value;
    }

    [HttpPost]
    public ActionResult CreateBigTaskAsync()
    {
        var factory = new ConnectionFactory { HostName = _configuration.Host, UserName = _configuration.User, Password= _configuration.Password };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        const string workerQueueName = "worker_queue";
        channel.QueueDeclare(queue: workerQueueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null);

        var channelProps = channel.CreateBasicProperties();
        channelProps.Persistent = true;

        var message = $"LongConsuming-{DateTime.Now.ToFileTimeUtc()}";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: string.Empty, routingKey: workerQueueName, basicProperties: channelProps, body: body);
        _logger.LogInformation("Message created: {Message}", message);
        return Ok();
    }
}
