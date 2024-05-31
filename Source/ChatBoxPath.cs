using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LivingRim
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "DoInspectPaneButtons")]
    public static class ChatBoxPath
    {
        private static bool Prefix(MainTabWindow_Inspect __instance, Rect rect)
        {
            float buttonWidth = 200f;
            float buttonHeight = 30f;
            float buttonX = rect.x + rect.width - buttonWidth - 10f;
            float buttonY = rect.y + rect.height - buttonHeight - 10f;

            if (Widgets.ButtonText(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Talk"))
            {
                var selectedThing = Find.Selector.SingleSelectedThing;
                if (selectedThing is Pawn pawn)
                {
                    Find.WindowStack.Add(new Dialog_Input(pawn, text =>
                    {
                        // This is the callback for sending messages
                        var dialog = Find.WindowStack.WindowOfType<Dialog_Input>();
                        if (dialog != null)
                        {
                            // Additional actions if needed
                        }
                    }));
                }
                else
                {
                    Log.Error("No pawn selected for conversation.");
                }
            }
            return true;
        }
    }
}
