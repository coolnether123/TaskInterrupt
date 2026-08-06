using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaskInterrupt.Bootstrap;
using TaskInterrupt.Presentation;
using Spine.Api;
using Spine.Harmony;
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
        private static readonly IHarmonyPatchInstaller Installer =
            SpineApi.Patching.CreateInstaller(HarmonyId, "[Task Interrupt]");

        internal static void Install()
        {
            Installer.TryPatch(
                "pawn gizmos",
                AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyMethod(
                    typeof(TaskInterruptPatches),
                    nameof(PawnGizmosPostfix)));
        }

        private static void PawnGizmosPostfix(
            Pawn __instance,
            ref IEnumerable<Gizmo> __result)
        {
            if (!TaskInterruptMod.Settings.ShowGizmo ||
                __instance == null ||
                !(__instance.IsColonistPlayerControlled ||
                  __instance.IsColonyMechPlayerControlled))
            {
                return;
            }

            // A drafted pawn has no civilian task to interrupt, and the drafted
            // gizmo row is already crowded. Offering a permanently disabled
            // command there is noise, so contribute nothing at all. A mixed
            // selection still gets the command from its undrafted pawns, and
            // the drafted ones are reported as skipped when it is used.
            if (__instance.Drafted)
            {
                return;
            }

            __result = (__result ?? Enumerable.Empty<Gizmo>())
                .Concat(new Gizmo[] { new Command_TaskInterrupt() });
        }
    }
}
