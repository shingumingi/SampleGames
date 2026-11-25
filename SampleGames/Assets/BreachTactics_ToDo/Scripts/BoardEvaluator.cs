using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace BreachTactics.Student
{
    /// <summary>
    /// 3단계 실습: 현재 전장을 하나의 점수로 평가합니다.
    /// </summary>
    public sealed class BoardEvaluator : IBoardEvaluator
    {
        private const int BuildingHealthWeight = 100;
        private const int AllyHealthWeight = 20;
        private const int EnemyPenalty = 30;
        private const int TurnPenalty = 10;

        public int Evaluate(
            IReadOnlyList<BreachUnitState> units,
            int turn)
        {
            // TODO 21: 건물 체력과 아군 체력을 합산하세요.
            // TODO 22: 살아 있는 적의 수를 계산하세요.
            // TODO 23: 가중치를 이용해 최종 점수를 계산하세요.

            if(units == null)
            {
                return 0;
            }

            int buildingHealth = SumHealth(units, BreachTeam.Building);
            int allHealth = SumHealth(units, BreachTeam.Ally);
            int enemyCount = CountAlive(units, BreachTeam.Enemy);

            int score = buildingHealth * BuildingHealthWeight;
            score += allHealth * AllyHealthWeight;
            score -= enemyCount * EnemyPenalty;
            score -= CalculateTurnPenalty(turn);

            // 안전한 시작값: UI에는 0이 나오지만 전투는 계속됩니다.
            return 0;
        }

        private static int SumHealth(
            IReadOnlyList<BreachUnitState> units,
            BreachTeam team)
        {
            // TODO 24: 지정한 팀의 살아 있는 유닛 체력을 더하세요.
            int total = 0;

            foreach(BreachUnitState unit in units)
            {
                if(IsAliveTeam(unit, team))
                {
                    total += unit.Health;
                }
            }

            return total;
        }

        private static int CountAlive(
            IReadOnlyList<BreachUnitState> units,
            BreachTeam team)
        {
            // TODO 25: 지정한 팀의 살아 있는 유닛 수를 세세요.
            int count = 0;

            foreach(BreachUnitState unit in units)
            {
                if(IsAliveTeam(unit, team))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CalculateTurnPenalty(int turn)
        {
            // TODO 26: 1턴 이후부터 턴마다 감점하세요.
            return Mathf.Max(0, turn - 1) * TurnPenalty;
        }

        private static bool IsAliveTeam(
            BreachUnitState unit,
            BreachTeam team)
        {
            // TODO 27: null이 아니고 살아 있으며 같은 팀인지 검사하세요.
            return unit != null && unit.Alive && unit.Team == team;
        }
    }
}
