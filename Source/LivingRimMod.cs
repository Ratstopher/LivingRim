using UnityEngine;
using Verse;

namespace LivingRim
{
    public class LivingRimMod : Mod
    {
        public static LivingRimSettings settings;

        public LivingRimMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<LivingRimSettings>();
        }

        public override string SettingsCategory()
        {
            return "LivingRim";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Dialog_ModSettings.Open();
        }
    }
}
