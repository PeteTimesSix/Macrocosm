using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    class Building_ScoutingLocationManned : Building
    {
        private Comp_ScoutLocationManned scoutComp;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.scoutComp = base.GetComp<Comp_ScoutLocationManned>();
            //LessonAutoActivator.TeachOpportunity(ConceptDefOf.BuildOrbitalTradeBeacon, OpportunityType.GoodToKnow);
        }
    }
}
 