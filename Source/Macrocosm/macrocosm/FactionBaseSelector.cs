using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Macrocosm.extensions;

namespace Macrocosm.macrocosm
{
    static class SettlementSelector
    {
        public enum SelectionCriteria { Hostile, Allied, AlliedAndSelf, Self, Player }

        
        private static IEnumerable<Settlement> fitCandidates(int startTile, Faction faction, SelectionCriteria selection, List<Settlement> exclusions = null, bool includePlayer = true)
        {
            if (exclusions == null)
                exclusions = Extensions.EmptyList<Settlement>();

            List<Settlement> bases = Find.WorldObjects.Settlements;
            IEnumerable<Settlement> candidates;

            switch (selection)
            {
                case SelectionCriteria.Hostile:
                    candidates = bases.Where(settlement => settlement.Tile != startTile && settlement.Faction.HostileTo(faction) && (includePlayer || !settlement.Faction.IsPlayer) && !exclusions.Contains(settlement));
                    break;
                case SelectionCriteria.Allied:
                    candidates = bases.Where(settlement => settlement.Tile != startTile && !settlement.Faction.HostileTo(faction) && !settlement.Faction.Equals(faction) && (includePlayer || !settlement.Faction.IsPlayer) && !exclusions.Contains(settlement));
                    break;
                case SelectionCriteria.AlliedAndSelf:
                    candidates = bases.Where(settlement => settlement.Tile != startTile && !settlement.Faction.HostileTo(faction) && (includePlayer || !settlement.Faction.IsPlayer) && !exclusions.Contains(settlement));
                    break;
                case SelectionCriteria.Self:
                    candidates = bases.Where(settlement => settlement.Tile != startTile && settlement.Faction.Equals(faction) && (includePlayer || !settlement.Faction.IsPlayer) && !exclusions.Contains(settlement));
                    break;
                case SelectionCriteria.Player:
                    candidates = bases.Where(settlement => settlement.Tile != startTile && settlement.Faction.IsPlayer && (includePlayer || !settlement.Faction.IsPlayer) && !exclusions.Contains(settlement)); //well that bit dont make much sense but there you go
                    break;
                default:
                    candidates = bases;
                    break;
            }
            foreach(Settlement fBase in candidates)
            {
                yield return fBase;
            }
        }

        public static IEnumerable<Settlement> selectNearestBase(Settlement start, Faction faction, SelectionCriteria selection, int minSkips = 0, int maxSkips = 0, List<Settlement> exclusions = null, bool includePlayer = true)
        {
            return selectNearestBase(start.Tile, faction, selection, minSkips, maxSkips, exclusions, includePlayer);
        }

        public static IEnumerable<Settlement> selectNearestBase(int startTile, Faction faction, SelectionCriteria selection, int minSkips = 0, int maxSkips = 0, List<Settlement> exclusions = null, bool includePlayer = true)
        {
            IEnumerable<Settlement> candidates = fitCandidates(startTile, faction, selection, exclusions, includePlayer);
            return selectNearestBase(startTile, candidates, minSkips, maxSkips);
        }


        private static IEnumerable<Settlement> selectNearestBase(int startTile, IEnumerable<Settlement> candidates, int minSkips = 0, int maxSkips = 0)
        {
            IEnumerable<Settlement> nearest = candidates.OrderBy(candidate => Find.WorldGrid.ApproxDistanceInTiles(startTile, candidate.Tile));

            int skips = Rand.RangeInclusive(minSkips, maxSkips);
            //Log.Message("Skipped nearest: " + skips);

            Settlement output = null;

            int i = 0;
            foreach (FactionBase factionBase in nearest)
            {
                output = factionBase;
                i++;
                if (i > skips)
                {
                    yield return output;
                }
            }
            //make sure at least one is always returned
            if (i <= skips)
                yield return output;
        }

        public static Settlement FindSettlementOppositeOf(int startTile, int distantTile, Faction faction, SelectionCriteria criteria, float maxAngle, int minSkips = 0, int maxSkips = 0, List<Settlement> exclusions = null, bool includePlayer = false)
        {
            WorldGrid grid = Find.WorldGrid;

            IEnumerable <Settlement> nearest = fitCandidates(startTile, faction, criteria, exclusions, includePlayer).OrderBy(candidate => grid.ApproxDistanceInTiles(startTile, candidate.Tile));

            int skips = Rand.RangeInclusive(minSkips, maxSkips);
            int i = 0;
            Settlement output = null;

            foreach (Settlement candidate in nearest)
            {
                double AtoB = grid.ApproxDistanceInTiles(distantTile, startTile);
                double BtoC = grid.ApproxDistanceInTiles(startTile, candidate.Tile);
                double CtoA = grid.ApproxDistanceInTiles(candidate.Tile, distantTile);
                double angleAtB = Math.Acos(((AtoB * AtoB) + (BtoC * BtoC) - (CtoA * CtoA)) / (2 * AtoB * BtoC)) * (180/Math.PI);

                //Log.Message("angle between " + Find.WorldObjects.SettlementAt(originTile).Label + " and " + candidate.Label + " is " + angleAtB+ " inverse "+ (180f - angleAtB));

                if ((180f - angleAtB) < maxAngle)
                {
                    output = candidate;
                    i++;
                    if (i > skips)
                        break;
                }
            }
            
            return output;
        }
    }
}
