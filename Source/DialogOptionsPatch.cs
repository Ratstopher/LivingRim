using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LivingRim
{
    [HarmonyPatch(typeof(Dialog_Options), "DoWindowContents")]
    public static class DialogOptionsPatch
    {
        [HarmonyPostfix]
        public static void AddModSettingsButton(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Gap(12f);

            Rect buttonRect = listingStandard.GetRect(30f);
            if (Widgets.ButtonText(buttonRect, "LivingRim Settings"))
            {
                Find.WindowStack.Add(new Dialog_ModSettings());
            }

            listingStandard.End();
        }
    }
}
