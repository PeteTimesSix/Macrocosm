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
    class WorldLayer_TestDraw : WorldLayer
    {
        public HashSet<int> tiles = new HashSet<int>();
        public bool dirty = true;

        private static WorldLayer_TestDraw instance;
        public static WorldLayer_TestDraw Instance
        {
            get { return instance; }
        }

        public WorldLayer_TestDraw()
        {
            instance = this;
        }

        public override bool ShouldRegenerate
        {
            get
            {
                return dirty;
            }
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

            //while (i < tilesCount)
            foreach (int tile_ID in tiles)
            {
                Tile tile = grid[tile_ID];
                Material mat = WorldMaterials_Macrocosm.Mat_TestDraw;
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
