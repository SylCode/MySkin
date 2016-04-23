using MySkin_Alpha.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Media.Capture;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Display;
using Windows.Devices.Sensors;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.MediaProperties;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using Windows.Foundation;
using Windows.Media.Devices;
using Windows.System.Display;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MySkin_Alpha
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        public MediaCapture MyMediaCapture { get; private set; }
        private readonly DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
        private DisplayOrientations displayOrientation = DisplayOrientations.Portrait;
        private readonly SimpleOrientationSensor orientationSensor = SimpleOrientationSensor.GetDefault();
        private SimpleOrientation deviceOrientation = SimpleOrientation.NotRotated;
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");
        private bool externalCamera;
        private bool mirroringPreview;
        BitmapImage bit = new BitmapImage();
        string filename = "Mole_";
        bool isPreviewing = false;

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            RegisterOrientationEventHandlers();
            InitCamera();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Current = this;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="Common.NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="Common.SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="Common.NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");


            openPicker.PickSingleFileAndContinue();
        }

        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.Files.Count > 0)
            {
                try
                {
                    StorageFile img = await StorageFile.GetFileFromPathAsync(args.Files[0].Path);
                    if (img != null)
                    {
                        Frame.Navigate(typeof(ImagePage), img);
                    }
                }
                finally
                { }
            }
            else
            {
                //OutputTextBlock.Text = "Operation cancelled.";
            }
        }

        private readonly DisplayRequest displayRequest = new DisplayRequest();

        private async void InitCamera()
        {
            // Create new MediaCapture 
            displayRequest.RequestActive();
            MyMediaCapture = new MediaCapture();
            await MyMediaCapture.InitializeAsync();
            MyMediaCapture.SetPreviewRotation(ConvertSimpleOrientationToVideoRotation(deviceOrientation));

            await MyMediaCapture.VideoDeviceController.SceneModeControl.SetValueAsync(CaptureSceneMode.Auto);

            if (MyMediaCapture.VideoDeviceController.WhiteBalanceControl.Supported)
                await MyMediaCapture.VideoDeviceController.WhiteBalanceControl.SetPresetAsync(ColorTemperaturePreset.Auto);

            if (MyMediaCapture.VideoDeviceController.IsoSpeedControl.Supported)
                await MyMediaCapture.VideoDeviceController.IsoSpeedControl.SetAutoAsync();

            if (MyMediaCapture.VideoDeviceController.ExposureControl.Supported)
                await MyMediaCapture.VideoDeviceController.ExposureControl.SetAutoAsync(true);

            if (MyMediaCapture.VideoDeviceController.ExposureCompensationControl.Supported)
                await MyMediaCapture.VideoDeviceController.ExposureCompensationControl.SetValueAsync(0);

            if (MyMediaCapture.VideoDeviceController.FlashControl.Supported)
            {
                MyMediaCapture.VideoDeviceController.FlashControl.Auto = true;
                MyMediaCapture.VideoDeviceController.FlashControl.Enabled = true;
            }

            if (MyMediaCapture.VideoDeviceController.FocusControl.Supported)
                MyMediaCapture.VideoDeviceController.FocusControl.Configure(new FocusSettings
                {
                    AutoFocusRange = AutoFocusRange.Macro,
                    Mode = FocusMode.Continuous,
                    DisableDriverFallback = true,
                    WaitForFocus = true,
                    Distance = ManualFocusDistance.Nearest
                });

            //await MyMediaCapture.InitializeAsync(new MediaCaptureInitializationSettings
            //{
            //    StreamingCaptureMode = StreamingCaptureMode.Video,
            //    PhotoCaptureSource = PhotoCaptureSource.Photo, // I believe your bug was here
            //    AudioDeviceId = string.Empty,

            //});
            SetResolution();



            // Assign to Xaml CaptureElement.Source and start preview
            myCaptureElement.Source = MyMediaCapture;
            myCaptureElement.FlowDirection = mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // show preview
            try
            {
                await MyMediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch
            {

            }

        }

        private void RegisterOrientationEventHandlers()
        {
            // If there is an orientation sensor present on the device, register for notifications
            if (orientationSensor != null)
            {
                orientationSensor.OrientationChanged += OrientationSensor_OrientationChanged;
                deviceOrientation = orientationSensor.GetCurrentOrientation();
            }

            displayInformation.OrientationChanged += DisplayInformation_OrientationChanged;
            displayOrientation = displayInformation.CurrentOrientation;
        }

        private void OrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
        {
            if (args.Orientation != SimpleOrientation.Faceup && args.Orientation != SimpleOrientation.Facedown)
            {
                deviceOrientation = args.Orientation;
                if (MyMediaCapture != null)
                    MyMediaCapture.SetPreviewRotation(ConvertSimpleOrientationToVideoRotation(deviceOrientation));
            }
        }

        private async void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            displayOrientation = sender.CurrentOrientation;

            if (isPreviewing)
            {
                await SetPreviewRotationAsync();
            }

        }

        private async Task SetPreviewRotationAsync()
        {
            // Populate orientation variables with the current state
            displayOrientation = displayInformation.CurrentOrientation;

            // Calculate which way and how far to rotate the preview
            int rotationDegrees = ConvertDisplayOrientationToDegrees(displayOrientation);

            // The rotation direction needs to be inverted if the preview is being mirrored
            if (mirroringPreview)
            {
                rotationDegrees = (360 - rotationDegrees) % 360;
            }

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = MyMediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, rotationDegrees);
            await MyMediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);

        }

        private SimpleOrientation GetCameraOrientation()
        {

            // If the preview is being mirrored for a front-facing camera, then the rotation should be inverted
            if (mirroringPreview)
            {
                // This only affects the 90 and 270 degree cases, because rotating 0 and 180 degrees is the same clockwise and counter-clockwise
                switch (deviceOrientation)
                {
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        return SimpleOrientation.Rotated270DegreesCounterclockwise;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        return SimpleOrientation.Rotated90DegreesCounterclockwise;
                }
            }

            return deviceOrientation;
        }

        private static int ConvertDisplayOrientationToDegrees(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.Portrait:
                    return 90;
                case DisplayOrientations.LandscapeFlipped:
                    return 180;
                case DisplayOrientations.PortraitFlipped:
                    return 270;
                case DisplayOrientations.Landscape:
                default:
                    return 0;
            }
        }

        private static VideoRotation ConvertSimpleOrientationToVideoRotation(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return VideoRotation.None;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return VideoRotation.Clockwise180Degrees;
                default:
                    return VideoRotation.Clockwise90Degrees;
            }
        }

        private static int ConvertDeviceOrientationToDegrees(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return 90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return 180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return 270;
                case SimpleOrientation.NotRotated:
                default:
                    return 0;
            }
        }

        private static PhotoOrientation ConvertOrientationToPhotoOrientation(SimpleOrientation orientation)
        {
            switch (orientation)
            {
                case SimpleOrientation.Rotated90DegreesCounterclockwise:
                    return PhotoOrientation.Rotate90;
                case SimpleOrientation.Rotated180DegreesCounterclockwise:
                    return PhotoOrientation.Rotate180;
                case SimpleOrientation.Rotated270DegreesCounterclockwise:
                    return PhotoOrientation.Rotate270;
                case SimpleOrientation.NotRotated:
                default:
                    return PhotoOrientation.Normal;
            }
        }

        private static async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, string filename, PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                var file = await KnownFolders.PicturesLibrary.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }
        }

        private async Task TakePhotoAsync()
        {
            var stream = new InMemoryRandomAccessStream();
            if (MyMediaCapture.VideoDeviceController.FocusControl.Supported)
                await MyMediaCapture.VideoDeviceController.FocusControl.FocusAsync();
            try
            {
                await MyMediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                var photoOrientation = ConvertOrientationToPhotoOrientation(GetCameraOrientation());
                await ReencodeAndSavePhotoAsync(stream, "photo.jpg", photoOrientation);
            }
            catch (Exception ex)
            {
            }
        }

        public async void SetResolution()
        {
            System.Collections.Generic.IReadOnlyList<IMediaEncodingProperties> res;
            res = MyMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
            uint maxResolution = 0;
            int indexMaxResolution = 0;

            if (res.Count >= 1)
            {
                for (int i = 0; i < res.Count; i++)
                {
                    VideoEncodingProperties vp = (VideoEncodingProperties)res[i];

                    if (vp.Width > maxResolution)
                    {
                        indexMaxResolution = i;
                        maxResolution = vp.Width;
                    }
                }
                await MyMediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, res[indexMaxResolution]);
            }
        }

        private async void captureButton_Click(object sender, RoutedEventArgs e)
        {
            await TakePhotoAsync();
            //StorageFolder folder = KnownFolders.PicturesLibrary;
            //StorageFolder appFolder = await folder.CreateFolderAsync("MySkin_Moles", CreationCollisionOption.OpenIfExists);
            //StorageFile file = await appFolder.CreateFileAsync(filename + Convert.ToString(DateTime.Now.Ticks), CreationCollisionOption.GenerateUniqueName);
            //ImageEncodingProperties props = ImageEncodingProperties.CreateJpeg();

            //Stream stream = 
            //await MyMediaCapture.CapturePhotoToStreamAsync(props, stream);

        }
    }
}
