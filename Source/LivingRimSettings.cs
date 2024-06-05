using System.Collections.Generic;
using Verse;

namespace LivingRim
{
    public class LivingRimSettings : ModSettings
    {
        public bool pauseOnDialogOpen = true;
        public Dictionary<string, string> defaultPersonas = new Dictionary<string, string>();
        public string SaveDataFolderPath = "Mods/LivingRimTurbo/Servers/data";
        public string LogFileFolderPath = "Mods/LivingRimTurbo/Servers/logs";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref pauseOnDialogOpen, "pauseOnDialogOpen", true);
            Scribe_Collections.Look(ref defaultPersonas, "defaultPersonas", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref SaveDataFolderPath, "SaveDataFolderPath", SaveDataFolderPath);
            Scribe_Values.Look(ref LogFileFolderPath, "LogFileFolderPath", LogFileFolderPath);
            base.ExposeData();
        }

        public void RestoreDefaults()
        {
            pauseOnDialogOpen = true;
            defaultPersonas = new Dictionary<string, string>();
            SaveDataFolderPath = "Mods/LivingRimTurbo/Servers/data";
            LogFileFolderPath = "Mods/LivingRimTurbo/Servers/logs";
        }
    }
}
