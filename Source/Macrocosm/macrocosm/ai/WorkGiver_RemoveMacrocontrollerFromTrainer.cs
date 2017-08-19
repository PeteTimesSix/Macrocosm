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
    class WorkGiver_RemoveMacrocontrollerFromTrainer : WorkGiver_Scanner
    { 
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForDef(Macrocosm_ThingDefOf.MacrocontrollerTrainer);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_MacrocontrollerTrainer trainer = t as Building_MacrocontrollerTrainer;
            return trainer != null && trainer.FinishedTraining && !t.IsBurning() && !t.IsForbidden(pawn) && pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return new Job(Macrocosm_JobDefOf.EmptyMacrocontrollerTrainer, t);
        }
    }
}
