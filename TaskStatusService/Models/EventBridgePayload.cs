namespace TaskStatusService.Models
{
    public class EventBridgePayload
    {
        public string EventId { get; set; }
        public dynamic AdaptiveCardJson { get; set; }
    }
}
