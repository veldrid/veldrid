namespace Veldrid.NeoDemo
{
    public class FrameTimeAverager
    {
        private readonly double _timeLimit = 666;

        private double _accumulatedTime = 0;
        private int _frameCount = 0;
        private readonly double _decayRate = .3;

        public double CurrentAverageFrameTimeSeconds { get; private set; }
        public double CurrentAverageFrameTimeMilliseconds => CurrentAverageFrameTimeSeconds * 1000.0;
        public double CurrentAverageFramesPerSecond => 1 / CurrentAverageFrameTimeSeconds;

        public FrameTimeAverager(double maxTimeSeconds)
        {
            _timeLimit = maxTimeSeconds;
        }

        public void Reset()
        {
            _accumulatedTime = 0;
            _frameCount = 0;
        }

        public void AddTime(double seconds)
        {
            _accumulatedTime += seconds;
            _frameCount++;
            if (_accumulatedTime >= _timeLimit)
            {
                Average();
            }
        }

        private void Average()
        {
            double total = _accumulatedTime;
            CurrentAverageFrameTimeSeconds =
                (CurrentAverageFrameTimeSeconds * _decayRate)
                + ((total / _frameCount) * (1 - _decayRate));

            _accumulatedTime = 0;
            _frameCount = 0;
        }
    }
}
