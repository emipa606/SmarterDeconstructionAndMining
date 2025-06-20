using Verse;
using Verse.AI;

namespace SmartDeconstruct;

public class JobInfo(Job job)
{
    public readonly JobDef Def = job.def;

    public readonly bool PlayerForced = job.playerForced;

    public LocalTargetInfo TargetA = job.targetA;

    public LocalTargetInfo TargetB = job.targetB;

    public LocalTargetInfo TargetC = job.targetC;
}