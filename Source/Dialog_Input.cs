using System;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_Input : Window
    {
        private string title;
        private string buttonLabel;
        private Action<string> onConfirm;
        private string inputText = "";

        public Dialog_Input(string title, string buttonLabel, Action<string> onConfirm)
        {
            this.title = title;
            this.buttonLabel = buttonLabel;
            this.onConfirm = onConfirm;
            this.doCloseX = true;
            this.closeOnAccept = false;
            this.closeOnClickedOutside = true;
            this.forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 150f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 30f), title);

            Text.Font = GameFont.Small;
            inputText = Widgets.TextField(new Rect(inRect.x, inRect.y + 40f, inRect.width, 30f), inputText);

            if (Widgets.ButtonText(new Rect(inRect.x, inRect.y + 80f, 100f, 30f), buttonLabel))
            {
                onConfirm(inputText);
                Close();
            }
        }
    }
}
