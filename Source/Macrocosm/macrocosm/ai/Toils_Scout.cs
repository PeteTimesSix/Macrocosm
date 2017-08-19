using Macrocosm.macrocosm.buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Macrocosm.macrocosm.ai
{
    class Toils_Scout
    {
        public static Toil FinalizeScouting(TargetIndex scoutLocationInd)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.CurJob;
                Thing thing = curJob.GetTarget(scoutLocationInd).Thing;
                thing.TryGetComp<Comp_ScoutLocationManned>().DoScout();
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}
