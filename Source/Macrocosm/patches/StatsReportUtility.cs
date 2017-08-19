using Harmony;
using Macrocosm.defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(StatsReportUtility), "StatsToDraw", new Type[] { typeof(Def), typeof(ThingDef)})]
    public static class StatsReportUtility_DrawStatsReport_Postfix
    {

        [HarmonyPostfix]
        private static void StatsToDraw(ref IEnumerable<StatDrawEntry> __result, Def def, ThingDef stuff)
        {
            List<StatDrawEntry> ret = new List<StatDrawEntry>();
            ret.AddRange(__result);

            OutpostBuildingDef eDef = def as OutpostBuildingDef;
            if (eDef != null)
            {
                foreach (StatModifier mod in eDef.statBases)
                {
                    ret.Add(new StatDrawEntry(mod.stat.category, mod.stat, mod.value, StatRequest.ForEmpty(), ToStringNumberSense.Undefined));
                }
            }

            __result = ret;
        }
    }
}
