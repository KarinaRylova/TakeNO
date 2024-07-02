using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var webhookEvent = JsonConvert.DeserializeObject<JiraWebhookEvent>(body);
            await HandleWebhookEvent(webhookEvent);
            return Ok();
        }

        private async Task HandleWebhookEvent(JiraWebhookEvent webhookEvent)
        {
            // Обработка события
            if (webhookEvent.WebhookEvent == "jira:issue_updated")
            {
                var issue = webhookEvent.Issue;
                var message = $"Issue updated: {issue.Key} - {issue.Fields.Summary}";
                await SendMessageToTelegram(message);
            }
        }

        private async Task SendMessageToTelegram(string message)
        {
            var chatId = "YOUR_CHAT_ID"; // Замените на ваш chat ID
            var botToken = "YOUR_BOT_TOKEN"; // Замените на ваш токен бота
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={message}";
            using var httpClient = new HttpClient();
            await httpClient.GetAsync(url);
        }
    }

    public class JiraWebhookEvent
    {
        public string WebhookEvent { get; set; }
        public Issue Issue { get; set; }
        // Дополнительные поля в зависимости от типа webhook
    }

    public class Issue
    {
        public string Key { get; set; }
        public Fields Fields { get; set; }
    }

    public class Fields
    {
        public string Summary { get; set; }
        public User Assignee { get; set; }
        public Status Status { get; set; }
    }

    public class User
    {
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
    }

    public class Status
    {
        public string Name { get; set; }
    }
}
