using Macrocosm.macrocosm;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.rimworld
{

    public class WorldLayer_FogOfWar : WorldLayer
    {
        //private int playerMapObjectCount = -1;
        
        private static WorldLayer_FogOfWar instance;
        public static WorldLayer_FogOfWar Instance
        {
            get { return instance; }
        }

        public WorldLayer_FogOfWar()
        {
            instance = this;
        }
        
        [DebuggerHidden]
        public override IEnumerable Regenerate()
        {
            foreach (object result in base.Regenerate())
            {
                yield return result;
            }
            //Rand.PushState();
            //Rand.Seed = Find.World.info.Seed;
            WorldGrid grid = Find.WorldGrid;
            List<Vector3> verts = new List<Vector3>();

            if (grid == null || Macrocosm.saveData == null || Macrocosm.saveData.ScoutingManager == null)
                yield break;

            //while (i < tilesCount)
            foreach (int tile_ID in Macrocosm.saveData.ScoutingManager.TilesNotInFOW)            
            {
                Tile tile = grid[tile_ID];
                Material mat = WorldMaterials_Macrocosm.Mat_FoWNegative;
                LayerSubMesh subMesh = base.GetSubMesh(mat);
                Vector3 pos = grid.GetTileCenter(tile_ID);
                Vector3 origPos = pos;
                float length = pos.magnitude;
                pos += Rand.PointOnSphere * grid.averageTileSize;
                pos = pos.normalized * length;
                verts.Clear();
                Find.WorldGrid.GetTileVertices(tile_ID, verts);
                 
                int startVertIndex = subMesh.verts.Count;
                int j = 0;
                int count = verts.Count;
                while (j < count)
                {
                    subMesh.verts.Add(verts[j] + verts[j].normalized * 0.012f);
                    subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count, j) + Vector2.one) / 2f);
                    if (j < count - 2)
                    {
                        subMesh.tris.Add(startVertIndex + j + 2); 
                        subMesh.tris.Add(startVertIndex + j + 1);
                        subMesh.tris.Add(startVertIndex);
                    }
                    j++;
                }
            }
            //Rand.PopState();
            base.FinalizeMesh(MeshParts.All, true);
        }
    }
}
