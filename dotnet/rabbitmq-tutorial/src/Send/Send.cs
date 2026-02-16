using RabbitMQ.Client;
using System.Text;

var factory = new ConnectionFactory
{
    HostName = "localhost",
};
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "direct_logs", type: ExchangeType.Direct);

// Publish a message
var result = GetMessage(args);

if (result.HasValue)
{
    await channel.BasicPublishAsync(
        exchange: "direct_logs",
        routingKey: result.Value.severity,
        body: Encoding.UTF8.GetBytes(result.Value.message)
    );
    Console.WriteLine($" [x] Sent {result.Value.message}");
}
else
{
    Console.WriteLine("Invalid input.");
}

static (string message, string severity)? GetMessage(string[] args)
{
    if (args.Length == 0) return null;
    if (args.Length == 1) return (args[0], "info");

    var severity = args[0];
    var message = string.Join(' ', args[1..]);
    return (message, severity);
}