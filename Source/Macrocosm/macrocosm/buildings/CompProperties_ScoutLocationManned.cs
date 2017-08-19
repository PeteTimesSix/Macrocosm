using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    public class CompProperties_ScoutLocationManned : CompProperties
    {
        public int ticksCapacity;
        public int ticksForJob;
        public int potentialTileRange;
        public int scoutingTimeTicks;
        public bool allowsRoof;

        public CompProperties_ScoutLocationManned()
        {
            this.compClass = typeof(Comp_ScoutLocationManned);
        }
    }
}
