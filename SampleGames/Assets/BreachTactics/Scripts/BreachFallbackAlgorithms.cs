using System.Collections.Generic;
using System.Linq;

namespace BreachTactics
{
    internal sealed class FallbackEnemyIntentPlanner : IEnemyIntentPlanner
    {
        public List<EnemyIntent> Plan(IReadOnlyList<BreachUnitState> units)
        {
            var result = new List<EnemyIntent>();
            List<BreachUnitState> targets = units
                .Where(unit => unit.Alive && unit.Team != BreachTeam.Enemy)
                .ToList();

            foreach (BreachUnitState enemy in units.Where(unit => unit.Alive && unit.Team == BreachTeam.Enemy))
            {
                BreachUnitState target = targets
                    .OrderBy(unit => enemy.Position.Manhattan(unit.Position))
                    .FirstOrDefault();

                if (target == null)
                {
                    continue;
                }

                GridPos delta = new GridPos(
                    target.Position.X - enemy.Position.X,
                    target.Position.Y - enemy.Position.Y);

                BreachDirection direction = System.Math.Abs(delta.X) > System.Math.Abs(delta.Y)
                    ? delta.X > 0 ? BreachDirection.Right : BreachDirection.Left
                    : delta.Y > 0 ? BreachDirection.Up : BreachDirection.Down;

                result.Add(new EnemyIntent(enemy.Id, target.Id, direction));
            }

            return result;
        }
    }

    internal sealed class FallbackPushResolver : IPushResolver
    {
        public PushResult Resolve(GridPos targetPosition, GridPos pushDirection,
            IReadOnlyDictionary<GridPos, int> occupied, int width, int height)
        {
            GridPos destination = targetPosition.Add(pushDirection);
            bool boundary = destination.X < 0 || destination.X >= width
                || destination.Y < 0 || destination.Y >= height;

            if (boundary)
            {
                return new PushResult(targetPosition, false, true, -1);
            }

            if (occupied.TryGetValue(destination, out int collisionId))
            {
                return new PushResult(targetPosition, false, false, collisionId);
            }

            return new PushResult(destination, true, false, -1);
        }
    }

    internal sealed class FallbackDamageCalculator : IDamageCalculator
    {
        public int Calculate(int baseDamage, bool collision, bool hitBoundary, bool targetIsBuilding)
        {
            return System.Math.Max(0, baseDamage + (collision || hitBoundary ? 1 : 0));
        }
    }

    internal sealed class FallbackBoardEvaluator : IBoardEvaluator
    {
        public int Evaluate(IReadOnlyList<BreachUnitState> units, int turn)
        {
            return units.Where(unit => unit.Team == BreachTeam.Building && unit.Alive)
                .Sum(unit => unit.Health) * 100 - turn * 10;
        }
    }
}
