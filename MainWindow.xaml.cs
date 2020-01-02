using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace Ostsoft.Games.SuperKinectroid
{
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private bool useColor = false;
        private static SuperKinectroid superKinectroid = null;
        public static KinectSensor kinectSensor = null;
        private BodyDriver _bodyDriver = null;
        private ColorDriver _colorDriver = null;
        private InfraredDriver _infraredDriver = null;

        private DrawingImage _bodySource = null;
        private WriteableBitmap _colorBitmap = null;
        private WriteableBitmap _infraredBitmap = null;

        private static DrawingImage _zoneSource = new DrawingImage(new DrawingGroup());

        private Visibility _bodyVisibility = Visibility.Hidden;

        public Visibility BodyVisibility
        {
            get { return _bodyVisibility; }
            set
            {
                _bodyVisibility = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BodyVisibility"));
            }
        }

        private Visibility _colorVisibility = Visibility.Hidden;

        public Visibility ColorVisibility
        {
            get { return _colorVisibility; }
            set
            {
                _colorVisibility = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ColorVisibility"));
            }
        }

        private Visibility _infraredVisibility = Visibility.Hidden;

        public Visibility InfraredVisibility
        {
            get { return _infraredVisibility; }
            set
            {
                _infraredVisibility = value;
                PropertyChanged(this, new PropertyChangedEventArgs("InfraredVisibility"));
            }
        }

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
//            ws = new WS();
//            home = new Home();
            superKinectroid = new SuperKinectroid();

            // one sensor is currently supported
            kinectSensor = KinectSensor.GetDefault();

            var colorFrameDescription = kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
//            colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0,
//                96.0, PixelFormats.Bgr32, null);

            _bodySource = BodyDriver.GetSource();
            _colorBitmap = ColorDriver.GetBitmap(kinectSensor);
            _infraredBitmap = InfraredDriver.GetBitmap(kinectSensor);

            // set IsAvailableChanged event notifier
            kinectSensor.IsAvailableChanged += Sensor_IsAvailableChanged;

            // open the sensor
            kinectSensor.Open();

            // set the status text
            StatusText = kinectSensor.IsAvailable ? "Running" : "No ready Kinect found!";


            // use the window object as the view model in this simple example
            DataContext = this;


            // initialize the components (controls) of the window
            InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource BodySource => _bodySource;

        public ImageSource ColorBitmap => _colorBitmap;

        public ImageSource InfraredBitmap => _infraredBitmap;

        public ImageSource ZoneSource => _zoneSource;

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get { return statusText; }

            set
            {
                if (statusText != value)
                {
                    statusText = value;

                    // notify any bound elements that the text has changed
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetColor(false);
            SetInfrared(false);
            SetBody(true);
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            _bodyDriver?.Dispose();
            _bodyDriver = null;

            _colorDriver?.Dispose();
            _colorDriver = null;

            _infraredDriver?.Dispose();
            _infraredDriver = null;

            kinectSensor?.Close();
            kinectSensor = null;
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            StatusText = kinectSensor.IsAvailable ? "Running" : "Kinect not available!";
        }

        private void InfraredButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Turning Infrared " + (_infraredDriver == null ? "on" : "off"));
            SetInfrared(_infraredDriver == null);
        }


        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Turning Color " + (_colorDriver == null ? "on" : "off"));
                SetColor(_colorDriver == null);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        private void BodyButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Turning Body " + (_bodyDriver == null ? "on" : "off"));
            SetBody(_bodyDriver == null);
        }

        private void SetBody(bool state)
        {
            if (state && _bodyDriver == null)
            {
                _bodyDriver = new BodyDriver(kinectSensor, _bodySource);
                BodyVisibility = Visibility.Visible;
            }
            else if (!state && _bodyDriver != null)
            {
                _bodyDriver?.Dispose();
                _bodyDriver = null;
                BodyVisibility = Visibility.Hidden;
            }
        }

        private void SetColor(bool state)
        {
            if (state && _colorDriver == null)
            {
                _colorDriver = new ColorDriver(kinectSensor, _colorBitmap);
                ColorVisibility = Visibility.Visible;
            }
            else if (!state && _colorDriver != null)
            {
                _colorDriver?.Dispose();
                _colorDriver = null;
                ColorVisibility = Visibility.Hidden;
            }
        }

        private void SetInfrared(bool state)
        {
            if (state && _infraredDriver == null)
            {
                _infraredDriver = new InfraredDriver(kinectSensor, _infraredBitmap);
                InfraredVisibility = Visibility.Visible;
            }
            else if (!state && _infraredDriver != null)
            {
                _infraredDriver?.Dispose();
                _infraredDriver = null;
                InfraredVisibility = Visibility.Hidden;
            }
        }

        public static void UpdateBodies(Body[] bodies)
        {
            superKinectroid?.UpdateBodies(bodies, _zoneSource);
        }
    }
}