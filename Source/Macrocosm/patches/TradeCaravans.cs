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
    [HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "TryExecute")]
    static class IncidentWorker_TraderCaravanArrival_TryExecute_Prefix
    {
        [HarmonyPrefix]
        private static bool TryExecute(IncidentWorker_TraderCaravanArrival __instance, ref IncidentParms parms, ref bool __result)
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
            List<Pawn> list = RimworldUtilities.createPawns(parms, PawnGroupKindDefOf.Trader);
            if (list.Count == 0)
            {
                __result = false;
                return false;
            }

            DoDetouredCaravan(ref __instance, parms, parmsInArray, ref __result);
            return false;
        }

        private static void DoDetouredCaravan(ref IncidentWorker_TraderCaravanArrival __instance, IncidentParms parms, object[] parmsInArray, ref bool __result)
        {
            Map map = (Map)parms.target;
            IncidentParmsUtility.AdjustPointsForGroupArrivalParams(parms);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
            List<Pawn> pawns = PawnGroupMakerUtility.GeneratePawns(PawnGroupKindDefOf.Trader, defaultPawnGroupMakerParms, true).ToList<Pawn>();
            if (pawns.Count == 0)
            {
                Log.Error("Got no pawns spawning trade caravan from parms " + parms);
                __result = false;
                return;
            }

            TraderKindDef traderKindDef = null;
            for (int j = 0; j < pawns.Count; j++)
            {
                Pawn pawn = pawns[j];
                if (pawn.TraderKind != null)
                {
                    traderKindDef = pawn.TraderKind;
                    break;
                }
            }
            if (traderKindDef == null)
            {
                Log.Error("Did not generate trader for trader caravan from parms " + parms);
                __result = false;
                return;
            }

            Settlement nearest = SettlementSelector.selectNearestBase(map.Tile, parms.faction, SettlementSelector.SelectionCriteria.Self, 0, 1).First();

            Caravan_Macrocosm caravan = RimworldUtilities.MakeCaravan(pawns, parms.faction, nearest, true, Caravan_Macrocosm.CaravanKind.Trader);
            NPCaravanArrivalAction_TradeWithColony action = new NPCaravanArrivalAction_TradeWithColony(Find.WorldObjects.SettlementAt(map.Tile));
            caravan.caravanData.caravanKind = Caravan_Macrocosm.CaravanKind.RaidFriendly;

            caravan.pather.StartPath(map.Tile, action, true);

            __result = true;
        }
    }
}
