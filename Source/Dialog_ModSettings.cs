using UnityEngine;
using Verse;

namespace LivingRim
{
    public class Dialog_ModSettings : Window
    {
        private Vector2 scrollPosition = Vector2.zero;

        public Dialog_ModSettings()
        {
            this.doCloseX = true;
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 500f);

        public override void DoWindowContents(Rect inRect)
        {
            float lineHeight = 30f;
            float spacing = 10f;
            float y = 0f;

            Widgets.BeginScrollView(inRect, ref scrollPosition, new Rect(0f, 0f, inRect.width - 16f, inRect.height));

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(new Rect(0f, 0f, inRect.width - 16f, inRect.height - 16f));

            // Settings UI
            listingStandard.Label("LivingRim Settings");
            listingStandard.Gap();

            listingStandard.CheckboxLabeled("Pause on Dialog Open", ref LivingRimMod.settings.pauseOnDialogOpen, "Should the game pause when the dialog window is opened?");
            listingStandard.Gap();

            listingStandard.Label("Save Data Folder Path:");
            LivingRimMod.settings.SaveDataFolderPath = listingStandard.TextEntry(LivingRimMod.settings.SaveDataFolderPath);
            listingStandard.Gap();

            listingStandard.Label("Log File Folder Path:");
            LivingRimMod.settings.LogFileFolderPath = listingStandard.TextEntry(LivingRimMod.settings.LogFileFolderPath);
            listingStandard.Gap();

            if (listingStandard.ButtonText("Reset Default Personas"))
            {
                LivingRimMod.settings.defaultPersonas.Clear();
            }

            listingStandard.Gap();
            if (listingStandard.ButtonText("Restore Defaults"))
            {
                LivingRimMod.settings.RestoreDefaults();
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        public override void PreClose()
        {
            base.PreClose();
            LivingRimMod.settings.Write();
        }

        public static void Open()
        {
            Find.WindowStack.Add(new Dialog_ModSettings());
        }
    }
}
