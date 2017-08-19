using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Macrocosm.macrocosm;
using Verse;
using RimWorld;

namespace Macrocosm.defs
{
    public class SpecialPrerequisiteDef : Def
    {

        public bool forPawn = false;

        internal bool isMetByOutpost(ScoutingOutpost outpost)
        {
            if(this == SpecialPrerequisiteDefOf.FarmingPeriodLong)
            {
                List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(outpost.Tile, 10f, 42f);
                if (list.Count >= 6)
                    return true;
                else
                    return false;
            }
            if(this == SpecialPrerequisiteDefOf.FarmingPeriodShort)
            {
                List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(outpost.Tile, 10f, 42f);
                if (list.Count >= 3)
                    return true;
                else
                    return false;
            }
            if (this == SpecialPrerequisiteDefOf.HuntingAnimals)
            {
                List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(outpost.Tile, -10f, 45f);
                if (list.Count >= 9)
                    return true;
                else
                    return false;
            }
            return true;
        }

        internal bool isMetByPawn(Pawn pawn)
        {
            if(this == SpecialPrerequisiteDefOf.RangedWeapon)
            {
                if (pawn == null || pawn.equipment == null || pawn.equipment.Primary == null || !pawn.equipment.Primary.def.IsRangedWeapon)
                    return false;

                return true;
            }
            return true;
        }
    }
}
