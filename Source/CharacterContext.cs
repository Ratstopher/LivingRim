using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;
using RimWorld;
using JsonFx.Json;

namespace LivingRim
{
    public static class CharacterContext
    {
        public static CharacterDetails GetCharacterDetails(string characterId, string persona, string description)
        {
            var pawn = Find.CurrentMap?.mapPawns?.AllPawns?.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            if (pawn == null)
            {
                Log.Error($"Pawn with Character ID {characterId} not found.");
                return null;
            }

            var details = new CharacterDetails
            {
                characterId = characterId,
                name = pawn.Name?.ToStringShort ?? "Unknown",
                faction = pawn.Faction?.Name ?? "Unknown",
                gender = pawn.gender.ToString(),
                ageBiologicalYears = pawn.ageTracker?.AgeBiologicalYears.ToString() ?? "Unknown",
                ageChronologicalYears = pawn.ageTracker?.AgeChronologicalYears.ToString() ?? "Unknown",
                mood = pawn.needs?.mood?.CurLevel.ToString() ?? "Unknown",
                health = pawn.health?.summaryHealth?.SummaryHealthPercent.ToString() ?? "Unknown",
                personality = GetPersonalityTraits(pawn),
                relationships = GetRelationships(pawn),
                environment = GetEnvironmentDetails(pawn),
                needs = GetAllNeeds(pawn),
                backstory = GetPawnBackstory(pawn),
                skills = GetPawnSkills(pawn),
                passions = GetPawnPassions(pawn),
                currentJob = pawn.CurJob?.def?.label ?? "None",
                inventory = GetPawnInventory(pawn),
                recentEvents = GetPawnRecentEvents(pawn),
                persona = persona,
                description = description
            };

            // Log any fields that are still "Unknown" or null
            LogMissingFields(details);

            return details;
        }

        public static void SaveCharacterDetails(CharacterDetails details)
        {
            string path = Path.Combine(GenFilePaths.SaveDataFolderPath, $"{details.characterId}_details.json");
            string json = new JsonWriter().Write(details);
            File.WriteAllText(path, json);
            Log.Message($"Character details saved to {path}");
        }

        public static CharacterDetails LoadCharacterDetails(string characterId)
        {
            string path = Path.Combine(GenFilePaths.SaveDataFolderPath, $"{characterId}_details.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return new JsonReader().Read<CharacterDetails>(json);
            }
            return null;
        }

        private static void LogMissingFields(CharacterDetails details)
        {
            foreach (var prop in details.GetType().GetProperties())
            {
                var value = prop.GetValue(details);
                if (value == null || value.ToString() == "Unknown")
                {
                    Log.Warning($"Character {details.characterId} - Missing or unknown field: {prop.Name}");
                }
            }
        }

        private static string GetPersonalityTraits(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.story.traits.allTraits.Select(t => t.LabelCap)) : "None";
        }

        private static string GetRelationships(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.relations.DirectRelations.Select(r => r.def.defName)) : "None";
        }

        private static Dictionary<string, int> GetPawnSkills(Pawn pawn)
        {
            return pawn.skills.skills.ToDictionary(skill => skill.def.defName, skill => skill.Level);
        }

        private static Dictionary<string, string> GetPawnPassions(Pawn pawn)
        {
            return pawn.skills.skills.ToDictionary(skill => skill.def.defName, skill => skill.passion.ToString());
        }

        private static string GetPawnInventory(Pawn pawn)
        {
            return pawn.inventory?.innerContainer?.ContentsString ?? "None";
        }

        private static string GetPawnBackstory(Pawn pawn)
        {
            var childhood = pawn.story.Childhood?.title ?? "Unknown Childhood";
            var adulthood = pawn.story.Adulthood?.title ?? "Unknown Adulthood";
            return $"Childhood: {childhood}, Adulthood: {adulthood}";
        }

        private static string GetEnvironmentDetails(Pawn pawn)
        {
            if (pawn == null || pawn.Map == null)
            {
                return "Unknown";
            }

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

        private static List<string> GetPawnRecentEvents(Pawn pawn)
        {
            return new List<string>(); // Add logic to fetch recent events if applicable
        }
    }
}
