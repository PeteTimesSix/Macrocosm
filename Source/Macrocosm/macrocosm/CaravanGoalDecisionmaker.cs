using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Macrocosm.extensions;
using static Macrocosm.macrocosm.SettlementSelector;
using Macrocosm.rimworld.CaravanArrival;

namespace Macrocosm.macrocosm
{
    public static class CaravanGoalDecisionmaker
    {

        public static NPCaravanArrivalAction decide(Caravan_Macrocosm caravan)
        {
            //Log.Message("making decision for caravan " + caravan.Label);
            return decide(caravan, selectTargetSettlement(caravan));
        }

        public static NPCaravanArrivalAction decide(Caravan_Macrocosm caravan, Settlement nextStop)
        {
            switch (caravan.caravanData.caravanKind)
            {
                case Caravan_Macrocosm.CaravanKind.Trader:          
                             
                    if (nextStop.Faction == caravan.Faction)
                        return new NPCaravanArrivalAction_ReturnHome(nextStop);
                    else
                        return new NPCaravanArrivalAction_TradeWithColony(nextStop);
                    
                case Caravan_Macrocosm.CaravanKind.RaidEnemy:

                    if (nextStop.Faction.HostileTo(caravan.Faction))
                        return new NPCaravanArrivalAction_RaidColony(nextStop);
                    else
                    {
                        if (nextStop.Faction == caravan.Faction)
                            return new NPCaravanArrivalAction_ReturnHome(nextStop);
                        else
                            return new NPCaravanArrivalAction_VisitColony(nextStop);
                    }

                case Caravan_Macrocosm.CaravanKind.RaidFriendly:
                    if (!nextStop.Faction.HostileTo(caravan.Faction) && nextStop.Faction != caravan.Faction)
                        return new NPCaravanArrivalAction_RaidColony(nextStop);
                    else
                    {
                        if (nextStop.Faction == caravan.Faction)
                            return new NPCaravanArrivalAction_ReturnHome(nextStop);
                        else
                            return new NPCaravanArrivalAction_RaidColony(nextStop);
                    }

                case Caravan_Macrocosm.CaravanKind.Passerby:

                    if (nextStop.Faction == caravan.Faction)
                        return new NPCaravanArrivalAction_ReturnHome(nextStop);
                    else
                        return new NPCaravanArrivalAction_TravelByColony(nextStop);

                case Caravan_Macrocosm.CaravanKind.Visitor:

                    if (nextStop.Faction == caravan.Faction)
                        return new NPCaravanArrivalAction_ReturnHome(nextStop);
                    else
                        return new NPCaravanArrivalAction_VisitColony(nextStop);

                case Caravan_Macrocosm.CaravanKind.ReturnHome:

                    return new NPCaravanArrivalAction_ReturnHome(nextStop);

                case Caravan_Macrocosm.CaravanKind.Unspecified:
                default:
                    
                    return new NPCaravanArrivalAction_ReturnHome(nextStop);
            }
        }

        public static Settlement selectTargetSettlement(Caravan_Macrocosm caravan)
        {
            Settlement nextStop;

            switch (caravan.caravanData.caravanKind)
            {
                case Caravan_Macrocosm.CaravanKind.Passerby:

                    nextStop = SettlementSelector.FindSettlementOppositeOf(caravan.Tile, caravan.caravanData.visitedSites[caravan.caravanData.visitedSites.Count - 2].Tile, caravan.Faction, SelectionCriteria.AlliedAndSelf, 45f, 0,0, caravan.caravanData.visitedSites);
                    if (nextStop == null)
                        nextStop = SettlementSelector.selectNearestBase(caravan.Tile, caravan.Faction, SettlementSelector.SelectionCriteria.AlliedAndSelf, 0, 0, caravan.caravanData.visitedSites).Where(x => !caravan.caravanData.visitedSites.Contains(x)).First();

                    return nextStop;

                case Caravan_Macrocosm.CaravanKind.Trader:
                case Caravan_Macrocosm.CaravanKind.Visitor:

                    nextStop = SettlementSelector.selectNearestBase(caravan.Tile, caravan.Faction, SettlementSelector.SelectionCriteria.AlliedAndSelf, 0, 0, caravan.caravanData.visitedSites).First();
                    return nextStop;

                case Caravan_Macrocosm.CaravanKind.RaidEnemy:
                case Caravan_Macrocosm.CaravanKind.RaidFriendly:
                case Caravan_Macrocosm.CaravanKind.ReturnHome:
                case Caravan_Macrocosm.CaravanKind.Unspecified:
                default:

                    Settlement home = caravan.caravanData.visitedSites.First();
                    List<Settlement> visits = new List<Settlement>();
                    visits.AddRange(caravan.caravanData.visitedSites);
                    visits.Remove(home);
                    nextStop = SettlementSelector.selectNearestBase(caravan.Tile, caravan.Faction, SettlementSelector.SelectionCriteria.Self, 0, 0, visits).FirstOrDefault();

                    if(nextStop == null)
                        nextStop = SettlementSelector.selectNearestBase(caravan.Tile, caravan.Faction, SettlementSelector.SelectionCriteria.AlliedAndSelf, 0, 0, visits, includePlayer: false).FirstOrDefault();

                    return nextStop;
            }
        }
    }
}
