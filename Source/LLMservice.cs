using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using JsonFx.Json;
using Verse;

namespace LivingRim
{
    public class LLMService
    {
        private static string selectedApi = "openrouter"; // Default API

        public static void SetSelectedApi(string api)
        {
            selectedApi = api;
        }

        public static void GetResponseFromLLM(string prompt, string characterId, Action<string> callback)
        {
            Log.Message("Entered GetResponseFromLLM method");
            Log.Message($"Prompt: {prompt}, Character ID: {characterId}");
            
            var config = LoadConfig("Mods/LivingRim/api/config.json");
            if (config == null)
            {
                Log.Error("Config is null");
                callback("Error processing request.");
                return;
            }

            if (!config.Apis.ContainsKey(selectedApi))
            {
                Log.Error($"API configuration for '{selectedApi}' not found.");
                callback("Error processing request.");
                return;
            }

            var apiConfig = config.Apis[selectedApi];
            var detailedPrompt = $"Character context: {prompt}";

            Log.Message("Creating JSON request content");

            var jsonWriter = new JsonWriter();
            var requestContent = jsonWriter.Write(new
            {
                prompt = detailedPrompt,
                max_tokens = apiConfig.MaxTokens,
                temperature = apiConfig.Temperature
            });

            Log.Message($"Request Content: {requestContent}");

            string url;
            if (selectedApi == "openrouter")
            {
                url = "https://openrouter.ai/api/v1/completions";
            }
            else if (selectedApi == "openai")
            {
                url = $"https://api.openai.com/v1/engines/{apiConfig.Model}/completions";
            }
            else
            {
                url = "https://api.cohere.ai/generate";
            }

            Log.Message($"URL: {url}");

            Verse.LongEventHandler.QueueLongEvent(() =>
            {
                CoroutineHelper.Instance.StartCoroutine(SendRequest(url, requestContent, apiConfig.ApiKey, callback, characterId));
            }, "SendingRequest", false, null);
        }

        private static IEnumerator SendRequest(string url, string requestContent, string apiKey, Action<string> callback, string characterId)
        {
            var webRequest = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestContent)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            yield return webRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
#else
            if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
            {
                Log.Error($"Error: {webRequest.error}");
                callback("Error processing request.");
            }
            else
            {
                try
                {
                    var responseText = webRequest.downloadHandler.text;
                    var jsonReader = new JsonReader();
                    var jsonResponse = jsonReader.Read<Dictionary<string, object>>(responseText);
                    
                    Log.Message($"Response JSON: {responseText}");

                    // Extract the response text from the nested dictionary
                    var choices = jsonResponse["choices"] as List<object>;
                    var firstChoice = choices?[0] as Dictionary<string, object>;
                    var response = firstChoice?["text"]?.ToString() ?? "No response from the model.";

                    CharacterContext.AddInteraction(characterId, response);
                    callback(response);
                }
                catch (Exception ex)
                {
                    Log.Error($"JSON Parse Error: {ex.Message}");
                    callback("Error processing request.");
                }
            }
        }

        private static Config LoadConfig(string filePath)
        {
            Log.Message($"Attempting to load config file: {filePath}");
            try
            {
                var json = File.ReadAllText(filePath);
                Log.Message("Config file read successfully. Attempting to parse JSON.");
                var jsonReader = new JsonReader();
                var config = jsonReader.Read<Config>(json);
                Log.Message("Config file parsed successfully.");
                return config;
            }
            catch (FileNotFoundException ex)
            {
                Log.Error($"Config file not found: {filePath}, Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error($"Unexpected error loading config file: {filePath}, Error: {ex.Message}");
            }
            return null;
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

    public static class Log
    {
        public static void Error(string message)
        {
            // Implementation for logging errors
            Verse.Log.Error(message); // Use Verse.Log.Error for better integration with RimWorld's logging system
        }

        public static void Message(string message)
        {
            // Implementation for logging messages
            Verse.Log.Message(message); // Use Verse.Log.Message for better integration with RimWorld's logging system
        }
    }

    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("CoroutineHelper");
                    _instance = obj.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }
    }
}
