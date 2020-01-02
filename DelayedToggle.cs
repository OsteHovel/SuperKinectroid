using System.Diagnostics;

namespace Ostsoft.Games.SuperKinectroid
{
    public class DelayedToggle
    {
        private Stopwatch _timer = new Stopwatch();
        private Stopwatch _bounceTimer = new Stopwatch();

        private long delayOn;
        private long bounceDelay;

        public DelayedToggle(long delayOn = 2000, long bounceDelay = 1000)
        {
            this.delayOn = delayOn;
            this.bounceDelay = bounceDelay;
        }


        public void setActive(bool state)
        {
            if (_bounceTimer.IsRunning)
            {
                if (state)
                {
                    _bounceTimer.Restart();
                }

                if (_bounceTimer.ElapsedMilliseconds < bounceDelay)
                {
                    return;
                }

                _bounceTimer.Reset();
            }

            if (state)
            {
                _timer.Start();
            }
            else
            {
                _timer.Reset();
            }
        }

        public bool isDelayOnActive()
        {
            return _timer.IsRunning;
        }

        public bool isTrigged()
        {
            if (_timer.ElapsedMilliseconds > delayOn)
            {
                _timer.Reset();
                _bounceTimer.Start();
                return true;
            }

            return false;
        }
    }
}