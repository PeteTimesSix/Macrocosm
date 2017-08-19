using Macrocosm.defs;
using Macrocosm.macrocosm.buildings;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Macrocosm.macrocosm.ai
{
    class WorkGiver_Scout : WorkGiver_Scanner
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(Macrocosm_JobDefOf.JobScout, t);
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            IEnumerable<Building> scoutables = pawn.Map.listerBuildings.allBuildingsColonist.Where(
                o =>    o.GetComp<Comp_ScoutLocationManned>() != null &&
                        o.GetComp<Comp_ScoutLocationManned>().Usable &&
                        (o.GetComp<CompPowerTrader>() == null || o.GetComp<CompPowerTrader>().PowerOn));

            Building bestScoutable = scoutables.OrderByDescending(o => o.GetComp<Comp_ScoutLocationManned>().TileRange).FirstOrDefault();
            
            if (bestScoutable != null && bestScoutable.GetComp<Comp_ScoutLocationManned>().NeedsRefresh && !bestScoutable.IsForbidden(pawn) && !bestScoutable.IsBrokenDown() && !bestScoutable.IsBurning() && pawn.CanReserve(bestScoutable))
            {
                yield return bestScoutable;
            }
        }
    }
}
