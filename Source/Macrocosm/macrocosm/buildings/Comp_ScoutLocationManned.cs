using Macrocosm.defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    class Comp_ScoutLocationManned : ThingComp
    {
        public bool NeedsRefresh { get { return Macrocosm.saveData.ScoutingManager.TicksFor(this.parent, this.Props.potentialTileRange) < this.Props.ticksForJob; } }

        public int TileRange {
            get
            {
                return (int)(parent as Building).GetStatValue(Macrocosm_StatDefOf.ScoutingTileRange);
            }
        }
        
        public CompProperties_ScoutLocationManned Props
        {
            get
            {  
                return (CompProperties_ScoutLocationManned)this.props;
            }
        }

        public bool IsNotRoofed { 
            get 
            {
                foreach (IntVec3 current in this.parent.OccupiedRect())
                {
                    if (this.parent.Map.roofGrid.Roofed(current))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool Usable
        {
            get
            {
                return (this.props as CompProperties_ScoutLocationManned).allowsRoof || IsNotRoofed;
            }
        } 

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());
            if (!Usable)
            {
                stringBuilder.AppendLine("WindTurbine_WindPathIsBlockedByRoof".Translate());
            }
            return stringBuilder.ToString();
        }

        internal void DoScout()
        {
            if(Usable)
                Macrocosm.saveData.ScoutingManager.UpdateFromScoutLocation(this.parent, this.TileRange, this.Props.ticksCapacity);
        }
    }
}
