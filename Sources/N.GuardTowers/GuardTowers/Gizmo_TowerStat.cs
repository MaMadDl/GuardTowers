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
    class Gizmo_TowerStat : Gizmo
    {
        //public Dictionary<String, String> StatKeys;
        public String TwName = "test";
        public List<String> StatKeys;
        public Gizmo_TowerStat()
        {
            //StatKeys = new Dictionary<String, String>();
        }

        public override float GetWidth(float maxWidth)
        {
            return Mathf.Min(300f, maxWidth);
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Widgets.DrawWindowBackground(rect);
            Text.WordWrap = false;
            GUI.BeginGroup(rect);
            Rect rect2 = rect.AtZero().ContractedBy(10f);
            float num =  7f;
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;

            foreach (var stat in StatKeys)
            {
                Rect rect4;
                rect4 = new Rect(rect2.x, num, rect2.width , 100f);         

                Widgets.Label(rect4, stat.ToString().Truncate(rect4.width, null));
                num += Text.LineHeight + Text.SpaceBetweenLines;
                

            }
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            GUI.EndGroup();
            Text.WordWrap = true;
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
