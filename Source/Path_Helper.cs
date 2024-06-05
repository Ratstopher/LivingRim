using System.IO;
using Verse;

namespace LivingRim
{
    public static class Path_Helper
    {
        public static string GetLogFileFolderPath()
        {
            string modFolder = Path.Combine(GetModDirectoryPath(), "LivingRimTurbo", "Servers", "logs");
            Directory.CreateDirectory(modFolder);
            Log.Message($"LogFileFolderPath: {modFolder}");
            return modFolder;
        }
        public static string GetModDirectoryPath()
        {
            DirectoryInfo modDirectory = new DirectoryInfo(GenFilePaths.ModsFolderPath);
            string path = modDirectory.FullName;
            Log.Message($"ModDirectoryPath: {path}");
            return path;
        }

        public static string GetSaveDataFolderPath()
        {
            string modFolder = Path.Combine(GetModDirectoryPath(), "LivingRimTurbo", "Servers", "data");
            Directory.CreateDirectory(modFolder);
            Log.Message($"SaveDataFolderPath: {modFolder}");
            return modFolder;
        }

        public static string GetSaveDataFilePath(string fileName)
        {
            string modFolder = GetSaveDataFolderPath();
            string filePath = Path.Combine(modFolder, $"{fileName}.xml");
            Log.Message($"SaveDataFilePath: {filePath}");
            return filePath;
        }
    }
}
