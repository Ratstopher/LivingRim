using Verse;

namespace LivingRim
{
    public class ChatLogEntry : IExposable
    {
        // Private fields for properties
        private string characterId;
        private string name;
        private string interaction;
        private string content;
        private string timestamp;

        // Properties
        public string CharacterId { get => characterId; set => characterId = value; }
        public string Name { get => name; set => name = value; }
        public string Interaction { get => interaction; set => interaction = value; }
        public string Content { get => content; set => content = value; }
        public string Timestamp { get => timestamp; set => timestamp = value; }

        public void ExposeData()
        {
            Scribe_Values.Look(ref characterId, "CharacterId");
            Scribe_Values.Look(ref name, "Name");
            Scribe_Values.Look(ref interaction, "Interaction");
            Scribe_Values.Look(ref content, "Content");
            Scribe_Values.Look(ref timestamp, "Timestamp");
        }
    }
}
