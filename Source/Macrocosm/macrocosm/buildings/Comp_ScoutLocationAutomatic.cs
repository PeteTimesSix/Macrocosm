using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    class Comp_ScoutLocationAutomatic : ThingComp
    {
        public bool IsScouting {
            get
            {
                CompPowerTrader power = parent.GetComp<CompPowerTrader>();
                if (power != null)
                {
                    return power.PowerOn && !parent.IsBrokenDown();
                }
                else
                {
                    return !parent.IsBrokenDown();
                }
            }
        }

        public int PotentialTileRange { get { return this.Props.potentialTileRange; } }

        public CompProperties_ScoutLocationAutomatic Props
        {
            get
            {  
                return (CompProperties_ScoutLocationAutomatic)this.props;
            }
        }

        public override void CompTick()
        {
            this.DoTicks(1);
        }

        public override void CompTickRare()
        {
            this.DoTicks(250);
        }

        private void DoTicks(int delta)
        {
            base.CompTick();

            if (IsScouting)
            {
                Macrocosm.saveData.ScoutingManager.UpdateFromScoutLocation(this.parent, this.Props.potentialTileRange, this.Props.ticksCapacity);
            }
        }
    }
}
