using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaskBreak.Bootstrap;
using TaskBreak.Presentation;
using Spine.Api;
using Spine.Harmony;
using Verse;

namespace TaskBreak.Patches
{
    internal static class TaskBreakPatches
    {
        private const string HarmonyId = "CoolNether123.TaskBreak";
        private static readonly IHarmonyPatchInstaller Installer =
            SpineApi.Patching.CreateInstaller(HarmonyId, "[Task Break]");

        internal static void Install()
        {
            Installer.TryPatch(
                "pawn gizmos",
                AccessTools.Method(typeof(Pawn), nameof(Pawn.GetGizmos)),
                postfix: new HarmonyMethod(
                    typeof(TaskBreakPatches),
                    nameof(PawnGizmosPostfix)));
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
