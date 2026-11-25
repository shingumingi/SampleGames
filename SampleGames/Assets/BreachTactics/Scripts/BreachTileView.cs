using UnityEngine;

namespace BreachTactics
{
    public sealed class BreachTileView : MonoBehaviour
    {
        [SerializeField] private int x;
        [SerializeField] private int y;

        public GridPos Position => new GridPos(x, y);

        public void Configure(int gridX, int gridY)
        {
            x = gridX;
            y = gridY;
            name = $"Tile_{gridX:00}_{gridY:00}";
        }
    }
}
