using Macrocosm.rimworld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Macrocosm.macrocosm.buildings;

namespace Macrocosm.macrocosm
{
    public class SettlementScoutState : IExposable
    {
        public Settlement settlement;

        //public int TicksLeft = 0;
        public int ActiveSpotDistance = 0;
        public int SpotDistance
        {
            get { return TicksLeft.Keys.OrderByDescending(o => o).FirstOrDefault(); }
        }
         
        Dictionary<int, int> TicksLeft = new Dictionary<int, int>();

        public void ExposeData()
        {
            Scribe_References.Look<Settlement>(ref settlement, "settlement");
            Scribe_Collections.Look<int, int>(ref TicksLeft, "TicksLeft", LookMode.Value, LookMode.Value);
            Scribe_Values.Look<int>(ref ActiveSpotDistance, "ActiveSpotDistance");
            //Scribe_Values.Look<int>(ref SpotDistance, "SpotDistance");
        }

        private const int BASEPAWN_TICK_LATENCY = 200;

        public void Update(int ticks)
        {
            //Log.Message("tick update of "+ticks);
            IEnumerable<int> keys = new List<int>(TicksLeft.Keys);

            foreach (int key in keys)
            {
                TicksLeft[key] -= ticks;
                if (TicksLeft[key] <= 0)
                    TicksLeft.Remove(key);
            }
            if (!TicksLeft.ContainsKey(1))
            {
                if (settlement.Map.mapPawns.ColonistsSpawnedCount > 0)
                    TicksLeft[1] = BASEPAWN_TICK_LATENCY;
            }
        }

        internal void UpdateSpecific(int range, int ticks)
        {
            //Log.Message(String.Format("update of range {0} for {1} ticks", range, ticks));
            if (TicksLeft.ContainsKey(range))
            {
                if (TicksLeft[range] < ticks)
                    TicksLeft[range] = ticks;
            }
            else
            {
                TicksLeft[range] = ticks;
            }
        }

        internal int TicksFor(int range, bool includeBetters)
        {
            int ticks = 0;
            if(TicksLeft.ContainsKey(range))
            {
                ticks = TicksLeft[range];
            }
            if (includeBetters)
            {
                foreach(int key in TicksLeft.Keys.Where(o => o > range))
                {
                    if (TicksLeft[key] > ticks)
                        ticks = TicksLeft[key];
                }
            }
            return ticks;
        }
    }

    public class ScoutingManager : IExposable
    {
        public List<SettlementScoutState> settlementStates = new List<SettlementScoutState>();
        private HashSet<int> tilesScouted = new HashSet<int>();
        private bool dirty = true;

        public HashSet<int> TilesNotInFOW {
            get
            {
                if (dirty)
                {
                    RegenTiles();
                }
                return tilesScouted;
            }
        }

        public ScoutingManager()
        {
            dirty = true;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look<SettlementScoutState>(ref settlementStates, "settlementStates", LookMode.Deep);
            Scribe_Collections.Look<int>(ref tilesScouted, "tilesScouted", LookMode.Value);
        }

        public SettlementScoutState GetStateForSettlement(Settlement settlement)
        {
            SettlementScoutState state = settlementStates.Where(o => o.settlement == settlement).FirstOrDefault();
            if (state == null)
            {
                state = new SettlementScoutState() { settlement = settlement };
                settlementStates.Add(state);
                state.Update(0);
            }
            return state;
        }

        public void SetDirty()
        {
            dirty = true;
        }

        public void DoTicks(int ticks)
        {
            if (dirty)
            {
                RegenTiles();
                WorldLayer_FogOfWar.Instance.SetDirty();
            }
            foreach (SettlementScoutState scoutState in settlementStates)
            {
                scoutState.Update(ticks);
            }
        }

        private IEnumerable<FactionBase> GetPlayerHomes()
        {
            return from factionBase in Find.WorldObjects.FactionBases where factionBase.Map != null && factionBase.Map.IsPlayerHome select factionBase;
        }

        private IEnumerable<WorldObject> GetPlayerOutposts()
        {
            return from worldObject in Find.WorldObjects.AllWorldObjects where worldObject is ScoutingOutpost && worldObject.Faction.IsPlayer select worldObject;
        }

        public void RegenTiles()
        {
            IEnumerable<FactionBase> playerHomes = GetPlayerHomes();
            IEnumerable<WorldObject> scoutingOutposts = GetPlayerOutposts();

            tilesScouted = new HashSet<int>();

            foreach (FactionBase playerHome in playerHomes)
            {
                if (playerHome == null)
                    Log.Message("enumerated null playerHome!");
                //Log.Message("playerHome "+playerHome.Label);
                int range = Macrocosm.saveData.ScoutingManager.SpotDistance(playerHome);

                if (range <= 0)
                    continue;
                
                Find.WorldFloodFiller.FloodFill(playerHome.Map.Tile, (int x) => true, delegate (int tile, int traversalDistance)
                {
                    if (traversalDistance > range)
                    {
                        return true;
                    }
                    tilesScouted.Add(tile);
                    return false;
                }, 2147483647);
            }

            foreach (WorldObject scoutingOutpost in scoutingOutposts)
            {
                if (scoutingOutpost == null)
                    Log.Message("enumerated null world object!");
                //Log.Message("world object included: " + scoutingOutpost.Label);

                int range = Macrocosm.saveData.ScoutingManager.SpotDistance(scoutingOutpost);

                if (range <= 0)
                    continue;

                Find.WorldFloodFiller.FloodFill(scoutingOutpost.Tile, (int x) => true, delegate (int tile, int traversalDistance)
                {
                    if (traversalDistance > range)
                    {
                        return true;
                    }
                    tilesScouted.Add(tile);
                    return false;
                }, 2147483647);
            }
            this.dirty = false;
        }

        public int SpotDistance(WorldObject obj)
        {
            if(obj is Settlement)
            {
                Settlement settlement = obj as Settlement;
                if(settlement.Faction.IsPlayer && settlement.Map != null)
                {
                    return GetStateForSettlement(settlement).SpotDistance;
                }
            }
            else if(obj is ScoutingOutpost)
            {
                return (obj as ScoutingOutpost).SpotDistance;
            }
            return 0;
        }


        public bool IsScouted(WorldObject worldObject)
        {
            int tile = worldObject.Tile;

            foreach(int scoutedTile in TilesNotInFOW)
            {
                if (tile == scoutedTile)
                    return true;
            }

            return false;
        }

        internal void UpdateFromScoutLocation(ThingWithComps parent, int range, int ticks)
        {
            //Log.Message(String.Format("update from {0} of range {1} for {2} ticks", parent.Label, range, ticks));

            Settlement settlement = Find.WorldObjects.WorldObjectAt<Settlement>(parent.Map.Tile);
            if (settlement == null || !settlement.Faction.IsPlayer)
                return;

            GetStateForSettlement(settlement).UpdateSpecific(range, ticks);
        }

        public void Tick(int currentTick)
        {
            foreach(SettlementScoutState scoutState in settlementStates)
            {
                if (scoutState.ActiveSpotDistance != scoutState.SpotDistance)
                {
                    Macrocosm.saveData.ScoutingManager.SetDirty();
                    scoutState.ActiveSpotDistance = scoutState.SpotDistance;
                }
            }
            DoTicks(1);
        }

        internal int TicksFor(ThingWithComps parent, int range)
        {
            Settlement settlement = Find.WorldObjects.WorldObjectAt<Settlement>(parent.Map.Tile);
            SettlementScoutState state = GetStateForSettlement(settlement);

            return state.TicksFor(range, true);
        }
    }
}
