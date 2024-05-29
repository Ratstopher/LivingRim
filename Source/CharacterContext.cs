using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonFx.Json;
using Verse;
using RimWorld;

namespace LivingRim
{
    public class CharacterContext
    {
        public string CharacterId { get; set; }
        public List<string> Interactions { get; set; } = new List<string>();

        private static string contextFilePath = "Mods/LivingRim/api/context.json";

        /// <summary>
        /// Loads the contexts from the JSON file.
        /// </summary>
        /// <returns>A list of CharacterContext objects.</returns>
        public static List<CharacterContext> LoadContexts()
        {
            Log.Message("Attempting to load contexts.");
            if (!File.Exists(contextFilePath))
            {
                Log.Message("Context file not found. Returning empty list.");
                return new List<CharacterContext>();
            }

            try
            {
                var json = File.ReadAllText(contextFilePath);
                Log.Message($"Raw JSON content: {json}");
                var jsonReader = new JsonReader();
                var contexts = jsonReader.Read<List<CharacterContext>>(json) ?? new List<CharacterContext>();
                Log.Message($"Loaded {contexts.Count} contexts.");
                return contexts;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load contexts: {ex.Message}");
                return new List<CharacterContext>();
            }
        }

        /// <summary>
        /// Saves the contexts to the JSON file.
        /// </summary>
        /// <param name="contexts">The list of CharacterContext objects to save.</param>
        public static void SaveContexts(List<CharacterContext> contexts)
        {
            try
            {
                var jsonWriter = new JsonWriter();
                var json = jsonWriter.Write(contexts);
                File.WriteAllText(contextFilePath, json);
                Log.Message("Contexts saved successfully.");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to save contexts: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets or creates a CharacterContext for a given character ID.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>The CharacterContext object.</returns>
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

        /// <summary>
        /// Adds an interaction to a character's context and logs it.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="interaction">The player's interaction message.</param>
        /// <param name="response">The character's response message.</param>
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

        /// <summary>
        /// Logs the interaction to a file.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <param name="interaction">The player's interaction message.</param>
        /// <param name="response">The character's response message.</param>
        private static void LogInteractionToFile(string characterId, string interaction, string response)
        {
            string logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "../chat_log.txt");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{timestamp} Player: {interaction}");
                    writer.WriteLine($"{timestamp} {GetPawnName(characterId)}: {response}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to log interaction to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the name of a pawn by their character ID.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>The name of the pawn.</returns>
        public static string GetPawnName(string characterId)
        {
            var pawn = Find.CurrentMap?.mapPawns?.AllPawns?.FirstOrDefault(p => p.ThingID.ToString() == characterId);
            return pawn?.Name?.ToStringShort ?? "Unknown";
        }

        /// <summary>
        /// Gets detailed information about a character.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>A CharacterDetails object containing detailed information about the character.</returns>
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
                    backstory = "Unknown"
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
                backstory = GetPawnBackstory(characterId)
            };
        }

        /// <summary>
        /// Gets the personality traits of a pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>A string containing the personality traits.</returns>
        private static string GetPersonalityTraits(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.story.traits.allTraits.Select(t => t.LabelCap)) : "None";
        }

        /// <summary>
        /// Gets the relationships of a pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>A string containing the relationships.</returns>
        private static string GetRelationships(Pawn pawn)
        {
            return pawn != null ? string.Join(", ", pawn.relations.DirectRelations.Select(r => r.def.defName)) : "None";
        }

        /// <summary>
        /// Gets environmental details of a pawn's current location.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>A string containing the environmental details.</returns>
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

        /// <summary>
        /// Gets the needs of a pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>A string containing the needs.</returns>
        private static string GetAllNeeds(Pawn pawn)
        {
            if (pawn.needs == null)
                return "None";

            var needsList = pawn.needs.AllNeeds.Select(n => $"{n.LabelCap}: {n.CurLevelPercentage.ToString("P0")}");
            return string.Join(", ", needsList);
        }

        /// <summary>
        /// Gets the backstory of a pawn.
        /// </summary>
        /// <param name="characterId">The character ID.</param>
        /// <returns>A string containing the backstory.</returns>
        public static string GetPawnBackstory(string characterId)
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
    }
}
