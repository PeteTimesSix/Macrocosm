using Macrocosm.defs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.macrocosm.buildings
{
    [StaticConstructorOnStartup]
    class Building_MacrocontrollerTrainer : Building
    {
        private bool empty = true;
        private float progressInt;

        public float Progress
        {
            get
            {
                return this.progressInt;
            }
            set
            {
                if (value == this.progressInt)
                {
                    return;
                }
                this.progressInt = value;
            }
        }

        public bool ReadyForNewCore
        {
            get
            {
                return empty;
            }
        }

        public bool FinishedTraining
        {
            get
            {
                return !empty && Progress >= 1f;
            }
        }

        private float progressPerTick = 1f/20000f;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.empty, "empty", true, false);
            Scribe_Values.Look<float>(ref this.progressInt, "progress", 0f, false);
        }

        public override void TickRare()
        {
            base.TickRare();
            if (!empty && GetComp<CompPowerTrader>().PowerOn)
            {
                Progress = Mathf.Min(Progress + 250f * progressPerTick, 1f);
            }
        }

        public void InsertCore(Thing core)
        {
            if (!empty)
                Log.Warning("Inserted new core into an already full macrocontroller trainer!");

            empty = false;
            core.Destroy(DestroyMode.Vanish);
        }

        public Thing RemoveCore()
        {
            if (!this.FinishedTraining)
            {
                Log.Warning("Tried to remove macrocontroller but it's not yet trained.");
                return null;
            }
            Thing thing = ThingMaker.MakeThing(Macrocosm_ThingDefOf.Macrocontroller, null);
            thing.stackCount = 1;
            Reset();
            return thing;
        }

        public void Reset()
        {
            progressInt = 0;
            empty = true;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            if (stringBuilder.Length != 0)
            {
                stringBuilder.AppendLine();
            }

            if (!empty)
            {
                if (FinishedTraining)
                {
                    stringBuilder.AppendLine("Trainer_FinishedTraining".Translate());
                }
                else
                {
                    if (GetComp<CompPowerTrader>().PowerOn)
                    {
                        stringBuilder.AppendLine("Trainer_TrainingInProgress".Translate());
                    }
                    else
                    {
                        stringBuilder.AppendLine("Trainer_TrainingInProgressNoPower".Translate());
                    }
                }
            }
            else
            {
                stringBuilder.AppendLine("Trainer_NoCore".Translate());
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }

        private static readonly Material CBarFilledMat   = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 1.0f, 0.3f), false);
        private static readonly Material CBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        public override void Draw()
        {  
            base.Draw();
            if (!empty)
            {
                Vector3 drawPos = this.DrawPos;
                drawPos.y += 0.046875f;
                drawPos.z += 0.25f;
                GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
                {
                    center = drawPos,
                    size = Building_MacrocontrollerTrainer.BarSize,
                    fillPercent = Progress,
                    filledMat = CBarFilledMat,
                    unfilledMat = CBarUnfilledMat,
                    margin = 0.1f,
                    rotation = Rot4.North
                });
            }
        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (Prefs.DevMode && !empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 0.5",
                    action = delegate
                    {
                        this.Progress = 0.5f;
                    }
                };
            }

            if (Prefs.DevMode && !empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 0.95",
                    action = delegate
                    {
                        this.Progress = 0.95f;
                    }
                };
            }

            if (Prefs.DevMode && !empty)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set progress to 1",
                    action = delegate
                    {
                        this.Progress = 1f;
                    }
                };
            }
        }
    }
}
