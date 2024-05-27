using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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

        public static async Task<string> GetResponseFromLLM(string prompt, string characterId)
        {
            var config = LoadConfig("Mods/LivingRim/api/config.json");
            var apiConfig = config.Apis[selectedApi];
            var detailedPrompt = $"Character context: {prompt}";

            string requestContent = $"{{\"prompt\": \"{detailedPrompt}\", \"max_tokens\": {apiConfig.MaxTokens}, \"temperature\": {apiConfig.Temperature}}}";
            string url = selectedApi == "openrouter" ? "https://api.openrouter.com/v1/completions" : selectedApi == "openai" ? $"https://api.openai.com/v1/engines/{apiConfig.Model}/completions" : "https://api.cohere.ai/generate";

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiConfig.ApiKey}");

            var response = await client.PostAsync(url, new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var responseContent = await response.Content.ReadAsStringAsync();

            var jsonResponse = JObject.Parse(responseContent);
            var responseText = jsonResponse["choices"][0]["text"].ToString();

            CharacterContext.AddInteraction(characterId, responseText);

            return responseText;
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
