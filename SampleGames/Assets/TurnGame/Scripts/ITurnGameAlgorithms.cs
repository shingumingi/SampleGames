using System.Collections.Generic;

namespace TurnGame
{
    public interface ITurnScheduler
    {
        int SelectNext(IReadOnlyList<BattleSnapshot> units);
        float CalculateDelay(int speed, int speedBonus);
    }

    public interface ITargetSelector
    {
        int SelectTarget(IReadOnlyList<BattleSnapshot> units, BattleTeam targetTeam);
    }

    public interface IExperienceCalculator
    {
        int RequiredExperience(int level);
        ExperienceResult AddExperience(int level, int currentExperience, int gainedExperience);
    }

    public interface IDamageCalculator { DamageResult Calculate(DamageRequest request); }
    public interface ITurnEffectProcessor { float Apply(float delay, TurnEffectType effect, float amount); }
    public interface IEnemyDecisionAI { int SelectTarget(IReadOnlyList<BattleSnapshot> units, BattleElement attackElement); }
    public interface IBattleRankCalculator { string CalculateRank(BattleRankInput input); }
}
