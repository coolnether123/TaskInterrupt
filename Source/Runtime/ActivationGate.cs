using System.Collections.Generic;

namespace TaskBreak.Runtime
{
    public sealed class ActivationGate
    {
        private readonly int cooldownTicks;
        private readonly Dictionary<int, int> lastActivationByPawn =
            new Dictionary<int, int>();

        public ActivationGate(int cooldownTicks)
        {
            this.cooldownTicks = cooldownTicks < 1 ? 1 : cooldownTicks;
        }

        public bool TryEnter(int pawnId, int currentTick)
        {
            if (lastActivationByPawn.TryGetValue(pawnId, out int lastTick) &&
                currentTick >= lastTick &&
                currentTick - lastTick < cooldownTicks)
            {
                return false;
            }

            lastActivationByPawn[pawnId] = currentTick;
            if (lastActivationByPawn.Count > 256)
            {
                RemoveExpired(currentTick);
            }

            return true;
        }

        private void RemoveExpired(int currentTick)
        {
            var expired = new List<int>();
            foreach (KeyValuePair<int, int> entry in lastActivationByPawn)
            {
                if (currentTick - entry.Value >= cooldownTicks)
                {
                    expired.Add(entry.Key);
                }
            }

            foreach (int pawnId in expired)
            {
                lastActivationByPawn.Remove(pawnId);
            }
        }
    }
}
