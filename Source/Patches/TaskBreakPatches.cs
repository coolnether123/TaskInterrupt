using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using TaskBreak.Bootstrap;
using TaskBreak.Definitions;
using TaskBreak.Presentation;
using TaskBreak.Runtime;
using UnityEngine;
using Verse;

namespace TaskBreak.Patches
{
    internal static class TaskBreakPatches
    {
        private const string HarmonyId = "CoolNether123.TaskBreak";
        private static bool installed;

        internal static void Install()
        {
            if (installed)
            {
                return;
            }

            var harmony = new Harmony(HarmonyId);
            harmony.Patch(
                AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyMethod(
                    typeof(TaskBreakPatches),
                    nameof(PawnGizmosPostfix)));
            harmony.Patch(
                AccessTools.Method(
                    typeof(Dialog_DefineBinding),
                    nameof(Dialog_DefineBinding.DoWindowContents)),
                prefix: new HarmonyMethod(
                    typeof(TaskBreakPatches),
                    nameof(BindingDialogPrefix)));
            harmony.Patch(
                AccessTools.Method(
                    typeof(UIRoot_Play),
                    nameof(UIRoot_Play.UIRootUpdate)),
                postfix: new HarmonyMethod(
                    typeof(TaskBreakPatches),
                    nameof(PlayUiUpdatePostfix)));
            installed = true;
        }

        private static bool BindingDialogPrefix(
            Dialog_DefineBinding __instance,
            KeyBindingDef ___keyDef,
            KeyPrefs.BindingSlot ___slot,
            KeyPrefsData ___keyPrefsData)
        {
            if (___keyDef != TaskBreakDefOf.TaskBreak_CancelCurrentTask)
            {
                return true;
            }

            Event current = Event.current;
            if (current == null ||
                current.type != EventType.MouseDown ||
                current.button < 3 ||
                current.button > 6)
            {
                return true;
            }

            KeyCode keyCode =
                (KeyCode)((int)KeyCode.Mouse0 + current.button);

            ___keyPrefsData.EraseConflictingBindingsForKeyCode(
                ___keyDef,
                keyCode,
                oldDef => Messages.Message(
                    "KeyBindingOverwritten".Translate(oldDef.LabelCap),
                    MessageTypeDefOf.TaskCompletion,
                    historical: false));
            ___keyPrefsData.SetBinding(___keyDef, ___slot, keyCode);
            __instance.Close();
            current.Use();
            return false;
        }

        private static void PlayUiUpdatePostfix()
        {
            KeyPrefsData keyPrefs = KeyPrefs.KeyPrefsData;
            KeyBindingDef keyDef =
                TaskBreakDefOf.TaskBreak_CancelCurrentTask;
            if (Current.ProgramState != ProgramState.Playing ||
                Find.WindowStack == null ||
                Find.WindowStack.Count > 0 ||
                Find.WindowStack.AnySearchWidgetFocused ||
                keyPrefs == null ||
                keyDef == null)
            {
                return;
            }

            KeyCode primary = keyPrefs.GetBoundKeyCode(
                keyDef,
                KeyPrefs.BindingSlot.A);
            KeyCode secondary = keyPrefs.GetBoundKeyCode(
                keyDef,
                KeyPrefs.BindingSlot.B);
            bool sideButtonsOnly = TaskBreakMod.Settings.ShowGizmo;
            if (AssignedKeyActivation.IsPressed(
                (int)primary,
                (int)secondary,
                sideButtonsOnly,
                (int)KeyCode.Mouse3,
                (int)KeyCode.Mouse6,
                key => Input.GetKeyDown((KeyCode)key)))
            {
                TaskBreakController.ActivateSelected();
            }
        }

        private static void PawnGizmosPostfix(
            Pawn __instance,
            ref IEnumerable<Gizmo> __result)
        {
            if (!TaskBreakMod.Settings.ShowGizmo ||
                __instance == null ||
                !(__instance.IsColonistPlayerControlled ||
                  __instance.IsColonyMechPlayerControlled))
            {
                return;
            }

            __result = (__result ?? Enumerable.Empty<Gizmo>())
                .Concat(new Gizmo[] { new Command_TaskBreak() });
        }
    }
}
