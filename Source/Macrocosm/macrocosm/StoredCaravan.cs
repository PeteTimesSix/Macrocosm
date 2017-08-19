using Macrocosm.extensions;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm
{
    public class StoredCaravan : IExposable
    {
        public List<Pawn> caravanPawns = new List<Pawn>();
        public Caravan_Macrocosm.CaravanKind caravanKind = Caravan_Macrocosm.CaravanKind.Unspecified;
        public bool firstTimeGenerationDone = false;
        public Pawn traderPawn;
        public List<Settlement> visitedSites = new List<Settlement>();
        public List<Pawn> releasedPrisoners = new List<Pawn>();

        public RaidStrategyDef storedRaidStrategy = null;
        public float storedRaidPoints = -1;

        public StoredCaravan() { }

        public void ExposeData()
        {
            Scribe_Collections.Look<Pawn>(ref caravanPawns, "caravanPawns", LookMode.Reference, new object[0]);
            Scribe_Values.Look<bool>(ref firstTimeGenerationDone, "firstTimeGenerationDone");
            Scribe_References.Look<Pawn>(ref traderPawn, "traderPawn");
            Scribe_Collections.Look<Settlement>(ref visitedSites, "visitedSites", LookMode.Reference);
            Scribe_Collections.Look<Pawn>(ref releasedPrisoners, "releasedPrisoners", LookMode.Reference);
            Scribe_Defs.Look<RaidStrategyDef>(ref this.storedRaidStrategy, "raidStrategy");
            Scribe_Values.Look<float>(ref this.storedRaidPoints, "raidPoints");
        }
    }
}
