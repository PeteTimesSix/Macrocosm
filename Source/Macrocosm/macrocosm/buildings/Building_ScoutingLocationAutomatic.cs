using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    class Building_ScoutingLocationAutomatic : Building
    {
        private Comp_ScoutLocationAutomatic scoutComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.scoutComp = base.GetComp<Comp_ScoutLocationAutomatic>();
            //LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
        }
    }
}
 