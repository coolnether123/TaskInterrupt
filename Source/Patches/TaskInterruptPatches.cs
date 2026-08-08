using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaskInterrupt.Bootstrap;
using TaskInterrupt.Compatibility;
using TaskInterrupt.Presentation;
#if TASK_INTERRUPT_USE_SPINE
using Spine.Api;
using Spine.Harmony;
#endif
using Verse;

namespace TaskInterrupt.Patches
{
    /// <summary>
    /// Connects the vanilla pawn-gizmo extension point to the native command
    /// while keeping Harmony ownership outside presentation and policy.
    /// </summary>
    internal static class TaskInterruptPatches
    {
        private const string HarmonyId = "CoolNether123.TaskInterrupt";
        private static bool installed;
#if TASK_INTERRUPT_USE_SPINE
        private static readonly IHarmonyPatchInstaller Installer =
            SpineApi.Patching.CreateInstaller(HarmonyId, "[Interrupt Task]");
#else
        private static readonly Harmony Harmony = new Harmony(HarmonyId);
#endif

        internal static void Install()
        {
            if (installed)
            {
                return;
            }

#if TASK_INTERRUPT_HAS_GET_GIZMOS
#if TASK_INTERRUPT_USE_SPINE
            Installer.TryPatch(
                "pawn gizmos",
                AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyMethod(
                    typeof(TaskInterruptPatches),
                    nameof(PawnGizmosPostfix)));
#else
            Harmony.Patch(
                AccessTools.Method(typeof(Pawn), "GetGizmos"),
                postfix: new HarmonyMethod(
                    typeof(TaskInterruptPatches),
                    nameof(PawnGizmosPostfix)));
#endif
#else
            Harmony.Patch(
                AccessTools.Method(typeof(Pawn), "GetCommands"),
                postfix: new HarmonyMethod(
                    typeof(TaskInterruptPatches),
                    nameof(PawnCommandsPostfix)));
#endif
            installed = true;
        }

#if TASK_INTERRUPT_HAS_GET_GIZMOS
        private static void PawnGizmosPostfix(
            Pawn __instance,
            ref IEnumerable<Gizmo> __result)
        {
            if (!TaskInterruptMod.Settings.ShowGizmo ||
                __instance == null ||
                !TaskInterruptApi.IsPlayerControlled(__instance))
            {
                return;
            }

            // A drafted pawn has no civilian task to interrupt, and the drafted
            // gizmo row is already crowded. Offering a permanently disabled
            // command there is noise, so contribute nothing at all. A mixed
            // selection still gets the command from its undrafted pawns, and
            // the drafted ones are reported as skipped when it is used.
            if (TaskInterruptApi.IsDrafted(__instance))
            {
                return;
            }

            __result = (__result ?? Enumerable.Empty<Gizmo>())
                .Concat(new Gizmo[] { new Command_TaskInterrupt() });
        }
#else
        private static void PawnCommandsPostfix(
            Pawn __instance,
            ref IEnumerable<Command> __result)
        {
            if (__instance == null ||
                !TaskInterruptApi.IsPlayerControlled(__instance))
            {
                return;
            }

            __result = (__result ?? Enumerable.Empty<Command>())
                .Concat(new Command[] { new Command_TaskInterrupt() });
        }
#endif
    }
}
