using UnityEngine;

namespace BreachTactics
{
    public sealed class BreachUnitView : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private string unitName;
        [SerializeField] private BreachTeam team;
        [SerializeField] private int gridX;
        [SerializeField] private int gridY;
        [SerializeField] private int health = 2;
        [SerializeField] private int attack = 1;
        [SerializeField] private int moveRange = 3;
        [SerializeField] private bool canPush;

        public int Id => id;

        public BreachUnitState CreateState()
        {
            return new BreachUnitState
            {
                Id = id,
                Name = unitName,
                Team = team,
                Position = new GridPos(gridX, gridY),
                Health = health,
                Attack = attack,
                MoveRange = moveRange,
                CanPush = canPush
            };
        }

        public void Configure(int unitId, string displayName, BreachTeam unitTeam,
            GridPos position, int hp, int damage, int range, bool push)
        {
            id = unitId;
            unitName = displayName;
            team = unitTeam;
            gridX = position.X;
            gridY = position.Y;
            health = hp;
            attack = damage;
            moveRange = range;
            canPush = push;
            name = $"Unit_{unitId:00}_{displayName}";
        }
    }
}
