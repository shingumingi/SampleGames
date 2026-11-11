using System.Collections.Generic;
using UnityEngine;
using TurnGame;
namespace TurnGame.Student
{
    public class EnemyDecisionAI : IEnemyDecisionAI
    {
        public int SelectTarget(IReadOnlyList<BattleSnapshot> units, BattleElement attackElement)
        {
            int bestIndex = -1; 
            float bestScore = float.MinValue;
            for (int i = 0; i < units.Count; i++)
            {
                // TODO 27: 죽은 캐릭터와 적 팀은 제외하세요.
                // TODO 28: HP가 낮을수록 최대 40점을 더하세요.
                // TODO 29: 이번 공격으로 쓰러뜨릴 수 있으면 100점을 더하세요.
                // TODO 30: 속성 약점이면 30점을 더하세요.
                // TODO 31: 가장 높은 점수의 인덱스를 저장하세요.

                BattleSnapshot target = units[i];
                if (!target.IsAlive || target.Team != BattleTeam.Heroes) continue;

                float hpRatio = target.HP / (float)Mathf.Max(1, target.MaxHP);
                float score = (1f - hpRatio) * 40f;

                if (target.HP <= 25)
                {
                    score += 100;
                }
                if (attackElement != BattleElement.None && target.Weakness == attackElement)
                {
                    score += 30;
                }
                if (target.Guarding)                // 가드 시 점수 15점을 뺀다.
                {
                    score -= 15f;
                }

                if(score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }

            }
            return bestIndex;
        }
    }
}
