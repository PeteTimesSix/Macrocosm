using Macrocosm.extensions;
using Macrocosm.rimworld;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.runningEvents
{
    class RunningEvent_ToxicFallout : CloudyRunningEvent
    {

        #region curves
        private static readonly SimpleCurve ToxicFallout_ThicknessCurve = new SimpleCurve
        {
            {
                new CurvePoint(4f, 1f),
                true
            },
            {
                new CurvePoint(6f, 0.6f),
                true
            },
            {
                new CurvePoint(7f, 0f),
                true
            }
        };

        private static readonly SimpleCurve ToxicFallout_DistanceCurve = new SimpleCurve
        {
            {
                new CurvePoint(0f, 0.5f),
                true
            },
            {
                new CurvePoint(0.2f, 1f),
                true
            },
            {
                new CurvePoint(0.5f, 1.15f),
                true
            },
            {
                new CurvePoint(0.9f, 1.4f),
                true
            },
            {
                new CurvePoint(1f, 0.5f),
                true
            }
        };
        #endregion

        public override IncidentDef Incident { get { return IncidentDefOf.ToxicFallout; } }
        public override GameConditionDef ConditionDef { get { return GameConditionDefOf.ToxicFallout; } }
        public override ICloudLayer CloudLayer { get { return WorldLayer_ToxicFallout.Instance; } }
        public override SimpleCurve ThicknessCurve { get { return ToxicFallout_ThicknessCurve; } }
        public override SimpleCurve DistanceCurve { get { return ToxicFallout_DistanceCurve; } }

        internal override int getTicksPerTilesAppearing(){ return 80; }
        internal override int getTicksPerTilesDisappearing() { return 60; }

        internal override int getTilesAppearingPerLoop() { return 8; }
        internal override int getTilesDisappearingPerLoop() { return 8; }

        public RunningEvent_ToxicFallout() : base() { }

        public RunningEvent_ToxicFallout(int duration, int targetTile) : base(duration, targetTile)
        {
        }
    }
}
