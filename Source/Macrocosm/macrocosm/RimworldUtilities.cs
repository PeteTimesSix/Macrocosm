using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld.Planet;
using Macrocosm.extensions;
using Macrocosm.defs;

namespace Macrocosm.macrocosm
{
    class RimworldUtilities
    {
        public static List<Pawn> createPawns(IncidentParms parms, PawnGroupKindDef pawnGroupDef)
        {
            Map map = (Map)parms.target;
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(parms);
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupDef, defaultPawnGroupMakerParms, false).ToList<Pawn>();
            return list;
        }

        internal static void sanityCheck()
        {
            Log.Message("Method calls exist");
        }

        public static Caravan_Macrocosm RemakeCaravan(IEnumerable<Pawn> pawns, Faction faction, Settlement start, bool addToWorldPawnsIfNotAlready, StoredCaravan stored)
        {
            Caravan_Macrocosm recreation = MakeCaravan(pawns, faction, start, addToWorldPawnsIfNotAlready, stored.caravanKind);
            recreation.caravanData = stored;
            return recreation;
        }

        public static Caravan_Macrocosm MakeCaravan(IEnumerable<Pawn> pawns, Faction faction, Settlement start, bool addToWorldPawnsIfNotAlready, Caravan_Macrocosm.CaravanKind caravanKind)
        {
            List<Pawn> tmpPawns = new List<Pawn>();

            if (start.Tile < 0 && addToWorldPawnsIfNotAlready)
            {
                Log.Warning("Tried to create a caravan but chose not to spawn a caravan but pass pawns to world. This can cause bugs because pawns can be discarded.");
            }
            tmpPawns.Clear();
            tmpPawns.AddRange(pawns);

            WorldObject obj = WorldObjectMaker.MakeWorldObject(Macrocosm_WorldObjectDefOf.Caravan_Macrocosm);

            Caravan_Macrocosm caravan = (Caravan_Macrocosm)obj;
            caravan.Tile = start.Tile;
            caravan.caravanData.visitedSites.Add(start);
            caravan.SetFaction(faction);
            caravan.Name = CaravanNameGenerator.GenerateCaravanName(caravan);
            Find.WorldObjects.Add(caravan);

            for (int i = 0; i < tmpPawns.Count; i++)
            {
                Pawn pawn = tmpPawns[i];
                caravan.join(pawn, addToWorldPawnsIfNotAlready);
            }
            caravan.caravanData.caravanKind = caravanKind;
            caravan.caravanData.caravanPawns.AddRange(pawns);

            return caravan;
        }
    }
}
