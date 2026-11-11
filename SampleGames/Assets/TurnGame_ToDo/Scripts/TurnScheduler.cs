using System.Collections.Generic;
using UnityEngine;
using TurnGame;

namespace TurnGame.Student
{
    public class TurnScheduler : ITurnScheduler
    {
        public int SelectNext(IReadOnlyList<BattleSnapshot> units)
        {
            int best = -1;
            for (int i = 0; i < units.Count; i++)
            {
                if (!units[i].IsAlive) continue;

                if (best < 0)
                {
                    best = i;
                }
                else if (Mathf.Approximately(units[i].NextActionTime, units[best].NextActionTime))
                {
                    if (units[i].Speed > units[best].Speed)
                    {
                        best = i;
                    }
                }
                else if (units[i].NextActionTime < units[best].NextActionTime)
                {
                    best = i;
                }
            }

            return best;
        }

        public float CalculateDelay(int speed, int speedBonus)
        {
            // TODO 05: 실제 속도 = speed + speedBonus를 계산하세요.
            // TODO 06: 실제 속도를 최소 1로 제한하세요.
            // TODO 07: 100 / 실제 속도를 반환하세요.
            int effectiveSpeed = Mathf.Max(1, speed + speedBonus);
            return 100f / effectiveSpeed;
        }
    }
}
