using System.Collections.Generic;
using TurnGame;

namespace TurnGame.Student
{
    public class TargetSelector : ITargetSelector
    {
        public int SelectTarget(IReadOnlyList<BattleSnapshot> units, BattleTeam targetTeam)
        {
            // TODO 08: best를 -1로 시작하세요.
            int best = -1;
            // TODO 09: 죽은 유닛과 다른 팀은 건너뛰세요.
            // TODO 10: HP 비율이 가장 낮은 대상을 선택하세요.
            // TODO 11: HP 비율이 같으면 현재 HP가 낮은 대상을 선택하세요.
            // 미완성 상태에서는 조건에 맞는 첫 번째 대상을 선택합니다.
            for (int i = 0; i < units.Count; i++)
                if (units[i].IsAlive && units[i].Team == targetTeam) return i;
            return best;
        }
    }
}
