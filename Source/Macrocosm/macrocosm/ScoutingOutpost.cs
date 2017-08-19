using Macrocosm.rimworld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Macrocosm.defs;
using RimWorld;
using Macrocosm.macrocosm.ui;
using Harmony;

namespace Macrocosm.macrocosm
{
    public class ScoutingOutpost : WorldObject, ILoadReferenceable, IThingHolder
    {

        public ThingOwner<Thing> innerContainer;

        private Material cachedMat;
        public override Material Material
        {
            get
            {
                if (this.cachedMat == null) 
                { 
                    Color color;
                    if (base.Faction != null) 
                    {
                        color = base.Faction.Color;
                    }
                    else
                    { 
                        color = Color.white;
                    }
                    this.cachedMat = MaterialPool.MatFrom(this.def.texture, ShaderDatabase.WorldOverlayTransparentLit, color, WorldMaterials.WorldObjectRenderQueue);
                }
                return this.cachedMat;
            } 
        }


        public List<OutpostBuildingDef> buildings = new List<OutpostBuildingDef>();

        private int spotDistanceCached = 0;
        public int PotentialSpotDistance
        {
            get
            {
                return spotDistanceCached;
            }
        }

        public int SpotDistance
        {
            get
            {
                if (IsManned && IsConnected)
                    return spotDistanceCached;
                else
                    return 0;
            }
        }

        private int commsRangeCached = 0;
        private bool isConnectedCached = false;

        public int CommsRange
        {
            get
            {
                return commsRangeCached;
            }
        }

        public bool IsConnected
        {
            get
            {
                return isConnectedCached;
            }
        }

        private void Recalc()
        {
            int rangeNow = SpotDistance;

            int commRange = 0;
            int spotRange = 0;
            if (innerContainer.FirstOrDefault() != null)
                spotRange = 1;
            foreach (OutpostBuildingDef building in buildings.Where(o => o.statBases != null))
            {
                if (!building.isSatisfied(this, this.innerContainer.FirstOrDefault() as Pawn))
                    continue;
                foreach (StatModifier stat in building.statBases.Where(o => o.stat == Macrocosm_StatDefOf.CommunicationTileRange))
                {
                    if (stat.value > commRange)
                        commRange = (int)stat.value;
                }
                foreach (StatModifier stat in building.statBases.Where(o => o.stat == Macrocosm_StatDefOf.ScoutingTileRange))
                {
                    if (stat.value > spotRange)
                        spotRange = (int)stat.value;
                }
            }
            spotDistanceCached = spotRange;
            commsRangeCached = commRange;

            isConnectedCached = false;
            foreach (WorldObject worldObj in Find.WorldObjects.FactionBases.Where(o => o.Faction.IsPlayer))
            {
                if (Find.WorldGrid.TraversalDistanceBetween(this.Tile, worldObj.Tile) <= commRange)
                {
                    isConnectedCached = true;
                    break;
                }
            }

            if (SpotDistance != rangeNow)
                Macrocosm.saveData.ScoutingManager.SetDirty();
        }

        public bool IsManned
        {
            get
            {
                return innerContainer.FirstOrDefault() != null;
            }
        }

        public Pawn CurrentPawn { get { return innerContainer.FirstOrDefault() as Pawn; } }

        public ScoutingOutpost() : base()
        {
            innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<OutpostBuildingDef>(ref buildings, "buildings", LookMode.Def);
            Scribe_Deep.Look<ThingOwner<Thing>>(ref this.innerContainer, "innerContainer", new object[]
            {
                this
            });
        }

        public override void PostAdd()
        {
            Macrocosm.saveData.ScoutingManager.SetDirty();
        }

        public bool CanConstruct(OutpostBuildingDef def, Caravan caravan, bool sendMessages)
        {
            if(def.usedSpot != null && def.usedSpot.positions != null && def.usedSpot.positions.Count > 0)
            {
                if (def.usedSpot.positions.Count <= buildings.Where(o => o.usedSpot != null && o.usedSpot.Equals(def.usedSpot)).Count())
                {
                    if(sendMessages)
                        Messages.Message("Message_CannotConstruct_NoSlot".Translate(new object[]
                        {
                        def.label
                        }), MessageSound.RejectInput);
                    return false;
                }
            }
            if(def.prerequisiteBuildings != null)
            {
                foreach (OutpostBuildingDef prerequisite in def.prerequisiteBuildings)
                {
                    if (!buildings.Contains(prerequisite))
                    {
                        if (sendMessages)
                            Messages.Message("Message_CannotConstruct_MissingPrerequisiteBuilding".Translate(new object[]
                            {
                            def.label,
                            prerequisite.label
                            }), MessageSound.RejectInput);
                        return false;
                    }
                }
            }
            if (def.prerequisiteBuildingTags != null)
            {
                foreach (OutpostBuildingTagDef prerequisite in def.prerequisiteBuildingTags)
                {
                    if (buildings.Where(o => o.tags.Contains(prerequisite)).FirstOrDefault() == null)
                    {
                        if (sendMessages)
                            Messages.Message("Message_CannotConstruct_MissingPrerequisiteBuildingTag".Translate(new object[]
                            {
                            def.label,
                            prerequisite.label
                            }), MessageSound.RejectInput);
                        return false;
                    }
                }
            }
            if (def.prerequisiteResearch != null)
            {
                foreach (ResearchProjectDef prerequisite in def.prerequisiteResearch)
                {
                    if (!prerequisite.IsFinished)
                    {
                        if (sendMessages)
                            Messages.Message("Message_CannotConstruct_MissingResearch".Translate(new object[]
                            {
                            def.label,
                            prerequisite.label
                            }), MessageSound.RejectInput);
                        return false; 
                    }
                }
            }
            if (def.prerequisiteSpecials != null)
            {
                foreach(SpecialPrerequisiteDef special in def.prerequisiteSpecials.Where(o => o.forPawn == false))
                {
                    if (!special.isMetByOutpost(this))
                    {
                        if (sendMessages)
                            Messages.Message("Message_CannotConstruct_MissingResources".Translate(new object[]
                            {
                            def.label,
                            special.label
                            }), MessageSound.RejectInput);
                        return false;
                    }
                }
            }
            if (def.costList != null)
            {
                foreach (ThingCountClass needed in def.costList)
                {
                    if (!HasAccessToEnough(needed, caravan))
                    {
                        if (sendMessages)
                            Messages.Message("Message_CannotConstruct_MissingResources".Translate(new object[]
                            {
                            def.label,
                            needed.thingDef.label
                            }), MessageSound.RejectInput);
                        return false;
                    }
                }
            }
            return true;
        }

        public bool HasAccessToEnough(ThingCountClass needed, Caravan caravan)
        {
            int totalCount = 0;
            if(caravan != null)
            {
                foreach (Thing thing in caravan.Goods)
                {
                    if (thing.def == needed.thingDef)
                    {
                        if (totalCount + thing.stackCount >= needed.count)
                        {
                            totalCount = needed.count;
                            break;
                        }
                        else
                        {
                            totalCount += thing.stackCount;
                        }
                    }
                }
            }
            if (!(totalCount >= needed.count))
            {
                return false;
            }
            return true;
        }

        internal bool TryConstruct(OutpostBuildingDef def, Caravan caravan)
        {
            if (!CanConstruct(def, caravan, true))
                return false;

            foreach (ThingCountClass needed in def.costList)
            {
                int totalNeeded = needed.count;
                foreach (Thing thing in caravan.Goods)
                {
                    if (thing.def == needed.thingDef)
                    {
                        if(thing.stackCount < totalNeeded)
                        {
                            totalNeeded -= thing.stackCount;
                            Thing splitOff = thing.SplitOff(thing.stackCount);
                        }
                        else
                        {
                            Thing splitOff = thing.SplitOff(totalNeeded);
                            break;
                        }
                    }
                }
            }
            return true;
        }

        internal void StartConstruction(OutpostBuildingDef def)
        {
            this.buildings.Add(def);
            this.buildings = this.buildings.OrderBy(o => o.layer).ToList();
            Recalc();
        }

        internal bool TryDeconstruct(OutpostBuildingDef def, Caravan caravan)
        {
            foreach(OutpostBuildingDef building in buildings)
            {
                if(building.prerequisiteBuildings != null)
                {
                    OutpostBuildingDef match = building.prerequisiteBuildings.Where(o => o == def).FirstOrDefault();
                    if (match != null)
                    {
                        Messages.Message("Message_CannotDeconstruct_PrerequisiteBuilding".Translate(new object[]
                        {
                            def.label,
                            building.label
                        }), MessageSound.RejectInput);
                        return false;
                    }
                }
            }
            if(def.tags != null)
            {
                foreach(OutpostBuildingTagDef tag in def.tags)
                {
                    IEnumerable<OutpostBuildingDef> otherMatches = buildings.Where(o => o != def && o.tags != null && o.tags.Contains(tag) && o.isSatisfied(this, CurrentPawn));
                    if (otherMatches.FirstOrDefault() != null)
                        continue;

                    foreach (OutpostBuildingDef building in buildings)
                    {
                        if (building.prerequisiteBuildingTags != null)
                        {
                            OutpostBuildingTagDef match = building.prerequisiteBuildingTags.Where(o => o == tag).FirstOrDefault();
                            if (match != null)
                            {
                                Messages.Message("Message_CannotDeconstruct_PrerequisiteBuildingTag".Translate(new object[]
                                {
                                    def.label,
                                    match.label,
                                    building.label
                                }), MessageSound.RejectInput);
                                return false;
                            }
                        }
                    }

                    if (CurrentThingNeedsTag(tag))
                    {
                        Messages.Message("Message_CannotDeconstruct_PrerequisiteOccupantTag".Translate(new object[]
                                {
                                    def.label,
                                    innerContainer.FirstOrDefault().Label
                                }), MessageSound.RejectInput);
                        return false;
                    }
                }
            }

            foreach (ThingCountClass needed in def.costList)
            {
                int totalNeeded = (needed.count > 1) ? (int)(needed.count * 0.75f) : needed.count;

                //TODO: refunds
            }
            return true;
        }

        private bool CurrentThingNeedsTag(OutpostBuildingTagDef tag)
        {
            Thing currentThing = innerContainer.FirstOrDefault();
            if (currentThing == null)
                return false;

            var needs = GetNeeds(currentThing);

            return needs.Contains(tag);
        }

        private List<OutpostBuildingTagDef> GetNeeds(Thing thing)
        {
            List<OutpostBuildingTagDef> neededTags = new List<OutpostBuildingTagDef>();
            Pawn pawn = thing as Pawn;

            if (pawn != null)
            {
                neededTags.Add(OutpostBuildingTagDefOf.Housing);
                neededTags.Add(OutpostBuildingTagDefOf.FoodSupply);
            } 
            else 
            {
                neededTags.Add(OutpostBuildingTagDefOf.PowerSupply);
                neededTags.Add(OutpostBuildingTagDefOf.AIHousing);
            }
            return neededTags;
        }  

        internal void StartDeconstruction(OutpostBuildingDef def)
        {  
            this.buildings.Remove(def); 
            //this.buildings = this.buildings.OrderBy(o => o.layer).ToList(); 
            Recalc(); 
        } 

        internal bool tagSatisfied(OutpostBuildingTagDef tag, Pawn pawn) 
        { 
            foreach (OutpostBuildingDef building in buildings.Where(o => o.tags != null && o.tags.Contains(tag)))
            {
                if (building.isSatisfied(this, pawn))
                {
                    //Log.Message("building " + building.label + " is satisfied");
                    return true;
                }
                else
                {
                    //Log.Message("building " + building.label + " not satisfied");
                }
            }
            return false;  
        }    
          
        public bool TryAssignThing(Thing thing, Caravan source)
        {
            if(thing == null || this.Accepts(thing))
            {
                Thing currentThing = this.innerContainer.FirstOrDefault();
                if (currentThing != null)
                { 
                    if(source != null)
                    {
                        if (currentThing is Pawn && (currentThing as Pawn).IsColonist)
                        {
                            //Find.WorldPawns.PassToWorld(currentThing as Pawn);
                            currentThing.holdingOwner.TryTransferToContainer(currentThing, source.pawns, true);
                            Traverse.Create(Find.WorldPawns).Method("AddPawn", new object[] { currentThing as Pawn }).GetValue();
                            Log.Message("pawn in world pawns now:" + (currentThing as Pawn).IsWorldPawn());
                        } 
                        else 
                        {
                            Pawn grabbingPawn = CaravanInventoryUtility.FindPawnToMoveInventoryTo(currentThing, source.PawnsListForReading, null, null);
                            if (grabbingPawn == null)
                            {
                                Log.Error("Could not find pawn to move bought thing to (bought by player). thing=" + currentThing);
                                currentThing.Destroy(DestroyMode.Vanish);
                            }
                            else 
                            {
                                if (!currentThing.holdingOwner.TryTransferToContainer(currentThing, grabbingPawn.inventory.innerContainer, true))
                                {
                                    Log.Error("Could not add item to inventory.");
                                    currentThing.Destroy(DestroyMode.Vanish);
                                }
                            }
                        }
                    }
                    else
                    {
                        if(currentThing is Pawn && (currentThing as Pawn).IsColonist)
                        {
                            WorldObject obj = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Caravan);

                            Caravan caravan = (Caravan)obj;
                            caravan.Tile = this.Tile;
                            caravan.SetFaction(this.Faction);
                            caravan.Name = CaravanNameGenerator.GenerateCaravanName(caravan);

                            currentThing.holdingOwner.TryTransferToContainer(currentThing, caravan.pawns, true);

                            Find.WorldObjects.Add(caravan);
                        }
                        else
                        {
                            currentThing.Destroy(DestroyMode.Vanish);
                            currentThing = null;
                        }
                    }
                }

                bool changed = false;
                if(thing != null)
                {
                    if(thing is Pawn)
                    {
                        Find.WorldPawns.RemovePawn(thing as Pawn);
                    }
                    if (thing.holdingOwner != null)
                    {
                        thing.holdingOwner.TryTransferToContainer(thing, this.innerContainer, thing.stackCount, true);
                        changed = true;
                    }
                    else
                    {
                        changed = this.innerContainer.TryAdd(thing, true);
                    }
                }
                if (changed && source != null)
                {
                    if (source.pawns.InnerListForReading.Where(o => o.IsColonist).Count() == 0)
                    {
                        Find.WorldObjects.Remove(source);
                    }
                }
                Recalc();
                return true;
            }
            
            return false;
        }

        public bool Accepts(Thing thing)
        {
            var neededTags = GetNeeds(thing);
            Pawn pawn = thing as Pawn;

            foreach (OutpostBuildingTagDef tag in neededTags)
            {
                if (this.tagSatisfied(tag, pawn))
                {
                    continue;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        internal Thing getManningThing()
        {
            if (innerContainer == null)
                return null;
            return innerContainer.FirstOrDefault();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            yield return (new Command_Action
            {
                defaultLabel = "ManageOutpost".Translate(),
                icon = Macrocosm_Textures.ManageOutpostCommand,
                action = delegate
                {
                    Find.WindowStack.Add(new Dialog_ScoutingOutpostManager(this, null));
                }
            });
        }
    }    
}
 