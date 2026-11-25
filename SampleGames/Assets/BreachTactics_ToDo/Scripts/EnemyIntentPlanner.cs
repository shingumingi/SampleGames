using System.Collections.Generic;
using System;
using Unity.VisualScripting;

namespace BreachTactics.Student
{
    /// <summary>
    /// 1단계 실습: 적마다 목표와 공격 방향을 결정합니다.
    /// 모든 함수가 존재하므로 TODO를 작성하는 중에도 컴파일됩니다.
    /// </summary>
    public sealed class EnemyIntentPlanner : IEnemyIntentPlanner
    {
        public List<EnemyIntent> Plan(IReadOnlyList<BreachUnitState> units)
        {
            // TODO 01: 결과 목록을 만드세요.
            // TODO 02: 공격 가능한 목표를 수집하세요.
            // TODO 03: 살아 있는 적마다 가장 가까운 목표를 찾으세요.
            // TODO 04: 목표 방향을 계산해 EnemyIntent를 추가하세요.

            var result = new List<EnemyIntent>();

            if(units == null)
            {
                return result;
            }

            List<BreachUnitState> targets = CollectTargets(units);

            foreach(BreachUnitState enemy in units)
            {
                if(enemy == null || !enemy.Alive || enemy.Team != BreachTeam.Enemy)
                {
                    continue;
                }

                BreachUnitState target = FindClosestTarget(enemy, targets);

                if(target == null)
                {
                    continue;
                }

                BreachDirection direction = ChooseDirection(enemy.Position, target.Position);

                result.Add(new EnemyIntent(enemy.Id, target.Id, direction));

            }

            // 안전한 시작값: 공격 예고가 없어도 Scene은 계속 실행됩니다.
            return result;
        }

        private static List<BreachUnitState> CollectTargets(
            IReadOnlyList<BreachUnitState> units)
        {
            // TODO 05: 살아 있는 아군과 건물만 결과에 넣으세요.
            var result = new List<BreachUnitState>();

            foreach (BreachUnitState unit in units)
            {
                if(CanBeTarget(unit))
                {
                    result.Add(unit);
                }
            }

            return result;
        }

        private static BreachUnitState FindClosestTarget(
            BreachUnitState enemy,
            IReadOnlyList<BreachUnitState> targets)
        {
            // TODO 06: Manhattan 거리가 가장 짧은 목표를 반환하세요.

            BreachUnitState best = null;
            int bestDistance = int.MaxValue;

            foreach(BreachUnitState target in targets)
            {
                int distance = Distance(enemy.Position, target.Position);

                if(distance >= bestDistance)
                {
                    continue;
                }
                best = target;
                bestDistance = distance;
            }

            return best;
        }

        private static BreachDirection ChooseDirection(
            GridPos from,
            GridPos to)
        {
            // TODO 07: X와 Y 차이 중 큰 축을 공격 방향으로 선택하세요.
            int dx = to.X - from.X;
            int dy = to.Y - from.Y;
            
            if(Math.Abs(dx) > Math.Abs(dy))
            {
                return dx > 0 ? BreachDirection.Right : BreachDirection.Left;
            }

            return dy > 0 ? BreachDirection.Up : BreachDirection.Down;
        }

        private static int Distance(GridPos a, GridPos b)
        {
            // TODO 08: GridPos.Manhattan을 이용해 거리를 반환하세요.
            return a.Manhattan(b);
        }

        private static bool CanBeTarget(BreachUnitState unit)
        {
            // TODO 09: null, 사망, 적 팀을 제외하세요.
            return unit != null && unit.Alive && unit.Team != BreachTeam.Enemy;
        }
    }
}
