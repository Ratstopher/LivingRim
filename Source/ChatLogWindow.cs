using System;
using System.Collections;
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

        public ChatLogWindow(Pawn pawn, List<ChatLogEntry> chatLogEntries)
        {
            this.pawn = pawn;
            this.chatLogEntries = chatLogEntries ?? new List<ChatLogEntry>();
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            float headerHeight = 30f;
            float buttonHeight = 30f;
            float spacing = 10f;

            // Header
            Text.Font = GameFont.Medium;
            string headerText = $"{pawn.Name.ToStringShort} - {DateTime.Now.ToString("M/d/yyyy h:mm tt")}";
            Widgets.Label(new Rect(0f, 0f, inRect.width, headerHeight), headerText);

            // Response display box
            float responseBoxY = headerHeight + spacing;
            float responseBoxHeight = inRect.height - responseBoxY - buttonHeight - 2 * spacing;
            Rect responseBoxRect = new Rect(inRect.x, responseBoxY, inRect.width, responseBoxHeight);
            Widgets.DrawBoxSolid(responseBoxRect, Color.black);

            // Scrollable area inside the response box
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

            // Navigation buttons
            float buttonAreaY = inRect.height - buttonHeight;
            float buttonWidth = 60f;
            float centerX = inRect.width / 2;

            Rect prevButtonRect = new Rect(centerX - buttonWidth - 30f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(prevButtonRect, "<") && currentLogIndex > 0)
            {
                currentLogIndex--;
                scrollPosition = Vector2.zero; // Reset scroll position
            }

            Rect backButtonRect = new Rect(centerX - buttonWidth / 2, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(backButtonRect, "Back"))
            {
                Find.WindowStack.Add(new Dialog_Input(pawn, message => { }));
                Close();
            }

            Rect nextButtonRect = new Rect(centerX + 30f, buttonAreaY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(nextButtonRect, ">") && currentLogIndex < chatLogEntries.Count - 1)
            {
                currentLogIndex++;
                scrollPosition = Vector2.zero; // Reset scroll position
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

    public class ChatLogEntry
    {
        public string CharacterId { get; set; }
        public string Name { get; set; }
        public string Interaction { get; set; }
        public string Content { get; set; }
        public string Timestamp { get; set; }
    }
}
