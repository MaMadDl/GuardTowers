using System.Collections.Generic;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;
using HarmonyLib;

namespace NGT
{

    public class ITab_Tower : ITab
    {
        private readonly List<Pawn> listInt = new List<Pawn>();


        public ITab_Tower()
        {
            
            labelKey = "TowerContents";
            size = new Vector2(500f, 470f);
        }

        public List<Pawn> container
        {
            get
            {
                var building_Casket = SelThing as BaseGuardTower;
                listInt.Clear();

                if (building_Casket is not { HasAnyContents: true })
                {
                    return listInt;
                }

                foreach (var pawn in building_Casket.GetInner())
                {
                    listInt.Add(pawn);

                }

                return listInt;
            }
        }
        private float SpecificNeedsTabWidth
        {
            get
            {
                if (this.specificNeedsTabForPawn.DestroyedOrNull())
                {
                    return 0f;
                }
                return NeedsCardUtility.GetSize(this.specificNeedsTabForPawn).x;
            }
        }

        private Vector2 scrollPosition;

        private float scrollViewHeight;

        private Pawn specificNeedsTabForPawn;

        private Vector2 thoughtScrollPosition;

        private bool doNeeds;


        protected override void FillTab()
        {

            if (container.Count < 0)
            {
                return;
            }
            this.EnsureSpecificNeedsTabForPawnValid();

            CaravanNeedsTabUtility.DoRows(size, container, null, ref scrollPosition, ref scrollViewHeight, ref specificNeedsTabForPawn, doNeeds);

        }
        protected override void UpdateSize()
        {
            this.EnsureSpecificNeedsTabForPawnValid();
            base.UpdateSize();
            this.size = CaravanNeedsTabUtility.GetSize(container, this.PaneTopY, true);
            if (this.size.x + this.SpecificNeedsTabWidth > (float)UI.screenWidth)
            {
                this.doNeeds = false;
                this.size = CaravanNeedsTabUtility.GetSize(container, this.PaneTopY, false);
            }
            else
            {
                this.doNeeds = true;
            }
            this.size.y = Mathf.Max(this.size.y, NeedsCardUtility.FullSize.y);
        }
        private void EnsureSpecificNeedsTabForPawnValid()
        {
            if (this.specificNeedsTabForPawn != null && (this.specificNeedsTabForPawn.Destroyed || !container.Contains(this.specificNeedsTabForPawn)))
            {
                this.specificNeedsTabForPawn = null;
            }
        }
        protected override void ExtraOnGUI()
        {
            this.EnsureSpecificNeedsTabForPawnValid();
            base.ExtraOnGUI();
            Pawn localSpecificNeedsTabForPawn = this.specificNeedsTabForPawn;
            if (localSpecificNeedsTabForPawn != null)
            {
                Rect tabRect = base.TabRect;
                float specificNeedsTabWidth = this.SpecificNeedsTabWidth;
                Rect rect = new Rect(tabRect.xMax - 1f, tabRect.yMin, specificNeedsTabWidth, tabRect.height);
                Find.WindowStack.ImmediateWindow(1439870015, rect, WindowLayer.GameUI, delegate
                {
                    if (localSpecificNeedsTabForPawn.DestroyedOrNull())
                    {
                        return;
                    }
                    NeedsCardUtility.DoNeedsMoodAndThoughts(rect.AtZero(), localSpecificNeedsTabForPawn, ref this.thoughtScrollPosition);
                    if (Widgets.CloseButtonFor(rect.AtZero()))
                    {
                        this.specificNeedsTabForPawn = null;
                        SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                    }
                }, true, false, 1f, null);
            }
        }

    }


    [HarmonyPatch(typeof(CaravanThingsTabUtility), nameof(CaravanThingsTabUtility.DoAbandonButton)
        , new[] { typeof(Rect), typeof(Thing),typeof(Caravan) })]
    static class guardtower_banishButtPatch
    {
        static bool Prefix(Rect rowRect, Thing t, Caravan caravan)
        {
            
            if (caravan == null)
            {
                return false;
            }
            return true;
        }
    }


}