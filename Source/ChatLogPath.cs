using System.IO;
using UnityEngine;
using Verse;
using RimWorld;

namespace LivingRim
{
    public class MainTabWindow_ChatLog : MainTabWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private string chatLogContent;

        public MainTabWindow_ChatLog()
        {
            this.closeOnAccept = false;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.doCloseX = true;
        }

        public override Vector2 RequestedTabSize => new Vector2(600f, 800f);

        public override void DoWindowContents(Rect inRect)
        {
            if (chatLogContent == null)
            {
                LoadChatLog();
            }

            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, Text.CalcHeight(chatLogContent, inRect.width - 16f));
            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            Widgets.Label(viewRect, chatLogContent);
            Widgets.EndScrollView();
        }

        private void LoadChatLog()
        {
            string logFilePath = Path.Combine(GenFilePaths.SaveDataFolderPath, "chat_log.txt");

            if (File.Exists(logFilePath))
            {
                chatLogContent = File.ReadAllText(logFilePath);
            }
            else
            {
                chatLogContent = "No chat log available.";
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class ChatLogTab
    {
        static ChatLogTab()
        {
            var newDef = new MainButtonDef
            {
                defName = "ChatLog",
                label = "Chat Log",
                buttonVisible = true,
                tabWindowClass = typeof(MainTabWindow_ChatLog),
                order = MainButtonDefOf.Inspect.order + 1
            };

            DefDatabase<MainButtonDef>.Add(newDef);
        }
    }
}
