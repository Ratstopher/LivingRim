using System;
using Verse;

namespace LivingRim
{
    public class PersonaDescription : IExposable
    {
        public string Persona;
        public string Description;

        public PersonaDescription()
        {
            Persona = "Stranger";
            Description = "A marooned stranger.";
        }

        public PersonaDescription(string persona, string description)
        {
            Persona = persona;
            Description = description;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref Persona, "Persona", "Stranger");
            Scribe_Values.Look(ref Description, "Description", "A marooned stranger.");
        }
    }
}
