using System;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_DraggableResizableInput : Window
    {
        private string inputText = string.Empty;
        private string label;
        private string responseText = string.Empty;
        private Action<string> onConfirm;
        private bool isResizing;
        private Vector2 lastMousePos;
        private Vector2 scrollPosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="Dialog_DraggableResizableInput"/> class.
        /// </summary>
        /// <param name="label">The label to display in the dialog.</param>
        /// <param name="confirmButtonText">The text for the confirm button.</param>
        /// <param name="onConfirm">The action to perform when the confirm button is clicked.</param>
        public Dialog_DraggableResizableInput(string label, string confirmButtonText, Action<string> onConfirm)
        {
            this.label = label;
            this.onConfirm = onConfirm;
            this.closeOnAccept = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.draggable = true;
            this.resizeable = true;
        }

        /// <summary>
        /// Gets the initial size of the window.
        /// </summary>
        public override Vector2 InitialSize => new Vector2(400f, 400f);

        /// <summary>
        /// Draws the window contents.
        /// </summary>
        /// <param name="inRect">The rectangle in which to draw the contents.</param>
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
            num += 40f;

            // Inlaid panel for displaying response
            Rect scrollViewRect = new Rect(0f, num, inRect.width, inRect.height - num - 10f);
            Rect contentRect = new Rect(0f, 0f, scrollViewRect.width - 16f, Text.CalcHeight(responseText, scrollViewRect.width - 16f));

            Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, contentRect);
            Widgets.Label(contentRect, responseText);
            Widgets.EndScrollView();

            HandleResizing(inRect);
        }

        /// <summary>
        /// Handles the resizing of the window.
        /// </summary>
        /// <param name="inRect">The rectangle of the window.</param>
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

        /// <summary>
        /// Sets the response text to be displayed in the dialog.
        /// </summary>
        /// <param name="response">The response text.</param>
        public void SetResponseText(string response)
        {
            responseText = response;
        }
    }
}
