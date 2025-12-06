using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

public class ChatController : Controller
{
    private readonly string apiKey = "";

    public ActionResult ChatAI()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> SendMessage(string message)
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "gpt-4o-mini",
                input = message
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync("https://api.openai.com/v1/responses", content);
            var responseString = await response.Content.ReadAsStringAsync();

            dynamic data = JsonConvert.DeserializeObject(responseString);

            string reply = data.output[0].content[0].text;

            return Content(reply);
        }
        catch (Exception ex)
        {
            return Content("Lỗi: " + ex.Message);
        }
    }
}
