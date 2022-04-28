using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using Verse;

namespace NGT
{
    public class MB_Shootout : MentalState
    {
		public override bool ForceHostileTo(Thing t)
		{
			return true;
		}
		public override bool ForceHostileTo(Faction f)
		{
			return true;
		}
		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}
        public override void PreStart()
        {
            base.PreStart();
        }
    }
}
