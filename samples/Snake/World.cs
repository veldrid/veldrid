using System;
using System.Numerics;

namespace Snake
{
    public class World
    {
        private readonly Random _random = new Random();

        public Vector2 Size { get; }
        public float CellSize { get; }

        public Vector2 CurrentFoodLocation { get; private set; }

        public World(Vector2 size, float cellSize)
        {
            Size = size;
            CellSize = cellSize;
            CurrentFoodLocation = GetRandomFoodLocation();
        }

        public void CollectFood()
        {
            CurrentFoodLocation = GetRandomFoodLocation();
        }

        private Vector2 GetRandomFoodLocation()
        {
            return new Vector2((int)(_random.NextDouble() * Size.X), (int)(_random.NextDouble() * Size.Y));
        }

        public void Render(SpriteRenderer sr)
        {
            sr.AddSprite(CurrentFoodLocation * CellSize, new Vector2(CellSize, CellSize), "food.png");
        }
    }
}
