using System.Collections.Generic;
using TurnGame;

namespace TurnGame.Student
{
    public class TargetSelector : ITargetSelector
    {
        public int SelectTarget(IReadOnlyList<BattleSnapshot> units, BattleTeam targetTeam)
        {
            int best = -1;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive || units[i].Team != targetTeam) continue;
                if (best < 0) { best = i; continue; }

                long currentRatio = (long)units[i].HP * units[best].MaxHP;
                long bestRatio = (long)units[best].HP * units[i].MaxHP;

                if (currentRatio < bestRatio ||
                    (currentRatio == bestRatio && units[i].HP < units[best].HP))
                {
                    best = i;
                }
            }

            return best;
        }
    }
}
