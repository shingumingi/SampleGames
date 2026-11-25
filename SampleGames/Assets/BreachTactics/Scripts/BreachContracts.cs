using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreachTactics
{
    public enum BreachTeam { Ally, Enemy, Building }
    public enum BreachDirection { Up, Right, Down, Left }

    [Serializable]
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int Manhattan(GridPos other)
        {
            return Mathf.Abs(X - other.X) + Mathf.Abs(Y - other.Y);
        }

        public GridPos Add(GridPos other)
        {
            return new GridPos(X + other.X, Y + other.Y);
        }

        public bool Equals(GridPos other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static GridPos FromDirection(BreachDirection direction)
        {
            return direction switch
            {
                BreachDirection.Up => new GridPos(0, 1),
                BreachDirection.Right => new GridPos(1, 0),
                BreachDirection.Down => new GridPos(0, -1),
                _ => new GridPos(-1, 0)
            };
        }
    }

    [Serializable]
    public sealed class BreachUnitState
    {
        public int Id;
        public string Name;
        public BreachTeam Team;
        public GridPos Position;
        public int Health;
        public int Attack;
        public int MoveRange;
        public bool CanPush;
        public bool Acted;
        public bool Alive => Health > 0;
    }

    public readonly struct EnemyIntent
    {
        public readonly int EnemyId;
        public readonly int TargetId;
        public readonly BreachDirection Direction;

        public EnemyIntent(int enemyId, int targetId, BreachDirection direction)
        {
            EnemyId = enemyId;
            TargetId = targetId;
            Direction = direction;
        }
    }

    public readonly struct PushResult
    {
        public readonly GridPos Destination;
        public readonly bool Moved;
        public readonly bool HitBoundary;
        public readonly int CollisionUnitId;

        public PushResult(GridPos destination, bool moved, bool hitBoundary, int collisionUnitId)
        {
            Destination = destination;
            Moved = moved;
            HitBoundary = hitBoundary;
            CollisionUnitId = collisionUnitId;
        }
    }

    public interface IEnemyIntentPlanner
    {
        List<EnemyIntent> Plan(IReadOnlyList<BreachUnitState> units);
    }

    public interface IPushResolver
    {
        PushResult Resolve(GridPos targetPosition, GridPos pushDirection,
            IReadOnlyDictionary<GridPos, int> occupied, int width, int height);
    }

    public interface IDamageCalculator
    {
        int Calculate(int baseDamage, bool collision, bool hitBoundary, bool targetIsBuilding);
    }

    public interface IBoardEvaluator
    {
        int Evaluate(IReadOnlyList<BreachUnitState> units, int turn);
    }
}
