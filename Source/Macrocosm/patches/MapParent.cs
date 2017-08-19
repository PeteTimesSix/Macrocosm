using Harmony;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(MapParent), "PostMapGenerate")]
    public static class MapParent_PostMapGenerate_Postfix
    {

        [HarmonyPostfix]
        private static void PostMapGenerate(MapParent __instance)
        {
            //Log.Message("post map generate");
            if (Macrocosm.saveData != null && Macrocosm.saveData.RunningEventManager != null)
                Macrocosm.saveData.RunningEventManager.OnGenerateMapParent(__instance);
        }
    }

    [HarmonyPatch(typeof(Site), "PostMapGenerate")]
    public static class Site_PostMapGenerate_Postfix
    {

        [HarmonyPostfix]
        private static void PostMapGenerate(Site __instance)
        {
            //Log.Message("post map generate SITE");
            if (Macrocosm.saveData != null && Macrocosm.saveData.RunningEventManager != null)
                Macrocosm.saveData.RunningEventManager.OnGenerateMapParent(__instance);
        }
    }
}
