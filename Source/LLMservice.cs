using System;
using System.Collections.Generic;
using System.Text;
using Verse;
using System.Collections;
using UnityEngine.Networking;

namespace LivingRim
{
    public class LLMService
    {
        public static void GetResponseFromLLM(List<string> interactions, string characterId, CharacterDetails details, Action<string> callback)
        {
            Log.Message($"Entered GetResponseFromLLM method");

            var requestContent = new
            {
                characterId = characterId,
                interactions = interactions,
                details = details,
                timestamp = DateTime.Now.ToString("o")
            };

            string requestContentJson = new JsonFx.Json.JsonWriter().Write(requestContent);
            string url = "http://localhost:3000/api/v1/chat/completions";

            Log.Message($"URL: {url}");
            Log.Message($"Request Content: {requestContentJson}");

            CoroutineHelper.Instance.StartCoroutine(SendRequest(url, requestContentJson, callback));
        }

        private static IEnumerator SendRequest(string url, string requestContent, Action<string> callback)
        {
            Log.Message($"Entered SendRequest coroutine");
            using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(requestContent);
                www.uploadHandler = new UploadHandlerRaw(bodyRaw);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Log.Error($"Error: {www.error}");
                    callback?.Invoke($"Error: {www.error}");
                }
                else
                {
                    Log.Message($"Response Text: {www.downloadHandler.text}");
                    try
                    {
                        var response = new JsonFx.Json.JsonReader().Read<ResponseContent>(www.downloadHandler.text);
                        callback?.Invoke(response.content);
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Failed to extract response text from JSON: {e.Message}");
                        callback?.Invoke("Failed to extract response text from JSON");
                    }
                }
            }
        }
    }

    [Serializable]
    public class ResponseContent
    {
        public string content;
    }
}
