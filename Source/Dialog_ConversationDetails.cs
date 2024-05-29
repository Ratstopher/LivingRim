using System;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_ConversationDetails : Window
    {
        private string characterId;
        private string characterName;
        private string interaction;
        private string response;
        private string timestamp;
        private Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_ConversationDetails"/> class.
        /// </summary>
        /// <param name="characterId">The ID of the character.</param>
        /// <param name="characterName">The name of the character.</param>
        /// <param name="interaction">The player's interaction message.</param>
        /// <param name="response">The character's response message.</param>
        /// <param name="timestamp">The timestamp of the interaction.</param>
        public Dialog_ConversationDetails(string characterId, string characterName, string interaction, string response, string timestamp)
        {
            this.characterId = characterId;
            this.characterName = characterName;
            this.interaction = interaction;
            this.response = response;
            this.timestamp = timestamp;
            this.doCloseX = true;  // Add close button
        }

        /// <summary>
        /// Gets the initial size of the window.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(600f, 400f);

        /// <summary>
        /// Draws the window contents.
        /// </summary>
        /// <param name="inRect">The rectangle in which to draw the contents.</param>
        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.y;

            // Display the character and timestamp
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), $"Character: {characterName} ({characterId})");
            num += 35f;
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), $"Timestamp: {timestamp}");
            num += 35f;

            // Display the interaction
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), $"Interaction: {interaction}");
            num += 35f;

            // Display the response in a scrollable area
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), "Response:");
            num += 30f;
            Rect outRect = new Rect(0f, num, inRect.width, inRect.height - num - 40f);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, Text.CalcHeight(response, inRect.width - 16f));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Widgets.Label(viewRect, response);
            Widgets.EndScrollView();
        }
    }
}
