using Verse;

namespace LivingRim
{
    public static class GlobalSettings
    {
        public static string Persona { get; set; } = "Stranger";
        public static string Description { get; set; } = "A marooned stranger.";

        public static void SaveGlobalSettings()
        {
            InMemoryStorage.SaveAllToDisk();
        }

        public static void LoadGlobalSettings()
        {
            InMemoryStorage.LoadAllFromDisk();
        }
    }
}
