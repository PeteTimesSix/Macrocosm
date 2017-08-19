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
using static Macrocosm.macrocosm.SettlementSelector;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "TryExecute")]
    static class IncidentWorker_VisitorGroup_TryExecute_Prefix
    {
        [HarmonyPrefix]
        private static bool TryExecute(IncidentWorker_VisitorGroup __instance, ref IncidentParms parms, ref bool __result)
        {
            //return true;
            Map map = (Map)parms.target;

            object[] parmsInArray = new object[1];
            parmsInArray[0] = parms;

            if (!Traverse.Create(__instance).Method("TryResolveParms", parmsInArray).GetValue<bool>(parmsInArray))
            {
                __result = false;
                return false;
            }
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                __result = false;
                return false;
            }
            List<Pawn> list = RimworldUtilities.createPawns(parms, PawnGroupKindDefOf.Normal);
            if (list.Count == 0)
            {
                __result = false;
                return false;
            }

            DoDetouredVisit(ref __instance, parms, parmsInArray, ref __result);
            return false;
        }

        private static void DoDetouredVisit(ref IncidentWorker_VisitorGroup __instance, IncidentParms parms, object[] parmsInArray, ref bool __result)
        {
            Map map = (Map)parms.target;
            IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
            List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
            if (pawns.Count == 0)
            {
                Log.Error("Got no pawns spawning visit caravan from parms " + parms);
                __result = false;
                return;
            }

            Settlement nearest = SettlementSelector.selectNearestBase(map.Tile, parms.faction, SettlementSelector.SelectionCriteria.Self, 0, 1).First();

            Caravan_Macrocosm caravan = RimworldUtilities.MakeCaravan(pawns, parms.faction, nearest, true, Caravan_Macrocosm.CaravanKind.Visitor);
            NPCaravanArrivalAction_VisitColony action = new NPCaravanArrivalAction_VisitColony(Find.WorldObjects.SettlementAt(map.Tile));
            caravan.caravanData.caravanKind = Caravan_Macrocosm.CaravanKind.Passerby;

            caravan.pather.StartPath(map.Tile, action, true);

            __result = true;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_TravelerGroup), "TryExecute")]
    static class IncidentWorker_TravelerGroup_TryExecute_Prefix
    {
        [HarmonyPrefix]
        private static bool TryExecute(IncidentWorker_TravelerGroup __instance, ref IncidentParms parms, ref bool __result)
        {
            //return true;
            Map map = (Map)parms.target;

            object[] parmsInArray = new object[1];
            parmsInArray[0] = parms;

            if (!Traverse.Create(__instance).Method("TryResolveParms", parmsInArray).GetValue<bool>(parmsInArray))
            {
                __result = false;
                return false;
            }
            if (parms.faction.HostileTo(Faction.OfPlayer))
            {
                __result = false;
                return false;
            }
            List<Pawn> list = RimworldUtilities.createPawns(parms, PawnGroupKindDefOf.Normal);
            if (list.Count == 0)
            {
                __result = false;
                return false;
            }

            DoDetouredVisit(ref __instance, parms, parmsInArray, ref __result);
            return false;
        }

        private static void DoDetouredVisit(ref IncidentWorker_TravelerGroup __instance, IncidentParms parms, object[] parmsInArray, ref bool __result)
        {
            Map map = (Map)parms.target;
            IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
            List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Normal, defaultPawnGroupMakerParms, true).ToList<Pawn>();
            if (pawns.Count == 0)
            {
                Log.Error("Got no pawns spawning travel-by caravan from parms " + parms);
                __result = false;
                return;
            }

            Settlement nearest = SettlementSelector.selectNearestBase(map.Tile, parms.faction, SettlementSelector.SelectionCriteria.Self, 0, 1).First();

            Caravan_Macrocosm caravan = RimworldUtilities.MakeCaravan(pawns, parms.faction, nearest, true, Caravan_Macrocosm.CaravanKind.Passerby);
            
            NPCaravanArrivalAction_TravelByColony action = new NPCaravanArrivalAction_TravelByColony(Find.WorldObjects.SettlementAt(map.Tile));
            caravan.caravanData.caravanKind = Caravan_Macrocosm.CaravanKind.Visitor;

            caravan.pather.StartPath(map.Tile, action, true);

            __result = true;
        }
    }
}
