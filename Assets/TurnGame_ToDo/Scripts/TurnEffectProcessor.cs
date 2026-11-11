using UnityEngine;
using TurnGame;
namespace TurnGame.Student
{
    public class TurnEffectProcessor : ITurnEffectProcessor
    {
        public float Apply(float delay, TurnEffectType effect, float amount)
        {
            // TODO 23: Haste는 대기 시간을 amount만큼 줄이세요.
            // TODO 24: Slow와 Delay는 대기 시간을 늘리세요.
            // TODO 25: Stun은 최소 5만큼 뒤로 미루세요.
            // TODO 26: 결과는 0보다 작을 수 없습니다.
            return Mathf.Max(0f, delay);
        }
    }
}
