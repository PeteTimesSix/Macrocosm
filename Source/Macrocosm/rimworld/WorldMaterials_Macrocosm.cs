using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.rimworld 
{
    [StaticConstructorOnStartup]
    public static class WorldMaterials_Macrocosm
    {
        public static readonly Material Mat_TestDraw;

        public static readonly Material Mat_FoWNegative;
        public static readonly Material Mat_ToxicFallout;
        public static readonly Material Mat_VolcanicWinter;
        

        static WorldMaterials_Macrocosm()
        {
            Mat_TestDraw = MaterialPool.MatFrom("World/TestDraw", ShaderDatabase.WorldOverlayAdditive, 3600);

            Mat_FoWNegative = MaterialPool.MatFrom("World/FoWNegative", ShaderDatabase.WorldOverlayAdditive, 3590);
            Mat_ToxicFallout = MaterialPool.MatFrom("World/ToxicFalloutTile", ShaderDatabase.WorldOverlayTransparent, 3575);
            Mat_VolcanicWinter = MaterialPool.MatFrom("World/VolcanicWinterTile", ShaderDatabase.WorldOverlayTransparent, 3576);
        }
    }
}
