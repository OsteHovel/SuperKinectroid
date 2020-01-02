using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Ostsoft.Games.SuperKinectroid
{
    public class OSD
    {
        private FormattedText _formattedText;
        private Stopwatch _timer = Stopwatch.StartNew();
        private long _timeout;

        private Typeface typeface = new Typeface(new FontFamily("Arial"),
            FontStyles.Normal,
            FontWeights.Normal,
            FontStretches.Normal);

        public OSD()
        {
            displayMessage("Super Kinectroid!");
        }

        public void displayMessage(string message, long timeout = 3000)
        {
            _formattedText = new FormattedText(message, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                typeface, 100, Brushes.Cyan);
            _timeout = timeout;
            _timer.Restart();
        }

        public void update(DrawingImage zoneSource)
        {
            if (!_timer.IsRunning) return;

            if (!(zoneSource.Drawing is DrawingGroup drawingGroup)) return;

            using (DrawingContext dc = drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null,
                    new Rect(0.0, 0.0, 1920, 1080));

                if (_timer.ElapsedMilliseconds < _timeout)
                {
                    dc.DrawText(_formattedText, new Point(10, 10));
                }
                else
                {
                    _timer.Reset();
                }

                drawingGroup.ClipGeometry =
                    new RectangleGeometry(new Rect(0.0, 0.0, 1920, 1080));
            }
        }
    }
}