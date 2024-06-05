using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace LivingRim
{
    public class InMemoryStorage : IExposable
    {
        private static Dictionary<string, DialogState> _pawnMessages = new Dictionary<string, DialogState>();
        private static PersonaDescription _personaDescription = new PersonaDescription();
        private static Dictionary<string, WindowState> _windowStates = new Dictionary<string, WindowState>();

        public static Dictionary<string, DialogState> PawnMessages
        {
            get => _pawnMessages;
            set => _pawnMessages = value;
        }

        public static PersonaDescription PersonaDescription
        {
            get => _personaDescription;
            set => _personaDescription = value;
        }

        public static Dictionary<string, WindowState> WindowStates
        {
            get => _windowStates;
            set => _windowStates = value;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref _pawnMessages, "PawnMessages", LookMode.Value, LookMode.Deep);
            Scribe_Deep.Look(ref _personaDescription, "PersonaDescription");
            Scribe_Collections.Look(ref _windowStates, "WindowStates", LookMode.Value, LookMode.Deep);
        }

        public static void SaveDialogInputState(string characterId, string lastUserMessage, string lastModelResponse)
        {
            _pawnMessages[characterId] = new DialogState(lastUserMessage, lastModelResponse);
            SaveAllToDisk();
        }

        public static (string lastUserMessage, string lastModelResponse) GetDialogInputState(string characterId)
        {
            if (_pawnMessages.ContainsKey(characterId))
            {
                var state = _pawnMessages[characterId];
                return (state.LastUserMessage, state.LastModelResponse);
            }
            return (string.Empty, string.Empty);
        }

        public static void SavePersonaDescription(string persona, string description)
        {
            _personaDescription.Persona = persona;
            _personaDescription.Description = description;
            SaveAllToDisk();
        }

        public static PersonaDescription GetPersonaDescription()
        {
            return _personaDescription;
        }

        public static void SaveWindowState(string windowId, Vector2 position, Vector2 size)
        {
            _windowStates[windowId] = new WindowState(position, size);
            SaveAllToDisk();
        }

        public static WindowState GetWindowState(string windowId)
        {
            if (_windowStates == null)
            {
                _windowStates = new Dictionary<string, WindowState>();
            }
            if (_windowStates.ContainsKey(windowId))
            {
                return _windowStates[windowId];
            }
            return new WindowState(Vector2.zero, new Vector2(600f, 500f)); // Default size
        }

        public static void SaveAllToDisk()
        {
            try
            {
                Scribe.saver.InitSaving(Path_Helper.GetSaveDataFilePath("InMemoryStorage"), "InMemoryStorage");
                var storage = Singleton;
                Scribe_Deep.Look(ref storage, "InMemoryStorage");
                Scribe.saver.FinalizeSaving();
                Log.Message("Data serialized to disk.");
            }
            catch (Exception e)
            {
                Log.Error($"Error during SaveAllToDisk: {e.Message}\n{e.StackTrace}");
            }
        }

        public static void LoadAllFromDisk()
        {
            string filePath = Path_Helper.GetSaveDataFilePath("InMemoryStorage");
            if (!File.Exists(filePath))
            {
                Log.Warning($"File not found: {filePath}. Creating a new file.");
                SaveAllToDisk();  // Create the file to avoid further warnings
                return;
            }

            try
            {
                Scribe.loader.InitLoading(filePath);
                var storage = Singleton;
                Scribe_Deep.Look(ref storage, "InMemoryStorage");
                Scribe.loader.FinalizeLoading();
                Log.Message("Data deserialized from disk.");
            }
            catch (Exception e)
            {
                Log.Error($"Error during LoadAllFromDisk: {e.Message}\n{e.StackTrace}");
            }
        }

        // Singleton pattern to ensure only one instance of InMemoryStorage is used
        private static InMemoryStorage _singleton;
        public static InMemoryStorage Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new InMemoryStorage();
                }
                return _singleton;
            }
        }
    }
}
