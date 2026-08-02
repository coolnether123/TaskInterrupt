using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using TaskBreak.Bootstrap;
using TaskBreak.Presentation;
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
            installed = true;
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
