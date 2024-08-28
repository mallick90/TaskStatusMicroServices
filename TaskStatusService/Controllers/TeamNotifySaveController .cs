using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using TaskStatusService.Models;

namespace TaskStatusService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeamNotifySaveController : Controller
    {

        [HttpPost]
        [Route("savenotifydata")]
        public async Task<IActionResult> SaveData([FromBody] EventBridgePayload payload)
        {
            var eventId = payload.EventId.ToString();

            var adaptiveCardJson = payload.AdaptiveCardJson.ToString();  // Extract Adaptive Card JSON from the event

            if (string.IsNullOrEmpty(adaptiveCardJson))
            {
                return BadRequest("Invalid event data");
            }

            await SaveAdaptiveCardNotifyToDatabase(eventId, adaptiveCardJson);

            return Ok("Adaptive Card Info saved successfully");
        }

        [HttpPost]
        [Route("updatenotifydata")]
        public async Task<IActionResult> UpdatedatafromTeam([FromBody] TeamResponsePayload teampayload)
        {
            var eventId = teampayload.EventId.ToString();

            var TaskStatusResposne = teampayload.TaskStatusRes.ToString();

            if (string.IsNullOrEmpty(TaskStatusResposne))
            {
                return BadRequest("Invalid event data");
            }

            await UpdateTeamResponseInfo(eventId, TaskStatusResposne);

            return Ok("Response has been saved successfully");
        }

        private async Task UpdateTeamResponseInfo(string eventId, string TaskStatusResposne)
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var connectionString = configurationBuilder.GetConnectionString("PostgreSqlConnection");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var updateQuery = "UPDATE task_status_detail SET task_status = @task_status,updated_by_user=@updated_by_user,updated_date=@updated_date WHERE id = @eventId";
                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@event_id", eventId);
                        command.Parameters.AddWithValue("@updated_by_user", "TrianzUser");
                        command.Parameters.AddWithValue("@updated_date", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@task_status", TaskStatusResposne);
                        var rowsAffected = command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }

        [HttpPost]
        [Route("savedata")]
        public async Task<IActionResult> SaveDataTask()
        {

            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var connectionString = configurationBuilder.GetConnectionString("DefaultConnection");
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var insertQuery = "INSERT INTO task_status_detail (task_desc, entered_by_user, entered_date,task_status)" +
                    " VALUES (@task_desc, @entered_by_user, @entered_date,@task_status)";

                using (var command = new NpgsqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@task_desc", "Notification has been Sent To Teams Channel");
                    command.Parameters.AddWithValue("@entered_by_user", "TrianzUser");
                    command.Parameters.AddWithValue("@entered_date", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@task_status", "IN-PROGRESS");
                    // Execute the insert query
                    var rowsAffected = command.ExecuteNonQuery();
                }
            }

            return Ok("Task Info saved successfully");
        }

        private async Task SaveAdaptiveCardNotifyToDatabase(string eventId, object adaptiveCardJson)
        {
            var configurationBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            var connectionString = configurationBuilder.GetConnectionString("PostgreSqlConnection");

            using (var connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var insertQuery = "INSERT INTO task_status_detail (event_id,task_desc,adapt_task_json, entered_by_user, entered_date,task_status)" +
                    " VALUES (@event_id,@task_desc,@adapt_task_json, @entered_by_user, @entered_date,@task_status)";

                    await using (var command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@event_id", eventId);
                        command.Parameters.AddWithValue("@task_desc", "Notification has been Sent To Teams Channel");
                        command.Parameters.AddWithValue("@adapt_task_json", adaptiveCardJson);
                        command.Parameters.AddWithValue("@entered_by_user", "TrianzUser");
                        command.Parameters.AddWithValue("@entered_date", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@task_status", "IN-PROGRESS");
                        command.ExecuteNonQuery();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
    }
}