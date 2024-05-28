using System;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_Input : Window
    {
        private string inputText = string.Empty;
        private string label;
        private Action<string> onConfirm;

        public Dialog_Input(string label, string confirmButtonText, Action<string> onConfirm)
        {
            this.label = label;
            this.onConfirm = onConfirm;
            this.closeOnAccept = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 200f);

        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.y;
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), label);
            num += 35f;
            inputText = Widgets.TextField(new Rect(0f, num, inRect.width, 30f), inputText);
            num += 35f;
            if (Widgets.ButtonText(new Rect(0f, num, inRect.width, 30f), "Send"))
            {
                onConfirm?.Invoke(inputText);
                Close();
            }
        }

        public void SetResponseText(string response)
        {
            inputText = response;
        }
    }
}
