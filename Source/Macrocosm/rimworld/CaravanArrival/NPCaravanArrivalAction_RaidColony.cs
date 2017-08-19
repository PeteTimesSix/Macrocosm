using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using Macrocosm.extensions;
using Macrocosm.macrocosm;
using RimWorld;
using Verse.AI.Group;
using Verse.AI;

namespace Macrocosm.rimworld.CaravanArrival
{
    class NPCaravanArrivalAction_RaidColony : NPCaravanArrivalAction
    {

        public NPCaravanArrivalAction_RaidColony() : base() { }

        public NPCaravanArrivalAction_RaidColony(Settlement settlement) : base(settlement) { }

        public override string ReportString
        {
            get
            {
                return "NPCaravan_Raiding".Translate(new object[]
                {
                    this.settlement.Label
                });
            }
        }

        private string GetLetterLabel(Caravan_Macrocosm caravan)
        {
            if(caravan.Faction.HostileTo(Faction.OfPlayer))
                return caravan.caravanData.storedRaidStrategy.letterLabelEnemy;
            else
                return caravan.caravanData.storedRaidStrategy.letterLabelFriendly;
        }

        private string GetLetterText(Caravan_Macrocosm caravan, List<Pawn> pawns)
        {
            if (caravan.Faction.HostileTo(Faction.OfPlayer))
            {
                string text = null;
                text = "EnemyRaidWalkIn".Translate(new object[]
                {
                    caravan.Faction.def.pawnsPlural,
                    caravan.Faction.Name
                });
                text += "\n\n";
                text += caravan.caravanData.storedRaidStrategy.arrivalTextEnemy;
                Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (pawn != null)
                {
                    text += "\n\n";
                    text += "EnemyRaidLeaderPresent".Translate(new object[]
                    {
                    pawn.Faction.def.pawnsPlural,
                    pawn.LabelShort
                    });
                }
                return text;
            }
            else
            {
                string text = null;
                text = "FriendlyRaidWalkIn".Translate(new object[]
                {
                    caravan.Faction.def.pawnsPlural,
                    caravan.Faction.Name
                });
                text += "\n\n";
                text += caravan.caravanData.storedRaidStrategy.arrivalTextFriendly;
                Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
                if (pawn != null)
                {
                    text += "\n\n";
                    text += "FriendlyRaidLeaderPresent".Translate(new object[]
                    {
                    pawn.Faction.def.pawnsPlural,
                    pawn.LabelShort
                    });
                }
                return text;
            }
        }

        private string GetRelatedPawnsInfoLetterText(Caravan_Macrocosm caravan)
        {
            if (caravan.Faction.HostileTo(Faction.OfPlayer))
            {
                return "LetterRelatedPawnsRaidEnemy".Translate(new object[]
                {
                    caravan.Faction.def.pawnsPlural
                });
            }
            else
            {
                return "LetterRelatedPawnsRaidFriendly".Translate(new object[]
                {
                    caravan.Faction.def.pawnsPlural
                });
            }
        }

        private LetterDef GetLetterDef(Caravan_Macrocosm caravan)
        {
            if (caravan.Faction.HostileTo(Faction.OfPlayer))
                return LetterDefOf.BadUrgent;
            else
                return LetterDefOf.Good;
        }

        public override void PlayerSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {

            Macrocosm.saveData.saveStoredCaravan(caravan.DataToStore, settlement);

            Map map = settlement.Map;
            List<Pawn> prePawns = caravan.PawnsListForReading;

            TargetInfo target = TargetInfo.Invalid;

            List<Pawn> pawns = new List<Pawn>();
            pawns.AddRange(prePawns);

            IntVec3 spawnCenter;
            RCellFinder.TryFindRandomPawnEntryCell(out spawnCenter, map, CellFinder.EdgeRoadChance_Hostile, null);

            Rot4 spawnRotation = Rot4.FromAngleFlat((map.Center - spawnCenter).AngleFlat);

            foreach (Pawn current in pawns)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(spawnCenter, map, 8, null);
                GenSpawn.Spawn(current, loc, map, spawnRotation, false);
                target = current;
            }

            string letterLabel = this.GetLetterLabel(caravan);
            string letterText = this.GetLetterText(caravan, pawns);
            PawnRelationUtility.Notify_PawnsSeenByPlayer(pawns, ref letterLabel, ref letterText, this.GetRelatedPawnsInfoLetterText(caravan), true);

            Find.LetterStack.ReceiveLetter(letterLabel, letterText, this.GetLetterDef(caravan), target);
            if (this.GetLetterDef(caravan) == LetterDefOf.BadUrgent)
            {
                TaleRecorder.RecordTale(TaleDefOf.RaidArrived, new object[0]);
            }

            IncidentParms gennedParms = new IncidentParms();
            gennedParms.faction = caravan.Faction;
            gennedParms.raidStrategy = caravan.caravanData.storedRaidStrategy;
            gennedParms.spawnCenter = spawnCenter;
            gennedParms.points = caravan.caravanData.storedRaidPoints;

            Lord lord = LordMaker.MakeNewLord(caravan.Faction, caravan.caravanData.storedRaidStrategy.Worker.MakeLordJob(gennedParms, map), map, pawns);
            AvoidGridMaker.RegenerateAvoidGridsFor(caravan.Faction, map);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int i = 0; i < pawns.Count; i++)
                {
                    Pawn pawn = pawns[i];
                    if (pawn.apparel.WornApparel.Any((Apparel ap) => ap is ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            if (DebugViewSettings.drawStealDebug && caravan.Faction.HostileTo(Faction.OfPlayer))
            {
                Log.Message(string.Concat(new object[]
                {
                    "Market value threshold to start stealing: ",
                    StealAIUtility.StartStealingMarketValueThreshold(lord),
                    " (colony wealth = ",
                    map.wealthWatcher.WealthTotal,
                    ")"
                }));
            }

            if (caravan.Spawned)
            {
                Find.WorldObjects.Remove(caravan);
            }       
        }

        public override void NPCSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {
            caravan.caravanData.visitedSites.Add(settlement);
            caravan.setTimeToDecision(4000);
            caravan.pather.StopDead();
            caravan.arrived = true;
        }
    }
}
