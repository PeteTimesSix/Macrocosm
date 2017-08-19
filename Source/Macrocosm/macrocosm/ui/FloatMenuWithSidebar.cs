using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.ui
{
    class FloatMenuWithSidebar : FloatMenu
    {
        public FloatMenuWithSidebar(List<FloatMenuOption> options) : base(options)
        {
        }

        public FloatMenuWithSidebar(List<FloatMenuOption> options, string title) : base(options, title)
        {
        }
        
        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            this.windowRect.width += Dialog_ScoutingOutpostManager.ASSIGNSIDEBAR_WIDTH;
            this.windowRect.height += 9999;
        }
    }
}
