using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LivingRim
{
    public class LLMService
    {
        /// <summary>
        /// Gets a response from the LLM based on the provided prompt and character ID.
        /// </summary>
        /// <param name="prompt">The user's input prompt.</param>
        /// <param name="characterId">The ID of the character.</param>
        /// <param name="callback">The callback to handle the response.</param>
        public static void GetResponseFromLLM(string prompt, string characterId, Action<string> callback)
        {
            string requestContent = null;

            try
            {
                Log.Message("Entered GetResponseFromLLM method");
                Log.Message($"Prompt: {prompt}, Character ID: {characterId}");

                CharacterDetails details = CharacterContext.GetCharacterDetails(characterId);

                if (details.name == "Unknown")
                {
                    Log.Error($"Character details for ID {characterId} are unknown.");
                    callback("Error: Character not found.");
                    return;
                }

                var requestBody = new
                {
                    characterId = characterId,
                    interactions = new List<string> { prompt },
                    details = details
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

        /// <summary>
        /// Sends a request to the LLM API.
        /// </summary>
        /// <param name="url">The URL of the LLM API.</param>
        /// <param name="requestContent">The request content in JSON format.</param>
        /// <param name="callback">The callback to handle the response.</param>
        /// <param name="characterId">The ID of the character.</param>
        /// <param name="prompt">The user's input prompt.</param>
        /// <returns>An IEnumerator for coroutine handling.</returns>
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
    }

    public static class Log
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public static void Error(string message)
        {
            Verse.Log.Error(message);
        }

        /// <summary>
        /// Logs a general message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Message(string message)
        {
            Verse.Log.Message(message);
        }
    }
}
