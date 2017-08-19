using Harmony;
using Macrocosm.extensions;
using Macrocosm.rimworld;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.patches
{
    /*[HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
    static class PawnGenerator_GenerateGearFor_Postfix
    {
        [HarmonyPostfix]
        private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
        { 
            PawnSidearmsGenerator.TryGenerateSidearmsFor(pawn);
        } 
    }*/

    [HarmonyPatch(typeof(SettleUtility), "AddNewHome")]
    static class SettleUtility_AddNewHome_Postfix
    {
        [HarmonyPostfix]
        private static void AddNewHome(int tile, Faction faction)
        {
            if (faction.IsPlayer)
                Macrocosm.saveData.ScoutingManager.SetDirty();
        } 
    }

    [HarmonyPatch(typeof(WorldObjectSelectionUtility), "HiddenBehindTerrainNow")]
    static class WorldObjectSelectionUtility_HiddenBehindTerrainNow_Postfix
    {
        [HarmonyPostfix]
        private static void HiddenBehindTerrainNow(ref bool __result, WorldObject o)
        {
            if(o is ISpottable)
            {
                if (!(o as ISpottable).Spotted)
                    __result = true;
            }
        }
    }
}
