using Macrocosm.enums;
using Macrocosm.macrocosm;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Macrocosm.defs
{
    public class OutpostBuildingDef : Def
    {
        public Vector2 positionOnOutpostView; 

        public OutpostBuildingCategory category;

        public List<OutpostBuildingTagDef> tags;

        public List<StatModifier> statBases;

        public List<ThingCountClass> costList;

        public Type thingClass;

        public int layer;

        public TickerType tickerType;

        public OutpostBuildingSpotDef usedSpot;

        public List<ResearchProjectDef> prerequisiteResearch;

        public List<OutpostBuildingDef> prerequisiteBuildings;
        
        public List<OutpostBuildingTagDef> prerequisiteBuildingTags;

        public List<SkillRequirement> skillRequirements;

        public List<TraitDef> requiredTraits;

        public List<TraitDef> disallowedTraits;
        
        public List<SpecialPrerequisiteDef> prerequisiteSpecials;

        [Unsaved]
        public Texture2D uiIcon = BaseContent.BadTex;

        [Unsaved]
        public Graphic graphic = BaseContent.BadGraphic;

        public string uiIconPath;

        public GraphicData graphicData;

        public Material DrawMatSingle
        {
            get
            {
                if (this.graphic == null)
                {
                    return null;
                }
                return this.graphic.MatSingle;
            }
        }

        public bool IsKnown
        {
            get
            {
                if (DebugSettings.godMode == true)
                    return true;
                if (prerequisiteResearch != null)
                {
                    foreach (ResearchProjectDef research in prerequisiteResearch)
                    {
                        if (!research.IsFinished)
                            return false;
                    }
                }
                return true;
            }
        }

        public override void PostLoad()
        {
            base.PostLoad();
            if (this.graphicData != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    if (this.graphicData.shaderType == ShaderType.None)
                    {
                        this.graphicData.shaderType = ShaderType.Cutout;
                    }
                    this.graphic = this.graphicData.Graphic;
                });
            }
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!this.uiIconPath.NullOrEmpty())
                {
                    this.uiIcon = ContentFinder<Texture2D>.Get(this.uiIconPath, true);
                }
                else if (this.DrawMatSingle != null && this.DrawMatSingle != BaseContent.BadMat)
                {
                    this.uiIcon = (Texture2D)this.DrawMatSingle.mainTexture;
                }
            });
        }

        private static readonly Vector2 noPositionData = new Vector2(-1000, -1000);

        internal Vector2 GetPosition(List<OutpostBuildingDef> allBuildings, float rectHeight, float texHeight)
        {
            Vector2 position = positionOnOutpostView;
            if (position.Equals(noPositionData))
            {
                if (usedSpot != null)
                {
                    int offset = allBuildings.Where(o => o.usedSpot != null && o.usedSpot == usedSpot).ToList().IndexOf(this);

                    if (!(usedSpot.positions == null || usedSpot.positions.Count == 0))
                    {
                        if (offset >= usedSpot.positions.Count)
                        {
                            Log.Warning("more buildings in slot than positions: " + offset + " " + usedSpot.defName + " has slots " + usedSpot.positions.Count);
                            return noPositionData;
                        }
                        position = new Vector2(usedSpot.positions[offset].x, usedSpot.positions[offset].y);
                        }   
                    else  
                    {
                        //Log.Warning(label + " is missing position data and its usedSpot " + usedSpot.defName + " has none either");
                        return noPositionData;
                    }
                }
                else
                {
                    //Log.Warning(label + " is missing position data and has no usedSpot");
                    return noPositionData; 
                }
            }
            position.y = rectHeight - (position.y + texHeight / 2);
            return position;
        }

        public bool isSatisfied(ScoutingOutpost outpost, Pawn pawn)
        {
            if (prerequisiteBuildings != null)
            {
                foreach (OutpostBuildingDef prerequisite in prerequisiteBuildings)
                {
                    if (!outpost.buildings.Contains(prerequisite))
                    {
                        //Log.Message("fail due to prerequisite building");
                        return false;
                    }
                }
            }
            if (prerequisiteBuildingTags != null)
            {
                foreach (OutpostBuildingTagDef prerequisite in prerequisiteBuildingTags)
                {
                    if (outpost.buildings.Where(o => o.tags.Contains(prerequisite)).FirstOrDefault() == null)
                    {
                        //Log.Message("fail due to prerequisite tag");
                        return false;
                    }
                }
            }
            if (prerequisiteSpecials != null)
            {
                foreach (SpecialPrerequisiteDef special in prerequisiteSpecials.Where(o => o.forPawn == false))
                {
                    if (!special.isMetByOutpost(outpost))
                    {
                        //Log.Message("fail due to prerequisite special(outpost)");
                        return false;
                    }
                }

                foreach (SpecialPrerequisiteDef special in prerequisiteSpecials.Where(o => o.forPawn == true))
                {
                    if (!special.isMetByPawn(pawn))
                    {
                        //Log.Message("fail due to prerequisite special(pawn) : "+pawn);
                        return false;
                    }
                }
            }

            if (skillRequirements != null)
            {
                foreach (SkillRequirement skillReq in skillRequirements)
                {
                    if (pawn == null || pawn.skills == null || pawn.skills.GetSkill(skillReq.skill).TotallyDisabled || pawn.skills.GetSkill(skillReq.skill).levelInt < skillReq.minLevel)
                    {
                        //Log.Message("fail due to prerequisite pawn skill : "+pawn);
                        return false;
                    }
                }
            }

            if (requiredTraits != null)
            {
                foreach (TraitDef reqTrait in requiredTraits)
                {
                    if (pawn != null && pawn.story != null && pawn.story.traits != null && !pawn.story.traits.HasTrait(reqTrait))
                    {
                        //Log.Message("fail due to prerequisite pawn trait : " + pawn);
                        return false;
                    }
                }
            }

            if (disallowedTraits != null)
            {
                foreach (TraitDef disTrait in disallowedTraits)
                {
                    if (pawn == null || pawn.story == null || pawn.story.traits == null || pawn.story.traits.HasTrait(disTrait))
                    {
                        //Log.Message("fail due to disallowed pawn trait : " + pawn);
                        return false;
                    }
                }
            }

            return true;
        }
    }
} 
