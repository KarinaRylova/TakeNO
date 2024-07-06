using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class JiraWebhookController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private const string TelegramBotToken = "YOUR_TELEGRAM_BOT_TOKEN";
    private const string TelegramChatId = "YOUR_TELEGRAM_CHAT_ID";

    public JiraWebhookController()
    {
        _httpClient = new HttpClient();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] JObject webhookData)
    {
        var issueKey = webhookData["issue"]?["key"]?.ToString();
        var issueSummary = webhookData["issue"]?["fields"]?["summary"]?.ToString();
        var issueDescription = webhookData["issue"]?["fields"]?["description"]?.ToString();
        var issueAssignee = webhookData["issue"]?["fields"]?["assignee"]?["displayName"]?.ToString();
        var issueStatus = webhookData["issue"]?["fields"]?["status"]?["name"]?.ToString();

        var message = $"New issue created:\n\nKey: {issueKey}\nSummary: {issueSummary}\nDescription: {issueDescription}\nAssignee: {issueAssignee}\nStatus: {issueStatus}";

        var telegramUrl = $"https://api.telegram.org/bot{TelegramBotToken}/sendMessage";
        var payload = new
        {
            chat_id = TelegramChatId,
            text = message
        };

        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(telegramUrl, content);

        if (response.IsSuccessStatusCode)
        {
            return Ok();
        }

        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
