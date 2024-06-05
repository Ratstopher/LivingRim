using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Verse;

namespace LivingRim
{
    public static class ChatLogFetcher
    {
        public static IEnumerator FetchAndShowLogs(string characterId, Action<List<ChatLogEntry>> callback)
        {
            Log.Message("Starting FetchAndShowLogs coroutine...");
            string url = $"http://localhost:3000/api/v1/chat/logs/{characterId}";
            Log.Message($"Requesting logs from URL: {url}");

            using (UnityWebRequest www = new UnityWebRequest(url, "GET"))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                List<ChatLogEntry> logEntries = new List<ChatLogEntry>();

                if (www.isNetworkError || www.isHttpError)
                {
                    Log.Error($"Error fetching chat logs: {www.error}");
                }
                else
                {
                    Log.Message($"Successfully fetched chat logs: {www.downloadHandler.text}");

                    try
                    {
                        var jsonResult = www.downloadHandler.text;
                        Log.Message($"Raw JSON Result: {jsonResult}");

                        logEntries = JsonHelper.ParseChatLogEntries(jsonResult, characterId);
                        Log.Message($"Parsed chat logs: {logEntries.Count} entries");
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error parsing chat logs: {e.Message}");
                    }
                }

                callback?.Invoke(logEntries);
            }
        }
    }
}
