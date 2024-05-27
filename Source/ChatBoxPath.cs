using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LivingRim
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "DoWindowContents")]
    public static class ChatBoxPatch
    {
        private static string playerInput = "";
        private static string characterResponse = "";
        private static Pawn selectedPawn;

        public static void Postfix(MainTabWindow_Inspect __instance, Rect inRect)
        {
            if (Find.Selector.SingleSelectedThing is Pawn pawn)
            {
                selectedPawn = pawn;
            }

            var chatBoxRect = new Rect(inRect.x + 10, inRect.y + 10, inRect.width - 20, 100);
            var inputRect = new Rect(inRect.x + 10, inRect.y + 120, inRect.width - 20, 30);

            Widgets.Label(chatBoxRect, "Character: " + characterResponse);
            playerInput = Widgets.TextField(inputRect, playerInput);

            if (Widgets.ButtonText(new Rect(inRect.x + 10, inRect.y + 160, inRect.width - 20, 30), "Send"))
            {
                SendPlayerInput(playerInput);
                playerInput = "";
            }
        }

        private static async void SendPlayerInput(string input)
        {
            if (selectedPawn != null)
            {
                var context = CharacterContext.GetCharacterContext(selectedPawn);
                characterResponse = await LLMService.GetResponseFromLLM(input);
                StoreContext(selectedPawn.ThingID, input, characterResponse);
            }
        }

        private static void StoreContext(string characterId, string playerInput, string characterResponse)
        {
            var context = new { CharacterId = characterId, PlayerInput = playerInput, CharacterResponse = characterResponse };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(context);
            System.IO.File.WriteAllText($"Mods/LivingRim/api/context_{characterId}.json", json);
        }
    }
}
