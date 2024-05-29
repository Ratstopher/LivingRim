using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LivingRim
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "DoInspectPaneButtons")]
    public static class ChatBoxPath
    {
        /// <summary>
        /// Prefix method for Harmony patch to add a "Talk" button to the inspect pane.
        /// </summary>
        /// <param name="__instance">The instance of MainTabWindow_Inspect.</param>
        /// <param name="rect">The rectangle area for the inspect pane.</param>
        /// <returns>True to continue with the original method, false to skip it.</returns>
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
                    string pawnName = CharacterContext.GetPawnName(characterId);

                    Dialog_Input dialog = null;
                    dialog = new Dialog_Input(pawnName, "Send", text =>
                    {
                        LLMService.GetResponseFromLLM(text, characterId, response =>
                        {
                            dialog.SetResponseText(response);

                            // Add chat bubble with extended duration
                            ThrowExtendedText(pawn.DrawPos, pawn.Map, response, 10f); // Display for 10 seconds
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

        /// <summary>
        /// Creates a text mote at the specified location with a specified duration.
        /// </summary>
        /// <param name="loc">The location to display the text mote.</param>
        /// <param name="map">The map where the text mote will be displayed.</param>
        /// <param name="text">The text to display.</param>
        /// <param name="duration">The duration the text will be displayed for.</param>
        private static void ThrowExtendedText(Vector3 loc, Map map, string text, float duration)
        {
            if (map == null)
            {
                Log.Error("Cannot spawn Mote_Text in a null map.");
                return;
            }

            MoteText moteText = (MoteText)ThingMaker.MakeThing(ThingDefOf.Mote_Text, null);
            moteText.exactPosition = loc;
            moteText.text = text;
            moteText.textColor = Color.white;
            moteText.def.mote.solidTime = duration;
            GenSpawn.Spawn(moteText, loc.ToIntVec3(), map);
        }
    }
}
