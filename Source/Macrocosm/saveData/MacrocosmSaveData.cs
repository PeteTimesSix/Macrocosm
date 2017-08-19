using HugsLib.Utils;
using Macrocosm.macrocosm;
using Macrocosm.macrocosm.runningEvents;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.saveData
{
    public class MacrocosmSaveData : UtilityWorldObject
    {
        public struct CaravanAtSettlement : IExposable
        {
            public StoredCaravan caravan;
            public Settlement settlement;

            public void ExposeData()
            {
                Scribe_Deep.Look<StoredCaravan>(ref caravan, "storedCaravan", null);
                Scribe_References.Look<Settlement>(ref settlement, "settlement");
            }
        }

        public Dictionary<Pawn, Pawn> kidnapeesKidnappers = new Dictionary<Pawn, Pawn>();
        public List<CaravanAtSettlement> storedCaravans = new List<CaravanAtSettlement>();

        private ScoutingManager scoutingManager;
        public ScoutingManager ScoutingManager { get { if (scoutingManager == null) scoutingManager = new ScoutingManager(); return scoutingManager; } }
        private RunningEventManager runningEventManager;
        public RunningEventManager RunningEventManager { get { if (runningEventManager == null) runningEventManager = new RunningEventManager(); return runningEventManager; } }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn, Pawn>(ref kidnapeesKidnappers, "kidnapeesKidnappers", LookMode.Reference, LookMode.Reference);
            Scribe_Collections.Look<CaravanAtSettlement>(ref storedCaravans, "storedCaravans", LookMode.Deep);

            Scribe_Deep.Look<ScoutingManager>(ref scoutingManager, "scoutingManager");
            Scribe_Deep.Look<RunningEventManager>(ref runningEventManager, "runningEventManager");
        }

        internal void setKidnapperFor(Pawn kidnapee, Pawn kidnapper)
        {
            kidnapeesKidnappers.Add(kidnapee, kidnapper);
        }

        internal Pawn findKidnapperFor(Pawn kidnapee)
        {
            Pawn kidnapper;
            kidnapeesKidnappers.TryGetValue(kidnapee, out kidnapper);
            return kidnapper;
        }

        internal void finishKidnapping(Pawn kidnapee)
        {
            kidnapeesKidnappers.Remove(kidnapee);
        }

        internal void cleanupFor(Pawn pawn)
        {
            kidnapeesKidnappers.Remove(pawn);
            var myKey = kidnapeesKidnappers.FirstOrDefault(x => x.Value == pawn).Key;
            if (myKey != null)
                kidnapeesKidnappers.Remove(myKey);
        }

        internal void saveStoredCaravan(StoredCaravan caravanData, Settlement location)
        {
            if(caravanData.caravanPawns.Count > 0)
            {
                storedCaravans.Add(new CaravanAtSettlement() { caravan = caravanData, settlement = location });
            }
        }

        internal StoredCaravan findMatchingStoredCaravan(Pawn pawn)
        {
            foreach(CaravanAtSettlement caravan in storedCaravans)
            {
                if (caravan.caravan.caravanPawns.Contains(pawn))
                {
                    return caravan.caravan;
                }
            }
            return null;
        }

        internal void FindAndRemoveStoredCaravan(StoredCaravan storedCaravan)
        {
            CaravanAtSettlement? found = null;
            foreach (CaravanAtSettlement caravan in storedCaravans)
            {
                //Log.Message("caravan!");
                if (caravan.caravan.Equals(storedCaravan))
                {
                    found = caravan;
                }
            }
            if (found != null)
                storedCaravans.Remove(found.Value);
        }



        public bool InToxicCloud(int tile)
        {
            foreach(RunningEvent_ToxicFallout ev in RunningEventManager.runningEvents.OfType<RunningEvent_ToxicFallout>())
            {
                if (ev.TileIDs.Contains(tile))
                    return true;
            }
            return false;
        }
    }
}
