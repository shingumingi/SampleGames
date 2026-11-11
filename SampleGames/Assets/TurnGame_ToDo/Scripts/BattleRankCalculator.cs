using System.Net.Sockets;
using TurnGame;
namespace TurnGame.Student
{
    public class BattleRankCalculator : IBattleRankCalculator
    {
        public string CalculateRank(BattleRankInput input)
        {
            // TODO 32: 100점에서 턴 수 * 2를 빼세요.
            // TODO 33: 받은 피해 10마다 1점을 빼세요.
            // TODO 34: 전원 생존이면 20점을 더하세요.
            // TODO 35: 스킬 사용 횟수 * 2를 더하세요.
            // TODO 36: 90 S, 70 A, 50 B, 나머지 C를 반환하세요.

            int score = 100;

            score -= input.TurnCount * 2;
            score -= input.DamageTaken / 10;

            if(input.SurvivingHeroes == input.TotalHeroes)
            {
                score += 20;
            }

            score += input.SkillUseCount * 2;

            if (score >= 90) return "S";
            if (score >= 70) return "A";
            if (score >= 50) return "B";

            return "C";
        }
    }
}
