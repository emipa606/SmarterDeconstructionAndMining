using System.Collections.Generic;
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

        foreach (var researchDefName in new List<string>
                     { "RTR_OverheadMountainRemoval", "OverheadMountainRemoval", "ThickStoneRoofRemoval" })
        {
            var foundResearch = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(researchDefName);
            if (foundResearch == null)
            {
                continue;
            }

            defaultResearchProjectDef = foundResearch;
            break;
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