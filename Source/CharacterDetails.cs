using System.Collections.Generic;

namespace LivingRim
{
    public class CharacterDetails
    {
        public string name { get; set; }
        public string mood { get; set; }
        public string health { get; set; }
        public string personality { get; set; }
        public string relationships { get; set; }
        public string environment { get; set; }
        public string needs { get; set; }
        public string backstory { get; set; }
        public Dictionary<string, int> skills { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, string> passions { get; set; } = new Dictionary<string, string>();
        public string currentJob { get; set; }
        public string inventory { get; set; }
        public List<string> recentEvents { get; set; } = new List<string>();
    }
}
