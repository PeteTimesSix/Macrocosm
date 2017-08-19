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
    class JobDriver_Scout : JobDriver
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(delegate
            {
                ThingWithComps thingWithComps = pawn.CurJob.GetTarget(TargetIndex.A).Thing as ThingWithComps;
                if (thingWithComps != null)
                {
                    Comp_ScoutLocationManned comp0 = thingWithComps.GetComp<Comp_ScoutLocationManned>();
                    if (comp0 == null || !comp0.Usable)
                        return true;
                    CompFlickable comp1 = thingWithComps.GetComp<CompFlickable>();
                    if (comp1 != null && !comp1.SwitchIsOn)
                        return true;
                    CompPowerTrader comp2 = thingWithComps.GetComp<CompPowerTrader>();
                    if (comp2 != null && !comp2.PowerOn)
                        return true;
                }
                return false;
            });
            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

            int waitTicks = (pawn.CurJob.GetTarget(TargetIndex.A).Thing as ThingWithComps).GetComp<Comp_ScoutLocationManned>().Props.scoutingTimeTicks;

            yield return Toils_General.Wait(waitTicks).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell).WithProgressBarToilDelay(TargetIndex.A);
            yield return Toils_Scout.FinalizeScouting(TargetIndex.A);
        }
    }
}
