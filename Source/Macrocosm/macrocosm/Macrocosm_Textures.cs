using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.macrocosm
{
    [StaticConstructorOnStartup]
    public static class Macrocosm_Textures
    {
        public static readonly Texture2D ManageOutpostCommand = ContentFinder<Texture2D>.Get("UI/ManageOutpost", true);
        public static readonly Texture2D FoundNewOutpostCommand = ContentFinder<Texture2D>.Get("UI/FoundNewOutpost", true);
    }
}
