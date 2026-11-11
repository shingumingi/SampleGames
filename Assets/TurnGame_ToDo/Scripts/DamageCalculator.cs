using UnityEngine;
using TurnGame;
namespace TurnGame.Student
{
    public class DamageCalculator : IDamageCalculator
    {
        public DamageResult Calculate(DamageRequest r)
        {
            // TODO 17: 공격력 * 스킬 위력을 계산하세요.
            // TODO 18: 방어력의 절반을 빼세요.
            // TODO 19: 약점 1.5배, 내성 0.5배를 적용하세요.
            // TODO 20: RandomValue가 0.1 미만이면 치명타 1.5배입니다.
            // TODO 21: 방어 중이면 피해를 절반으로 줄이세요.
            // TODO 22: 최종 피해는 최소 1이며 반올림합니다.
            float element = r.SkillElement != BattleElement.None && r.SkillElement == r.Weakness ? 1.5f : r.SkillElement != BattleElement.None && r.SkillElement == r.Resistance ? 0.5f : 1f;
            int damage = Mathf.Max(1, Mathf.RoundToInt((r.Attack - r.Defense * 0.5f) * element));
            return new DamageResult { Damage = damage, IsCritical = false, ElementMultiplier = element };
        }
    }
}
