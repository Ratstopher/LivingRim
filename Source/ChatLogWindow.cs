using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class ChatLogWindow : Window
    {
        private Pawn pawn;
        private List<ChatLogEntry> chatLogEntries;
        private int currentLogIndex = 0;
        private Vector2 scrollPosition = Vector2.zero;
        private bool isLoading = true;

        public ChatLogWindow(Pawn pawn, List<ChatLogEntry> chatLogEntries)
        {
            Log.Message("Initializing ChatLogWindow...");
            this.pawn = pawn;
            this.chatLogEntries = chatLogEntries ?? new List<ChatLogEntry>();
            this.forcePause = LivingRimMod.settings.pauseOnDialogOpen;
            this.absorbInputAroundWindow = false;
            this.doCloseX = true;
            this.draggable = true;
            this.resizeable = true;
            

            if (this.chatLogEntries.Count == 0)
            {
                FetchChatLogs();
            }
            else
            {
                isLoading = false;
            }

            Log.Message($"ChatLogWindow initialized with {this.chatLogEntries.Count} entries");
        }

        public override Vector2 InitialSize => new Vector2(600f, 500f);

        private void FetchChatLogs()
        {
            LongEventHandler.QueueLongEvent(() =>
            {
                CoroutineHelper.Instance.StartCoroutine(ChatLogFetcher.FetchAndShowLogs(pawn.ThingID.ToString(), SetChatLogs));
            }, "Fetching chat logs", false, null);
        }

        private void SetChatLogs(List<ChatLogEntry> logEntries)
        {
            chatLogEntries = logEntries;
            isLoading = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (isLoading)
            {
                Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "Loading chat logs...");
                return;
            }

            if (chatLogEntries.Count == 0)
            {
                Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), "No chat logs available.");
                return;
            }

            float headerHeight = 30f;
            float buttonHeight = 30f;
            float spacing = 10f;

            Text.Font = GameFont.Medium;
            string headerText = $"{pawn.Name.ToStringShort} - {chatLogEntries[currentLogIndex].Timestamp}";
            Widgets.Label(new Rect(0f, 0f, inRect.width, headerHeight), headerText);

            float responseBoxY = headerHeight + spacing;
            float responseBoxHeight = inRect.height - responseBoxY - buttonHeight - 2 * spacing;
            Rect responseBoxRect = new Rect(inRect.x, responseBoxY, inRect.width, responseBoxHeight);
            Widgets.DrawBoxSolid(responseBoxRect, Color.black);

            Rect scrollViewRect = new Rect(responseBoxRect.x + 10f, responseBoxRect.y + 10f, responseBoxRect.width - 20f, responseBoxRect.height - 20f);
            Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, new Rect(0f, 0f, scrollViewRect.width - 16f, scrollViewRect.height));
            float y = 0f;

            if (currentLogIndex >= 0 && currentLogIndex < chatLogEntries.Count)
            {
                var entry = chatLogEntries[currentLogIndex];
                DrawFormattedText(new Rect(0, y, scrollViewRect.width, Text.CalcHeight(entry.Content, scrollViewRect.width)), entry.Content, entry.Name == "Player");
                y += Text.CalcHeight(entry.Content, scrollViewRect.width) + spacing;
            }

            Widgets.EndScrollView();

            float buttonAreaY = inRect.height - buttonHeight;
            float buttonWidth = 60f;
            float centerX = inRect.width / 2;

            Rect prevButtonRect = new Rect(centerX - buttonWidth - 30f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(prevButtonRect, "<") && currentLogIndex > 0)
            {
                Log.Message("Previous button clicked");
                currentLogIndex--;
                scrollPosition = Vector2.zero;
                Log.Message($"Current log index: {currentLogIndex}");
            }

            Rect backButtonRect = new Rect(centerX - buttonWidth / 2, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(backButtonRect, "Back"))
            {
                Close();
            }

            Rect nextButtonRect = new Rect(centerX + 30f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(nextButtonRect, ">") && currentLogIndex < chatLogEntries.Count - 1)
            {
                Log.Message("Next button clicked");
                currentLogIndex++;
                scrollPosition = Vector2.zero;
                Log.Message($"Current log index: {currentLogIndex}");
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
    }
}
