using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace WebApplication15.Controllers
{
    public class ChatController : Controller
    {
        // Lưu ý: Thay thế API key của bạn ở đây
        private readonly string apiKey = "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"; // TODO: Thêm API key của bạn vào đây hoặc lưu trong Web.config

        public ActionResult ChatAI()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Content("Vui lòng cấu hình API key trong ChatController.");
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Content("Vui lòng nhập tin nhắn.");
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    // Sửa request body theo API OpenAI đúng
                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new[]
                        {
                            new
                            {
                                role = "system",
                                content = "Bạn là trợ lý ảo hỗ trợ khách hàng về mỹ phẩm và chăm sóc da."
                            },
                            new
                            {
                                role = "user",
                                content = message
                            }
                        },
                        max_tokens = 500,
                        temperature = 0.7
                    };

                    var content = new StringContent(
                        JsonConvert.SerializeObject(requestBody),
                        Encoding.UTF8,
                        "application/json"
                    );

                    // Sửa endpoint đúng theo API OpenAI
                    var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"API Error: {errorContent}");
                        return Content($"Lỗi từ API: {response.StatusCode}");
                    }

                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(responseString);

                    string reply = data.choices[0].message.content;

                    return Content(reply);
                }
            }
            catch (TaskCanceledException)
            {
                return Content("Yêu cầu đã hết thời gian chờ. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chat Error: {ex.Message}");
                return Content("Lỗi: Không thể kết nối đến dịch vụ AI. Vui lòng thử lại sau.");
            }
        }
    }
}
