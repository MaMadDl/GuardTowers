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
            Text.Font = GameFont.Small;
            Rect rect3 = new Rect(rect2.x, rect2.y - 2f, rect2.width, 100f);
            string NameStr = TwName;
            Widgets.Label(rect3, NameStr.Truncate(rect3.width, null));
            float num = rect2.y + Text.LineHeight + Text.SpaceBetweenLines + 7f;
            GUI.color = Color.white;
            Text.Font = GameFont.Tiny;
            int num2 = 0;
            foreach (var stat in StatKeys)
            {
                Rect rect4;
                if (num2 % 2 == 0)
                {
                    rect4 = new Rect(rect2.x, num, rect2.width / 2f, 100f);
                }
                else
                {
                    rect4 = new Rect(rect2.x + rect2.width / 2f, num, rect2.width / 2f, 100f);
                }
         
                Widgets.Label(rect4, stat.ToString().Truncate(rect4.width, null));
                if (num2 % 2 == 1)
                {
                    num += Text.LineHeight + Text.SpaceBetweenLines;
                }
                num2++;

            }
            GUI.color = Color.white;
            Text.Font = GameFont.Small;
            GUI.EndGroup();
            Text.WordWrap = true;
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
