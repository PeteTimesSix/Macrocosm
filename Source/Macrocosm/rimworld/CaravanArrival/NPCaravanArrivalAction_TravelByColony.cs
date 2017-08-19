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
    class NPCaravanArrivalAction_TravelByColony : NPCaravanArrivalAction
    {
        public NPCaravanArrivalAction_TravelByColony() : base() { }

        public NPCaravanArrivalAction_TravelByColony(Settlement settlement) : base(settlement) { }

        public override string ReportString
        {
            get
            {
                return "NPCaravan_TravellingBy".Translate(new object[]
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

            IntVec3 travelDest;
            if (!RCellFinder.TryFindTravelDestFrom(spawnCenter, map, out travelDest))
            {
                Log.Warning("Failed to do traveler incident from " + spawnCenter + ": couldn't find anywhere for the traveler to go.");
                return;
            }
            
            string text;
            if (pawns.Count == 1)
            {
                text = "SingleTravelerPassing".Translate(new object[]
                {
                    pawns[0].story.Title.ToLower(),
                    caravan.Faction.Name,
                    pawns[0].Name
                });
                text = text.AdjustedFor(pawns[0]);
            }
            else
            {
                text = "GroupTravelersPassing".Translate(new object[]
                {
                    caravan.Faction.Name
                });
            }

            Messages.Message(text, pawns[0], MessageSound.Standard);
            LordJob_TravelAndExit lordJob = new LordJob_TravelAndExit(travelDest);
            LordMaker.MakeNewLord(caravan.Faction, lordJob, map, pawns);
            string empty = string.Empty;
            string empty2 = string.Empty;
            PawnRelationUtility.Notify_PawnsSeenByPlayer(pawns, ref empty, ref empty2, "LetterRelatedPawnsNeutralGroup".Translate(), true);
            if (!empty2.NullOrEmpty())
            {
                Find.LetterStack.ReceiveLetter(empty, empty2, LetterDefOf.Good, pawns[0], null);
            }

            if (caravan.Spawned)
            {
                Find.WorldObjects.Remove(caravan);
            }
        }

        public override void NPCSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {
            caravan.caravanData.visitedSites.Add(settlement);
            caravan.setTimeToDecision(1000);
            caravan.pather.StopDead();
            caravan.arrived = true;
        }
    }

}
