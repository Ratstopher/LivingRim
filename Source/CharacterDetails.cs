using System.Collections.Generic;

namespace LivingRim
{
    public class CharacterDetails
    {   
        public string characterId { get; set; }
        public string name { get; set; }
        public string faction { get; set; }
        public string gender { get; set; }
        public string ageBiologicalYears { get; set; }
        public string ageChronologicalYears { get; set; }
        public string mood { get; set; }
        public string health { get; set; }
        public List<string> hediffs { get; set; }
        public List<string> capacities { get; set; }
        public string personality { get; set; }
        public string relationships { get; set; }
        public string environment { get; set; }
        public string needs { get; set; }
        public string backstory { get; set; }
        public Dictionary<string, int> skills { get; set; }
        public Dictionary<string, string> passions { get; set; }
        public string currentJob { get; set; }
        public string inventory { get; set; }
        public List<string> recentEvents { get; set; }
        public string persona { get; set; }
        public string description { get; set; }
        public List<string> pronounw { get; set; }
    }
}
