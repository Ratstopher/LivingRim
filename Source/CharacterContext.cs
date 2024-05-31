using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using System.IO;
using System;

namespace LivingRim
{
    public class CharacterContext
    {
        public string CharacterId { get; set; }
        public List<string> Interactions { get; set; } = new List<string>();

        public static void AddInteraction(string characterId, string interaction, string response)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var interactionLog = new List<string>
            {
                $"{timestamp} Player: {interaction}",
                $"{timestamp} {GetPawnName(characterId)}: {response}"
            };

            LogInteractionToFile(characterId, interactionLog);
            Log.Message($"Added interaction for Character ID: {characterId}");
        }

        private static void LogInteractionToFile(string characterId, List<string> interactionLog)
        {
            string logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "../chat_log.txt");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    foreach (var log in interactionLog)
                    {
                        writer.WriteLine(log);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to log interaction to file: {ex.Message}");
            }
        }

        public static string GetPawnName(string characterId)
        {
            var pawn = Find.CurrentMap?.mapPawns?.AllPawns?.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            return pawn?.Name?.ToStringShort ?? "Unknown";
        }

        public static CharacterDetails GetCharacterDetails(string characterId)
        {
            Log.Message($"Attempting to get details for Character ID: {characterId}");
            var pawn = Find.CurrentMap?.mapPawns?.AllPawns?.FirstOrDefault(p => p.ThingID.ToString() == characterId);

            if (pawn == null)
            {
                Log.Error($"Pawn with Character ID {characterId} not found or null map.");
                return new CharacterDetails
                {
                    name = "Unknown",
                    mood = "Unknown",
                    health = "Unknown",
                    personality = "None",
                    relationships = "None",
                    environment = "Unknown",
                    needs = "None",
                    backstory = "Unknown",
                    skills = new Dictionary<string, int>(),
                    passions = new Dictionary<string, string>(),
                    currentJob = "Unknown",
                    inventory = "Unknown",
                    recentEvents = new List<string>()
                };
            }

            return new CharacterDetails
            {
                name = pawn.Name?.ToStringShort ?? "Unknown",
                mood = pawn.needs?.mood?.CurLevel.ToString() ?? "Unknown",
                health = pawn.health?.summaryHealth?.SummaryHealthPercent.ToString() ?? "Unknown",
                personality = GetPersonalityTraits(pawn),
                relationships = GetRelationships(pawn),
                environment = GetEnvironmentDetails(pawn),
                needs = GetAllNeeds(pawn),
                backstory = GetPawnBackstory(characterId),
                skills = GetPawnSkills(pawn),
                passions = GetPawnPassions(pawn),
                currentJob = pawn.CurJob?.def?.label ?? "None",
                inventory = GetPawnInventory(pawn),
                recentEvents = GetPawnRecentEvents(pawn)
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

        private static string GetPawnBackstory(string characterId)
        {
            var pawn = Find.CurrentMap.mapPawns.AllPawns.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            if (pawn != null)
            {
                var childhood = pawn.story.Childhood?.title ?? "Unknown Childhood";
                var adulthood = pawn.story.Adulthood?.title ?? "Unknown Adulthood";
                return $"Childhood: {childhood}, Adulthood: {adulthood}";
            }
            return "Unknown Backstory";
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

        private static List<string> GetPawnRecentEvents(Pawn pawn)
        {
            return new List<string>(); // Add logic to fetch recent events if applicable
        }
    }
}
