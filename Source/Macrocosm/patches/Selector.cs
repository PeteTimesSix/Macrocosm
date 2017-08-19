using Harmony;
using Macrocosm.extensions;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.patches
{
    [HarmonyPatch(typeof(GenWorldUI), "WorldObjectsUnderMouse")]
    public static class GenWorldUI_WorldObjectsUnderMouse_Postfix
    {
        [HarmonyPostfix]
        private static void WorldObjectsUnderMouse(List<WorldObject> __result)
        {
            //Log.Message("WorldObjectsUnderMouse postfix");
            for(int i = __result.Count - 1; i >= 0; i--)
            {
                if(__result[i] is Caravan_Macrocosm)
                {
                    //Log.Message("selection contains macrocosm caravan");
                    if(!((__result[i] as Caravan_Macrocosm).Spotted))
                    {
                        //Log.Message("and it wasnt spotted yet. Removing...");
                        __result.RemoveAt(i);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorldObjectSelectionUtility), "MultiSelectableWorldObjectsInScreenRectDistinct")]
    public static class WorldObjectSelectionUtility_MultiSelectableWorldObjectsInScreenRectDistinct_Postfix
    {
        [HarmonyPostfix]
        private static void MultiSelectableWorldObjectsInScreenRectDistinct(ref IEnumerable<WorldObject> __result)
        {
            List<WorldObject> list = new List<WorldObject>();
            list.AddRange(__result);

            //Log.Message("MultiSelectableWorldObjectsInScreenRectDistinct postfix");
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] is Caravan_Macrocosm)
                {
                    //Log.Message("selection contains macrocosm caravan");
                    if (!((list[i] as Caravan_Macrocosm).Spotted))
                    {
                        //Log.Message("and it wasnt spotted yet. Removing...");
                        list.RemoveAt(i);
                    }
                }
            }

            __result = list;
        }
    }
}
