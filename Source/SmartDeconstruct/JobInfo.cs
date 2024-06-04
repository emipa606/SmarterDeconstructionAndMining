using Verse;
using Verse.AI;

namespace SmartDeconstruct;

public class JobInfo(Job job)
{
    public readonly JobDef def = job.def;

    public readonly bool playerForced = job.playerForced;

    public LocalTargetInfo targetA = job.targetA;

    public LocalTargetInfo targetB = job.targetB;

    public LocalTargetInfo targetC = job.targetC;
}