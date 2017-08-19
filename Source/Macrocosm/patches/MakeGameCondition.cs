using Harmony;
using Macrocosm.macrocosm.runningEvents;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(IncidentWorker_MakeGameCondition), "TryExecute")]
    static class IncidentWorker_MakeGameCondition_TryExecute_Prefix
    {
        [HarmonyPrefix]
        private static bool TryExecute(IncidentWorker_MakeGameCondition __instance, ref IncidentParms parms, ref bool __result)
        {
            if( __instance.def.gameCondition == GameConditionDefOf.ToxicFallout ||
                __instance.def.gameCondition == GameConditionDefOf.VolcanicWinter
                )
            {
                GameConditionManager gameConditionManager = parms.target.GameConditionManager;
                int duration = Mathf.RoundToInt(__instance.def.durationDays.RandomInRange * 60000f);

                BaseRunningEvent runningEvent = null;

                if (__instance.def.gameCondition == GameConditionDefOf.ToxicFallout)
                {
                    runningEvent = new RunningEvent_ToxicFallout(duration, parms.target.GameConditionManager.map.Tile);
                }
                else if (__instance.def.gameCondition == GameConditionDefOf.VolcanicWinter)
                {
                    runningEvent = new RunningEvent_VolcanicWinter(duration, parms.target.GameConditionManager.map.Tile);
                }


                Macrocosm.saveData.RunningEventManager.RegisterNewEvent(runningEvent);

                __result = true;
                return false;
            }

            return true;
        }
    }
}
