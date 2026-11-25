using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Data;
using Unity.Multiplayer.Center.Common;

namespace BreachTactics.Student
{
    /// <summary>
    /// 2단계 실습 A: 공격받은 유닛의 밀치기 결과를 계산합니다.
    /// </summary>
    public sealed class PushResolver : IPushResolver
    {
        public PushResult Resolve(
            GridPos targetPosition,
            GridPos pushDirection,
            IReadOnlyDictionary<GridPos, int> occupied,
            int width,
            int height)
        {
            // TODO 10: 밀려날 destination 좌표를 계산하세요.
            // TODO 11: 보드 경계 밖이면 HitBoundary 결과를 반환하세요.
            // TODO 12: 목적지가 점유됐다면 충돌 유닛 ID를 반환하세요.
            // TODO 13: 비어 있다면 이동 성공 결과를 반환하세요.

            GridPos destination = GetDestination(targetPosition, pushDirection);

            if(!IsInside(destination, width, height))
            {
                return new PushResult(targetPosition, false, true, -1);
            }

            int collisionId = FindCollision(destination, occupied);

            if (collisionId >= 0) return new PushResult(targetPosition, false, false, collisionId);

            // 안전한 시작값: 움직이지 않지만 공격 자체는 계속 작동합니다.
            return new PushResult(destination, true, false, -1);
        }

        private static GridPos GetDestination(
            GridPos position,
            GridPos direction)
        {
            // TODO 14: position.Add(direction)을 반환하세요.
            return position.Add(direction);
        }

        private static bool IsInside(
            GridPos position,
            int width,
            int height)
        {
            // TODO 15: X와 Y가 보드 범위 안인지 검사하세요.
            return position.X >= 0 && position.X < width && position.Y >= 0 && position.Y < height;
        }

        private static int FindCollision(
            GridPos destination,
            IReadOnlyDictionary<GridPos, int> occupied)
        {
            // TODO 16: 점유된 유닛 ID를 찾고, 없다면 -1을 반환하세요.
            if(occupied == null)
            {
                return -1;
            }

            if(occupied.TryGetValue(destination, out int unitId))
            {
                return unitId;
            }

            return -1;
        }
    }
}
