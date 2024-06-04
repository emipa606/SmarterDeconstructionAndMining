using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Mlie;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SmartDeconstruct;

public class SmartDeconstructMod : Mod
{
    private static readonly List<IntVec3> tempBuildRoofCells = [];

    private static readonly List<IntVec3> tempRoofCells = [];

    public static Harmony Harm;

    public static SmartDeconSettings Settings;
    private static string currentVersion;

    private readonly List<ResearchProjectDef> researches = [];

    private readonly QuickSearchWidget search = new QuickSearchWidget();

    private Vector2 scrollPos;

    public SmartDeconstructMod(ModContentPack content)
        : base(content)
    {
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Harm = new Harmony("legodude17.smartdecon");
        Settings = GetSettings<SmartDeconSettings>();
        Harm.Patch(AccessTools.Method(typeof(JobDriver_RemoveBuilding), "MakeNewToils"), null,
            new HarmonyMethod(typeof(SmartDeconstructMod), nameof(CheckForRoofsBeforeJob)));
        Harm.Patch(AccessTools.Method(typeof(JobDriver_Mine), "MakeNewToils"), null,
            new HarmonyMethod(typeof(SmartDeconstructMod), nameof(CheckForRoofsBeforeJob)));
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            researches.AddRange(DefDatabase<ResearchProjectDef>.AllDefs);
        });
    }

    public override string SettingsCategory()
    {
        return "SmartDeconstruct".Translate();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(inRect);
        listing_Standard.CheckboxLabeled("SmartDeconstruct.Buildings.Label".Translate(), ref Settings.Buildings,
            "SmartDeconstruct.Buildings.Tooltip".Translate());
        listing_Standard.CheckboxLabeled("SmartDeconstruct.Mining.Label".Translate(), ref Settings.Mining,
            "SmartDeconstruct.Mining.Tooltip".Translate());
        listing_Standard.CheckboxLabeled("SmartDeconstruct.CancelThick.Label".Translate(), ref Settings.CancelThick,
            "SmartDeconstruct.CancelThick.Tooltip".Translate());
        listing_Standard.CheckboxLabeled("SmartDeconstruct.NonColonists.Label".Translate(), ref Settings.NonColonists,
            "SmartDeconstruct.NonColonists.Tooltip".Translate());
        listing_Standard.CheckboxLabeled("SmartDeconstruct.Animals.Label".Translate(), ref Settings.Animals,
            "SmartDeconstruct.Animals.Tooltip".Translate());
        listing_Standard.CheckboxLabeled("SmartDeconstruct.Thick.Label".Translate(), ref Settings.Thick,
            "SmartDeconstruct.Thick.Tooltip".Translate());
        var rect = listing_Standard.GetRect(inRect.height - listing_Standard.CurHeight);
        var rect2 = rect.LeftHalf().TopPartPixels(Text.LineHeight + 6f).ContractedBy(0f, 3f);
        if (currentVersion != null)
        {
            var rect3 = rect.LeftHalf().TopPartPixels((Text.LineHeight * 2) + 6f).BottomHalf().ContractedBy(0f, 3f);
            GUI.contentColor = Color.gray;
            Widgets.Label(rect3, "SmartDeconstruct.CurrentModVersion.Label".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        Widgets.Label(rect2, "SmartDeconstruct.ThickResearch.Label".Translate());
        Widgets.DrawHighlightIfMouseover(rect2);
        TooltipHandler.TipRegionByKey(rect2, "SmartDeconstruct.ThickResearch.Tooltip");
        rect = rect.RightHalf();
        search.OnGUI(rect.TopPartPixels(23f), delegate
        {
            researches.Clear();
            researches.AddRange(DefDatabase<ResearchProjectDef>.AllDefs.Where(def => search.filter.Matches(def.label)));
        });
        rect.yMin += 25f;
        var viewRect = new Rect(0f, 0f, rect.width - 20f, researches.Count * 32f);
        var curY = 0f;
        Widgets.BeginScrollView(rect, ref scrollPos, viewRect);
        if (Settings.ThickResearch != null)
        {
            if (Widgets.ButtonText(Func(), Settings.ThickResearch.LabelCap))
            {
                Settings.ThickResearch = null;
            }

            GUI.color = Color.yellow;
            Widgets.DrawBox(Func(), 2);
            GUI.color = Color.white;
            curY += 32f;
        }

        foreach (var item in researches.Except(Settings.ThickResearch).OrderBy(def => def.label))
        {
            if (Widgets.ButtonText(Func(), item.LabelCap))
            {
                Settings.ThickResearch = item;
            }

            curY += 32f;
        }

        Widgets.EndScrollView();
        listing_Standard.End();
        return;

        Rect Func()
        {
            return new Rect(1f, curY, viewRect.width - 2f, 30f);
        }
    }

    public static bool ShouldApply(Pawn pawn, bool isDecon, bool isMine)
    {
        if (isDecon && !Settings.Buildings)
        {
            return false;
        }

        if (isMine && !Settings.Mining)
        {
            return false;
        }

        if (!pawn.IsColonist && !Settings.NonColonists)
        {
            return false;
        }

        return !pawn.AnimalOrWildMan() || Settings.Animals;
    }

    public static IEnumerable<Toil> CheckForRoofsBeforeJob(IEnumerable<Toil> toils, JobDriver __instance)
    {
        var isDecon = __instance is JobDriver_RemoveBuilding;
        var isMine = __instance is JobDriver_Mine;
        if (ShouldApply(__instance.pawn, isDecon, isMine))
        {
            yield return new Toil
            {
                initAction = delegate
                {
                    var pawn = __instance.pawn;
                    var map2 = pawn.Map;
                    var jobInfo = new JobInfo(__instance.job);
                    var thing2 = jobInfo.targetA.Thing;
                    var roofs = new List<IntVec3>();
                    var supporters = new List<Building>();
                    var designation = isMine ? DesignationDefOf.Mine : isDecon ? DesignationDefOf.Deconstruct : null;
                    if (!thing2.def.holdsRoof)
                    {
                        return;
                    }

                    map2.floodFiller.FloodFill(thing2.Position,
                        x => x.InHorDistOf(thing2.Position, 13.8f) && x.Roofed(map2) ||
                             thing2.OccupiedRect().Contains(x), delegate(IntVec3 x)
                        {
                            if (x.InHorDistOf(thing2.Position, 6.9f) && x.Roofed(map2))
                            {
                                roofs.Add(x);
                            }

                            for (var j = 0; j < 5; j++)
                            {
                                var c3 = x + GenAdj.CardinalDirectionsAndInside[j];
                                if (!c3.InBounds(map2))
                                {
                                    continue;
                                }

                                var edifice2 = c3.GetEdifice(map2);
                                if (edifice2 == null || !edifice2.def.holdsRoof || edifice2 == thing2 ||
                                    GetDesignation(edifice2, designation, map2) != null ||
                                    supporters.Contains(edifice2))
                                {
                                    continue;
                                }

                                supporters.Add(edifice2);
                                break;
                            }
                        });
                    foreach (var building in supporters)
                    {
                        map2.floodFiller.FloodFill(building.Position,
                            x => x.InHorDistOf(building.Position, 6.9f) &&
                                x.InHorDistOf(building.Position, 13.8f) && x.Roofed(map2) || x == building.Position,
                            delegate(IntVec3 x)
                            {
                                if (roofs.Contains(x))
                                {
                                    roofs.Remove(x);
                                }
                            });
                    }

                    if (!roofs.Any())
                    {
                        return;
                    }

                    if (!Settings.CanThick && Settings.CancelThick && !jobInfo.playerForced &&
                        roofs.Any(roof => roof.GetRoof(map2).isThickRoof))
                    {
                        if (PawnUtility.ShouldSendNotificationAbout(pawn))
                        {
                            Messages.Message(
                                "SmartDeconstruct.StoppedByThickRoof".Translate(pawn, thing2,
                                    ("Designator" + designation?.defName).Translate().ToLower()),
                                new LookTargets(thing2, pawn), MessageTypeDefOf.NegativeEvent);
                        }

                        if (pawn.Faction is not { IsPlayer: true })
                        {
                            return;
                        }

                        var designation2 = GetDesignation(thing2, designation, map2);
                        if (designation2 != null)
                        {
                            map2.designationManager.RemoveDesignation(designation2);
                        }

                        pawn.jobs?.EndCurrentJob(JobCondition.Ongoing | JobCondition.Succeeded);
                    }
                    else
                    {
                        roofs.RemoveAll(roof =>
                            roof.GetRoof(map2).isThickRoof && !Settings.CanThick ||
                            !pawn.CanReach(roof, PathEndMode.ClosestTouch, Danger.Some));
                        if (!roofs.Any())
                        {
                            return;
                        }

                        roofs.SortByDescending(c => c.DistanceTo(thing2.Position));
                        var list2 = (from roof in roofs
                            select JobMaker.MakeJob(JobDefOf.RemoveRoof, roof, roof)
                            into job1
                            where job1.TryMakePreToilReservations(pawn, false)
                            select job1).ToList();
                        if (!list2.Any())
                        {
                            return;
                        }

                        pawn.jobs.EndCurrentJob(JobCondition.Ongoing | JobCondition.Incompletable,
                            false);
                        foreach (var item in list2)
                        {
                            var cell = item.targetA.Cell;
                            if (!map2.areaManager.NoRoof[cell])
                            {
                                map2.areaManager.NoRoof[cell] = true;
                                tempRoofCells.Add(cell);
                            }

                            if (map2.areaManager.BuildRoof[cell])
                            {
                                map2.areaManager.BuildRoof[cell] = false;
                                if (!tempRoofCells.Contains(cell))
                                {
                                    tempRoofCells.Add(cell);
                                }

                                tempBuildRoofCells.Add(cell);
                            }

                            item.playerForced = jobInfo.playerForced;
                            pawn.jobs.TryTakeOrderedJob(item, JobTag.MiscWork, true);
                        }

                        var job2 = JobMaker.MakeJob(jobInfo.def, jobInfo.targetA, jobInfo.targetB,
                            jobInfo.targetC);
                        job2.playerForced = jobInfo.playerForced;
                        pawn.jobs.TryTakeOrderedJob(job2, JobTag.MiscWork, true);
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }

        var list = toils.ToList();
        foreach (var toil in list)
        {
            if (ShouldApply(__instance.pawn, isDecon, isMine) && toil == list[list.Count - 1])
            {
                toil.AddFinishAction(delegate
                {
                    var toRoof = new List<IntVec3>();
                    var toBuild = new List<IntVec3>();
                    var sawAdditional = false;
                    var thing = __instance.job.targetA.Thing;
                    var map = __instance.pawn.Map;
                    if (!thing.def.holdsRoof || !tempRoofCells.Any())
                    {
                        return;
                    }

                    map.floodFiller.FloodFill(thing.Position,
                        x => tempRoofCells.Contains(x) || thing.OccupiedRect().Contains(x), delegate(IntVec3 x)
                        {
                            if (!tempRoofCells.Contains(x))
                            {
                                return;
                            }

                            toRoof.Add(x);
                            if (!tempBuildRoofCells.Contains(x))
                            {
                                return;
                            }

                            toBuild.Add(x);
                            for (var i = 0; i < 5; i++)
                            {
                                var c2 = x + GenAdj.CardinalDirectionsAndInside[i];
                                if (!c2.InBounds(map))
                                {
                                    continue;
                                }

                                var edifice = c2.GetEdifice(map);
                                if (edifice == null || !edifice.def.holdsRoof || edifice == thing ||
                                    map.designationManager.DesignationOn(edifice,
                                        DesignationDefOf.Deconstruct) == null)
                                {
                                    continue;
                                }

                                sawAdditional = true;
                                break;
                            }
                        });
                    if (sawAdditional)
                    {
                        return;
                    }

                    foreach (var item2 in toRoof)
                    {
                        tempRoofCells.Remove(item2);
                        map.areaManager.NoRoof[item2] = false;
                    }

                    foreach (var item3 in toBuild)
                    {
                        tempBuildRoofCells.Remove(item3);
                        map.areaManager.BuildRoof[item3] = true;
                    }
                });
            }

            yield return toil;
        }
    }

    private static Designation GetDesignation(Thing b, DesignationDef def, Map map)
    {
        return def.targetType == TargetType.Cell ? map.designationManager.DesignationAt(b.Position, def) :
            def.targetType == TargetType.Thing ? map.designationManager.DesignationOn(b, def) : null;
    }
}