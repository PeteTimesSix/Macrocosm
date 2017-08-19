using Harmony;
using Macrocosm.macrocosm;
using Macrocosm.rimworld.CaravanArrival;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static Macrocosm.macrocosm.SettlementSelector;

namespace Macrocosm.extensions
{
    public class Caravan_Macrocosm : Caravan, ISpottable
    {
        public StoredCaravan caravanData = new StoredCaravan();
        public StoredCaravan DataToStore { get { return caravanData; } }

        public bool arrived = false;

        private NPCaravanArrivalAction ArrivalAction {
            get { return this.pather.arrivalAction as NPCaravanArrivalAction; }
        }

        private bool spottedEver = false;
        private bool spottedNow = false;
        public bool Spotted { get { return spottedNow; } private set { if (value) { spottedEver = true; spottedNow = true; } else { spottedNow = false; } } }

        public enum CaravanKind { Unspecified, RaidEnemy, RaidFriendly, Trader, Visitor, Passerby, ReturnHome };

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look<StoredCaravan>(ref this.caravanData, "caravanData");
            Scribe_Values.Look<bool>(ref this.arrived, "arrived");
        }

        public override void Draw()
        {
            if (Spotted || DebugViewSettings.drawPaths)
            {
                if (this.pather.curPath != null)
                    this.pather.curPath.DrawPath(this);

                base.Draw();
            }
        }

        public void setTimeToDecision(int delayTicks)
        {
            decisionTicksLeft = delayTicks;
        }
        
        public void delayDeparture(int delayTicks)
        {
            startTicksLeft += delayTicks;
        }

        public void delayDepartureUpTo(int delayTicks)
        {
            if (startTicksLeft < delayTicks)
                startTicksLeft = delayTicks;
        }

        private int decisionTicksLeft = 0;
        private int startTicksLeft = 600;
        
        public void GetSpotted(bool generateLetter)
        {
            if(!spottedEver && generateLetter)
                GenerateFirstSpotLetter();
            Spotted = true;
        }

        private void GenerateFirstSpotLetter()
        {
            LetterDef letterDef = LetterDefOf.Good;
            string label = "CaravanSpottedLetterLabel".Translate(new object[]
            {
                this.ArrivalAction.settlement.LabelShort
            });
            string text = "CaravanSpottedLetterText".Translate(new object[]
            {
                this.Faction.Name,
                this.ArrivalAction.settlement.LabelShort
            });
            text += "\n\n";
            switch (caravanData.caravanKind)
            {
                case CaravanKind.RaidEnemy:
                    text += "CaravanKind_RaidSpotText".Translate();
                    letterDef = LetterDefOf.BadNonUrgent;
                    break;
                case CaravanKind.RaidFriendly:
                    text += "CaravanKind_RaidSpotText".Translate();
                    break;
                case CaravanKind.Trader:
                    text += "CaravanKind_TraderSpotText".Translate();
                    break;
                case CaravanKind.Visitor:
                case CaravanKind.Passerby:
                case CaravanKind.Unspecified:
                case CaravanKind.ReturnHome:
                    text += "CaravanKind_GenericSpotText".Translate();
                    break;
            }
            Find.LetterStack.ReceiveLetter(label, text, letterDef, this);
        }

        public override void Tick()
        {
            if (this.IsHashIntervalTick(20))
            {
                //NPCs are food-tax exempt for simplicity
                if (!Faction.IsPlayer)
                {
                    foreach (Pawn pawn in pawns)
                    {
                        pawn.needs.food.ForceSetLevel(0.65f);
                    }
                    if(decisionTicksLeft <= 0)
                    {
                        if (decisionTicksLeft <= 0 && this.arrived)
                        {
                            NPCaravanArrivalAction arrivalAction = CaravanGoalDecisionmaker.decide(this);
                            this.pather.StartPath(arrivalAction.settlement.Tile, arrivalAction, true);
                            this.arrived = false;
                        }
                    }
                    if (!Spotted)
                    {
                        if (Macrocosm.saveData.ScoutingManager.IsScouted(this))
                            GetSpotted(true);
                    }
                }
            }
            if(startTicksLeft > 0)
            {
                startTicksLeft--;
                this.pather.nextTileCostLeft = this.pather.nextTileCostTotal;
            }
            if(decisionTicksLeft > 0)
            {
                decisionTicksLeft--;
            }
            base.Tick(); 
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.Resting)
            {    
                stringBuilder.Append("CaravanResting".Translate());
            }
            else if (this.pather.Moving)
            {
                if (this.pather.arrivalAction != null) 
                {  
                    stringBuilder.Append(this.pather.arrivalAction.ReportString);
                }    
                else   
                {
                    stringBuilder.Append("CaravanTraveling".Translate());
                }
            }
            else 
            { 
                Settlement settlement = CaravanVisitUtility.SettlementVisitedNow(this);
                if (settlement != null)
                {
                    stringBuilder.Append("CaravanVisiting".Translate(new object[]
                    {
                        settlement.Label
                    }));
                }
                else
                {
                    stringBuilder.Append("CaravanWaiting".Translate());
                }
            }
            return stringBuilder.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "M_Dev: Randomly change faction",
                    action = delegate
                    {
                        Faction res;
                        Find.FactionManager.AllFactions.TryRandomElement(out res);
                    
                        Traverse.Create(this).Field("factionInt").SetValue(res);

                        foreach (Pawn pawn in pawns)
                        {
                            Traverse.Create(pawn).Field("factionInt").SetValue(res);
                        }

                        Find.ColonistBar.MarkColonistsDirty();

                        Log.Message("changed to faction " + res.Name);

                        Traverse.Create(this).Field("cachedMat").SetValue(null);
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "M_Dev: Force nextStop to player colony",
                    action = delegate
                    {
                        Settlement target = SettlementSelector.selectNearestBase(ArrivalAction.settlement, Faction, SettlementSelector.SelectionCriteria.Player, 0, 0, caravanData.visitedSites, true).First();
                        NPCaravanArrivalAction newAction = CaravanGoalDecisionmaker.decide(this, target);
                        pather.StartPath(target.Tile, newAction, true);
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "M_Dev: Force nextStop to non-player allied colony",
                    action = delegate
                    {
                        Settlement target = SettlementSelector.selectNearestBase(ArrivalAction.settlement, Faction, SettlementSelector.SelectionCriteria.AlliedAndSelf, 0, 0, caravanData.visitedSites, false).First();
                        NPCaravanArrivalAction newAction = CaravanGoalDecisionmaker.decide(this, target);
                        pather.StartPath(target.Tile, newAction, true);
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "M_Dev: Force nextStop to non-player hostile colony",
                    action = delegate
                    {
                        Settlement target = SettlementSelector.selectNearestBase(ArrivalAction.settlement, Faction, SettlementSelector.SelectionCriteria.Hostile, 0, 0, caravanData.visitedSites, false).First();
                        NPCaravanArrivalAction newAction = CaravanGoalDecisionmaker.decide(this, target);
                        pather.StartPath(target.Tile, newAction, true);
                    }
                };
            }
        }

        internal static Caravan_Macrocosm spawnCaravan(Pawn firstPawn, Settlement start, StoredCaravan storedCaravan = null)
        {
            if (storedCaravan == null)
            {
                storedCaravan = new StoredCaravan();
                storedCaravan.caravanKind = CaravanKind.ReturnHome;
            }

            storedCaravan.visitedSites.Add(start);

            List<Pawn> listedPawn = new List<Pawn>();
            listedPawn.Add(firstPawn);
            Caravan_Macrocosm caravan = RimworldUtilities.RemakeCaravan(listedPawn, firstPawn.Faction, start, true, storedCaravan);

            NPCaravanArrivalAction action = CaravanGoalDecisionmaker.decide(caravan);
            caravan.pather.StartPath(action.settlement.Tile, action, true);

            Macrocosm.saveData.FindAndRemoveStoredCaravan(storedCaravan);
            
            return caravan;
        }

        internal void join(Pawn pawn, bool addToWorldPawnsIfNotAlready)
        {
            if (pawn.Spawned)
            {
                pawn.DeSpawn();
            }
            if (pawn.Dead)
            {
                Log.Warning("Tried to form a caravan with a dead pawn " + pawn);
            }
            else
            {
                this.AddPawn(pawn, addToWorldPawnsIfNotAlready);
                if (addToWorldPawnsIfNotAlready && !pawn.IsWorldPawn())
                {
                    if (pawn.Spawned)
                    {
                        pawn.DeSpawn();
                    }
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Decide);
                }
                this.delayDepartureUpTo(600);
            }
        }
    }


} 
