using HarmonyLib;
using Verse;

namespace LivingRim
{
    /// <summary>
    /// Main entry point for the LivingRim mod. Initializes the Harmony patching system.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.Ratstopher.LivingRim");
            harmony.PatchAll();
        }
    }
}
