using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using RimWorld;
using Verse;
using Macrocosm.extensions;
using Verse.AI.Group;
using Macrocosm.macrocosm;

namespace Macrocosm.rimworld.CaravanArrival
{
    class NPCaravanArrivalAction_VisitColony : NPCaravanArrivalAction
    {
        public NPCaravanArrivalAction_VisitColony() : base() { }

        public NPCaravanArrivalAction_VisitColony(Settlement settlement) : base(settlement) { }

        public override string ReportString
        {
            get
            {
                return "NPCaravan_Visiting".Translate(new object[]
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

            IntVec3 spawnCenter;
            RCellFinder.TryFindRandomPawnEntryCell(out spawnCenter, map, CellFinder.EdgeRoadChance_Neutral, null);

            foreach (Pawn current in pawns)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(spawnCenter, map, 5, null);
                GenSpawn.Spawn(current, loc, map);
            }

            IntVec3 chillSpot;
            RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out chillSpot);
            LordJob_VisitColony lordJob = new LordJob_VisitColony(caravan.Faction, chillSpot);
            LordMaker.MakeNewLord(caravan.Faction, lordJob, map, pawns);

            bool flag;
            if (!caravan.caravanData.firstTimeGenerationDone)
            {
                flag = false;
                if (Rand.Value < 0.75f)
                {
                    Pawn trader;
                    flag = TryConvertOnePawnToSmallTrader(pawns, caravan.Faction, map, out trader);
                    caravan.caravanData.traderPawn = trader;
                }
            }
            else
            {
                flag = caravan.caravanData.traderPawn != null;
                if(!caravan.caravanData.traderPawn.Dead)
                    caravan.caravanData.traderPawn.mindState.wantsToTradeWithColony = true;
            }
            
            Pawn pawn = pawns.Find((Pawn x) => caravan.Faction.leader == x);
            string label;
            string text3;
            if (pawns.Count == 1)
            {
                string text = (!flag) ? string.Empty : "SingleVisitorArrivesTraderInfo".Translate();
                string text2 = (pawn == null) ? string.Empty : "SingleVisitorArrivesLeaderInfo".Translate();
                label = "LetterLabelSingleVisitorArrives".Translate();
                text3 = "SingleVisitorArrives".Translate(new object[]
                {
                    pawns[0].story.Title.ToLower(),
                    caravan.Faction.Name,
                    pawns[0].Name,
                    text,
                    text2
                });
                text3 = text3.AdjustedFor(pawns[0]);
            }
            else
            {
                string text4 = (!flag) ? string.Empty : "GroupVisitorsArriveTraderInfo".Translate();
                string text5 = (pawn == null) ? string.Empty : "GroupVisitorsArriveLeaderInfo".Translate(new object[]
                {
                    pawn.LabelShort
                });
                label = "LetterLabelGroupVisitorsArrive".Translate();
                text3 = "GroupVisitorsArrive".Translate(new object[]
                {
                    caravan.Faction.Name,
                    text4,
                    text5
                });
            }
            PawnRelationUtility.Notify_PawnsSeenByPlayer(pawns, ref label, ref text3, "LetterRelatedPawnsNeutralGroup".Translate(), true);
            Find.LetterStack.ReceiveLetter(label, text3, LetterDefOf.Good, pawns[0], null);

            if (caravan.Spawned)
            {
                Find.WorldObjects.Remove(caravan);
            }
        }

        private static bool TryConvertOnePawnToSmallTrader(List<Pawn> pawns, Faction faction, Map map, out Pawn trader)
        {
            if (faction.def.visitorTraderKinds.NullOrEmpty<TraderKindDef>())
            {
                trader = null;
                return false;
            }
            Pawn pawn = pawns.RandomElement<Pawn>();
            Lord lord = pawn.GetLord();
            pawn.mindState.wantsToTradeWithColony = true;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn, true);
            TraderKindDef traderKindDef = faction.def.visitorTraderKinds.RandomElementByWeight((TraderKindDef traderDef) => traderDef.commonality);
            pawn.trader.traderKind = traderKindDef;
            pawn.inventory.DestroyAll(DestroyMode.Vanish);
            ItemCollectionGeneratorParams parms = default(ItemCollectionGeneratorParams);
            parms.traderDef = traderKindDef;
            parms.forTile = map.Tile;
            parms.forFaction = faction;
            foreach (Thing current in ItemCollectionGeneratorDefOf.TraderStock.Worker.Generate(parms))
            {
                Pawn pawn2 = current as Pawn;
                if (pawn2 != null)
                {
                    if (pawn2.Faction != pawn.Faction)
                    {
                        pawn2.SetFaction(pawn.Faction, null);
                    }
                    IntVec3 loc = CellFinder.RandomClosewalkCellNear(pawn.Position, map, 5, null);
                    GenSpawn.Spawn(pawn2, loc, map);
                    lord.AddPawn(pawn2);
                }
                else if (!pawn.inventory.innerContainer.TryAdd(current, true))
                {
                    current.Destroy(DestroyMode.Vanish);
                }
            }
            PawnInventoryGenerator.GiveRandomFood(pawn);

            //Macrocosm.saveData.tempSmallTraders.Add(pawn);

            trader = pawn;

            return true;
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
