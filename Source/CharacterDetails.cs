using System.Collections.Generic;
using Verse;

namespace LivingRim
{
    public class CharacterDetails : IExposable
    {
        public string characterId;
        public string name;
        public string faction;
        public string gender;
        public string ageBiologicalYears;
        public string ageChronologicalYears;
        public string mood;
        public string health;
        public List<string> hediffs;
        public List<string> capacities;
        public string personality;
        public string relationships;
        public string environment;
        public string needs;
        public string backstory;
        public Dictionary<string, int> skills;
        public Dictionary<string, string> passions;
        public string currentJob;
        public string inventory;
        public List<string> recentEvents;
        public string persona;
        public string description;
        public List<string> pronoun;

        public void ExposeData()
        {
            Scribe_Values.Look(ref characterId, "characterId");
            Scribe_Values.Look(ref name, "name");
            Scribe_Values.Look(ref faction, "faction");
            Scribe_Values.Look(ref gender, "gender");
            Scribe_Values.Look(ref ageBiologicalYears, "ageBiologicalYears");
            Scribe_Values.Look(ref ageChronologicalYears, "ageChronologicalYears");
            Scribe_Values.Look(ref mood, "mood");
            Scribe_Values.Look(ref health, "health");
            Scribe_Collections.Look(ref hediffs, "hediffs", LookMode.Value);
            Scribe_Collections.Look(ref capacities, "capacities", LookMode.Value);
            Scribe_Values.Look(ref personality, "personality");
            Scribe_Values.Look(ref relationships, "relationships");
            Scribe_Values.Look(ref environment, "environment");
            Scribe_Values.Look(ref needs, "needs");
            Scribe_Values.Look(ref backstory, "backstory");
            Scribe_Collections.Look(ref skills, "skills", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref passions, "passions", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref currentJob, "currentJob");
            Scribe_Values.Look(ref inventory, "inventory");
            Scribe_Collections.Look(ref recentEvents, "recentEvents", LookMode.Value);
            Scribe_Values.Look(ref persona, "persona");
            Scribe_Values.Look(ref description, "description");
            Scribe_Collections.Look(ref pronoun, "pronoun", LookMode.Value);
        }
    }
}
