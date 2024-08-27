using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaskStatusService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamNotifySaveController : Controller
    {

        
        //[HttpPost]
        //[Route("savenotifydata")]
        //public async Task<IActionResult> SaveData([FromBody] JObject eventJson)
        //{
        //    var eventId = eventJson["id"]?.ToString();

        //    var adaptiveCardJson = eventJson["detail"]?.ToString();  // Extract Adaptive Card JSON from the event

        //    if (string.IsNullOrEmpty(adaptiveCardJson))
        //    {
        //        return BadRequest("Invalid event data");
        //    }

        //    await SaveAdaptiveCardNotifyToDatabase(eventId,adaptiveCardJson);

        //    return Ok("Adaptive Card Info saved successfully");
        //}

        [HttpPost]
        [Route("savedata")]
        public async Task<IActionResult> SaveDataTask()
        {
        
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var connectionString = configurationBuilder.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = "INSERT INTO TASK_STATUS_DETAIL (TaskDesc, EnteredByUser, EnteredDate,TaskStatus) VALUES (@TaskDesc, @EnteredByUser, @EnteredDate,@TaskStatus)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskDesc", "Notification has been Sent To Teams Channel");
                    command.Parameters.AddWithValue("@EnteredByUser", "TRAINZUSER");
                    command.Parameters.AddWithValue("@EnteredDate", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@TaskStatus", "IN-PROGRESS");
                    //command.Parameters.AddWithValue("@UpdatedByUser", "EventBridge");
                    //command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);

                    await command.ExecuteNonQueryAsync();
                }
            }

            return Ok("Task Info saved successfully");
        }

        //private async Task SaveAdaptiveCardNotifyToDatabase(string eventId, object adaptiveCardJson)
        //{
        //    var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        //    var connectionString = configurationBuilder.GetConnectionString("ConnectionStrings");
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        await connection.OpenAsync();

        //        var query = "INSERT INTO TASK_STATUS_DETAIL (AdaptTaskJson, EnteredByUser, EnteredDate) VALUES (@AdaptTaskJson, @EnteredByUser, @EnteredDate)";
        //        using (var command = new SqlCommand(query, connection))
        //        {
        //            command.Parameters.AddWithValue("@EventId", eventId);
        //            command.Parameters.AddWithValue("@AdaptTaskJson", adaptiveCardJson);
        //            command.Parameters.AddWithValue("@EnteredByUser", "EventBridge"); 
        //            command.Parameters.AddWithValue("@EntryDate", DateTime.UtcNow);
        //            //command.Parameters.AddWithValue("@UpdatedByUser", "EventBridge");
        //            //command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);

        //            await command.ExecuteNonQueryAsync();
        //        }
        //    }
        //}
    }
}
