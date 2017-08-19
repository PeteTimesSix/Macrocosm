using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.macrocosm.ui
{
    class FloatMenuOptionWithSidebar : FloatMenuOption
    {
        private float sidebarWidth;
        private float sidebarHeight;
        private Func<Rect, bool> sidebarOnGUI;

        public FloatMenuOptionWithSidebar(string label, Action action, MenuOptionPriority priority = MenuOptionPriority.Default, Action mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null, float sidebarWidth = 0f, float sidebarHeight = 0f, Func<Rect, bool> sidebarOnGUI = null) : base(label, action, priority, mouseoverGuiAction, revalidateClickTarget, extraPartWidth, extraPartOnGUI, revalidateWorldClickTarget)
        {
            this.sidebarWidth = sidebarWidth;
            this.sidebarHeight = sidebarHeight;
            this.sidebarOnGUI = sidebarOnGUI;
        } 

        public override bool DoGUI(Rect rect, bool colonistOrdering)
        {
            bool retval = base.DoGUI(rect, colonistOrdering);

            if (Mouse.IsOver(rect))
            {
                if(sidebarOnGUI != null)
                {
                    Rect sidebarRect = new Rect(rect.x + rect.width, rect.y, sidebarWidth, sidebarHeight);
                    Widgets.DrawWindowBackground(sidebarRect);
                    sidebarRect = sidebarRect.ContractedBy(2f);
                    sidebarOnGUI(sidebarRect);
                }
            }

            return retval;
        }
    }
}
