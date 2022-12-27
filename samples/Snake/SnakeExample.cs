using System;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.SampleGallery;

namespace Snake
{
    public class SnakeExample : BasicExample
    {
        public bool LowResolution { get; } = false;

        public int Width { get; } = 60;

        public int Height { get; } = 34;

        private SpriteRenderer _spriteRenderer;
        private World _world;
        private Snake _snake;
        // private TextRenderer _textRenderer;
        private float _cellSize;
        private Vector2 _worldSize;
        private int _highScore;
        private RgbaFloat _clearColor = new RgbaFloat(0, 0, 0.2f, 1f);

        private const float LowResCellSize = 16;
        private const float HighResCellSize = 32;

        public override async Task LoadResourcesAsync()
        {
            _cellSize = LowResolution ? LowResCellSize : HighResCellSize;
            _worldSize = new Vector2(Width, Height);

            _spriteRenderer = new SpriteRenderer(Device);
            _world = new World(_worldSize, _cellSize);
            _snake = new Snake(_world);
            _snake.ScoreChanged += () => _highScore = Math.Max(_highScore, _snake.Score);
        }

        protected override void Render(double deltaSeconds, CommandBuffer commandBuffer)
        {
            Update(deltaSeconds);
            DrawFrame(commandBuffer);
        }

        private void Update(double deltaSeconds)
        {
            _snake.Update(deltaSeconds);

            if (_snake.Dead && InputTracker.GetKeyDown(Key.Space))
            {
                _snake.Revive();
                _world.CollectFood();
            }
        }

        private void DrawFrame(CommandBuffer cb)
        {
            cb.BeginRenderPass(
                Framebuffers[FrameIndex],
                LoadAction.Clear,
                StoreAction.Store,
                _clearColor,
                1f);
            _snake.Render(_spriteRenderer);
            _world.Render(_spriteRenderer);
            _spriteRenderer.Draw(Device, cb);
            cb.EndRenderPass();
            // Texture targetTex = _textRenderer.TextureView.Target;
            // Vector2 textPos = new Vector2(
            //     (_window.Width / 2f) - targetTex.Width / 2f,
            //     _window.Height - targetTex.Height - 10f);
            // 
            // _spriteRenderer.RenderText(_gd, cb, _textRenderer.TextureView, textPos);
        }
    }
}
