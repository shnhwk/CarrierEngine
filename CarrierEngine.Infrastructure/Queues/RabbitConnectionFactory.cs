using CarrierEngine.Domain.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CarrierEngine.Infrastructure.Queues;

public sealed class RabbitConnectionFactory : IRabbitConnectionFactory
{ 
    private readonly ConnectionFactory _factory;

    public RabbitConnectionFactory(IOptions<RabbitMqOptions> options)
    { 
        _factory = new ConnectionFactory
        {
            HostName = options.Value.HostName,
            Port = options.Value.Port,
            VirtualHost = options.Value.VirtualHost,
            UserName = options.Value.UserName,
            Password = options.Value.Password,
            ClientProvidedName = options.Value.ClientProvidedName,
        };

        if (options.Value.UseSsl)
        {
            _factory.Ssl = new SslOption
            {
                Enabled = true,
                ServerName = options.Value.HostName,
                Version = System.Security.Authentication.SslProtocols.Tls12
            };
        }
    }

    public Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
        => _factory.CreateConnectionAsync(cancellationToken);
}