using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonFx.Json;
using Verse;
using RimWorld;
using UnityEngine;

namespace LivingRim
{
    public class CharacterContext
    {
        public string CharacterId { get; set; }
        public List<string> Interactions { get; set; } = new List<string>();

        private static string contextFilePath = "Mods/LivingRim/api/context.json";

        public static List<CharacterContext> LoadContexts()
        {
            Log.Message("Attempting to load contexts.");
            if (!File.Exists(contextFilePath))
            {
                Log.Message("Context file not found. Returning empty list.");
                return new List<CharacterContext>();
            }

            var json = File.ReadAllText(contextFilePath);
            Log.Message($"Raw JSON content: {json}");
            var jsonReader = new JsonReader();
            var contexts = jsonReader.Read<List<CharacterContext>>(json) ?? new List<CharacterContext>();
            Log.Message($"Loaded {contexts.Count} contexts.");
            return contexts;
        }

        public static void SaveContexts(List<CharacterContext> contexts)
        {
            var jsonWriter = new JsonWriter();
            var json = jsonWriter.Write(contexts);
            File.WriteAllText(contextFilePath, json);
            Log.Message("Contexts saved successfully.");
        }

        public static CharacterContext GetOrCreateContext(string characterId)
        {
            var contexts = LoadContexts();
            var context = contexts.FirstOrDefault(c => c.CharacterId == characterId);
            if (context == null)
            {
                context = new CharacterContext { CharacterId = characterId };
                contexts.Add(context);
                SaveContexts(contexts);
                Log.Message($"Created new context for Character ID: {characterId}");
            }
            return context;
        }

        public static void AddInteraction(string characterId, string interaction, string response)
        {
            var contexts = LoadContexts();
            var context = GetOrCreateContext(characterId);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            context.Interactions.Add($"{timestamp} Player: {interaction}");
            context.Interactions.Add($"{timestamp} {GetPawnName(characterId)}: {response}");

            SaveContexts(contexts);
            LogInteractionToFile(characterId, interaction, response);
            Log.Message($"Added interaction for Character ID: {characterId}");
        }

        private static void LogInteractionToFile(string characterId, string interaction, string response)
        {
            string logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "../chat_log.txt");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{timestamp} Player: {interaction}");
                writer.WriteLine($"{timestamp} {GetPawnName(characterId)}: {response}");
            }
        }

        public static string GetPawnName(string characterId)  // Changed to public
        {
            var pawn = Find.CurrentMap.mapPawns.AllPawns.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            return pawn?.Name?.ToStringShort ?? "Unknown";
        }

        public static object GetCharacterDetails(string characterId)
        {
            var pawn = Find.CurrentMap.mapPawns.AllPawns.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            return new
            {
                name = pawn?.Name?.ToStringShort,
                mood = pawn?.needs?.mood?.CurLevel.ToString(),
                health = pawn?.health?.summaryHealth?.SummaryHealthPercent.ToString(),
                personality = GetPersonalityTraits(pawn),
                relationships = GetRelationships(pawn),
                environment = GetEnvironmentDetails(pawn),
                needs = GetAllNeeds(pawn)
            };
        }

        private static string GetPersonalityTraits(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.story.traits.allTraits.Select(t => t.LabelCap)) : "None";
        }

        private static string GetRelationships(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.relations.DirectRelations.Select(r => r.def.defName)) : "None";
        }

        private static string GetEnvironmentDetails(Pawn pawn)
        {
            var map = pawn.Map;
            var room = pawn.GetRoom(RegionType.Set_All);
            var temperature = map.mapTemperature.OutdoorTemp.ToString("F1");
            var weather = map.weatherManager.curWeather.label;
            var terrain = map.terrainGrid.TerrainAt(pawn.Position).label;
            var biome = map.Biome.label;
            var roomBeauty = room.GetStat(RoomStatDefOf.Impressiveness).ToString("F1");
            var timeOfDay = GenLocalDate.HourInteger(pawn.Map);

            return $"Temperature: {temperature}, Weather: {weather}, Terrain: {terrain}, Biome: {biome}, Room Beauty: {roomBeauty}, Time of Day: {timeOfDay}";
        }

        private static string GetAllNeeds(Pawn pawn)
        {
            if (pawn.needs == null)
                return "None";

            var needsList = pawn.needs.AllNeeds.Select(n => $"{n.LabelCap}: {n.CurLevelPercentage.ToString("P0")}");
            return string.Join(", ", needsList);
        }
    }
}
