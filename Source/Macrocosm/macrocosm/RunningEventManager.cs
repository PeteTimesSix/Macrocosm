using Macrocosm.macrocosm.runningEvents;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Macrocosm.macrocosm
{
    public class RunningEventManager : IExposable
    {
        public List<BaseRunningEvent> runningEvents = new List<BaseRunningEvent>();

        public void ExposeData()
        {
            Scribe_Collections.Look<BaseRunningEvent>(ref runningEvents, "runningEvents", LookMode.Deep);
        }

        internal void OnGenerateMapParent(MapParent mapParent)
        {
            //Log.Message("gen map parent");
            foreach(CloudyRunningEvent ev in runningEvents.OfType<CloudyRunningEvent>())
            {
                if (ev.TileIDs.Contains(mapParent.Tile))
                    ev.AffectMapParent(mapParent);
            }
        }

        public void Tick(int currentTick)
        {
            for(int i = runningEvents.Count - 1; i >= 0; i--)
            {
                bool remove = runningEvents[i].Tick(currentTick);
                
                if (remove)
                    runningEvents.RemoveAt(i);
            }
        }

        internal void RegisterNewEvent(BaseRunningEvent newEvent)
        {
            newEvent.InitialSetup();
            runningEvents.Add(newEvent);
        }
    }
}
