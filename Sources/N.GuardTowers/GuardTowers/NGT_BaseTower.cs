using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
namespace NGT
{
    
    public class BaseGuardTower : Building_TurretGun , IThingHolder
    {
        //public int Directions;
        public List<String> Stats;
        
        protected ThingOwner<Pawn> innerContainer;
        public int bonusRange;
        public int Capacity;

        public BaseGuardTower(int cap, int range)
        {
            bonusRange = range;
            innerContainer = new ThingOwner<Pawn>(this, false);
            Stats = new List<string>();
            Capacity = cap;
        }
    
        public bool HasAnyContents => innerContainer.Count > 0;

        public Thing ContainedThing => innerContainer.Count != 0 ? innerContainer[0] : null;

        public bool CanOpen => HasAnyContents;

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            UpdateRange();
        }

        public ThingOwner<Pawn> GetInner()
        {
            return innerContainer;
        }

        public override void TickRare()
        {
            base.TickRare();
            innerContainer.ThingOwnerTickRare();
        }


        public override void Tick()
        {
            if (innerContainer.Count < 1)
            {
                return;
            }

            base.Tick();
            innerContainer.ThingOwnerTick();
        }

        public virtual void Open()
        {
            if (!HasAnyContents)
            {
                return;
            }

            EjectAllContents();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look(ref Directions, "Directions");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }

        public override bool ClaimableBy(Faction fac)
        {
            if (!innerContainer.Any)
            {
                return base.ClaimableBy(fac);
            }

            foreach (var pawn in innerContainer)
            {
                if (pawn.Faction != fac)
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public virtual bool Accepts(Thing thing)
        {
            return innerContainer.CanAcceptAnyOf(thing);
        }

        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!Accepts(thing))
            {
                return false;
            }

            bool transfer;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
                transfer = true;
            }
            else
            {
                transfer = innerContainer.TryAdd(thing);
            }

            if (transfer)
            {
                var eqName = ((Pawn)thing).equipment.Primary.Label.ToString().CapitalizeFirst();
                var str = ((Pawn)thing).ToString() + $" ( {eqName} )";
                Stats.Add(str);

                foreach (var defVerb in ((Pawn)thing).equipment.Primary.def.Verbs)
                {
                    defVerb.range += bonusRange;
                }

            }
            UpdateRange();
            return transfer;
        }

        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (innerContainer.Count > 0 )
            {
                
                (AttackVerb as Verb_GuardTowers)?.ResetVerb();
                if (mode == DestroyMode.Deconstruct)
                {
                    if (innerContainer.InnerListForReading.Where(p => p.InMentalState).Any())
                    {
                        // add top Message to inform about mental break Later 
                        return;
                    }

                    EjectAllContents();

                }
                else if (mode == DestroyMode.KillFinalize)
                {
                    FixBonusStats(GetInner().InnerListForReading);

                    innerContainer.TryDropAll(this.InteractionCell, Map, ThingPlaceMode.Near, (pawn, res) => pawn.Kill(new DamageInfo(DamageDefOf.Crush, 1000)));
                }
                innerContainer.ClearAndDestroyContents(mode);
                
            }

            base.Destroy(mode);
        }

        public void FixBonusStats(List<Pawn> pawns)
        {
            foreach (var pawn in pawns)
            {
                foreach (var defVerb in pawn.equipment.Primary.def.Verbs)
                {
                    
                    defVerb.range -= bonusRange;
                }
            }
        }
        public virtual void EjectAllContents()
        {
            Log.Warning(innerContainer.Count.ToString());
            foreach(var p in innerContainer.InnerListForReading)
            { 
                foreach(var vb in p.equipment.PrimaryEq.AllVerbs)
                {
                    Log.Warning(vb.caster.ToString());
                }
            }

            if(innerContainer.InnerListForReading.Where(p => p.InMentalState).Any())
            {
                return;
            }
            (AttackVerb as Verb_GuardTowers)?.ResetVerb();
            
            FixBonusStats(GetInner().InnerListForReading);

            Stats.Clear();

            innerContainer.TryDropAll(this.InteractionCell, Map, ThingPlaceMode.Near, 
                                        (p, ret) => (p as Pawn).equipment.PrimaryEq.AllVerbs.ForEach(v => v.caster = p));

        }
       
        public override IEnumerable<FloatMenuOption> GetMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            foreach (var o in base.GetMultiSelectFloatMenuOptions(selPawns))
            {
                yield return o;
            }

            if (innerContainer.Count == Capacity)
            {
                yield break;
            }

            var assignedPawns = innerContainer.Count;
            var pawnList = new List<Pawn>();

            foreach (var pawn in selPawns)
            {
                if (pawn.equipment.Primary != null)
                {
                    if (!pawn.equipment.Primary.def.IsRangedWeapon)
                    {
                        yield break;
                    }
                }
                else
                {
                    yield break;
                }

                if (assignedPawns >= Capacity)
                {
                    yield break;
                }

                pawnList.Add(pawn);
            }

            void jobAction()
            {
                MultiEnter(pawnList);
            }

            yield return new FloatMenuOption("NGT_EnterTower".Translate(), jobAction);
        }

        private void MultiEnter(List<Pawn> pawnsToEnter)
        {
            var jobDef = DefDatabase<JobDef>.GetNamed("NGT_EnterTower");
            foreach (var pawn in pawnsToEnter)
            {
                var job = new Job(jobDef, this);
                bool ret = pawn.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
               
            }
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (var o in base.GetFloatMenuOptions(myPawn))
            {
                yield return o;
            }

            if(myPawn.equipment.Primary != null)
            {
                if (!myPawn.equipment.Primary.def.IsRangedWeapon)
                {
                    yield break;
                }
            }
            else
            {
                yield break;
            }
            if (innerContainer.Count >= Capacity )
            {
                yield break;
            }
            var jobDef = DefDatabase<JobDef>.GetNamed("NGT_EnterTower"); //JobDefOf.EnterCryptosleepCasket;
            string jobStr = "NGT_EnterTower".Translate();

            void jobAction()
            {
                var job = new Job(jobDef, this);
                bool ret= myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);      
            }

            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction),
                myPawn, this);
        }
        // change icon UI Button
        public override IEnumerable<Gizmo> GetGizmos()
        {

            if (innerContainer.InnerListForReading.Where(p => p.InMentalState).Any())
            {
                yield break;
            }

            var stats = new Gizmo_TowerStat
            {
                StatKeys = Stats

            };
            stats.order = -1000; //big number to push this to back :) 
            yield return stats;


            foreach (var c in base.GetGizmos())
            {
                yield return c;
            }

            if (Faction == Faction.OfPlayer && innerContainer.Count > 0)
            {
                var eject = new Command_Action 
                {
                    action = SelectColonist,
                    defaultLabel = "ExitTower".Translate(),
                    defaultDesc = "ExitTowerDesc".Translate()
                };
                if (innerContainer.Count == 0)
                {
                    eject.Disable("CommandPodEjectFailEmpty".Translate());
                }

                eject.hotKey = KeyBindingDefOf.Misc1;
                eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
                yield return eject;
            }
        }

        private void SelectColonist()
        {
            var list = new List<FloatMenuOption>();
            if (innerContainer.Count == 0)
            {
                return;
            }

            foreach (var pawn in innerContainer.InnerListForReading.Where(p => !p.InMentalState))
            {   
                var postfix = new TaggedString();
                if (pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
                {
                    postfix = $" ({pawn.equipment.Primary.def.label})";
                }

                var textToAdd = $"{pawn.NameFullColored}{postfix}";
                var pawnToEject = pawn;
                list.Add(new FloatMenuOption(textToAdd,
                    delegate
                    {                        
                        //innerContainer.TryDrop(pawnToEject, Toils_Towers.GetEnterOutLoc(this), Map, ThingPlaceMode.Near, out _);
                        foreach (var defVerb in pawnToEject.equipment.Primary.def.Verbs)
                        {
                            defVerb.range -= bonusRange;                            
                        }
                        pawnToEject.DrawGUIOverlay();
                        
                        var index=innerContainer.InnerListForReading.FindIndex(p=> p == pawnToEject);
                        Stats.RemoveAt(index);

                        innerContainer.TryDrop(pawnToEject, this.InteractionCell, Map, ThingPlaceMode.Near,out _,
                                                (p, ret) => (p as Pawn).equipment.PrimaryEq.AllVerbs.ForEach(v => v.caster = p)) ;
                    }, MenuOptionPriority.Default, null, null, 29f));
            }

            var sortedList = list.OrderBy(option => option.Label).ToList();
            sortedList.Add(new FloatMenuOption("Everyone".Translate(), EjectAllContents,
                MenuOptionPriority.Default, null, null, 29f));
            Find.WindowStack.Add(new FloatMenu(sortedList));
        }

        public void UpdateRange()
        {
            var maxRange = 0f;
            foreach (var pawn in innerContainer)
            {
                if (pawn.equipment.Primary == null || !pawn.equipment.Primary.def.IsRangedWeapon)
                {
                    continue;
                }

                foreach (var defVerb in pawn.equipment.Primary.def.Verbs)
                {
                    if (defVerb.range > maxRange)
                    {
                        maxRange = defVerb.range;
                    }
                }
            }

            AttackVerb.verbProps.range = maxRange;
        }

        public override string GetInspectString()
        {
            var text = base.GetInspectString();
            
            var str = $"{innerContainer.Count}/{Capacity}";

            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            if (innerContainer.InnerListForReading.Where(p => p.InMentalState).Any())
            {
                str += "\nOccupied, Someone Is Breaking Inside";
            }


            return text +"Range: " + bonusRange.ToString() + "\nCapacity: " + str.CapitalizeFirst() +
                   (innerContainer.Count == Capacity ? "(Full)" : "");
        }
    }

    public class SimpleGT : BaseGuardTower
    {
        public SimpleGT() : base(1, 4) { }

    }

    public class ConcreteGT : BaseGuardTower
    {
        public ConcreteGT() : base(2, 6) { }
    }

    public class FortifiedGT : BaseGuardTower
    {
        public FortifiedGT() : base(3, 3) { }
    }
}
