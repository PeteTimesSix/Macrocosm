using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.defs
{
    [StaticConstructorOnStartup]
    static class Macrocosm_Materials
    {
        public static readonly Texture2D SkyTex = ContentFinder<Texture2D>.Get("Outpost/Sky", true);
    }
}
