using System.Collections.Generic;
using UnityEngine;
using TurnGame;

namespace TurnGame.Student
{
    public class TurnScheduler : ITurnScheduler
    {
        public int SelectNext(IReadOnlyList<BattleSnapshot> units)
        {
            // TODO 01: best를 -1로 시작하세요.
            int best = -1;
            // TODO 02: 살아 있는 유닛을 순회하세요.
            // TODO 03: NextActionTime이 가장 작은 인덱스를 저장하세요.
            // TODO 04: 시간이 같으면 Speed가 큰 유닛을 먼저 선택하세요.
            // 미완성 상태에서는 첫 번째 생존 유닛을 선택합니다.
            for (int i = 0; i < units.Count; i++)
                if (units[i].IsAlive) return i;
            return best;
        }

        public float CalculateDelay(int speed, int speedBonus)
        {
            // TODO 05: 실제 속도 = speed + speedBonus를 계산하세요.
            // TODO 06: 실제 속도를 최소 1로 제한하세요.
            // TODO 07: 100 / 실제 속도를 반환하세요.
            return 10f;
        }
    }
}
