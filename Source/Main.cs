using HarmonyLib;
using Verse;

namespace LivingRim
{
    [StaticConstructorOnStartup]
    public static class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.LivingRim.rimworldmod");
            harmony.PatchAll();
        }
    }
}
