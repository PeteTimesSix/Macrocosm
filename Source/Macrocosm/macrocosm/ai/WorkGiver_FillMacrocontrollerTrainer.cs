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
    class WorkGiver_FillMacrocontrollerTrainer : WorkGiver_Scanner
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
            if (trainer == null || !trainer.ReadyForNewCore)
            {
                return false;
            }
            /*CompPowerTrader compPower = trainer.TryGetComp<CompPowerTrader>();
            if(compPower == null || !compPower.PowerOn)
            {
                return false;
            }*/
            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (this.FindCore(pawn, trainer) == null)
            {
                JobFailReason.Is("NoUntrainedCores".Translate());
                return false;
            }
            return !t.IsBurning();
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_MacrocontrollerTrainer trainer = (Building_MacrocontrollerTrainer)t;
            Thing t2 = this.FindCore(pawn, trainer);
            return new Job(Macrocosm_JobDefOf.FillMacrocontrollerTrainer, t, t2)
            {
                count = 1
            };
        }

        private Thing FindCore(Pawn pawn, Building_MacrocontrollerTrainer trainer)
        {
            Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
            Predicate<Thing> validator = predicate;
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(Macrocosm_ThingDefOf.MacrocontrollerUntrained), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
        }
    }
}
