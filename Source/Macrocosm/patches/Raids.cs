using Harmony;
using Macrocosm.extensions;
using Macrocosm.macrocosm;
using Macrocosm.rimworld.CaravanArrival;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.patches
{

    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryExecute")]
    static class IncidentWorker_Raid_TryExecute_Prefix
    {
        [HarmonyPrefix]
        private static bool TryExecute(IncidentWorker_Raid __instance, ref IncidentParms parms, ref bool __result)
        {
            Map map = (Map)parms.target;

            object[] parmsInArray = new object[1];
            parmsInArray[0] = parms;

            Traverse.Create(__instance).Method("ResolveRaidPoints", parmsInArray).GetValue(parmsInArray);
            //__instance.ResolveRaidPoints(parms);
            if (!Traverse.Create(__instance).Method("TryResolveRaidFaction", parmsInArray).GetValue<bool>(parmsInArray))
            {
                __result = false;
                return false;
            }

            Traverse.Create(__instance).Method("ResolveRaidStrategy", parmsInArray).GetValue(parmsInArray);
            //__instance.ResolveRaidStrategy(parms);
            Traverse.Create(__instance).Method("ResolveRaidArriveMode", parmsInArray).GetValue(parmsInArray);
            //__instance.ResolveRaidArriveMode(parms);

            //Log.Message("Raid strategy is "+parms.raidArrivalMode);

            if (parms.raidArrivalMode != PawnsArriveMode.EdgeWalkIn)
            {
                //Log.Warning("NOT detouring raid");
                return true;
            }
            else
            {
                //Log.Warning("detouring raid");
                DoDetouredRaid(ref __instance, parms, parmsInArray, ref __result);
                return false;
            }
        }

        private static void DoDetouredRaid(ref IncidentWorker_Raid __instance, IncidentParms parms, object [] parmsInArray, ref bool __result)
        {
            Map map = (Map)parms.target;
            IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
            List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
            if (pawns.Count == 0)
            {
                Log.Error("Got no pawns spawning raid from parms " + parms);
                __result = false;
                return;
            }

            Settlement nearest = SettlementSelector.selectNearestBase(map.Tile, parms.faction, SettlementSelector.SelectionCriteria.Self, 0, 2).First();

            Caravan_Macrocosm caravan = RimworldUtilities.MakeCaravan(pawns, parms.faction, nearest, true, Caravan_Macrocosm.CaravanKind.RaidEnemy);
            //Log.Message("Created raid caravan at " + Find.WorldObjects.FactionBaseAt(startingTile).Name);

            caravan.caravanData.storedRaidStrategy = parms.raidStrategy;
            caravan.caravanData.storedRaidPoints = parms.points;

            NPCaravanArrivalAction_RaidColony action = new NPCaravanArrivalAction_RaidColony(Find.WorldObjects.SettlementAt(map.Tile));

            if (__instance is IncidentWorker_RaidEnemy)
                caravan.caravanData.caravanKind = Caravan_Macrocosm.CaravanKind.RaidEnemy;
            if (__instance is IncidentWorker_RaidFriendly)
                caravan.caravanData.caravanKind = Caravan_Macrocosm.CaravanKind.RaidFriendly;

            caravan.pather.StartPath(map.Tile, action, true);
            //Log.Message("Sent it to " + Find.WorldObjects.FactionBaseAt(map.Tile).Name);

            __result = true;
        }

        /*
        private static void OnArrival()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
            foreach (Pawn current2 in list)
            {
                string str = (current2.equipment == null || current2.equipment.Primary == null) ? "unarmed" : current2.equipment.Primary.LabelCap;
                stringBuilder.AppendLine(current2.KindLabel + " - " + str);
            }
            string letterLabel = this.GetLetterLabel(parms);
            string letterText = this.GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer(list, ref letterLabel, ref letterText, this.GetRelatedPawnsInfoLetterText(parms), true);
            Find.LetterStack.ReceiveLetter(letterLabel, letterText, this.GetLetterDef(), target, stringBuilder.ToString());
            if (this.GetLetterDef() == LetterDefOf.BadUrgent)
            {
                TaleRecorder.RecordTale(TaleDefOf.RaidArrived, new object[0]);
            }
            Lord lord = LordMaker.MakeNewLord(parms.faction, parms.raidStrategy.Worker.MakeLordJob(parms, map), map, list);
            AvoidGridMaker.RegenerateAvoidGridsFor(parms.faction, map);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Pawn pawn = list[i];
                    if (pawn.apparel.WornApparel.Any((Apparel ap) => ap is ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            if (DebugViewSettings.drawStealDebug && parms.faction.HostileTo(Faction.OfPlayer))
            {
                Log.Message(string.Concat(new object[]
                {
                    "Market value threshold to start stealing: ",
                    StealAIUtility.StartStealingMarketValueThreshold(lord),
                    " (colony wealth = ",
                    map.wealthWatcher.WealthTotal,
                    ")"
                }));
            }
            return true;
        }*/
    }
}
