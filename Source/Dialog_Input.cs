using RimWorld;
using System.Collections.Generic;
using System;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_Input : Window
    {
        private string inputText = string.Empty;
        private Pawn pawn;
        private Action<string> onSendCallback;
        private bool isLoading = false;
        private string lastUserMessage;
        private string lastModelResponse;
        private Vector2 responseScrollPosition = Vector2.zero;
        private string characterId;

        public Dialog_Input(Pawn pawn, Action<string> onSendCallback)
        {
            this.pawn = pawn;
            this.onSendCallback = onSendCallback;
            this.closeOnAccept = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.doCloseX = true;
            this.draggable = true;
            this.resizeable = true;
            this.doCloseButton = false;
            this.characterId = pawn.ThingID.ToString();

            GlobalSettings.LoadGlobalSettings();

            LoadState(); // Load state when the window is created
        }

        public override Vector2 InitialSize => new Vector2(600f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            if (isLoading)
            {
                Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "Loading chat logs...");
                return;
            }

            float headerHeight = 30f;
            float portraitHeight = 100f;
            float buttonHeight = 30f;
            float inputHeight = 30f;
            float spacing = 10f;
            float buttonWidth = 80f;

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 0f, inRect.width, headerHeight), $"Chat with {pawn.Name.ToStringShort}");

            if (pawn != null)
            {
                Rect pawnRect = new Rect(0f, headerHeight + spacing, 100f, 100f);
                GUI.DrawTexture(pawnRect, PortraitsCache.Get(pawn, new Vector2(100f, 100f), Rot4.South));
            }

            float responseBoxY = headerHeight + portraitHeight + 2 * spacing;
            float responseBoxHeight = inRect.height - responseBoxY - 2 * buttonHeight - 3 * spacing;
            Rect responseBoxRect = new Rect(inRect.x, responseBoxY, inRect.width, responseBoxHeight);
            Widgets.DrawBoxSolid(responseBoxRect, Color.black);
            Widgets.DrawBoxSolidWithOutline(responseBoxRect, Color.black, Color.white);

            Rect scrollViewRect = new Rect(responseBoxRect.x + 10f, responseBoxRect.y + 10f, responseBoxRect.width - 20f, responseBoxRect.height - 20f);
            Widgets.BeginScrollView(scrollViewRect, ref responseScrollPosition, new Rect(0f, 0f, scrollViewRect.width - 16f, scrollViewRect.height));

            float y = 0f;
            if (!string.IsNullOrEmpty(lastUserMessage))
            {
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(lastUserMessage, scrollViewRect.width)), $"{GlobalSettings.Persona}: {lastUserMessage}", true);
                y += Text.CalcHeight(lastUserMessage, scrollViewRect.width) + spacing;
            }
            if (!string.IsNullOrEmpty(lastModelResponse))
            {
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(lastModelResponse, scrollViewRect.width)), $"{lastModelResponse}", false);
                y += Text.CalcHeight(lastModelResponse, scrollViewRect.width) + spacing;
            }
            Widgets.EndScrollView();

            float buttonAreaY = responseBoxY + responseBoxHeight + spacing;

            Rect personasButtonRect = new Rect(inRect.x + inRect.width - 95f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(personasButtonRect, "Personas"))
            {
                Find.WindowStack.Add(new Dialog_Personas());
            }

            Rect logsButtonRect = new Rect(inRect.x + inRect.width - 185f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(logsButtonRect, "Logs"))
            {
                LongEventHandler.QueueLongEvent(() => CoroutineHelper.Instance.StartCoroutine(ChatLogFetcher.FetchAndShowLogs(characterId, logEntries =>
                {
                    Find.WindowStack.Add(new ChatLogWindow(pawn, logEntries));
                    Close();
                })), "Fetching logs", false, null);
            }

            Rect settingsButtonRect = new Rect(inRect.x + inRect.width - 275f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(settingsButtonRect, "Settings"))
            {
                Dialog_ModSettings.Open();
            }

            float inputAreaY = buttonAreaY + buttonHeight + spacing;
            Rect inputRect = new Rect(inRect.x, inputAreaY, inRect.width - 100f - spacing, inputHeight);
            inputText = Widgets.TextField(inputRect, inputText);

            Rect sendButtonRect = new Rect(inRect.x + inRect.width - 95f, inputAreaY, buttonWidth, inputHeight);
            if (Widgets.ButtonText(sendButtonRect, "Send") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                SendMessage();
            }

            HandleScrollEvents(inRect);
        }

        private void SendMessage()
        {
            lastUserMessage = inputText;
            inputText = string.Empty;
            onSendCallback?.Invoke(lastUserMessage);
            var interactions = new List<string> { lastUserMessage };

            var characterDetails = CharacterContext.GetCharacterDetails(pawn.ThingID.ToString(), GlobalSettings.Persona, GlobalSettings.Description);

            LLMService.GetResponseFromLLM(interactions, pawn.ThingID.ToString(), characterDetails, SetResponseText);
        }

        public void SetResponseText(string response)
        {
            lastModelResponse = response;
            InMemoryStorage.SaveDialogInputState(characterId, lastUserMessage, lastModelResponse);
        }

        private void HandleScrollEvents(Rect inRect)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.ScrollWheel && inRect.Contains(currentEvent.mousePosition))
            {
                responseScrollPosition.y += currentEvent.delta.y * 20f;
                responseScrollPosition.y = Mathf.Clamp(responseScrollPosition.y, 0, float.MaxValue);
                currentEvent.Use();
            }
        }

        private void DrawFormattedText(Rect rect, string text, bool isPlayer)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = isPlayer ? Color.green : Color.white;

            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            float y = rect.y;

            foreach (var line in lines)
            {
                Rect lineRect = new Rect(rect.x, y, rect.width, Text.CalcHeight(line, rect.width));
                Widgets.Label(lineRect, line);
                y += lineRect.height;
            }

            GUI.color = Color.white;
        }

        private void SaveState()
        {
            InMemoryStorage.SaveDialogInputState(characterId, lastUserMessage, lastModelResponse);
        }

        private void LoadState()
        {
            InMemoryStorage.LoadAllFromDisk();
            var state = InMemoryStorage.GetDialogInputState(characterId);
            lastUserMessage = state.lastUserMessage;
            lastModelResponse = state.lastModelResponse;
        }
    }
}
