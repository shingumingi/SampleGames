using UnityEngine;
using TurnGame;
using System.Diagnostics.Tracing;
namespace TurnGame.Student
{
    public class DamageCalculator : IDamageCalculator
    {
        public DamageResult Calculate(DamageRequest request)
        {
            // TODO 17: 공격력 * 스킬 위력을 계산하세요.
            // TODO 18: 방어력의 절반을 빼세요.
            // TODO 19: 약점 1.5배, 내성 0.5배를 적용하세요.
            // TODO 20: RandomValue가 0.1 미만이면 치명타 1.5배입니다.
            // TODO 21: 방어 중이면 피해를 절반으로 줄이세요.
            // TODO 22: 최종 피해는 최소 1이며 반올림합니다.

            float baseDamage = request.Attack * request.SkillPower / 100f;
            baseDamage -= request.Defense * 0.5f;
            baseDamage = Mathf.Max(1f, baseDamage);

            float elementMultiplier = 1f;
            if (request.SkillElement != BattleElement.None && request.SkillElement == request.Resistance)
                elementMultiplier = 1.5f;
            else if (request.SkillElement != BattleElement.None && request.SkillElement == request.Resistance)
                elementMultiplier = 0.5f;

            bool isCritical = request.RandomValue < 0.1f;
            float criticalMultiplier = isCritical ? 1.5f : 1f;
            float guradMultiplier = request.Guarding ? 0.5f : 1f;
            float finalDamage = baseDamage * elementMultiplier;
            finalDamage *= criticalMultiplier;
            finalDamage *= guradMultiplier;
            int damage = Mathf.Max(1, Mathf.RoundToInt(finalDamage));

            return new DamageResult
            {
                Damage = damage,
                IsCritical = isCritical,
                ElementMultiplier = elementMultiplier,
            };
        }
    }
}
