using Verse;
using RimWorld;

namespace LivingRim
{
    [DefOf]
    public static class KeyBindingDefOf
    {
        public static KeyBindingDef OpenChatBox; // Declare the field

        static KeyBindingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(KeyBindingDefOf));
            OpenChatBox = DefDatabase<KeyBindingDef>.GetNamed("OpenChatBox"); // Assign the value
        }
    }
}
