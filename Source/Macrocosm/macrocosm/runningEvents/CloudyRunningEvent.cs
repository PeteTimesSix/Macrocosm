using Macrocosm.extensions;
using Macrocosm.rimworld;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.runningEvents
{
    public abstract class CloudyRunningEvent : BaseRunningEvent
    {
        protected List<CloudTile> toBeAffectedTiles = new List<CloudTile>();
        protected List<CloudTile> affectedTiles = new List<CloudTile>();

        public IEnumerable<int> TileIDs
        {
            get
            {
                foreach (CloudTile cloudTile in affectedTiles)
                {
                    yield return cloudTile.tile;
                }
            }
        }

        internal virtual int getTicksPerTilesAppearing() { return 600; }
        internal virtual int getTicksPerTilesDisappearing() { return 600; }

        internal virtual int getTilesAppearingPerLoop() { return 5; }
        internal virtual int getTilesDisappearingPerLoop() { return 5; }
        
        public abstract ICloudLayer CloudLayer { get; }
        public abstract GameConditionDef ConditionDef { get; }
        public abstract IncidentDef Incident { get; }

        public abstract SimpleCurve ThicknessCurve { get; }
        public abstract SimpleCurve DistanceCurve { get; }

        public CloudyRunningEvent()
        {
        }

        public CloudyRunningEvent(int duration, int targetTile)
        {
            this.duration = duration;
            int siteTile;
            bool found = TileFinder.TryFindPassableTileWithTraversalDistance(targetTile, 15, 45, out siteTile, (int x) => !Find.WorldObjects.AnyWorldObjectAt(x) && TileFinder.IsValidTileForNewSettlement(x, null), false);

            if (found)
                GenerateCloud(siteTile, targetTile);
            else
                Log.Error("Could not find any tile to start a toxic fallout event from!");
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<CloudTile>(ref toBeAffectedTiles, "toBeAffectedTiles", LookMode.Deep);
            Scribe_Collections.Look<CloudTile>(ref affectedTiles, "affectedTiles", LookMode.Deep);
            Scribe_Values.Look<int>(ref ticksSinceLastTile, "ticksSinceLastTile");
        }

        private int ticksSinceLastTile = 0;
        /// <returns>True if event should be removed</returns>
        public override bool Tick(int currentTick)
        {
            ticksSinceLastTile++;
            if (ticksSinceLastTile > int.MaxValue / 2)
                ticksSinceLastTile = int.MaxValue / 2;

            if ((ticksSinceLastTile > getTicksPerTilesAppearing() || affectedTiles.Count == 0) && toBeAffectedTiles.Count > 0)
            {
                int loopCount = getTilesAppearingPerLoop();
                if (loopCount > toBeAffectedTiles.Count)
                    loopCount = toBeAffectedTiles.Count;
                for (int i = 0; i < loopCount; i++)
                {
                    AffectNextTile();
                }
                ticksSinceLastTile = 0;

                //flip the list around, so we start removing at the beggining
                if (toBeAffectedTiles.Count == 0)
                    affectedTiles.Reverse();
            }

            if (toBeAffectedTiles.Count == 0 && duration > 0)
            {
                duration--;
            }

            if (duration <= 0)
            {
                if (ticksSinceLastTile > getTicksPerTilesDisappearing() && affectedTiles.Count > 0)
                {
                    int loopCount = getTilesDisappearingPerLoop();
                    if (loopCount > affectedTiles.Count)
                        loopCount = affectedTiles.Count;
                    for (int i = 0; i < loopCount; i++)
                    {
                        RemoveNextTile();
                    }
                    ticksSinceLastTile = 0;
                }

                if (affectedTiles.Count == 0)
                {
                    FinaliseEvent();
                    return true;
                }
            }
            return false;
        }

        public virtual void AffectNextTile()
        {
            if (toBeAffectedTiles.Count > 0)
            {
                CloudTile tile = toBeAffectedTiles.Last();
                affectedTiles.Add(tile);
                toBeAffectedTiles.Remove(tile);

                MapParent mapParent = Find.WorldObjects.MapParentAt(tile.tile);

                if (mapParent != null)
                    AffectMapParent(mapParent);

                CloudLayer.SetDirty();
            }
        }

        public virtual void RemoveNextTile()
        {
            if (affectedTiles.Count > 0)
            {
                CloudTile tile = affectedTiles.Last();
                affectedTiles.Remove(tile);

                MapParent mapParent = Find.WorldObjects.MapParentAt(tile.tile);

                if (mapParent != null)
                    UnaffectMapParent(mapParent);

                CloudLayer.SetDirty();
            }
        }

        public virtual void AffectMapParent(MapParent mapParent)
        {
            //Log.Message("affect map parent");
            if (mapParent.Map != null)
            {
                GameCondition cond = GameConditionMaker.MakeCondition(ConditionDef, 900000000, 0);
                mapParent.Map.gameConditionManager.RegisterCondition(cond);

                if(mapParent.Faction != null && mapParent.Faction.IsPlayer)
                    SendStandardLetter(Incident);
            }
        }

        public virtual void UnaffectMapParent(MapParent mapParent)
        {
            if (mapParent.Map != null)
            {
                GameCondition cond = mapParent.Map.gameConditionManager.ActiveConditions.Where(o => o.def == ConditionDef).FirstOrDefault();
                if (cond != null)
                {
                    cond.End();
                }
            }
        }

        private void GenerateCloud(int originTile, int destinationTile)
        {
            IEnumerable<int> directPathTiles = GenerateDirectPath(originTile, destinationTile);
            IEnumerable<CloudTile> cloudEnvelope = GenerateCloudEnvelope(directPathTiles, ThicknessCurve, ThicknessCurve.Max_X() * DistanceCurve.Max_Y(), originTile, destinationTile, DistanceCurve);

            this.toBeAffectedTiles.AddRange(cloudEnvelope);

            WorldGrid grid = Find.WorldGrid;
            toBeAffectedTiles = this.toBeAffectedTiles.OrderBy(o => grid.ApproxDistanceInTiles(originTile, o.tile)).ToList();

            this.toBeAffectedTiles.Reverse();

            //Log.Message(String.Format("Generated {0} tiles in toxic cloud", toBeAffectedTiles.Count));
        }

        public static IEnumerable<CloudTile> GenerateCloudEnvelope(IEnumerable<int> tilesToWrap, SimpleCurve thicknessCurve, float maxThickness, int startTile, int finalTile, SimpleCurve distanceCurve)
        {
            HashSet<CloudTile> tilesInCloud = new HashSet<CloudTile>();
            KeyedPriorityQueue<CloudTile> candidateTiles = new KeyedPriorityQueue<CloudTile>();

            List<int> neighbours = new List<int>();
            WorldGrid grid = Find.WorldGrid;

            float distanceBetweenEndPoints = grid.ApproxDistanceInTiles(startTile, finalTile);

            float maxThicknessNotMulted = thicknessCurve.Max_X();

            foreach (int tile in tilesToWrap)
            {
                float distFraction = grid.ApproxDistanceInTiles(startTile, tile) / distanceBetweenEndPoints;
                candidateTiles.Push(new CloudTile() { tile = tile, dist = 0, distFraction = distFraction }, 0);
            }

            int loopProtector = 0;

            while (candidateTiles.Count > 0)
            {
                loopProtector++;

                if (loopProtector > 99999)
                {
                    Log.Error("Too many loops in cloud-envelope generation!");
                    break;
                }

                float dist;
                CloudTile candidate = candidateTiles.Pop(out dist);

                float maxThicknessHere = distanceCurve.Evaluate(candidate.distFraction) * maxThicknessNotMulted;

                //dist *= distanceCurve.Evaluate(candidate.distFraction);

                if (dist > maxThicknessHere)
                {
                    //Log.Warning("reached max thickness:" + dist); 
                    continue;
                }

                if (tilesInCloud.Contains(candidate))
                    continue;

                //Find.WorldDebugDrawer.FlashTile(candidate.tile, (float)10f, String.Format("{0}", candidate.distFraction));

                tilesInCloud.Add(candidate);

                neighbours.Clear();
                Find.WorldGrid.GetTileNeighbors(candidate.tile, neighbours);
                foreach (int neighbour in neighbours)
                {
                    CloudTile nTile = new CloudTile() { tile = neighbour, dist = (int)dist + 1, distFraction = candidate.distFraction };
                    CloudTile duplicate;
                    bool hasDuplicate = candidateTiles.ContainsLimited(nTile, dist, out duplicate);
                    if (hasDuplicate)
                    {
                        duplicate.distFraction = (duplicate.distFraction + nTile.distFraction) / 2;
                    }
                    else
                    {
                        candidateTiles.Push(nTile, dist + 1);
                    }
                }
            }

            List<CloudTile> retList = new List<CloudTile>();

            foreach (CloudTile tile in tilesInCloud)
            {
                float thicknessOddsHere = thicknessCurve.Evaluate(tile.dist / distanceCurve.Evaluate(tile.distFraction));
                //Find.WorldDebugDrawer.FlashTile(tile.tile, (float)10f, String.Format("{0}", thicknessOddsHere));
                if (Rand.Chance(thicknessOddsHere))
                    retList.Add(tile);
            }

            return retList;

            //if (Rand.RangeInclusive(0, 1) > thicknessCurve.Evaluate((dist - minThickness) / (maxThickness - minThickness)))
        }

        public static IEnumerable<int> GenerateDirectPath(int startTile, int destTile)
        {
            List<int> retList = new List<int>();

            WorldGrid grid = Find.WorldGrid;

            KeyedPriorityQueue<int> candidateTiles = new KeyedPriorityQueue<int>();
            List<int> neighbours = new List<int>();

            candidateTiles.Push(startTile, grid.ApproxDistanceInTiles(startTile, destTile));

            retList.Add(startTile);

            int loopProtector = 0;

            while (candidateTiles.Count > 0)
            {
                loopProtector++;

                if (loopProtector > 99999)
                {
                    Log.Error("Too many loops in direct-path generation!");
                    retList.AddRange(candidateTiles.Values);
                    return retList;
                }

                float dist;
                int tile = candidateTiles.Pop(out dist);
                retList.Add(tile);

                if (tile == destTile)
                    break;

                neighbours.Clear();
                Find.WorldGrid.GetTileNeighbors(tile, neighbours);

                foreach (int neighbour in neighbours)
                {
                    float n_dist = grid.ApproxDistanceInTiles(neighbour, destTile);

                    if ((n_dist < dist) && !candidateTiles.Contains(neighbour))
                        candidateTiles.Push(neighbour, n_dist);
                }
            }
            //retList.AddRange(candidateTiles.Values);
            return retList;
        }

    }
}
