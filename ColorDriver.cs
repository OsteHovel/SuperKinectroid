using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Ostsoft.Games.SuperKinectroid
{
    public class ColorDriver : IDisposable
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        public ColorDriver(KinectSensor kinectSensor, WriteableBitmap colorBitmap)
        {
            this.kinectSensor = kinectSensor;

            var colorFrameDescription = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            this.colorBitmap = colorBitmap;

            // get size of joint space
            displayWidth = colorFrameDescription.Width;
            displayHeight = colorFrameDescription.Height;

            colorFrameReader = kinectSensor.ColorFrameSource.OpenReader();
            colorFrameReader.FrameArrived += Reader_ColorFrameArrived;
        }

        public ImageSource ColorBitmap => colorBitmap;

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == colorBitmap.PixelWidth) &&
                            (colorFrameDescription.Height == colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                colorBitmap.BackBuffer,
                                (uint) (colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth,
                                colorBitmap.PixelHeight));
                        }

                        colorBitmap.Unlock();
                    }
                }
            }
        }

        public void Dispose()
        {
            colorFrameReader?.Dispose();
        }

        public static WriteableBitmap GetBitmap(KinectSensor kinectSensor)
        {
            var colorFrameDescription = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            return new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0,
                PixelFormats.Bgr32, null);
        }
    }
}