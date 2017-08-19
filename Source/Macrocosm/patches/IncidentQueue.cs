using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(IncidentQueue), "Add", new Type [] {typeof(IncidentDef), typeof(int), typeof(IncidentParms)})]
    public static class IncidentQueue_Add_Prefix
    {

        [HarmonyPrefix]
        private static void Add(IncidentDef def, ref int fireTick, IncidentParms parms)
        {
            if(def.Equals(IncidentDefOf.TraderCaravanArrival) && fireTick == (Find.TickManager.TicksGame + 120000))
            {       //caravan requested through comms console
                fireTick = Find.TickManager.TicksGame + 600;
            }
        }
    }
}
