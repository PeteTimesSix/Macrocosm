using Harmony;
using Macrocosm.defs;
using Macrocosm.extensions;
using Macrocosm.macrocosm;
using Macrocosm.macrocosm.runningEvents;
using Macrocosm.macrocosm.ui;
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

    /*
    [HarmonyPatch(typeof(GetOrGenerateMapUtility), "GetOrGenerateMap", new Type[] { typeof(int), typeof(IntVec3), typeof(WorldObjectDef) })]
    static class GameCondition_Prefix
    {

        [HarmonyPrefix]
        private static void GetOrGenerateMap(int tile, IntVec3 size, WorldObjectDef suggestedMapParentDef)
        {
            Map map = Current.Game.FindMap(tile);
            Log.Message("map is " + map);
            if (map == null)
            {
                MapParent mapParent = Find.WorldObjects.MapParentAt(tile);
                Log.Message("mapParent is " + mapParent);
                if (mapParent == null)
                {
                    Log.Message("suggestedMapParentDef is " + suggestedMapParentDef);
                    if (suggestedMapParentDef == null)
                    {
                        Log.Error("Tried to get or generate map at " + tile + ", but there isn't any MapParent world object here and map parent def argument is null.");
                    }
                    //mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(suggestedMapParentDef);
                    //mapParent.Tile = tile;
                    //Find.WorldObjects.Add(mapParent);
                }
               // map = MapGenerator.GenerateMap(size, mapParent, mapParent.MapGeneratorDef, mapParent.ExtraGenStepDefs, null);
            }
        }
    }*/

/*[HarmonyPatch(typeof(Caravan), "GetGizmos")]
static class Caravan_GetGizmos_Postfix
{

    [HarmonyPostfix]
    private static void GetGizmos(Caravan __instance, ref IEnumerable<Gizmo> __result)
    {
        List<Gizmo> results = new List<Gizmo>();
        foreach (Gizmo gizmo in __result)
        {
            results.Add(gizmo);
        }
        results.Add(new Command_Action
        {
            defaultLabel = "M_Dev: Gen cloud from path",
            action = delegate
            {


                int curTile = __instance.Tile;


                IEnumerable<int> directPathTiles = GenerateDirectPath(curTile, __instance.pather.Destination);
                //IEnumerable<int> cloudEnvelope = GenerateCloudEnvelope(directPathTiles, curTile, __instance.Destination, 4, 10, new SimpleCurve());
                IEnumerable<CloudTile> cloudEnvelope = GenerateCloudEnvelope(directPathTiles, ToxicFallout_ThicknessCurve, ToxicFallout_ThicknessCurve.Max_X() * ToxicFallout_DistanceCurve.Max_Y(), curTile, __instance.pather.Destination, ToxicFallout_DistanceCurve);

                WorldLayer_TestDraw.Instance.tiles.Clear();
                //WorldLayer_TestDraw.Instance.tiles.AddRange(directPathTiles);

                List<int> cloudAsTileIDs = new List<int>();
                foreach(CloudTile tile in cloudEnvelope)
                {
                    cloudAsTileIDs.Add(tile.tile);
                }

                WorldLayer_TestDraw.Instance.tiles.AddRange(cloudAsTileIDs);

                WorldLayer_TestDraw.Instance.dirty = true;
                //Log.Message(rep);
            }
        });
        __result = results;
    }
}*/
}
