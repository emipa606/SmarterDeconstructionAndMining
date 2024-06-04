using Verse;

namespace SmartDeconstruct;

[StaticConstructorOnStartup]
public static class Main
{
    static Main()
    {
        SmartDeconstructMod.Settings.SetDefaults();
    }
}