using RimWorld;
using Spine.UI.ContextualSettings;
using TaskInterrupt.Bootstrap;
using TaskInterrupt.Definitions;
using TaskInterrupt.Domain;
using TaskInterrupt.Runtime;
using UnityEngine;
using Verse;

namespace TaskInterrupt.Presentation
{
    /// <summary>
    /// Exposes interruption through a grouped native command so RimWorld owns
    /// key routing while Spine can provide the contextual settings gesture.
    /// </summary>
    internal sealed class Command_TaskInterrupt : Command_Action
    {
        private const int SharedGroupKey = 188137392;

        internal Command_TaskInterrupt()
        {
            defaultLabel = TaskInterruptText.Translate("TaskInterrupt_Command");
            defaultDesc = TaskInterruptText.Translate("TaskInterrupt_Command_Tip");
            icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt");
            hotKey = TaskInterruptDefOf.TaskInterrupt_CancelCurrentTask;
            groupKey = SharedGroupKey;
            alsoClickIfOtherInGroupClicked = false;
            Order = float.MaxValue;
            shrinkable = true;
            action = TaskInterruptController.ActivateSelected;

            TaskInterruptDecision decision =
                TaskInterruptController.FirstDecision();
            if (!decision.CanBreak)
            {
                Disable(TaskInterruptText.Reason(decision.BlockReason));
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
            return TaskInterruptMod.ContextualSettings?.Bind(
                    visibleRect,
                    ContextualSettingsTarget.Exact(
                        "controls.gizmo",
                        "controls.header")) == true;
        }
    }
}
