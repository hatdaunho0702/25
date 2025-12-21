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
        // 1. Dán GitHub Personal Access Token của bạn vào đây
        // Lấy key tại: https://github.com/settings/tokens (hoặc từ trang Marketplace khi chọn model)
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
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Content("Vui lòng cấu hình GitHub Token trong ChatController.");
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Content("Vui lòng nhập tin nhắn.");
                }

                using (var httpClient = new HttpClient())
                {
                    // Header Authorization vẫn giữ nguyên định dạng Bearer
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    // 2. Cấu hình Body Request
                    var requestBody = new
                    {
                        // QUAN TRỌNG: GitHub Models thường dùng 'gpt-4o' hoặc 'gpt-4o-mini'
                        model = "gpt-4o",
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

                    // 3. Đổi Endpoint sang Server của GitHub Models (Azure AI Inference)
                    var response = await httpClient.PostAsync("https://models.inference.ai.azure.com/chat/completions", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"API Error: {errorContent}");
                        return Content($"Lỗi từ API GitHub: {response.StatusCode} - {errorContent}");
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