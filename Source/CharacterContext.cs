using System;
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

        private static string contextFilePath = "Mods/LivingRim/api/context.json";

        public static List<CharacterContext> LoadContexts()
        {
            if (!File.Exists(contextFilePath))
            {
                return new List<CharacterContext>();
            }

            var json = File.ReadAllText(contextFilePath);
            var jsonReader = new JsonReader();
            return jsonReader.Read<List<CharacterContext>>(json) ?? new List<CharacterContext>();
        }

        public static void SaveContexts(List<CharacterContext> contexts)
        {
            var jsonWriter = new JsonWriter();
            var json = jsonWriter.Write(contexts);
            File.WriteAllText(contextFilePath, json);
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
            }
            return context;
        }

        public static void AddInteraction(string characterId, string interaction)
        {
            var contexts = LoadContexts();
            var context = GetOrCreateContext(characterId);
            context.Interactions.Add(interaction);
            SaveContexts(contexts);
        }

        public string Name { get; set; }
        public string Mood { get; set; }
        public string Health { get; set; }
        public string Personality { get; set; }
        public string Relationships { get; set; }

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
