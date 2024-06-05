using System;
using System.Collections.Generic;
using Verse;
using JsonFx.Json;

namespace LivingRim
{
    public static class JsonHelper
    {
        public static List<ChatLogEntry> ParseChatLogEntries(string json, string characterId)
        {
            var logEntries = new List<ChatLogEntry>();

            try
            {
                var jsonReader = new JsonReader();
                var rawList = jsonReader.Read<List<Dictionary<string, object>>>(json);

                foreach (var item in rawList)
                {
                    if (item.TryGetValue("characterId", out var charIdObj) && charIdObj.ToString() == characterId)
                    {
                        var entry = new ChatLogEntry
                        {
                            CharacterId = charIdObj.ToString(),
                            Name = item.TryGetValue("name", out var nameObj) ? nameObj.ToString() : string.Empty,
                            Interaction = item.TryGetValue("interaction", out var interactionObj) ? interactionObj.ToString() : string.Empty,
                            Content = item.TryGetValue("content", out var contentObj) ? contentObj.ToString() : string.Empty,
                            Timestamp = item.TryGetValue("timestamp", out var timestampObj) ? timestampObj.ToString() : string.Empty
                        };

                        logEntries.Add(entry);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"Unexpected error when parsing chat log entry: {e.Message}");
            }

            return logEntries;
        }
    }
}
