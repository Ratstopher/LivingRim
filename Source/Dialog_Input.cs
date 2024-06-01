using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using System.Collections;
using UnityEngine.Networking;
using JsonFx.Json;

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
        private string persona = string.Empty;
        private string description = string.Empty;
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

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 0f, inRect.width, headerHeight), $"Chat with {pawn.Name.ToStringShort}");

            if (pawn != null)
            {
                Rect pawnRect = new Rect(0f, headerHeight + spacing, 100f, 100f);
                GUI.DrawTexture(pawnRect, PortraitsCache.Get(pawn, new Vector2(100f, 100f), Rot4.South));
            }

            float personaLabelY = headerHeight + portraitHeight + 2 * spacing;
            Widgets.Label(new Rect(110f, personaLabelY, 100f, inputHeight), "Persona:");
            persona = Widgets.TextField(new Rect(210f, personaLabelY, inRect.width - 320f, inputHeight), persona);

            float descriptionLabelY = personaLabelY + inputHeight + spacing;
            Widgets.Label(new Rect(110f, descriptionLabelY, 100f, inputHeight), "Description:");
            description = Widgets.TextField(new Rect(210f, descriptionLabelY, inRect.width - 320f, inputHeight), description);

            if (Widgets.ButtonText(new Rect(inRect.width - 100f, personaLabelY, 80f, inputHeight), "Save"))
            {
                var details = CharacterContext.GetCharacterDetails(pawn.ThingID.ToString(), persona, description);
                CharacterContext.SaveCharacterDetails(details);

            }

            float responseBoxY = descriptionLabelY + inputHeight + 2 * spacing;
            float responseBoxHeight = inRect.height - responseBoxY - buttonHeight - inputHeight - 4 * spacing;
            Rect responseBoxRect = new Rect(inRect.x, responseBoxY, inRect.width, responseBoxHeight);
            Widgets.DrawBoxSolid(responseBoxRect, Color.black);
            Widgets.DrawBoxSolidWithOutline(responseBoxRect, Color.black, Color.white);


            Rect scrollViewRect = new Rect(responseBoxRect.x + 10f, responseBoxRect.y + 10f, responseBoxRect.width - 20f, responseBoxRect.height - 20f);
            Widgets.BeginScrollView(scrollViewRect, ref responseScrollPosition, new Rect(0f, 0f, scrollViewRect.width - 16f, scrollViewRect.height));

            float y = 0f;
            if (!string.IsNullOrEmpty(lastUserMessage))
            {
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(lastUserMessage, scrollViewRect.width)), $"Player: {lastUserMessage}", true, persona);
                y += Text.CalcHeight(lastUserMessage, scrollViewRect.width) + spacing;
            }
            if (!string.IsNullOrEmpty(lastModelResponse))
            {
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(lastModelResponse, scrollViewRect.width)), $"{pawn.Name.ToStringShort}: {lastModelResponse}", false, persona);
                y += Text.CalcHeight(lastModelResponse, scrollViewRect.width) + spacing;
            }
            Widgets.EndScrollView();

            float inputAreaY = inRect.height - inputHeight - spacing;
            Rect inputRect = new Rect(inRect.x, inputAreaY, inRect.width - 100f - spacing, inputHeight);
            inputText = Widgets.TextField(inputRect, inputText);

            Rect sendButtonRect = new Rect(inRect.x + inRect.width - 95f, inputAreaY, 40f, inputHeight);
            if (Widgets.ButtonText(sendButtonRect, "Send") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                SendMessage();
            }

            Rect viewLogsButtonRect = new Rect(inRect.x + inRect.width - 45f, inputAreaY, 40f, inputHeight);
            if (Widgets.ButtonText(viewLogsButtonRect, "Logs"))
            {
                LongEventHandler.QueueLongEvent(() => CoroutineHelper.Instance.StartCoroutine(FetchAndShowLogs(characterId)), "Fetching logs", false, null);
            }

            HandleScrollEvents(inRect);
        }

        private void SendMessage()
        {
            lastUserMessage = inputText;
            inputText = string.Empty;
            onSendCallback?.Invoke(lastUserMessage);
            var interactions = new List<string> { lastUserMessage }; // Ensure interactions is an array
            LLMService.GetResponseFromLLM(interactions, pawn.ThingID.ToString(), CharacterContext.GetCharacterDetails(pawn.ThingID.ToString(), persona, description), SetResponseText);
            ScrollToBottom();
        }



        public void SetResponseText(string response)
        {
            lastModelResponse = response;
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            responseScrollPosition = new Vector2(responseScrollPosition.x, float.MaxValue);
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

        private void DrawFormattedText(Rect rect, string text, bool isPlayer, string persona)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.color = isPlayer ? Color.green : Color.white;

            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            float y = rect.y;

            foreach (var line in lines)
            {
                string formattedLine = isPlayer ? $"{persona}: {line}" : line;
                Rect lineRect = new Rect(rect.x, y, rect.width, Text.CalcHeight(formattedLine, rect.width));
                Widgets.Label(lineRect, formattedLine);
                y += lineRect.height;
            }

            GUI.color = Color.white;
        }

        private IEnumerator FetchAndShowLogs(string characterId)
        {
            string url = $"http://localhost:3000/api/v1/chat/logs/{characterId}";
            var www = new UnityWebRequest(url)
            {
                downloadHandler = new DownloadHandlerBuffer()
            };

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Log.Error("Error fetching chat logs: " + www.error);
            }
            else
            {
                var logs = www.downloadHandler.text;
                // Process the logs as needed
            }
        

        yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Log.Error("Error fetching chat logs: " + www.error);
            }
            else
            {
                var jsonResult = www.downloadHandler.text;
                var logs = new JsonReader().Read<List<ChatLogEntry>>(jsonResult);
                Find.WindowStack.Add(new ChatLogWindow(pawn, logs));
                Close();
            }
        }
    }
}
