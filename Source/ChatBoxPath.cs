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
            float buttonX = rect.x + rect.width - buttonWidth - 10f; // Adjusted position to the right
            float buttonY = rect.y + rect.height - buttonHeight - 10f; // Adjusted position to the bottom

            if (Widgets.ButtonText(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Talk"))
            {
                var selectedThing = Find.Selector.SingleSelectedThing;
                if (selectedThing is Pawn pawn)
                {
                    string characterId = pawn.ThingID.ToString(); // Ensure characterId is defined here

                    Dialog_Input dialog = null;
                    dialog = new Dialog_Input("Enter your message:", "Send", text =>
                    {
                        LLMService.GetResponseFromLLM(text, characterId, response =>
                        {
                            dialog.SetResponseText(response);

                            // Add chat bubble
                            MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, response);
                        });
                    });
                    Find.WindowStack.Add(dialog);
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
