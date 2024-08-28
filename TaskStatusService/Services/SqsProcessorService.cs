using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SqsProcessorService : IHostedService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _queueUrl;
    private readonly string _savenotifydataUrl;
    private readonly string _updatenotifydataUrl;
    private CancellationTokenSource _cts;
    private Task _executingTask;

    public SqsProcessorService(IAmazonSQS sqsClient, IConfiguration configuration, HttpClient httpClient)
    {
        _sqsClient = sqsClient;
        _configuration = configuration;
        _httpClient = httpClient;
        _queueUrl = _configuration.GetValue<string>("SQSQueueUrl");
        _savenotifydataUrl = _configuration.GetValue<string>("SavenotifydataUrl");
        _updatenotifydataUrl = _configuration.GetValue<string>("UpdatenotifydataUrl");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = Task.Run(() => ExecuteAsync(_cts.Token), _cts.Token);
        return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        try
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
        catch (OperationCanceledException)
        {
            // Log or handle exception
        }
    }

    private async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                VisibilityTimeout = 20,
                WaitTimeSeconds = 10
            };

            var receiveMessageResponse = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var message in receiveMessageResponse.Messages)
            {
                try
                {
                    var sqsMessage = JsonConvert.DeserializeObject<dynamic>(message.Body);
                    string endpointUrl = sqsMessage.Type == "NotifyData" ? _savenotifydataUrl : _updatenotifydataUrl;

                    var response = await _httpClient.PostAsync(endpointUrl, new StringContent(message.Body, Encoding.UTF8, "application/json"));

                    if (response.IsSuccessStatusCode)
                    {
                        var deleteMessageRequest = new DeleteMessageRequest
                        {
                            QueueUrl = _queueUrl,
                            ReceiptHandle = message.ReceiptHandle
                        };
                        await _sqsClient.DeleteMessageAsync(deleteMessageRequest, stoppingToken);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to process message: {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while processing message: {ex.Message}");
                }
            }
        }
    }
}