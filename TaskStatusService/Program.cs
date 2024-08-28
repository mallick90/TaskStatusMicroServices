using Amazon.SQS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddControllers();

                // Register AWS SQS client with default configuration
                services.AddSingleton<IAmazonSQS>(provider =>
                {
                    var options = new AmazonSQSConfig
                    {
                        RegionEndpoint = Amazon.RegionEndpoint.USEast1 // Update with your AWS region
                    };
                    return new AmazonSQSClient(options);
                });

                // Register the SQS Processor Service
                services.AddHttpClient<SqsProcessorService>();
                services.AddHostedService<SqsProcessorService>();
            });
}