using UnityEngine;

namespace BreachTactics.Student
{
    /// <summary>2단계 실습 B: 기본 피해와 충돌 추가 피해를 계산합니다.</summary>
    public sealed class DamageCalculator : IDamageCalculator
    {
        public int Calculate(
            int baseDamage,
            bool collision,
            bool hitBoundary,
            bool targetIsBuilding)
        {
            // TODO 17: 기본 피해가 음수가 되지 않게 보정하세요.
            // TODO 18: 일반 유닛이 충돌하거나 벽에 부딪히면 피해를 1 추가하세요.
            // TODO 19: 건물에는 충돌 추가 피해를 적용하지 마세요.

            int damage = Mathf.Max(0, baseDamage);

            if(HasCollisionBonus(collision, hitBoundary, targetIsBuilding))
            {
                damage += 1;
            }

            return damage;
        }

        private static bool HasCollisionBonus(
            bool collision,
            bool hitBoundary,
            bool targetIsBuilding)
        {
            // TODO 20: 추가 피해 조건을 논리 연산자로 작성하세요.
            return !targetIsBuilding && (collision || hitBoundary);
        }
    }
}
