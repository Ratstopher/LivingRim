using LivingRim;
using UnityEngine;
using Verse;

public class Dialog_Personas : Window
{
    private string persona;
    private string description;

    public Dialog_Personas()
    {
        this.doCloseX = true;
        this.forcePause = LivingRimMod.settings.pauseOnDialogOpen;
        this.absorbInputAroundWindow = true;

        LoadPersonaAndDescription();
    }

    public override void DoWindowContents(Rect inRect)
    {
        float inputHeight = 30f;
        float spacing = 10f;

        float personaLabelY = spacing;
        Widgets.Label(new Rect(10f, personaLabelY, 100f, inputHeight), "Persona:");
        persona = Widgets.TextField(new Rect(110f, personaLabelY, inRect.width - 120f, inputHeight), persona);

        float descriptionLabelY = personaLabelY + inputHeight + spacing;
        Widgets.Label(new Rect(10f, descriptionLabelY, 100f, inputHeight), "Description:");
        description = Widgets.TextField(new Rect(110f, descriptionLabelY, inRect.width - 120f, inputHeight), description);

        if (Widgets.ButtonText(new Rect(inRect.width - 100f, inRect.height - inputHeight - spacing, 80f, inputHeight), "Save"))
        {
            SavePersonaAndDescription();
        }
    }

    private void SavePersonaAndDescription()
    {
        if (persona == null) persona = "Stranger";
        if (description == null) description = "A marooned stranger.";

        Log.Message($"Saving Persona: {persona}, Description: {description}");
        InMemoryStorage.SavePersonaDescription(persona, description);
        GlobalSettings.Persona = persona;
        GlobalSettings.Description = description;
        GlobalSettings.SaveGlobalSettings();
    }

    private void LoadPersonaAndDescription()
    {
        InMemoryStorage.LoadAllFromDisk();
        var data = InMemoryStorage.GetPersonaDescription();
        persona = data?.Persona ?? "Stranger";
        description = data?.Description ?? "A marooned stranger.";
        GlobalSettings.Persona = persona;
        GlobalSettings.Description = description;
        Log.Message($"Loaded Persona: {persona}, Description: {description}");
    }
}
