using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LivingRim
{
    public class LLMService
    {
        private static readonly HttpClient client = new HttpClient();
        private static string selectedApi = "openrouter"; // Default API

        public static void SetSelectedApi(string api)
        {
            selectedApi = api;
        }

        public static async Task<string> GetResponseFromLLM(string prompt)
        {
            var config = LoadConfig("Mods/LivingRim/api/config.json");
            var apiConfig = config.Apis[selectedApi];
            var detailedPrompt = $"Character context: {prompt}";

            string requestContent = $"{{\"prompt\": \"{detailedPrompt}\", \"max_tokens\": {apiConfig.MaxTokens}, \"temperature\": {apiConfig.Temperature}}}";
            string url = "";

            if (selectedApi == "openrouter")
            {
                url = "https://api.openrouter.com/v1/completions";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiConfig.ApiKey}");
            }
            else if (selectedApi == "openai")
            {
                url = "https://api.openai.com/v1/engines/" + apiConfig.Model + "/completions";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiConfig.ApiKey}");
            }
            else if (selectedApi == "cohere")
            {
                url = "https://api.cohere.ai/generate";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiConfig.ApiKey}");
            }

            var response = await client.PostAsync(url, new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();

            var jsonResponse = JObject.Parse(responseContent);
            return jsonResponse["choices"][0]["text"].ToString();
        }

        private static Config LoadConfig(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(json);
        }
    }

    public class Config
    {
        public string DefaultApi { get; set; }
        public Dictionary<string, ApiConfig> Apis { get; set; }
    }

    public class ApiConfig
    {
        public string ApiKey { get; set; }
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public string Model { get; set; }
    }
}
