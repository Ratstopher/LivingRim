using System.Collections.Generic;
using System.IO;
using System.Linq;
using JsonFx.Json;
using Verse;

namespace LivingRim
{
    public class CharacterContext
    {
        public string CharacterId { get; set; }
        public List<string> Interactions { get; set; } = new List<string>();
        public string Name { get; set; }
        public string Mood { get; set; }
        public string Health { get; set; }
        public string Personality { get; set; }
        public string Relationships { get; set; }

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

        public static void AddInteraction(string characterId, string interaction)
        {
            var contexts = LoadContexts();
            var context = GetOrCreateContext(characterId);
            context.Interactions.Add(interaction);
            SaveContexts(contexts);
            Log.Message($"Added interaction for Character ID: {characterId}");
        }

        public static CharacterContext GetCharacterContext(Pawn pawn)
        {
            return new CharacterContext
            {
                CharacterId = pawn.ThingID.ToString(),
                Name = pawn.Name.ToStringShort,
                Mood = pawn.needs.mood.CurLevel.ToString(),
                Health = pawn.health.summaryHealth.SummaryHealthPercent.ToString(),
                Personality = GetPersonalityTraits(pawn),
                Relationships = GetRelationships(pawn)
            };
        }

        private static string GetPersonalityTraits(Pawn pawn)
        {
            return string.Join(", ", pawn.story.traits.allTraits.Select(t => t.LabelCap));
        }

        private static string GetRelationships(Pawn pawn)
        {
            return string.Join(", ", pawn.relations.DirectRelations.Select(r => r.def.defName));
        }
    }
}
