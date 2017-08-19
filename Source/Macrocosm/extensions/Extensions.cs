using Macrocosm.defs;
using Macrocosm.macrocosm;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.extensions
{
    public static class Extensions
    {
        public static List<T> EmptyList<T>()
        {
            return new List<T>();
        }

        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> enumerable)
        {
            foreach(T item in enumerable)
            {
                set.Add(item);
            }
        }

        public static float Max_X(this SimpleCurve curve)
        {
            float maxValue = float.MinValue;
            foreach(CurvePoint point in curve.AllPoints)
            {
                if (maxValue < point.x)
                    maxValue = point.x;
            }
            return maxValue;
        }

        public static float Max_Y(this SimpleCurve curve)
        {
            float maxValue = float.MinValue;
            foreach (CurvePoint point in curve.AllPoints)
            {
                if (maxValue < point.y)
                    maxValue = point.y;
            }
            return maxValue;
        }
    }
}
