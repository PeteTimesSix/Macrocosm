using Macrocosm.extensions;
using Macrocosm.macrocosm;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Macrocosm.rimworld.CaravanArrival
{
    class NPCaravanArrivalAction_TradeWithColony : NPCaravanArrivalAction
    {

        public NPCaravanArrivalAction_TradeWithColony() : base() { }

        public NPCaravanArrivalAction_TradeWithColony(Settlement settlement) : base(settlement) { }

        public override string ReportString
        {
            get
            {
                return "NPCaravan_Trading".Translate(new object[]
                {
                    this.settlement.Label
                });
            }
        }

        public override void PlayerSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {
            Macrocosm.saveData.saveStoredCaravan(caravan.DataToStore, settlement);

            Map map = settlement.Map;
            List<Pawn> prePawns = caravan.PawnsListForReading;
            
            List<Pawn> pawns = new List<Pawn>();
            pawns.AddRange(prePawns);


            TraderKindDef traderKindDef = null;
            Pawn trader = null;
            for (int j = 0; j < pawns.Count; j++)
            {
                Pawn pawn = pawns[j];
                if (pawn.TraderKind != null)
                {
                    traderKindDef = pawn.TraderKind;
                    trader = pawn;
                    break;
                }
            }

            IntVec3 spawnCenter;
            RCellFinder.TryFindRandomPawnEntryCell(out spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, null);

            foreach (Pawn current in pawns)
            {
                bool isTrader = (current == trader);
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(spawnCenter, map, 5, null);
                GenSpawn.Spawn(current, loc, map);
                //why does the trader forget he's a trader? I'd love to know that.
                if (isTrader)
                {
                    current.mindState.wantsToTradeWithColony = true;
                    PawnComponentsUtility.AddAndRemoveDynamicComponents(current, true);
                    current.trader.traderKind = traderKindDef;
                }
            }

            string label = "LetterLabelTraderCaravanArrival".Translate(new object[]
            {
                caravan.Faction.Name,
                traderKindDef.label
            }).CapitalizeFirst();
            string text = "LetterTraderCaravanArrival".Translate(new object[]
            {
                caravan.Faction.Name,
                traderKindDef.label
            }).CapitalizeFirst();

            PawnRelationUtility.Notify_PawnsSeenByPlayer(pawns, ref label, ref text, "LetterRelatedPawnsNeutralGroup".Translate(), true);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.Good, pawns[0], null);
            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out chillSpot);
            LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(caravan.Faction, chillSpot);
            LordMaker.MakeNewLord(caravan.Faction, lordJob, map, pawns);
            
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
