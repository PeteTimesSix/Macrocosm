using Macrocosm.extensions;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm.runningEvents
{
    public class CloudTile : IExposable
    {
        public float distFraction;
        public int tile;
        public int dist;

        public override bool Equals(Object obj)
        {
            if (!(obj is CloudTile))
                return false;
            return tile == ((CloudTile)obj).tile;
        }

        public override int GetHashCode()
        {
            return tile.GetHashCode();
        }

        public void ExposeData()
        {
            Scribe_Values.Look<float>(ref distFraction, "distFraction");
            Scribe_Values.Look<int>(ref tile, "tile");
            Scribe_Values.Look<int>(ref dist, "dist");
        }
    }

    public abstract class BaseRunningEvent : IExposable
    {
        public int duration = 600;


        public BaseRunningEvent()
        {

        }

        public BaseRunningEvent(int duration)
        {
            this.duration = duration;
        }

        public abstract bool Tick(int curTick);

        public virtual void InitialSetup()
        {
        }

        public virtual void FinaliseEvent()
        {

        }

        protected void SendStandardLetter(IncidentDef incident)
        {
            if (incident.letterLabel.NullOrEmpty() || incident.letterText.NullOrEmpty())
            {
                Log.Error("Sending standard incident letter with no label or text.");
            }
            Find.LetterStack.ReceiveLetter(incident.letterLabel, incident.letterText, incident.letterDef, null);
        }

        public virtual void ExposeData()
        {
            Scribe_Values.Look<int>(ref duration, "duration");
        }
    }
}
