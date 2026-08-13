using UnityEngine;
using Verse;
using TaskInterrupt.Compatibility;

namespace TaskInterrupt.Settings
{
    /// <summary>
    /// Stores global presentation and confirmation preferences; interruption
    /// safety itself deliberately remains fixed policy rather than save data.
    /// </summary>
    public sealed class TaskInterruptSettings
#if TASK_INTERRUPT_USE_SPINE || TASK_INTERRUPT_HAS_MOD_SETTINGS
        : ModSettings
#endif
    {
        public bool ShowGizmo = true;
        public bool ConfirmForcedTasks = true;
        public bool GoofyMode;

#if TASK_INTERRUPT_USE_SPINE
        public override void ExposeData()
        {
            TaskInterruptSettingsRegistry.Schema.Scribe(this);
            base.ExposeData();
        }
#elif TASK_INTERRUPT_HAS_MOD_SETTINGS
        public override void ExposeData()
        {
#if TASK_INTERRUPT_LEGACY_SCRIBE
            TaskInterruptApi.LookBool(ref ShowGizmo, "showGizmo", true);
            TaskInterruptApi.LookBool(ref ConfirmForcedTasks, "confirmForcedTasks", true);
            TaskInterruptApi.LookBool(ref GoofyMode, "goofyMode", false);
#else
            Scribe_Values.Look(ref ShowGizmo, "showGizmo", true);
            Scribe_Values.Look(ref ConfirmForcedTasks, "confirmForcedTasks", true);
            Scribe_Values.Look(ref GoofyMode, "goofyMode", false);
#endif
        }

        internal void Draw(Rect inRect)
        {
            Rect row = new Rect(inRect.x, inRect.y, inRect.width, 28f);
            Widgets.CheckboxLabeled(
                row,
                "TaskInterrupt_Settings_ShowGizmo".Translate(),
                ref ShowGizmo);
            row.y += 34f;
            Widgets.CheckboxLabeled(
                row,
                "TaskInterrupt_Settings_ConfirmForced".Translate(),
                ref ConfirmForcedTasks);
            row.y += 34f;
            Widgets.CheckboxLabeled(
                row,
                "TaskInterrupt_Settings_Goofy".Translate(),
                ref GoofyMode);
        }
#endif
    }
}
