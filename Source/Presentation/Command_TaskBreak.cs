using RimWorld;
using Spine.UI.ContextualSettings;
using TaskBreak.Bootstrap;
using TaskBreak.Definitions;
using TaskBreak.Domain;
using TaskBreak.Runtime;
using UnityEngine;
using Verse;

namespace TaskBreak.Presentation
{
    internal sealed class Command_TaskBreak : Command_Action
    {
        private const int SharedGroupKey = 188137392;

        internal Command_TaskBreak()
        {
            defaultLabel = "TaskBreak_Command".Translate();
            defaultDesc = "TaskBreak_Command_Tip".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt");
            hotKey = TaskBreakDefOf.TaskBreak_CancelCurrentTask;
            groupKey = SharedGroupKey;
            alsoClickIfOtherInGroupClicked = false;
            Order = float.MaxValue;
            shrinkable = true;
            action = TaskBreakController.BreakSelected;

            TaskBreakDecision decision =
                TaskBreakController.FirstDecision();
            if (!decision.CanBreak)
            {
                Disable(TaskBreakText.Reason(decision.BlockReason));
            }
        }

        public override GizmoResult GizmoOnGUI(
            Vector2 topLeft,
            float maxWidth,
            GizmoRenderParms parms)
        {
            if (BindSettings(new Rect(
                topLeft.x,
                topLeft.y,
                GetWidth(maxWidth),
                75f)))
            {
                return new GizmoResult(GizmoState.Clear);
            }

            return base.GizmoOnGUI(topLeft, maxWidth, parms);
        }

        public override GizmoResult GizmoOnGUIShrunk(
            Vector2 topLeft,
            float size,
            GizmoRenderParms parms)
        {
            if (BindSettings(new Rect(topLeft.x, topLeft.y, size, size)))
            {
                return new GizmoResult(GizmoState.Clear);
            }

            return base.GizmoOnGUIShrunk(topLeft, size, parms);
        }

        private static bool BindSettings(Rect visibleRect)
        {
            return TaskBreakMod.ContextualSettings?.Bind(
                    visibleRect,
                    ContextualSettingsTarget.Exact(
                        "controls.gizmo",
                        "controls.header")) == true;
        }
    }
}
