using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.defs
{
    public class OutpostBuildingTagDef : Def
    {
        public string needLabel;
        
        [Unsaved]
        public Texture2D uiIcon = BaseContent.BadTex;
        
        public string uiIconPath;
        
        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!this.uiIconPath.NullOrEmpty())
                {
                    this.uiIcon = ContentFinder<Texture2D>.Get(this.uiIconPath, true);
                }
            });
        }
    }
}
