using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;

namespace LivingRim
{
    public class MainTabWindow_ChatLog : MainTabWindow
    {
        private Vector2 scrollPosition = Vector2.zero;
        private List<ChatLogEntry> chatLogEntries = new List<ChatLogEntry>();
        private bool isLoading = false;

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
            if (!chatLogEntries.Any() && !isLoading)
            {
                isLoading = true;
                FetchChatLogs();
            }

            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, chatLogEntries.Count * 30f);

            Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
            float y = 0f;
            foreach (var entry in chatLogEntries)
            {
                DrawEntry(entry, ref y, viewRect.width);
            }
            Widgets.EndScrollView();
        }

        private void DrawEntry(ChatLogEntry entry, ref float y, float width)
        {
            Rect rect = new Rect(0f, y, width, 30f);
            if (Widgets.ButtonText(rect, $"Conversation at {entry.Timestamp}", true, false, true))
            {
                Find.WindowStack.Add(new Dialog_ConversationDetails(entry.CharacterId, entry.Name, entry.Interaction, entry.Content, entry.Timestamp));
            }
            y += rect.height + 10f;
        }

        private void FetchChatLogs()
        {
            CoroutineHelper.Instance.StartCoroutine(GetChatLogsFromServer());
        }

        private IEnumerator GetChatLogsFromServer()
        {
            string url = "http://localhost:3000/api/v1/chat/logs";
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
#else
                if (webRequest.isNetworkError || webRequest.isHttpError)
#endif
                {
                    Log.Error($"Error fetching chat logs: {webRequest.error}");
                }
                else
                {
                    try
                    {
                        string json = webRequest.downloadHandler.text;
                        var jsonReader = new JsonFx.Json.JsonReader();
                        chatLogEntries = jsonReader.Read<List<ChatLogEntry>>(json);
                        Log.Message("Chat logs fetched successfully");
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Error parsing chat logs: {ex.Message}");
                    }
                }
                isLoading = false;
            }
        }

        private class ChatLogEntry
        {
            public string CharacterId { get; set; }
            public string Name { get; set; }
            public string Interaction { get; set; }
            public string Content { get; set; }
            public string Timestamp { get; set; }
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

    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject("CoroutineHelper");
                    _instance = obj.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }
    }
}
