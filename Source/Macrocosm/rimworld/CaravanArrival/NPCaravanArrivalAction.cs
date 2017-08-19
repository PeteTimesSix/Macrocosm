using Macrocosm.extensions;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.rimworld.CaravanArrival
{
    public abstract class NPCaravanArrivalAction : CaravanArrivalAction
    {
        public Settlement settlement;

        public NPCaravanArrivalAction()
        {
        }

        public NPCaravanArrivalAction(Settlement settlement)
        {
            this.settlement = settlement;
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Settlement>(ref this.settlement, "settlement", false);
        }

        public override bool ShouldFail
        {
            get
            {
                return base.ShouldFail || this.settlement == null || !this.settlement.Spawned;
            }
        }

        public override void Arrived(Caravan caravan)
        {
            if (!(caravan is Caravan_Macrocosm))
            {
                Log.Error(this.GetType().Name + " on a regular caravan!");
            }
            else
            {
                if (settlement.Faction.IsPlayer)
                {
                    if (settlement.Map != null && settlement.Map.mapPawns.ColonistCount > 0)
                    {
                        PlayerSettlementArrivalAction(settlement, caravan as Caravan_Macrocosm);
                    }
                    else
                    {
                        DeadPlayerSettlementArrivalAction(settlement, caravan as Caravan_Macrocosm);
                    }
                }
                else
                {
                    NPCSettlementArrivalAction(settlement, caravan as Caravan_Macrocosm);
                }
            }
        }

        public abstract void PlayerSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan);

        public virtual void DeadPlayerSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {
            caravan.caravanData.visitedSites.Add(settlement);
            caravan.setTimeToDecision(1000);
            caravan.pather.StopDead();
            caravan.arrived = true;
        }

        public abstract void NPCSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan);
    }
}
