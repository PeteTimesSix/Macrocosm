using Harmony;
using Macrocosm.extensions;
using Macrocosm.macrocosm;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(Pawn), "ExitMap")]
    public static class Pawn_ExitMap_Prefix
    {
        //I am resorting to global flags with an alarming frequency
        private static Pawn globalTempKidnapee;
        private static Faction globalTempKidnapFaction;

        [HarmonyPrefix]
        private static bool ExitMap(Pawn __instance, bool allowedToJoinOrCreateCaravan)
        {
            if (globalTempKidnapee != __instance)
            {
                globalTempKidnapee = null;
                globalTempKidnapFaction = null;
            }

            if (!(__instance.RaceProps.Humanlike) && __instance.Faction == null)
                return true;

            if (__instance.IsCaravanMember())
                return true;

            if (allowedToJoinOrCreateCaravan && CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(__instance))
            {
                return true;
            }



            Map currentMap = __instance.Map;

            if (__instance.IsWorldPawn())
            {
                Log.Warning("Called ExitMap() on world pawn " + __instance);
                return false;
            }
            else
            {
                Log.Message("allowed to etc;" + allowedToJoinOrCreateCaravan);
                Log.Message("exit uti;" + CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(__instance));
            }

            Lord lord = __instance.GetLord();
            if (lord != null)
            {
                lord.Notify_PawnLost(__instance, PawnLostCondition.ExitedMap);
            }
            Pawn kidnapVictim = null;
            if (__instance.carryTracker != null && __instance.carryTracker.CarriedThing != null)
            {
                Pawn carriedPawn = __instance.carryTracker.CarriedThing as Pawn;
                if (carriedPawn != null)
                {
                    if (__instance.Faction != null && __instance.Faction != carriedPawn.Faction)
                    {
                        kidnapVictim = carriedPawn;
                        __instance.carryTracker.innerContainer.Remove(carriedPawn);
                    }
                    else
                    {
                        __instance.carryTracker.innerContainer.Remove(carriedPawn);
                        carriedPawn.ExitMap(false);
                    }
                }
                else
                {
                    //TODO: recover stolen items
                    __instance.carryTracker.CarriedThing.Destroy(DestroyMode.Vanish);
                }
                __instance.carryTracker.innerContainer.Clear();
            }
            bool flag = !__instance.IsCaravanMember() && !PawnUtility.IsTravelingInTransportPodWorldObject(__instance);
            bool isReleasedPrisoner = false;
            if (flag && __instance.HostFaction != null && __instance.guest != null && (__instance.guest.released || !__instance.IsPrisoner) && !__instance.InMentalState && __instance.health.hediffSet.BleedRateTotal < 0.001f && __instance.Faction.def.appreciative && !__instance.Faction.def.hidden)
            {
                //Log.Message(String.Format("pawn {0} is a released prisoner", __instance.Name));
                isReleasedPrisoner = true;
            }

            if (__instance.ownership != null)
            {
                __instance.ownership.UnclaimAll();
            }
            if (__instance.guest != null && flag)
            {
                __instance.guest.SetGuestStatus(null, false);
            }
            if (__instance.Spawned)
            {
                __instance.DeSpawn();
            }
            
            __instance.inventory.UnloadEverything = false;
            __instance.ClearMind(false);
            __instance.ClearReservations(true);
            
            JoinOrCreateCaravan(__instance, currentMap, isReleasedPrisoner);          

            if (kidnapVictim != null)
            {
                globalTempKidnapee = kidnapVictim;
                globalTempKidnapFaction = __instance.Faction;
                kidnapVictim.ExitMap(false);
            }
            
            return false;
        }

        private static void JoinOrCreateCaravan(Pawn __instance, Map currentMap, bool isReleasedPrisoner)
        {
            Log.Message("doing this");
            Faction targetFaction = __instance.Faction;
            if (globalTempKidnapee == __instance)
                targetFaction = globalTempKidnapFaction;

            IEnumerable<Caravan> caravans = Find.WorldObjects.Caravans.Where(caravan => caravan.Tile == currentMap.Tile && caravan is Caravan_Macrocosm && caravan.Faction == targetFaction);

            StoredCaravan storedCaravan = Macrocosm.saveData.findMatchingStoredCaravan(__instance);
            
            Caravan_Macrocosm joinableCaravan;            

            if (storedCaravan != null)
            {
                //Log.Message("pawn " + __instance.Name + " recreating caravan from stored");
                joinableCaravan = Caravan_Macrocosm.spawnCaravan(__instance, Find.WorldObjects.SettlementAt(currentMap.Tile), storedCaravan);
                joinableCaravan.GetSpotted(false);
            }
            else
            {
                joinableCaravan = (caravans.Where(x => (x as Caravan_Macrocosm).caravanData.caravanPawns.Contains(__instance)).FirstOrDefault()) as Caravan_Macrocosm;
                
                if (joinableCaravan == null)
                {
                    //Log.Message("pawn " + __instance.Name + " joining existing caravan with matching existingPawn ref");
                    joinableCaravan = caravans.FirstOrDefault() as Caravan_Macrocosm;
                }

                if (joinableCaravan == null)
                {
                    //Log.Message("pawn " + __instance.Name + " creating new caravan");
                    joinableCaravan = Caravan_Macrocosm.spawnCaravan(__instance, Find.WorldObjects.SettlementAt(currentMap.Tile), null);
                    joinableCaravan.GetSpotted(false);
                }
                else
                {
                    //Log.Message("pawn " + __instance.Name + " joining existing caravan");
                    joinableCaravan.join(__instance, true);
                }
            }

            if (isReleasedPrisoner)
            {
                joinableCaravan.caravanData.releasedPrisoners.Add(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Prefix
    {
        [HarmonyPrefix]
        private static void Kill(Pawn __instance)
        {
            Macrocosm.saveData.cleanupFor(__instance);
        }
    }
}
