using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using UnityEngine.Networking;

namespace LivingRim
{
    public class Dialog_Input : Window
    {
        private string inputText = string.Empty; // User input text
        private Pawn pawn; // The pawn being interacted with
        private Action<string> onSendCallback; // Callback for sending messages
        private bool isLoading = false; // Flag to indicate if chat logs are being loaded
        private string lastUserMessage; // Last message sent by the user
        private string lastModelResponse; // Last response from the model
        private Vector2 responseScrollPosition = Vector2.zero; // Scroll position for the response area

        // Constructor
        public Dialog_Input(Pawn pawn, Action<string> onSendCallback)
        {
            this.pawn = pawn;
            this.onSendCallback = onSendCallback;
            this.closeOnAccept = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.doCloseX = true;  // Add close button
            this.draggable = true;
            this.resizeable = true;
            this.doCloseButton = false;  // Hide the close button in the corner
        }

        // Initial size of the window
        public override Vector2 InitialSize => new Vector2(600f, 500f);

        // Main content rendering function
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

            // Header
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 0f, inRect.width, headerHeight), GetHeaderText());

            // Pawn Portrait
            if (pawn != null)
            {
                Rect pawnRect = new Rect(0f, headerHeight + spacing, 100f, 100f);
                GUI.DrawTexture(pawnRect, PortraitsCache.Get(
                    pawn,
                    new Vector2(100f, 100f),
                    Rot4.South,               // rotation
                    default(Vector3),         // cameraOffset
                    1f,                       // cameraZoom
                    true,                     // renderBody
                    true,                     // renderHeadgear
                    true,                     // renderWeapons
                    true,                     // renderApparel
                    null,                     // apparelColor
                    null,                     // humanlikeBodyColor
                    false,                    // renderArms
                    null                      // pawnHealthState
                ));
            }

            // Response display box
            float responseBoxY = headerHeight + portraitHeight + 2 * spacing;
            float responseBoxHeight = inRect.height - responseBoxY - buttonHeight - inputHeight - 4 * spacing;
            Rect responseBoxRect = new Rect(inRect.x, responseBoxY, inRect.width, responseBoxHeight);
            Widgets.DrawBoxSolid(responseBoxRect, Color.black); // Black color for the response box

            // Scrollable area inside the response box
            Rect scrollViewRect = new Rect(responseBoxRect.x + 10f, responseBoxRect.y + 10f, responseBoxRect.width - 20f, responseBoxRect.height - 20f);
            Widgets.BeginScrollView(scrollViewRect, ref responseScrollPosition, new Rect(0f, 0f, scrollViewRect.width - 16f, scrollViewRect.height));

            float y = 0f;
            if (!string.IsNullOrEmpty(lastUserMessage))
            {
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(lastUserMessage, scrollViewRect.width)), $"Player: {lastUserMessage}", true);
                y += Text.CalcHeight(lastUserMessage, scrollViewRect.width) + spacing; // Adjust the spacing between messages
            }
            if (!string.IsNullOrEmpty(lastModelResponse))
            {
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(lastModelResponse, scrollViewRect.width)), $"{pawn.Name.ToStringShort}: {lastModelResponse}", false);
                y += Text.CalcHeight(lastModelResponse, scrollViewRect.width) + spacing; // Adjust the spacing between messages
            }
            Widgets.EndScrollView();

            // Input Text Area and Buttons at the bottom
            float inputAreaY = inRect.height - inputHeight - spacing;
            Rect inputRect = new Rect(inRect.x, inputAreaY, inRect.width - 100f - spacing, inputHeight);
            inputText = Widgets.TextField(inputRect, inputText);

            // Send Button
            Rect sendButtonRect = new Rect(inRect.x + inRect.width - 95f, inputAreaY, 40f, inputHeight);
            if (Widgets.ButtonText(sendButtonRect, "Send") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                SendMessage();
            }

            // View Logs Button
            Rect viewLogsButtonRect = new Rect(inRect.x + inRect.width - 45f, inputAreaY, 40f, inputHeight);
            if (Widgets.ButtonText(viewLogsButtonRect, "Logs"))
            {
                LongEventHandler.QueueLongEvent(() => CoroutineHelper.Instance.StartCoroutine(FetchAndShowLogs()), "Fetching logs", false, null);
            }

            HandleScrollEvents(inRect);
        }

        // Get header text for the dialog
        protected string GetHeaderText()
        {
            return $"Chat with {pawn.Name.ToStringShort}";
        }

        // Send message function
        private void SendMessage()
        {
            lastUserMessage = inputText;
            inputText = string.Empty;
            onSendCallback?.Invoke(lastUserMessage);
            LLMService.GetResponseFromLLM(lastUserMessage, pawn.ThingID.ToString(), CharacterContext.GetCharacterDetails(pawn.ThingID.ToString()), SetResponseText);
            ScrollToBottom();
        }

        // Set response text from the model
        public void SetResponseText(string response)
        {
            lastModelResponse = response;
            ScrollToBottom();
        }

        // Scroll to the bottom of the response area
        private void ScrollToBottom()
        {
            responseScrollPosition = new Vector2(responseScrollPosition.x, float.MaxValue);
        }

        // Handle scroll events
        private void HandleScrollEvents(Rect inRect)
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.ScrollWheel && inRect.Contains(currentEvent.mousePosition))
            {
                responseScrollPosition.y += currentEvent.delta.y * 20f; // Adjust scroll speed if necessary
                responseScrollPosition.y = Mathf.Clamp(responseScrollPosition.y, 0, float.MaxValue);
                currentEvent.Use();
            }
        }

        // Draw formatted text
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

        // Fetch chat logs and show ChatLogWindow
        private IEnumerator FetchAndShowLogs()
        {
            string url = $"http://localhost:3000/api/v1/chat/logs/{pawn.Name.ToStringShort}";
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
                var jsonResult = www.downloadHandler.text;
                var logs = new JsonFx.Json.JsonReader().Read<List<ChatLogEntry>>(jsonResult);
                Find.WindowStack.Add(new ChatLogWindow(pawn, logs));
                Close();
            }
        }
    }

    // Log helper class
    public static class Log
    {
        public static void Error(string message)
        {
            Verse.Log.Error(message);
        }

        public static void Message(string message)
        {
            Verse.Log.Message(message);
        }
    }
}
