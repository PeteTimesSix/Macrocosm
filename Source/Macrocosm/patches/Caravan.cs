using Harmony;
using Macrocosm.defs;
using Macrocosm.macrocosm;
using Macrocosm.macrocosm.ui;
using Macrocosm.rimworld;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(Caravan), "Tick")]
    public static class Caravan_Tick_Postfix
    {

        [HarmonyPostfix]
        private static void Tick(Caravan __instance)
        {
            //Toxic fallout
            if (Find.TickManager.TicksGame % 3451 == 0)
            {
                if (__instance.pather.Moving && !__instance.Resting && !__instance.ImmobilizedByMass && Macrocosm.saveData.InToxicCloud(__instance.Tile))
                {
                    foreach(Pawn pawn in __instance.pawns)
                    { 
                        if (pawn.def.race.IsFlesh)
                        {
                            float num = 0.028758334f;
                            num *= pawn.GetStatValue(StatDefOf.ToxicSensitivity, true);
                            if (num != 0f)
                            {
                                float num2 = Mathf.Lerp(0.85f, 1.15f, Rand.ValueSeeded(pawn.thingIDNumber ^ 74374237));
                                num *= num2;
                                HealthUtility.AdjustSeverity(pawn, HediffDefOf.ToxicBuildup, num);
                            }
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Caravan), "GetGizmos")]
    static class Caravan_GetGizmos_Postfix
    {

        [HarmonyPostfix]
        private static void GetGizmos(Caravan __instance, ref IEnumerable<Gizmo> __result)
        {
            if (!__instance.Faction.IsPlayer)
                return;

            List<Gizmo> list = new List<Gizmo>();
            list.AddRange(__result);

            ScoutingOutpost outpost = Find.WorldObjects.WorldObjectAt<ScoutingOutpost>(__instance.Tile);

            if (outpost != null)
            {
                list.Add(new Command_Action
                {
                    defaultLabel = "ManageOutpost".Translate(),
                    icon = Macrocosm_Textures.ManageOutpostCommand,
                    action = delegate
                    {
                        Find.WindowStack.Add(new Dialog_ScoutingOutpostManager(outpost, __instance));
                    }
                }); 
            }
            else
            {
                if (Find.WorldObjects.AllWorldObjects.Where(o => o != __instance && o.Tile == __instance.Tile).Count() == 0)
                {
                    list.Add(new Command_Action
                    {
                        defaultLabel = "BuildOutpost".Translate(),
                        icon = Macrocosm_Textures.FoundNewOutpostCommand,
                        action = delegate
                        {
                            ScoutingOutpost newOutpost = (ScoutingOutpost)WorldObjectMaker.MakeWorldObject(Macrocosm_WorldObjectDefOf.ScoutingOutpost);
                            newOutpost.Tile = __instance.Tile;
                            newOutpost.SetFaction(__instance.Faction);
                            Find.WorldObjects.Add(newOutpost);
                            //Log.Message(rep);
                        }
                    });
                }
            }

            __result = list;
        }
    }
}
