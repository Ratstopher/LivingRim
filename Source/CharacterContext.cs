using System.Linq;
using Verse;

namespace LivingRim
{
    public class CharacterContext
    {
        public string Name { get; set; }
        public string Mood { get; set; }
        public string Health { get; set; }
        public string Personality { get; set; }
        public string Relationships { get; set; }

        public static CharacterContext GetCharacterContext(Pawn pawn)
        {
            return new CharacterContext
            {
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
