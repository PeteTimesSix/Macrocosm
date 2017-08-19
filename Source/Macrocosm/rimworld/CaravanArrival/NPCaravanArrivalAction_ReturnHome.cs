using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Macrocosm.extensions;
using Verse;
using RimWorld;

namespace Macrocosm.rimworld.CaravanArrival
{
    class NPCaravanArrivalAction_ReturnHome : NPCaravanArrivalAction
    {

        public NPCaravanArrivalAction_ReturnHome() : base() { }

        public NPCaravanArrivalAction_ReturnHome(Settlement settlement) : base(settlement) { }

        public override string ReportString
        {
            get
            {
                return "NPCaravan_ComingHome".Translate(new object[]
                {
                    this.settlement.Label
                });
            }
        }

        public override void PlayerSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {
            throw new NotSupportedException("NPC Caravan attempted to return home to player colony.");
        }

        public override void NPCSettlementArrivalAction(Settlement settlement, Caravan_Macrocosm caravan)
        {
            foreach (Pawn pawn in caravan.caravanData.releasedPrisoners)
            {
                if (!pawn.Dead)
                {
                    float num = 15f;
                    if (PawnUtility.IsFactionLeader(pawn))
                    {
                        num += 50f;
                    }
                    Messages.Message("MessagePawnExitMapRelationsGain".Translate(new object[]
                    {
                    pawn.LabelShort,
                    pawn.Faction.Name,
                    num.ToString("F0")
                    }), MessageSound.Benefit);
                    pawn.Faction.AffectGoodwillWith(Faction.OfPlayer, num);
                }
            }

            foreach (Pawn pawn in caravan.pawns)
            {
                if(pawn.Faction.IsPlayer && !caravan.Faction.IsPlayer)
                {
                    caravan.Faction.kidnapped.KidnapPawn(pawn, Macrocosm.saveData.findKidnapperFor(pawn));
                }
                
                if(pawn.Spawned)
                {
                    pawn.DeSpawn();
                }
            }
            
            if (caravan.Spawned)
            {
                Find.WorldObjects.Remove(caravan);
            }
        }
    }
}
