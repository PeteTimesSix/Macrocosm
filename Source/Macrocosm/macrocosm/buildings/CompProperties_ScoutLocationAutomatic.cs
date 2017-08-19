using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    public class CompProperties_ScoutLocationAutomatic : CompProperties
    {
        public int ticksCapacity;
        public int potentialTileRange;

        public CompProperties_ScoutLocationAutomatic()
        {
            this.compClass = typeof(Comp_ScoutLocationAutomatic);
        }
    }
}
