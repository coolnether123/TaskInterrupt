using RimWorld;
using TaskInterrupt.Bootstrap;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TaskInterrupt.Runtime
{
    /// <summary>
    /// Purely cosmetic. Goofy mode swaps the mod's wording for its sneeze
    /// variants and plays a small effect when a task actually stops. Nothing
    /// here changes which tasks can be interrupted or when.
    /// </summary>
    internal static class GoofyMode
    {
        private const string AchooSoundDefName = "TaskInterrupt_Achoo";

        private static readonly Color PuffColor =
            new Color(0.86f, 0.91f, 1f);
        private static readonly Color TextColor =
            new Color(1f, 0.97f, 0.75f);

        private static readonly string[] ShoutKeys =
        {
            "TaskInterrupt_Goofy_Achoo1",
            "TaskInterrupt_Goofy_Achoo2",
            "TaskInterrupt_Goofy_Achoo3"
        };

        internal static bool Active
        {
            get
            {
                TaskInterrupt.Settings.TaskInterruptSettings settings =
                    TaskInterruptMod.Settings;
                return settings != null && settings.GoofyMode;
            }
        }

        /// <summary>
        /// Plays the sneeze over <paramref name="pawn"/>. Safe to call for a
        /// pawn on any map or none; it simply does nothing when there is
        /// nowhere to draw.
        /// </summary>
        internal static void Celebrate(Pawn pawn)
        {
            if (!Active || pawn == null || !pawn.Spawned)
            {
                return;
            }

            Map map = pawn.Map;
            if (map == null)
            {
                return;
            }

            Vector3 origin = pawn.DrawPos;
            Vector3 ahead = origin +
                (pawn.Rotation.FacingCell.ToVector3() * 0.45f);

            FleckMaker.ThrowDustPuffThick(ahead, map, 1.7f, PuffColor);
            FleckMaker.ThrowAirPuffUp(origin, map);
            MoteMaker.ThrowText(
                origin + new Vector3(0f, 0f, 0.8f),
                map,
                Shout(pawn),
                TextColor,
                2.4f);

            SoundDef sound =
                DefDatabase<SoundDef>.GetNamedSilentFail(AchooSoundDefName);
            if (sound != null)
            {
                sound.PlayOneShot(
                    SoundInfo.InMap(new TargetInfo(pawn.Position, map)));
            }
        }

        /// <summary>
        /// Varies the shout without touching the game's random stream, so a
        /// cosmetic setting cannot perturb simulation determinism.
        /// </summary>
        private static string Shout(Pawn pawn)
        {
            int seed = pawn.thingIDNumber;
            if (Find.TickManager != null)
            {
                seed += Find.TickManager.TicksGame / 60;
            }

            int index = Mathf.Abs(seed) % ShoutKeys.Length;
            return ShoutKeys[index].Translate();
        }
    }
}
