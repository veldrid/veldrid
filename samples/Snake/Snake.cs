using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.SampleGallery;

namespace Snake
{
    public class Snake
    {
        private const string BodySprite = "snake-3.png";
        private const string HeadSprite = "snake-head.png";
        private const double InitialUpdatePeriod = 0.2125; // seconds
        private const double SpeedupFactor = 0.97;
        private const double MinPeriod = 0.05;
        private const int InitialSize = 3;

        private readonly World _world;

        public Vector2 HeadPosition => _positions.Last().Item1;

        private readonly List<(Vector2, float)> _positions = new List<(Vector2, float)>();
        private Vector2 _direction;
        private Vector2 _previousDir;
        private double _updateTimer;
        private int _currentFood;
        private double _updatePeriod;
        private bool _dead;

        public int Score { get; private set; }
        public event Action ScoreChanged;

        public bool Dead => _dead;

        public Snake(World world)
        {
            _world = world;
            Revive();
        }

        public void Revive()
        {
            _dead = false;
            _updatePeriod = InitialUpdatePeriod;
            _currentFood = 0;
            _direction = Vector2.UnitX;
            _positions.Clear();
            SetScore(0);
            Vector2 pos = new Vector2(3, 3);
            for (int i = 0; i < InitialSize; i++)
            {
                float rotation = GetRotation(_direction);
                _positions.Add((pos + i * new Vector2(1, 0), rotation));
            }
        }

        private void SetScore(int newScore)
        {
            Score = newScore;
            ScoreChanged?.Invoke();
        }

        private float GetRotation(Vector2 direction)
        {
            if (direction == Vector2.UnitY) { return 0; }
            if (direction == Vector2.UnitX) { return (float)Math.PI / 2; }
            if (direction == -Vector2.UnitX) { return -(float)Math.PI / 2; }
            else { return (float)Math.PI; }
        }

        public void Update(double deltaSeconds)
        {
            if (_dead)
            {
                return;
            }

            if (InputTracker.GetKeyDown(Key.Left))
            {
                TryChangeDirection(new Vector2(-1, 0));
            }
            else if (InputTracker.GetKeyDown(Key.Right))
            {
                TryChangeDirection(new Vector2(1, 0));
            }
            else if (InputTracker.GetKeyDown(Key.Up))
            {
                TryChangeDirection(new Vector2(0, 1));
            }
            else if (InputTracker.GetKeyDown(Key.Down))
            {
                TryChangeDirection(new Vector2(0, -1));
            }

            _updateTimer -= deltaSeconds;
            if (InputTracker.GetKey(Key.Space))
            {
                _updateTimer -= deltaSeconds * 2;
            }
            if (_updateTimer > 0)
            {
                return;
            }
            _updateTimer = _updatePeriod;

            Vector2 newPos = HeadPosition + _direction;

            if (Collides(newPos))
            {
                Vector2 pos = _positions[_positions.Count - 1].Item1;
                float rot = GetRotation(_direction);
                _positions[_positions.Count - 1] = (pos, rot);
                Die();
                return;
            }

            if (newPos == _world.CurrentFoodLocation)
            {
                _world.CollectFood();
                _updatePeriod = Math.Max(MinPeriod, SpeedupFactor * _updatePeriod);
                _currentFood += 2;
                SetScore(Score + 1);
            }

            _previousDir = _direction;

            _positions.Add((newPos, GetRotation(_direction)));

            if (_currentFood > 0)
            {
                _currentFood--;
            }

            if (_currentFood == 0)
            {
                _positions.RemoveAt(0);
            }
        }

        private void TryChangeDirection(Vector2 newDirection)
        {
            if (newDirection != -_previousDir)
            {
                _direction = newDirection;
            }
        }

        private bool Collides(Vector2 newPos)
        {
            foreach ((Vector2 pos, float rotation) in _positions)
            {
                if (newPos == pos)
                {
                    return true;
                }
            }

            return OffWorld(newPos);
        }

        private bool OffWorld(Vector2 newPos)
        {
            return newPos.X < 0 || newPos.X >= _world.Size.X
                || newPos.Y < 0 || newPos.Y >= _world.Size.Y;
        }

        private void Die()
        {
            _dead = true;
        }

        public void Render(SpriteRenderer sr)
        {
            for (int i = 0; i < _positions.Count - 1; i++)
            {
                (Vector2 pos, float rotation) = _positions[i];
                sr.AddSprite(
                    pos * _world.CellSize,
                    new Vector2(_world.CellSize, _world.CellSize),
                    BodySprite,
                    _dead ? new RgbaByte(255, 100, 100, 180) : RgbaByte.White,
                    rotation);
            }

            (Vector2 headPos, float headRot) = _positions[_positions.Count - 1];
            sr.AddSprite(
                headPos * _world.CellSize,
                new Vector2(_world.CellSize, _world.CellSize),
                HeadSprite,
                _dead ? new RgbaByte(255, 100, 100, 180) : RgbaByte.White,
                headRot);
        }
    }
}
