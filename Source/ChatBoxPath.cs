using System;
using System.Threading.Tasks;
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
            if (Widgets.ButtonText(new Rect(rect.x, rect.y, 200f, 30f), "Talk") || KeyBindingDefOf.OpenChatBox.JustPressed)
            {
                Find.WindowStack.Add(new Dialog_Input("Enter your message:", "Send", async text =>
                {
                    // Call the LLMService to get a response
                    string response = await LLMService.GetResponseFromLLM(text, "characterId"); // Replace with actual characterId
                    Messages.Message(response, MessageTypeDefOf.NeutralEvent, false);
                }));
            }
            return true;
        }
    }
}
