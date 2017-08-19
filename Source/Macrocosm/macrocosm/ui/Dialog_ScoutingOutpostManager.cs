using Macrocosm.defs;
using Macrocosm.enums;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Macrocosm.macrocosm.ui  
{
    class Dialog_ScoutingOutpostManager : Window 
    {
         
        private static readonly Color slotColorFree = Color.green;
        private static readonly Color slotColorOccupied = new Color(1f,0.7f, 0f); 

        private static readonly Color transparent = new Color(0.8f, 0.8f, 1f, 0.5f); 

        public ScoutingOutpost outpost { get; private set; }
        public Caravan caravan { get; private set; }

        private Vector2 scrollPosition = Vector2.zero;
        protected override float Margin { get { return 0f; } }

        private OutpostBuildingCategory activeCategory = OutpostBuildingCategory.Inactive;
        private OutpostBuildingDef selectedBuilding = null;

        public Dialog_ScoutingOutpostManager(ScoutingOutpost outpost, Caravan caravan)
        {  
            this.outpost = outpost; 
            this.caravan = caravan; 
              
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.closeOnEscapeKey = false;
            
            this.doCloseX = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(720f, 520f); 
            }
        }

        internal const float STATUSBAR_HEIGHT = 85f;

        internal const float BUILDABLE_RECT_HEIGHT = 82f;
        internal const float BUILDABLE_RECT_GAP = 6f;

        internal const float BUILDABLE_RECT_STATS = 160f;

        internal const float BUILDINGMENU_WIDTH = 220f;
        internal const float BUILDINGMENU_HEIGHT = 80f;
        internal const float BUILDINGSUBMENU_HEIGHT = 220f;

        internal const float ASSIGNSIDEBAR_WIDTH = 220f;
        internal const float ASSIGNSIDEBAR_HEIGHT = 300f;


        public override void DoWindowContents(Rect inRect)
        {
            TextAnchor initialAnchor = Text.Anchor;
            Color initialColor = GUI.color;

            float num = inRect.y;

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            //float textWidth = Text.CalcSize(outpost.Label).x; //(inRect.width / 2) - (textWidth / 2)
            Widgets.Label(new Rect(0, 2, inRect.width, 36f), outpost.LabelCap);
            num += 36f;
            Text.Anchor = TextAnchor.MiddleLeft;

            ExtraWidgets.DrawLineHorizontalWindowy(0, num, inRect.width);

            DrawScoutingRange(new Rect(0,num, inRect.width / 3, STATUSBAR_HEIGHT));
            ExtraWidgets.DrawLineVerticalWindowy(inRect.width / 3, num, STATUSBAR_HEIGHT);
            DrawMannedStatus(new Rect(inRect.width / 3, num, inRect.width / 3, STATUSBAR_HEIGHT));
            ExtraWidgets.DrawLineVerticalWindowy((inRect.width / 3) * 2, num, STATUSBAR_HEIGHT);
            DrawSelfSufficiency(new Rect((inRect.width / 3) * 2, num, inRect.width / 3, STATUSBAR_HEIGHT));

            num += STATUSBAR_HEIGHT;

            ExtraWidgets.DrawLineHorizontalWindowy(0, num, inRect.width);

            //Widgets.DrawBoxSolid(new Rect(0, num, inRect.width, inRect.height - num).ContractedBy(2), Color.blue);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect sceneRect = new Rect(inRect.x, num, inRect.width, inRect.height - num);
            sceneRect = sceneRect.ContractedBy(2);

            num = 0;
            //float width = outRect.width - 16f;
            //float height = 80; //Text.CalcHeight(this.text, width)
            //Rect viewRect = new Rect(0f, 0f, width, height);

            GUI.color = Color.white;

            Tile tile = Find.WorldGrid.tiles[outpost.Tile];
            
            Material tileMat = tile.biome.DrawMaterial;
            Texture2D tileTex = ContentFinder<Texture2D>.Get(tile.biome.texture, true);
            
            GUI.DrawTexture(sceneRect.BottomHalf(), tileTex);
            GUI.DrawTexture(sceneRect.TopHalf(), Macrocosm_Materials.SkyTex);
            //Widgets.DrawBoxSolid(viewRect, tileColor);

            GUI.BeginGroup(sceneRect);

            if (selectedBuilding != null)
            {
                if (selectedBuilding.usedSpot != null && !(selectedBuilding.usedSpot.positions == null || selectedBuilding.usedSpot.positions.Count == 0))
                {
                    GUI.color = selectedBuilding.usedSpot.color;
                    foreach (Rect rect in selectedBuilding.usedSpot.positions)
                    {
                        Rect spotRect = new Rect(rect);
                        spotRect.y = sceneRect.height - (spotRect.y + rect.height);
                        GUI.DrawTexture(spotRect, TexUI.FastFillTex);
                        Widgets.DrawBox(spotRect);
                        //Log.Message("position selected:" + position.ToString());
                    }
                }
                else
                {
                    GUI.color = transparent;
                    Rect spotRect = new Rect(selectedBuilding.positionOnOutpostView.x, selectedBuilding.positionOnOutpostView.y, selectedBuilding.DrawMatSingle.mainTexture.width / 2, selectedBuilding.DrawMatSingle.mainTexture.height / 2);
                    spotRect.y = sceneRect.height - (spotRect.y + selectedBuilding.DrawMatSingle.mainTexture.height / 2);
                    GUI.DrawTexture(spotRect, TexUI.FastFillTex);
                    Widgets.DrawBox(spotRect);
                }
            }

            GUI.color = Color.white;

            foreach (OutpostBuildingDef building in outpost.buildings.OrderBy(o => o.GetPosition(outpost.buildings, sceneRect.height, o.DrawMatSingle.mainTexture.height).y))
            {
                //Log.Message("have building: " + building.defName);
                Material buildableMat = building.DrawMatSingle;

                Vector2 position = building.GetPosition(outpost.buildings, sceneRect.height, buildableMat.mainTexture.height);

                Rect buildableRect = new Rect(position.x, position.y, buildableMat.mainTexture.width / 2, buildableMat.mainTexture.height / 2);
                GUI.DrawTexture(buildableRect, buildableMat.mainTexture); 
                //Log.Message("position built:" + position.ToString()); 
            }
             
            //debug draw
            /*
            foreach(OutpostBuildingSpotDef spot in DefDatabase<OutpostBuildingSpotDef>.AllDefs)
            {
                GUI.color = spot.color; 
                foreach (Rect rect in spot.positions) 
                { 
                    Rect spotRect = new Rect(rect);
                    spotRect.y = sceneRect.height - (spotRect.y + rect.height);
                    GUI.DrawTexture(spotRect, TexUI.FastFillTex);
                    Widgets.DrawBox(spotRect);
                    //Log.Message("position selected:" + position.ToString());
                }
            }  */

            GUI.EndGroup();

            GUI.color = Color.white;
            Rect buttons = new Rect(sceneRect.x, sceneRect.y, BUILDINGMENU_WIDTH, BUILDINGMENU_HEIGHT);

            Widgets.DrawBoxSolid(buttons, Color.black);                       

            if (Widgets.ButtonTextSubtle(buttons.TopHalf().LeftHalf(), "OutpostMenu_Scouting".Translate(), 0f, 8f, SoundDefOf.MouseoverCategory))
                MenuButtonClick(OutpostBuildingCategory.Scouting);
            if (Widgets.ButtonTextSubtle(buttons.TopHalf().RightHalf(), "OutpostMenu_Autonomy".Translate(), 0f, 8f, SoundDefOf.MouseoverCategory))
                MenuButtonClick(OutpostBuildingCategory.Autonomy);
            if (Widgets.ButtonTextSubtle(buttons.BottomHalf().LeftHalf(), "OutpostMenu_Comms".Translate(), 0f, 8f, SoundDefOf.MouseoverCategory))
                MenuButtonClick(OutpostBuildingCategory.Comms);
            if (Widgets.ButtonTextSubtle(buttons.BottomHalf().RightHalf(), "OutpostMenu_Misc".Translate(), 0f, 8f, SoundDefOf.MouseoverCategory))
                MenuButtonClick(OutpostBuildingCategory.Misc);
             
            num += BUILDINGMENU_HEIGHT; 

            if (selectedBuilding != null)
            {
                Rect rect = new Rect(sceneRect.x, sceneRect.y + num, BUILDINGMENU_WIDTH, BUILDINGSUBMENU_HEIGHT);

                Widgets.DrawWindowBackground(rect);

                rect = rect.ContractedBy(2f);

                GUI.BeginGroup(rect);

                float inNum = 24f;
                DrawPanelReadout(ref inNum, rect.width, selectedBuilding);

                Rect rect2 = new Rect(0f, inNum, rect.width, rect.height - (num + 26));
                string desc = selectedBuilding.description;
                GenText.SetTextSizeToFit(desc, rect2);
                Widgets.Label(rect2, desc);

                if (!outpost.buildings.Contains(selectedBuilding))
                {
                    if (Widgets.ButtonText(new Rect(rect.width / 4, rect.height - 22, rect.width / 2, 20), "OutpostMenu_Construct".Translate()))
                    {
                        ConstructBuildingClick(selectedBuilding);
                    }
                }
                else
                {
                    if (Widgets.ButtonText(new Rect(rect.width / 4, rect.height - 22, rect.width / 2, 20), "OutpostMenu_Deconstruct".Translate()))
                    {
                        DeconstructBuildingClick(selectedBuilding);
                    }
                }             

                GUI.EndGroup();
            }

            float gizNumH = BUILDINGMENU_WIDTH + 5; 
            float gizNumV = 0;
             

            foreach (OutpostBuildingDef def in DefDatabase<OutpostBuildingDef>.AllDefs.Where(o => o.category == activeCategory && o.IsKnown))
            {
                if((gizNumH + 80) > sceneRect.width)
                {
                    gizNumH = BUILDINGMENU_WIDTH + 5; 
                    gizNumV += 85;
                }
                
                DrawBuildGizmo(def, new Vector2(sceneRect.x + gizNumH, sceneRect.y + gizNumV + 5), outpost);

                gizNumH += 80;
            } 
            
            Text.Anchor = initialAnchor;
            GUI.color = initialColor;
        }

        private void DrawPanelReadout(ref float curY, float width, OutpostBuildingDef def)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0, 0, width -24f, 24f), selectedBuilding.LabelCap);

            Widgets.InfoCardButton(width - 24f, 0, def);

            Text.Font = GameFont.Tiny;
            List<ThingCountClass> list = def.costList;
            Color savedColor = GUI.color;

            //print resource needs
            if(def.costList != null)
            {
                foreach (ThingCountClass thingCountClass in def.costList)
                {
                    Texture2D image;
                    if (thingCountClass.thingDef == null)
                    {
                        image = BaseContent.BadTex;
                    }
                    else
                    {
                        image = thingCountClass.thingDef.uiIcon;
                        GUI.color = thingCountClass.thingDef.graphicData.color;
                    }
                    GUI.DrawTexture(new Rect(0f, curY, 20f, 20f), image);
                    if (thingCountClass.thingDef != null && thingCountClass.thingDef.resourceReadoutPriority != ResourceCountPriority.Uncounted && !outpost.HasAccessToEnough(thingCountClass, caravan))
                    {
                        GUI.color = Color.red;
                    }
                    Widgets.Label(new Rect(26f, curY + 2f, 50f, 100f), thingCountClass.count.ToString());
                    GUI.color = Color.white;
                    string text;
                    if (thingCountClass.thingDef == null)
                    {
                        text = "(" + "UnchosenStuff".Translate() + ")";
                    }
                    else
                    {
                        text = thingCountClass.thingDef.LabelCap;
                    }
                    float width2 = width - 60f;
                    float num = Text.CalcHeight(text, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                    curY += num;
                }
            }          
            
            if(def.prerequisiteBuildings != null)
            {
                foreach (OutpostBuildingDef prerequisite in def.prerequisiteBuildings)
                {
                    GUI.color = Color.white;
                    Texture2D badTex = prerequisite.uiIcon;
                    if (badTex == null)
                    {
                        badTex = BaseContent.BadTex;
                    }
                    GUI.DrawTexture(new Rect(0f, curY, 20f, 20f), badTex);
                    if (outpost.buildings.Contains(prerequisite))
                        GUI.color = Color.green;
                    else
                        GUI.color = Color.red;

                    float width2 = width - 60f;
                    float num = Text.CalcHeight(prerequisite.LabelCap, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), prerequisite.LabelCap);
                    curY += num;
                }
            }
            
            if(def.prerequisiteBuildingTags != null)
            {
                foreach (OutpostBuildingTagDef prerequisite in def.prerequisiteBuildingTags)
                {
                    Color color;

                    if (outpost.buildings.Where(o => o.tags.Contains(prerequisite)).FirstOrDefault() != null)
                        color = Color.green;
                    else
                        color = Color.red;

                    DrawTag(prerequisite, ref curY, color, width);
                }
            }
            
            if (def.prerequisiteSpecials != null)
            {
                foreach (SpecialPrerequisiteDef prerequisite in def.prerequisiteSpecials.Where(o => o.forPawn == false))
                {
                    if (prerequisite.isMetByOutpost(outpost))
                        GUI.color = Color.green;
                    else
                        GUI.color = Color.red;
                    float width2 = width - 60f;
                    string text = prerequisite.LabelCap;
                    float num = Text.CalcHeight(text, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                    curY += num;
                }
            }

            if (def.skillRequirements != null)
            {
                GUI.color = Color.yellow;
                foreach (SkillRequirement prerequisite in def.skillRequirements)
                {
                    float width2 = width - 60f;
                    string text = prerequisite.skill.LabelCap + " " + prerequisite.minLevel;
                    float num = Text.CalcHeight(text, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                    curY += num;
                }
            }

            if (def.requiredTraits != null)
            {
                GUI.color = Color.yellow;
                foreach (TraitDef reqTrait in def.requiredTraits)
                {
                    float width2 = width - 60f;
                    string text = reqTrait.degreeDatas[0].label.CapitalizeFirst();
                    float num = Text.CalcHeight(text, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                    curY += num;
                }
            }

            if (def.disallowedTraits != null)
            {
                GUI.color = Color.yellow;
                foreach (TraitDef disTrait in def.disallowedTraits)
                {
                    float width2 = width - 60f;
                    string text = "NotPrefix".Translate() + disTrait.degreeDatas[0].label.CapitalizeFirst();
                    float num = Text.CalcHeight(text, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                    curY += num;
                }
            }
         
            if (def.prerequisiteSpecials != null)
            {
                foreach (SpecialPrerequisiteDef prerequisite in def.prerequisiteSpecials.Where(o => o.forPawn == true))
                {
                    GUI.color = Color.yellow;
                    float width2 = width - 60f;
                    string text = prerequisite.LabelCap;
                    float num = Text.CalcHeight(text, width2) - 2f;
                    Widgets.Label(new Rect(60f, curY + 2f, width2, num), text);
                    curY += num;
                }
            }

            if (def.tags != null)
            {
                GUI.color = Color.white;
                string lab = "OutpostMenu_Provides".Translate(); 
                float labH = Text.CalcHeight(lab, width) - 2f;
                Widgets.Label(new Rect(10f, curY + 2f, width, labH), lab);
                curY += labH;

                foreach (OutpostBuildingTagDef tag in def.tags)
                {
                    DrawTag(tag, ref curY, Color.white, width);
                }
            }

            GUI.color = savedColor;
        }

        private void DrawTag(OutpostBuildingTagDef tag, ref float curY, Color color, float width)
        {
            GUI.color = Color.white;
            Texture2D badTex = tag.uiIcon;
            if (badTex == null)
            {
                badTex = BaseContent.BadTex;
            }
            GUI.DrawTexture(new Rect(0f, curY, 20f, 20f), badTex);
            GUI.color = color;

            float width2 = width - 60f;
            float num = Text.CalcHeight(tag.needLabel, width2) - 2f;
            if (tag.needLabel != null)
                Widgets.Label(new Rect(60f, curY + 2f, width2, num), tag.needLabel);
            else
                Log.Warning("tagDef " + tag.defName + " missing needLabel");
            curY += num;
        }

        private void DrawBuildGizmo(OutpostBuildingDef def, Vector2 position, ScoutingOutpost outpost)
        {
            Text.Font = GameFont.Tiny;
            Rect rect = new Rect(position.x, position.y, 75f, 75f);
            if (Mouse.IsOver(rect))
            {
                GUI.color = GenUI.MouseoverColor;
            }
            Texture2D badTex = def.uiIcon;
            if (badTex == null)
            {
                badTex = BaseContent.BadTex;
            }
            GUI.DrawTexture(rect, Command.BGTex);
            MouseoverSounds.DoRegion(rect, SoundDefOf.MouseoverCommand);
            GUI.color = Color.white;
            Widgets.DrawTextureFitted(new Rect(rect), badTex, 0.85f);
            GUI.color = Color.white; 
            string labelCap = def.LabelCap; 
            if (!labelCap.NullOrEmpty())
            {
                float num = Text.CalcHeight(labelCap, rect.width);
                Rect rect3 = new Rect(rect.x, rect.yMax - num + 12f, rect.width, num);
                GUI.DrawTexture(rect3, TexUI.GrayTextBG);
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect3, labelCap);


                if (def.usedSpot != null && def.usedSpot.positions != null && def.usedSpot.positions.Count > 0)
                {
                    string text = "OutpostMenu_Slot".Translate(new object[]
                    {
                        def.usedSpot.LabelCap
                    });
                    num = Text.CalcHeight(text, rect.width); 
                    Rect rect4 = new Rect(rect.x, rect.yMin, rect.width, num);
                    GUI.color = Color.white; 
                    GUI.DrawTexture(rect4, TexUI.GrayTextBG);
                    if (outpost.buildings.Where(o => o.usedSpot!= null && o.usedSpot.Equals(def.usedSpot)).Count() < def.usedSpot.positions.Count)
                        GUI.color = slotColorFree;
                    else
                        GUI.color = slotColorOccupied;
                    Text.Anchor = TextAnchor.LowerCenter;
                    Widgets.Label(rect4, text);
                }

                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }

            GUI.color = Color.white;

            TipSignal tip = def.description;
            TooltipHandler.TipRegion(rect, tip);

            if (Widgets.ButtonInvisible(rect, false))
            {
                DetailsBuildingClick(def);
                //ConstructBuildingClick(def); 
            }
        }

        private void MenuButtonClick(OutpostBuildingCategory selection)
        {
            if (selection == activeCategory)
                activeCategory = OutpostBuildingCategory.Inactive;
            else
                activeCategory = selection;

            selectedBuilding = null;
        }

        private void DetailsBuildingClick(OutpostBuildingDef def)
        {
            if (this.selectedBuilding != def)
                this.selectedBuilding = def;
            else
                this.selectedBuilding = null;
        }

        private void ConstructBuildingClick(OutpostBuildingDef def)
        {
            if (DebugSettings.godMode)
            {
                outpost.StartConstruction(def);
            }
            else
            {
                bool success = outpost.TryConstruct(def, caravan);
                if (success)
                    outpost.StartConstruction(def);
            }

        }

        private void DeconstructBuildingClick(OutpostBuildingDef def)
        {
            bool success = outpost.TryDeconstruct(def, caravan);
            if (success)
                outpost.StartDeconstruction(def);
        }

        private void DrawScoutingRange(Rect rect)
        {
            GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            float width = Math.Max(Text.CalcSize("ScoutingRange_Printout".Translate()).x, Text.CalcSize("CommsRange_Printout".Translate()).x);
            Widgets.Label(new Rect(rect.x + 4, rect.y, rect.width, 36f), "ScoutingRange_Printout".Translate());
            
            if (outpost.IsManned)
                GUI.color = Color.green;
            else
                GUI.color = Color.red;

            Widgets.Label(new Rect(rect.x + 4 + width, rect.y, 36f, 36f), outpost.PotentialSpotDistance.ToString());

            GUI.color = Color.white;
            Widgets.Label(new Rect(rect.x + 4, rect.y + 36f, rect.width, 36f), "CommsRange_Printout".Translate());

            if (outpost.IsConnected)
                GUI.color = Color.green;
            else
                GUI.color = Color.red;

            Widgets.Label(new Rect(rect.x + 4 + width, rect.y + 36f, 36f, 36f), outpost.CommsRange.ToString()); 
        }

        private void DrawMannedStatus(Rect rect)
        {
            GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(new Rect(rect.x + 4, rect.y, rect.width - 80f, rect.height), "MannedBy_Printout".Translate());

            Text.Font = GameFont.Tiny;
            Rect gizRect = new Rect((rect.x + rect.width) - 80f, (rect.y + (rect.height / 2)) - 37f, 75f, 75f);
            if (Mouse.IsOver(gizRect))
            {
                GUI.color = GenUI.MouseoverColor;
            }
            GUI.DrawTexture(gizRect, Command.BGTex);
            MouseoverSounds.DoRegion(gizRect, SoundDefOf.MouseoverCommand);

            if (outpost.IsManned)
            {
                Rect labelRect = new Rect(gizRect);
                labelRect.x -= 15f;
                labelRect.width += 30f;

                Thing thing = outpost.getManningThing();
                if(thing is Pawn)
                {
                    Pawn pawn = thing as Pawn;
                    ExtraWidgets.DrawColonist(gizRect, pawn);
                    Text.Anchor = TextAnchor.LowerCenter;
                    Widgets.Label(labelRect, pawn.LabelShort.CapitalizeFirst());
                }
                else
                {
                    ExtraWidgets.DrawItem(gizRect, thing);
                    Text.Anchor = TextAnchor.LowerCenter;
                    Widgets.Label(labelRect, thing.LabelShort.CapitalizeFirst());
                }
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(gizRect, "NotManned_Printout".Translate());
            }

            if (Widgets.ButtonInvisible(gizRect))
            {
                List<FloatMenuOption> menuOptions = new List<FloatMenuOption>();

                if (caravan != null)
                {
                    if(outpost.innerContainer.FirstOrDefault() != null)
                    {
                        FloatMenuOptionWithSidebar leaveOption = new FloatMenuOptionWithSidebar("UnassignManager".Translate(), delegate
                        {
                            outpost.TryAssignThing(null, caravan);
                        }, MenuOptionPriority.Default, null, null, 20f, null, null, 0, 0, null);
                        menuOptions.Add(leaveOption);
                    }

                    foreach (Pawn pawn in caravan.pawns.InnerListForReading.Where(o => o.IsColonist))
                    {
                        FloatMenuOptionWithSidebar option = new FloatMenuOptionWithSidebar(pawn.LabelCap, delegate
                        {
                            if (outpost.Accepts(pawn))
                            {
                                if (caravan.pawns.InnerListForReading.Where(o => o.IsColonist).Count() <= 1 && (outpost.innerContainer.FirstOrDefault() == null || !(outpost.innerContainer.FirstOrDefault() is Pawn)))
                                {
                                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDestroyCaravan".Translate(), delegate
                                    {
                                        outpost.TryAssignThing(pawn, caravan);
                                        if (caravan.pawns.InnerListForReading.Where(o => o.IsColonist).Count() <= 1)
                                        {
                                            this.Close(false);
                                            Dialog_ScoutingOutpostManager replacement = new Dialog_ScoutingOutpostManager(outpost, null);
                                            Find.WindowStack.Add(replacement);
                                        }
                                    }, false, null));
                                }
                                else
                                {
                                    outpost.TryAssignThing(pawn, caravan);
                                }
                                
                            }
                        }, MenuOptionPriority.Default, null, null, 20f, null, null, ASSIGNSIDEBAR_WIDTH, ASSIGNSIDEBAR_HEIGHT, delegate (Rect inRect)
                        {
                            DrawSidebarReadout(inRect, pawn);
                            return false;
                        });
                        menuOptions.Add(option);
                    }

                    foreach(Thing thing in caravan.Goods.Where(o => o.def == ThingDefOf.AIPersonaCore || o.def == Macrocosm_ThingDefOf.Macrocontroller))
                    {
                        FloatMenuOptionWithSidebar option = new FloatMenuOptionWithSidebar(thing.LabelCap, delegate
                        {
                            if (outpost.Accepts(thing))
                            {
                                outpost.TryAssignThing(thing, caravan);
                            }
                        }, MenuOptionPriority.Default, null, null, 20f, null, null, ASSIGNSIDEBAR_WIDTH, ASSIGNSIDEBAR_HEIGHT, delegate (Rect inRect)
                        {
                            DrawSidebarReadout(inRect, thing);
                            return false;
                        });
                        menuOptions.Add(option);
                    }
                }
                else
                {
                    if (outpost.innerContainer.FirstOrDefault() != null)
                    {
                        if (outpost.innerContainer.FirstOrDefault() as Pawn != null)
                        {
                            Pawn pawn = (outpost.innerContainer.FirstOrDefault() as Pawn);
                            if (pawn.IsColonist)
                            {
                                FloatMenuOptionWithSidebar leaveOption = new FloatMenuOptionWithSidebar("UnassignManager".Translate(), delegate
                                {
                                    outpost.TryAssignThing(null, null);
                                }, MenuOptionPriority.Default, null, null, 20f, null, null, 0, 0, null);
                                menuOptions.Add(leaveOption);
                            }
                        }
                        else
                        {
                            FloatMenuOptionWithSidebar leaveOption = new FloatMenuOptionWithSidebar("UnassignManager".Translate(), delegate
                            {
                                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDestroyManningThing".Translate(), delegate
                                {
                                    outpost.TryAssignThing(null, null);
                                }, false, null));
                            }, MenuOptionPriority.Default, null, null, 20f, null, null, 0, 0, null);
                            menuOptions.Add(leaveOption);
                        }
                    }
                }

                if(menuOptions.Count > 0)
                {
                    FloatMenuWithSidebar menu = new FloatMenuWithSidebar(menuOptions, "AssignOutpostMaster_Title".Translate());
                    Find.WindowStack.Add(menu);
                }
            }

            GUI.color = Color.white;

        }
        
        private void DrawSidebarReadout(Rect rect, Thing thing)
        {
            Color savedColor = GUI.color;
            

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Small;
            
            float num = rect.y;

            Widgets.Label(new Rect(rect.x, num + 2, rect.width, 16f), thing.LabelCap);

            num += 20;

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            Rect gizRect = new Rect(rect.x + rect.width - 80f, rect.y + 20f, 75f, 75f);
            GUI.DrawTexture(gizRect, Command.BGTex);

            List<OutpostBuildingTagDef> neededTags = new List<OutpostBuildingTagDef>();

            Pawn pawn = thing as Pawn;

            if (thing is Pawn)
            {
                ExtraWidgets.DrawColonist(gizRect, pawn);

                neededTags.Add(OutpostBuildingTagDefOf.Housing);
                neededTags.Add(OutpostBuildingTagDefOf.FoodSupply);              
            }
            else
            {
                ExtraWidgets.DrawItem(gizRect, thing);

                neededTags.Add(OutpostBuildingTagDefOf.PowerSupply);
                neededTags.Add(OutpostBuildingTagDefOf.AIHousing);
            }

            foreach(OutpostBuildingTagDef tag in neededTags)
            {
                if(outpost.tagSatisfied(tag, pawn))
                {
                    GUI.color = Color.green;
                    Widgets.Label(new Rect(rect.x + 4, num, rect.width - 30, 16f), tag.LabelCap);
                    num += 16;
                }
                else
                {
                    GUI.color = Color.red;
                    Widgets.Label(new Rect(rect.x + 4, num, rect.width - 30, 16f), tag.LabelCap);
                    num += 16;

                    if (outpost.buildings.Where(o => o.tags != null && o.tags.Contains(tag)).FirstOrDefault() == null)
                    {
                        GUI.color = Color.gray;
                        Widgets.Label(new Rect(rect.x + 20, num, rect.width - 20, 16f), "TagNotAvailable".Translate());
                        num += 16;
                    }
                    else
                    {
                        bool first = true;
                        foreach (OutpostBuildingDef building in outpost.buildings.Where(o => o.tags != null && o.tags.Contains(tag)))
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                GUI.color = Color.gray;
                                Widgets.Label(new Rect(rect.x + 75, num, rect.width - 20, 16f), "OrGap".Translate());
                                num += 16;
                            }

                            GUI.color = Color.white;
                            Widgets.Label(new Rect(rect.x + 20, num, rect.width - 30, 16f), building.LabelCap);
                            num += 16;

                            if (building.skillRequirements != null)
                            {
                                foreach (SkillRequirement skillReq in building.skillRequirements)
                                {
                                    if (pawn == null || pawn.skills == null || pawn.skills.GetSkill(skillReq.skill).TotallyDisabled || pawn.skills.GetSkill(skillReq.skill).levelInt < skillReq.minLevel)
                                        GUI.color = Color.red;
                                    else
                                        GUI.color = Color.green;

                                    string text = skillReq.skill.LabelCap + " " + skillReq.minLevel;
                                    Widgets.Label(new Rect(rect.x + 40, num, rect.width - 40, 16f), text);
                                    num += 16;
                                }
                            }

                            if (building.requiredTraits != null)
                            {
                                foreach (TraitDef reqTrait in building.requiredTraits)
                                {
                                    if (pawn != null && pawn.story != null && pawn.story.traits != null && pawn.story.traits.HasTrait(reqTrait))
                                        GUI.color = Color.green;
                                    else
                                        GUI.color = Color.red;

                                    string text = reqTrait.degreeDatas[0].label.CapitalizeFirst();
                                    Widgets.Label(new Rect(rect.x + 40, num, rect.width - 40, 16f), text);
                                    num += 16;
                                }
                            }
                             
                            if (building.disallowedTraits != null)
                            {
                                foreach (TraitDef disTrait in building.disallowedTraits)
                                {
                                    if (pawn == null || pawn.story == null || pawn.story.traits == null || !pawn.story.traits.HasTrait(disTrait))
                                        GUI.color = Color.green;
                                    else
                                        GUI.color = Color.red;

                                    string text = "NotPrefix".Translate() + disTrait.degreeDatas[0].label.CapitalizeFirst();
                                    Widgets.Label(new Rect(rect.x + 40, num, rect.width - 40, 16f), text);
                                    num += 16;
                                }
                            }

                            if(building.prerequisiteSpecials != null)
                            {
                                foreach(SpecialPrerequisiteDef special in building.prerequisiteSpecials.Where(o => o.forPawn == true))
                                {
                                    if (special.isMetByPawn(pawn))
                                        GUI.color = Color.green;
                                    else
                                        GUI.color = Color.red;
                                    
                                    string text = special.LabelCap;
                                    Widgets.Label(new Rect(rect.x + 40, num, rect.width - 40, 16f), text);
                                    num += 16;
                                }
                            }
                        }
                    }                   
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = savedColor;
        }
        
        private void DrawSelfSufficiency(Rect rect)
        {
            /*GUI.color = Color.white;
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 36f), "Self-sufficiency");*/
        }

    }
}
