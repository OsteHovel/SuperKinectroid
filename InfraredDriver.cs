using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Ostsoft.Games.SuperKinectroid
{
    public class InfraredDriver : IDisposable
    {
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Maximum value (as a float) that can be returned by the InfraredFrame
        /// </summary>
        private const float InfraredSourceValueMaximum = ushort.MaxValue;

        /// <summary>
        /// The value by which the infrared source data will be scaled
        /// </summary>
        private const float InfraredSourceScale = 0.75f;

        /// <summary>
        /// Smallest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// Largest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        /// <summary>
        /// Reader for infrared frames
        /// </summary>
        private InfraredFrameReader infraredFrameReader = null;

        /// <summary>
        /// Description (width, height, etc) of the infrared frame data
        /// </summary>
        private FrameDescription infraredFrameDescription = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap infraredBitmap = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public InfraredDriver(KinectSensor kinectSensor, WriteableBitmap infraredBitmap)
        {
            this.kinectSensor = kinectSensor;
            this.infraredBitmap = infraredBitmap;

            // open the reader for the depth frames
            infraredFrameReader = this.kinectSensor.InfraredFrameSource.OpenReader();

            // get FrameDescription from InfraredFrameSource
            infraredFrameDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;

            // wire handler for frame arrival
            infraredFrameReader.FrameArrived += Reader_InfraredFrameArrived;
        }

        public static WriteableBitmap GetBitmap(KinectSensor kinectSensor)
        {
            // get FrameDescription from InfraredFrameSource
            var infraredFrameDescription = kinectSensor.InfraredFrameSource.FrameDescription;

            // create the bitmap to display
            return new WriteableBitmap(infraredFrameDescription.Width, infraredFrameDescription.Height, 96.0,
                96.0, PixelFormats.Gray32Float, null);
        }

        /// <summary>
        /// Handles the infrared frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_InfraredFrameArrived(object sender, InfraredFrameArrivedEventArgs e)
        {
            // InfraredFrame is IDisposable
            using (InfraredFrame infraredFrame = e.FrameReference.AcquireFrame())
            {
                if (infraredFrame != null)
                {
                    // the fastest way to process the infrared frame data is to directly access 
                    // the underlying buffer
                    using (var infraredBuffer = infraredFrame.LockImageBuffer())
                    {
                        // verify data and write the new infrared frame data to the display bitmap
                        if (((infraredFrameDescription.Width * infraredFrameDescription.Height) ==
                             (infraredBuffer.Size / infraredFrameDescription.BytesPerPixel))
                            && (infraredFrameDescription.Width == infraredBitmap.PixelWidth) &&
                            (infraredFrameDescription.Height == infraredBitmap.PixelHeight)
                        )
                        {
                            ProcessInfraredFrameData(infraredBuffer.UnderlyingBuffer, infraredBuffer.Size);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the InfraredFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the infraredFrameData pointer.
        /// </summary>
        /// <param name="infraredFrameData">Pointer to the InfraredFrame image data</param>
        /// <param name="infraredFrameDataSize">Size of the InfraredFrame image data</param>
        private unsafe void ProcessInfraredFrameData(IntPtr infraredFrameData, uint infraredFrameDataSize)
        {
            // infrared frame data is a 16 bit value
            ushort* frameData = (ushort*) infraredFrameData;

            // lock the target bitmap
            infraredBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*) infraredBitmap.BackBuffer;

            // process the infrared data
            for (int i = 0; i < (int) (infraredFrameDataSize / infraredFrameDescription.BytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                backBuffer[i] = Math.Min(InfraredOutputValueMaximum,
                    frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale *
                    (1.0f - InfraredOutputValueMinimum) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            infraredBitmap.AddDirtyRect(new Int32Rect(0, 0, infraredBitmap.PixelWidth,
                infraredBitmap.PixelHeight));

            // unlock the bitmap
            infraredBitmap.Unlock();
        }

        public void Dispose()
        {
            infraredFrameReader?.Dispose();
        }
    }
}