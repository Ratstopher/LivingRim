using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace LivingRim
{
    public class LLMService
    {
        public static void GetResponseFromLLM(string prompt, string characterId, Action<string> callback)
        {
            string requestContent = null;

            try
            {
                Log.Message("Entered GetResponseFromLLM method");
                Log.Message($"Prompt: {prompt}, Character ID: {characterId}");

                var requestBody = new
                {
                    characterId = characterId,
                    interactions = new List<string> { prompt },
                    details = CharacterContext.GetCharacterDetails(characterId)
                };

                var jsonWriter = new JsonFx.Json.JsonWriter();
                requestContent = jsonWriter.Write(requestBody);
                Log.Message($"Request Content: {requestContent}");

                string url = "http://localhost:3000/api/v1/chat/completions";
                Log.Message($"URL: {url}");

                Verse.LongEventHandler.QueueLongEvent(() =>
                {
                    Log.Message("Starting coroutine for SendRequest");
                    CoroutineHelper.Instance.StartCoroutine(SendRequest(url, requestContent, callback, characterId, prompt));
                }, "SendingRequest", false, null);
            }
            catch (Exception ex)
            {
                Log.Error($"Exception in GetResponseFromLLM: {ex.Message}\n{ex.StackTrace}");
                callback("Error processing request.");
            }
        }

        private static IEnumerator SendRequest(string url, string requestContent, Action<string> callback, string characterId, string prompt)
        {
            Log.Message("Entered SendRequest coroutine");
            Log.Message($"URL: {url}, RequestContent: {requestContent}");

            var webRequest = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestContent)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

#if UNITY_2019_1_OR_NEWER
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
                    Log.Message($"Response Text: {responseText}");

                    var jsonReader = new JsonFx.Json.JsonReader();
                    var jsonResponse = jsonReader.Read<Dictionary<string, object>>(responseText);
                    Log.Message("Parsed JSON response successfully");

                    if (jsonResponse.TryGetValue("response", out var responseObj) && responseObj is string responseString)
                    {
                        Log.Message("Extracted response text successfully");
                        CharacterContext.AddInteraction(characterId, prompt, responseString);
                        LogInteractionDetails(characterId, prompt, responseString);
                        callback(responseString);
                    }
                    else
                    {
                        Log.Error("Failed to extract response text from JSON");
                        callback("No response from the model.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"JSON Parse Error: {ex.Message}");
                    callback("Error processing request.");
                }
            }
        }

        private static void LogInteractionDetails(string characterId, string prompt, string response)
        {
            string logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "../detailed_chat_log.txt");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var details = CharacterContext.GetCharacterDetails(characterId);

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{timestamp} CharacterId: {characterId}");
                writer.WriteLine($"Prompt: {prompt}");
                writer.WriteLine($"Details: {details}");
                writer.WriteLine($"Response: {response}");
                writer.WriteLine();
            }
        }
    }

    public static class Log
    {
        public static void Error(string message)
        {
            Verse.Log.Error(message);
        }

        public static void Message(string message)
        {
            Verse.Log.Message(message);
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
