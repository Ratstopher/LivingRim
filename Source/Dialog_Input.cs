using System;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_Input : Window
    {
        private string inputText = string.Empty;
        private string responseText = string.Empty;
        private string label;
        private string pawnName;
        private Action<string> onConfirm;
        private Vector2 scrollPosition = Vector2.zero;

        private bool isResizing;
        private Vector2 lastMousePos;

        public Dialog_Input(string pawnName, string confirmButtonText, Action<string> onConfirm)
        {
            this.pawnName = pawnName;
            this.label = $"Chatting with {pawnName}";
            this.onConfirm = onConfirm;
            this.closeOnAccept = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.doCloseX = true;  // Add close button
            this.draggable = true;
            this.resizeable = true;
        }

        public override Vector2 InitialSize => new Vector2(500f, 300f);

        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.y;

            // Display the label
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), label);
            num += 35f;

            // Input field
            Widgets.Label(new Rect(0f, num, inRect.width, 30f), "Your Message:");
            num += 30f;
            inputText = Widgets.TextField(new Rect(0f, num, inRect.width, 30f), inputText);
            num += 35f;

            // Response display
            if (!string.IsNullOrEmpty(responseText))
            {
                Widgets.Label(new Rect(0f, num, inRect.width, 30f), "Response:");
                num += 30f;
                Rect outRect = new Rect(0f, num, inRect.width, inRect.height - num - 40f);
                Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, Text.CalcHeight(responseText, inRect.width - 16f));
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
                Widgets.Label(viewRect, responseText);
                Widgets.EndScrollView();
            }

            // Send button
            if (Widgets.ButtonText(new Rect(0f, inRect.height - 35f, inRect.width, 30f), "Send"))
            {
                onConfirm?.Invoke(inputText);
                inputText = string.Empty;  // Clear input after sending
            }

            HandleResizing(inRect);
        }

        private void HandleResizing(Rect inRect)
        {
            Rect resizeHandle = new Rect(windowRect.width - 20f, windowRect.height - 20f, 20f, 20f);
            GUI.DrawTexture(resizeHandle, BaseContent.WhiteTex);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Mouse.IsOver(resizeHandle))
            {
                isResizing = true;
                lastMousePos = Event.current.mousePosition;
                Event.current.Use();
            }

            if (isResizing)
            {
                if (Event.current.type == EventType.MouseUp)
                {
                    isResizing = false;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 mouseDelta = Event.current.mousePosition - lastMousePos;
                    windowRect.width = Mathf.Max(InitialSize.x, windowRect.width + mouseDelta.x);
                    windowRect.height = Mathf.Max(InitialSize.y, windowRect.height + mouseDelta.y);
                    lastMousePos = Event.current.mousePosition;
                    Event.current.Use();
                }
            }
        }

        public void SetResponseText(string response)
        {
            responseText = response;
        }
    }
}
