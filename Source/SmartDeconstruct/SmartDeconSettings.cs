using Verse;

namespace SmartDeconstruct;

public class SmartDeconSettings : ModSettings
{
    public bool Animals;

    public bool Buildings = true;

    public bool CancelThick = true;
    [Unsaved] private ResearchProjectDef defaultResearchProjectDef;

    private bool defaultsSet;

    [Unsaved] private bool defaultThick;

    private bool explicitNull;

    public bool Mining = true;

    public bool NonColonists;

    private string research;

    public bool Thick;

    public ResearchProjectDef ThickResearch
    {
        get => research == null ? null : DefDatabase<ResearchProjectDef>.GetNamed(research);
        set
        {
            if (value == null)
            {
                explicitNull = true;
            }

            research = value?.defName;
        }
    }

    public bool CanThick => Thick && (ThickResearch == null || ThickResearch.IsFinished);

    public void SetDefaults()
    {
        if (defaultsSet)
        {
            return;
        }

        if (LoadedModManager.RunningModsListForReading.Any(pack => pack.Name.Contains("Raise The Roof")))
        {
            defaultThick = true;
            defaultResearchProjectDef = ResearchProjectDef.Named("OverheadMountainRemoval");
            Log.Message("[SmarterDeconstructionAndMining]: Added compatibility with Raise the Roof");
        }

        if (LoadedModManager.RunningModsListForReading.Any(pack => pack.Name.Contains("Expanded Roofing")))
        {
            defaultThick = true;
            defaultResearchProjectDef = ResearchProjectDef.Named("ThickStoneRoofRemoval");
            Log.Message("[SmarterDeconstructionAndMining]: Added compatibility with Expanded Roofing");
        }

        Thick = defaultThick;
        defaultsSet = true;
    }

    public override void ExposeData()
    {
        Scribe_Values.Look(ref Buildings, "Buildings", true);
        Scribe_Values.Look(ref CancelThick, "CancelThick", true);
        Scribe_Values.Look(ref explicitNull, "explicitNull");
        Scribe_Values.Look(ref Mining, "Mining", true);
        Scribe_Values.Look(ref Thick, "Thick", defaultThick);
        Scribe_Values.Look(ref NonColonists, "NonColonists");
        Scribe_Values.Look(ref Animals, "Animals");
        Scribe_Values.Look(ref research, "ThickResearch");
        if (ThickResearch == null && !explicitNull && defaultResearchProjectDef != null)
        {
            ThickResearch = defaultResearchProjectDef;
        }
    }
}