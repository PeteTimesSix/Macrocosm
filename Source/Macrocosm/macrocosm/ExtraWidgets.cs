using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.macrocosm
{
    public static class ExtraWidgets
    {
        public static void DrawLineHorizontalWindowy(float x, float y, float width)
        {
            Rect position = new Rect(x, y, width, 1f);
            ColorInt colorInt2 = new ColorInt(97, 108, 122);
            GUI.color = colorInt2.ToColor;
            GUI.DrawTexture(position, BaseContent.WhiteTex);
        }

        public static void DrawLineVerticalWindowy(float x, float y, float height)
        {
            Rect position = new Rect(x, y, 1f, height);
            ColorInt colorInt2 = new ColorInt(97, 108, 122);
            GUI.color = colorInt2.ToColor;
            GUI.DrawTexture(position, BaseContent.WhiteTex);
        }

        public static void DrawColonist(Rect rect, Pawn colonist)
        {
            GUI.color = Color.white;
            Rect rect2 = rect.ContractedBy(-2f);
            GUI.DrawTexture(rect, PortraitsCache.Get(colonist, new Vector2(rect.width, rect.height), Vector2.zero, 1.2f));
            GUI.color = Color.white;
        }

        internal static void DrawItem(Rect rect, Thing thing)
        {
            var iconTex = thing.def.uiIcon;
            Color color = thing.DrawColor;
          
            Texture resolvedIcon;
            if (!thing.def.uiIconPath.NullOrEmpty())
            {
                resolvedIcon = thing.def.uiIcon;
            }
            else
            {
                resolvedIcon = thing.Graphic.ExtractInnerGraphicFor(thing).MatSingle.mainTexture;
            }
            GUI.color = thing.DrawColor;
            GUI.DrawTexture(rect, resolvedIcon);
            GUI.color = Color.white;
        }
    }
}
