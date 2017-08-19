﻿using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm
{
    public static class RandomUtilityFunctions
    {
        const int DisabledWorkPriority = 0;
        const int DefaultWorkPriority = 3;

        // Allows to add WorkTypeDefs to an existing saved game without causing exceptions in the Work tab and work scheduler.
        // Returns true if the work type array had to be padded for at least one pawn.
        public static bool EnsureAllColonistsKnowAllWorkTypes(Map map)
        {
            try
            {
                var injectedPawns = new HashSet<Pawn>();
                if (map == null || map.mapPawns == null) return false;
                foreach (var pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
                {
                    if (pawn == null || pawn.workSettings == null) continue;
                    var priorityList = GetWorkPriorityListForPawn(pawn);
                    if (priorityList != null && priorityList.Count > 0)
                    {
                        var cyclesLeft = 100;
                        // the priority list must be padded to accomodate all available WorkTypeDef.index
                        // pad by the maximum index available to make provisions for other mods' worktypes
                        var maxIndex = DefDatabase<WorkTypeDef>.AllDefs.Max(d => d.index);
                        while (priorityList.Count <= maxIndex && cyclesLeft > 0)
                        {
                            cyclesLeft--;
                            priorityList.Add(DisabledWorkPriority);
                            injectedPawns.Add(pawn);
                        }
                        if (cyclesLeft == 0)
                        {
                            throw new Exception(String.Format("Ran out of cycles while trying to pad work priorities list:  {0} {1}", pawn.Name, priorityList.Count));
                        }
                    }
                }
                if (injectedPawns.Count > 0)
                {
                    //Log.Message("Padded work priority lists for pawns: {0}", injectedPawns.Join(", ", true));
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception while injecting WorkTypeDef into colonist pawns: " + e);
            }
            return false;
        }

        // due to other mods' worktypes, our worktype priority may start at zero. This should fix that.
        public static void EnsureAllColonistsHaveWorkTypeEnabled(WorkTypeDef def, Map map)
        {
            try
            {
                var activatedPawns = new HashSet<Pawn>();
                if (map == null || map.mapPawns == null) return;
                foreach (var pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
                {
                    var priorityList = GetWorkPriorityListForPawn(pawn);
                    if (priorityList != null && priorityList.Count > 0)
                    {
                        var curValue = priorityList[def.index];
                        if (curValue == DisabledWorkPriority)
                        {
                            var adjustedValue = GetWorkTypePriorityForPawn(def, pawn);
                            if (adjustedValue != curValue)
                            {
                                priorityList[def.index] = adjustedValue;
                                activatedPawns.Add(pawn);
                            }
                        }
                    }
                }
                if (activatedPawns.Count > 0)
                {
                    //Log.Message("Adjusted work type priority of {0} to default for pawns: {1}", def.defName, activatedPawns.Join(", ", true));
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception while adjusting work type priority in colonist pawns: " + e);
            }
        }

        private static List<int> GetWorkPriorityListForPawn(Pawn pawn)
        {
            if (pawn != null && pawn.workSettings != null)
            {
                var workDefMap = Traverse.Create(pawn.workSettings).Field("priorities").GetValue<DefMap<WorkTypeDef, int>>();
                if (workDefMap == null) throw new Exception("Failed to retrieve workDefMap for pawn: " + pawn);
                var priorityList = Traverse.Create(workDefMap).Field("values").GetValue<List<int>>();
                if (priorityList == null) throw new Exception("Failed to retrieve priority list for pawn: " + pawn);
                return priorityList;
            }
            return null;
        }

        // returns a work priority based on disabled work types and tags for that pawn
        private static int GetWorkTypePriorityForPawn(WorkTypeDef workDef, Pawn pawn)
        {
            if (pawn.story != null)
            {
                if (pawn.story.WorkTypeIsDisabled(workDef) || pawn.story.WorkTagIsDisabled(workDef.workTags))
                {
                    return DisabledWorkPriority;
                }
            }
            return DefaultWorkPriority;
        }
    }
}
