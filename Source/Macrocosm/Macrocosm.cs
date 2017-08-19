using HugsLib;
using HugsLib.Utils;
using Macrocosm.defs;
using Macrocosm.macrocosm;
using Macrocosm.saveData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm
{
    public class Macrocosm : ModBase
    {
        public static MacrocosmSaveData saveData;

        public override string ModIdentifier
        {
            get
            {
                return "Macrocosm";
            }
        }

        public override void WorldLoaded()
        {
            saveData = UtilityWorldObjectManager.GetUtilityWorldObject<MacrocosmSaveData>();
        }

        public override void MapLoaded(Map map)
        {
            // necessary when adding the mod to existing saves
            // thank ye kindly, UnlimitedHugs
            var injected = RandomUtilityFunctions.EnsureAllColonistsKnowAllWorkTypes(map);
            if (injected)
            {
                RandomUtilityFunctions.EnsureAllColonistsHaveWorkTypeEnabled(Macrocosm_WorkDefOf.ScoutingWork, map);
            }
        }
         
        public override void Tick(int currentTick)
        {
            base.Tick(currentTick);

            if(saveData != null)
            {
                if (saveData.RunningEventManager != null)
                    saveData.RunningEventManager.Tick(currentTick);
                else
                    Log.Error("runningEventManager is null!");


                if (saveData.ScoutingManager != null)
                    saveData.ScoutingManager.Tick(currentTick);
                else
                    Log.Error("runningEventManager is null!");
            }
        }
    }
}
