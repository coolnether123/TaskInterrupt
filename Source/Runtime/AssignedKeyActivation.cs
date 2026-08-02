using System;

namespace TaskBreak.Runtime
{
    internal static class AssignedKeyActivation
    {
        internal static bool IsPressed(
            int primary,
            int secondary,
            bool sideButtonsOnly,
            int firstSideButton,
            int lastSideButton,
            Func<int, bool> keyDown)
        {
            if (keyDown == null)
            {
                throw new ArgumentNullException(nameof(keyDown));
            }

            bool primaryEligible = primary != 0 &&
                (!sideButtonsOnly ||
                 primary >= firstSideButton && primary <= lastSideButton);
            if (primaryEligible &&
                keyDown(primary))
            {
                return true;
            }

            bool secondaryEligible = secondary != 0 &&
                (!sideButtonsOnly ||
                 secondary >= firstSideButton && secondary <= lastSideButton);
            return secondary != primary &&
                secondaryEligible &&
                keyDown(secondary);
        }
    }
}
