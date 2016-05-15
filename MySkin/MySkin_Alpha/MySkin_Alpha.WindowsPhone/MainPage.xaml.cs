using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Sensors;
using Windows.System.Display;
using Windows.Graphics.Display;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Media.Devices;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Core;
using MySkin_Alpha.Data;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace MySkin_Alpha
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public class NevParams
    {
        public Size captureElementSize;
        public StorageFile file;
        public string description;
        public double interLine;
    }

    public sealed partial class MainPage : Page, IFileOpenPickerContinuable
    {
        public static MainPage Current;

        List<Line> scaleLines = new List<Line>();
        List<Line> vertLines = new List<Line>();
        List<Line> horLines = new List<Line>();
        public List<double> captureElementSize;
        int interline = 30;
        private bool flash = true;
        //List<Nevus> nevData;
        public MediaCapture MyMediaCapture { get; private set; }
        private readonly DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
        private DisplayOrientations displayOrientation = DisplayOrientations.Portrait;
        private readonly SimpleOrientationSensor orientationSensor = SimpleOrientationSensor.GetDefault();
        private SimpleOrientation deviceOrientation = SimpleOrientation.NotRotated;
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private bool externalCamera;
        private bool mirroringPreview;
        private bool focused = false;
        BitmapImage bit = new BitmapImage();
        //string filename = "Mole_";
        bool isPreviewing = false;
        bool isInitialized = false;

        #region base



        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            RegisterOrientationEventHandlers();
            InitCamera();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Current = this;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //InitCamera();
            try
            {
                if (isPreviewing)
                {
                    await CleanupCameraAsync();
                    InitCamera();
                }
                //await MyMediaCapture.StartPreviewAsync();
                //isPreviewing = true;
            }
            catch
            { }

            //setMeasure(0);
            //drawMeasure();
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //await StopPreviewAsync();
            //await CleanupCameraAsync();

        }



        #endregion

        #region CameraSetup

        private async void InitCamera()
        {
            // Create new MediaCapture 
            displayRequest.RequestActive();
            MyMediaCapture = new MediaCapture();
            await MyMediaCapture.InitializeAsync();

            //MyMediaCapture.PhotoConfirmationCaptured += MyMediaCapture_PhotoConfirmationCaptured;
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
                    Mode = FocusMode.Auto,
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
            //SetResolution(MediaStreamType.VideoPreview);
            IReadOnlyList<IMediaEncodingProperties> resolutions = MyMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo);

            // set used resolution
            await MyMediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolutions[0]);


            // Assign to Xaml CaptureElement.Source and start preview
            myCaptureElement.Source = MyMediaCapture;
            myCaptureElement.FlowDirection = mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // show preview
            try
            {
                await MyMediaCapture.StartPreviewAsync();
                isPreviewing = true;
                isInitialized = true;
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Camera could'n be initialised. Error: " + ex.Message + "  " + ex.InnerException);
                await dialog.ShowAsync();
            }
            await SetPreviewRotationAsync();

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

        public async void SetResolution(MediaStreamType mediaStreamType)
        {
            IReadOnlyList<IMediaEncodingProperties> res;
            res = MyMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(mediaStreamType);
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

        private async Task StopPreviewAsync()
        {
            // Stop the preview
            try
            {
                isPreviewing = false;
                await MyMediaCapture.StopPreviewAsync();
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Exception when stopping the preview: {0}", ex.ToString());
            }

            // Use the dispatcher because this method is sometimes called from non-UI threads
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                myCaptureElement.Source = null;

                displayRequest.RequestRelease();
            });
        }

        private async Task CleanupCameraAsync()
        {

            if (isInitialized)
            {

                if (isPreviewing)
                {
                    await StopPreviewAsync();
                }

                isInitialized = false;
            }

            if (MyMediaCapture != null)
            {
                MyMediaCapture.Failed -= MediaCapture_Failed;
                MyMediaCapture.Dispose();
                MyMediaCapture = null;
            }
        }


        #endregion

        #region Events

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
                if (args.Orientation == SimpleOrientation.NotRotated)
                {
                    deviceOrientation = args.Orientation;
                    if (MyMediaCapture != null)
                        MyMediaCapture.SetPreviewRotation(ConvertSimpleOrientationToVideoRotation(deviceOrientation));
                }
            }
        }

        private async void DisplayInformation_OrientationChanged(DisplayInformation sender, object args)
        {
            //displayOrientation = sender.CurrentOrientation;
            displayOrientation = DisplayOrientations.Portrait;

            if (isPreviewing)
            {
                await SetPreviewRotationAsync();
            }

        }

        private async void PhotoButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            focusBrackets.Visibility = Visibility.Visible;
            await TakePhotoAsync();
            focusBrackets.Visibility = Visibility.Collapsed;
        }

        private async void myCaptureElement_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //await MyMediaCapture.VideoDeviceController.RegionsOfInterestControl.ClearRegionsAsync();
            //// focus in the center of the screen
            //await MyMediaCapture.VideoDeviceController.RegionsOfInterestControl.SetRegionsAsync(new[]{new RegionOfInterest() {Bounds = new Rect(0.49,0.49,0.02,0.02) }});
            if (MyMediaCapture.VideoDeviceController.FocusControl.Supported)
            {
                MyMediaCapture.VideoDeviceController.FocusControl.Configure(new FocusSettings { Mode = FocusMode.Auto });
                await MyMediaCapture.VideoDeviceController.FocusControl.FocusAsync();
                focused = true;
            }
        }

        private async void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            await CleanupCameraAsync();
        }

        #endregion

        #region converters

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
                    return PhotoOrientation.Rotate270;
                default:
                    return PhotoOrientation.Rotate270;
            }
        }

        #endregion

        #region IO

        private async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, string filename, PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                StorageFile file = await KnownFolders.PicturesLibrary.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
                NevParams nevParams = new NevParams();
                nevParams.file = file;
                nevParams.captureElementSize = myCaptureElement.RenderSize;
                nevParams.interLine = refLine2.Y1 - refLine1.Y1;
                Frame.Navigate(typeof(CheckPage), nevParams);
            }
        }

        private async Task TakePhotoAsync()
        {
            var stream = new InMemoryRandomAccessStream();
            if (!focused)
                if (MyMediaCapture.VideoDeviceController.FocusControl.Supported)
                    await MyMediaCapture.VideoDeviceController.FocusControl.FocusAsync();
            if (flash)
            {
                if (MyMediaCapture.VideoDeviceController.FlashControl.Supported)
                {
                    MyMediaCapture.VideoDeviceController.FlashControl.Auto = true;
                    MyMediaCapture.VideoDeviceController.FlashControl.Enabled = true;
                }
            }
            else
            {
                if (MyMediaCapture.VideoDeviceController.FlashControl.Supported)
                {
                    MyMediaCapture.VideoDeviceController.FlashControl.Auto = false;
                    MyMediaCapture.VideoDeviceController.FlashControl.Enabled = false;
                }
            }
            try
            {

                //SetResolution(MediaStreamType.Photo);
                
                await MyMediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                var photoOrientation = ConvertOrientationToPhotoOrientation(deviceOrientation);
                await ReencodeAndSavePhotoAsync(stream, "photo.jpg", photoOrientation);
                //SetResolution(MediaStreamType.VideoPreview);
            }
            catch (Exception ex)
            {
            }
            stream.Dispose();
        }

        #endregion

        #region actions
        


        //private async void captureButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //StorageFolder folder = KnownFolders.PicturesLibrary;
        //    //StorageFolder appFolder = await folder.CreateFolderAsync("MySkin_Moles", CreationCollisionOption.OpenIfExists);
        //    //StorageFile file = await appFolder.CreateFileAsync(filename + Convert.ToString(DateTime.Now.Ticks), CreationCollisionOption.GenerateUniqueName);
        //    //ImageEncodingProperties props = ImageEncodingProperties.CreateJpeg();

        //    //Stream stream = 
        //    //await MyMediaCapture.CapturePhotoToStreamAsync(props, stream);

        //}
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
                        NevParams nevParams = new NevParams();
                        nevParams.file = img;
                        nevParams.captureElementSize = new Size(myCaptureElement.ActualWidth, myCaptureElement.ActualHeight);
                        nevParams.interLine = interline;
                        Frame.Navigate(typeof(CheckPage), nevParams);
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


        #endregion

        private void dataBaseButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(DatabasePage));
        }

        private void flushButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DataSource.ClearJSON();
        }

        private void FlashButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(flash)
            {
                flash = false;
                symbol.Symbol = Symbol.View;
            }
            else
            {
                flash = true;
                symbol.Symbol = Symbol.Target;
            }
        }

        private void slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            //interline += Convert.ToInt16(e.NewValue);
            setMeasure(Convert.ToInt16(e.NewValue));
            drawMeasure();
        }

        private void drawMeasure()
        {
            Lines.Children.Clear();
            if (scaleLines.Count != 0 && vertLines.Count != 0 && horLines.Count != 0)
            {
                foreach (Line l in scaleLines)
                    Lines.Children.Add(l);
                foreach (Line l in vertLines)
                    Lines.Children.Add(l);
                foreach (Line l in horLines)
                    Lines.Children.Add(l);
            }
        }
        private void setMeasure(int interval)
        {
            vertLines.Clear();
            horLines.Clear();
            scaleLines.Clear();

            SolidColorBrush br = new SolidColorBrush(Colors.Red);
            int vX = Convert.ToInt32(Lines.ActualWidth / 4);
            int vY = 40;

            scaleLines.Add(new Line() {
                X1 = vX,
                X2 = vX,
                Y1 = 0,
                Y2 = 220 + interval*7,
                Stroke = br,
                StrokeThickness = 2,
                Name = "VLine",
            });

            scaleLines.Add(new Line() {
                Stroke = br,
                StrokeThickness = 2,
                Name = "HLine",
                X1 = vX,
                X2 = vX + 181 + interval*7,
                Y1 = vY,
                Y2 = vY
            });

            int tX = vX;
            int tY = vY;

            for (int i=0; i<7; i++)
            {
                if (i % 2 == 0)
                {
                    horLines.Add(new Line()
                    {
                        Stroke = br,
                        StrokeThickness = 2,
                        Name = "h_" + i,
                        X1 = vX,
                        X2 = vX - 40,
                        Y1 = tY,
                        Y2 = tY
                    });
                    vertLines.Add(new Line()
                    {
                        Stroke = br,
                        StrokeThickness = 2,
                        Name = "v_" + i,
                        X1 = tX,
                        X2 = tX,
                        Y1 = vY,
                        Y2 = vY-40
                    });
                }
                else
                {
                    horLines.Add(new Line()
                    {
                        Stroke = br,
                        StrokeThickness = 2,
                        Name = "h_" + i,
                        X1 = vX,
                        X2 = vX - 20,
                        Y1 = tY,
                        Y2 = tY
                    });
                    vertLines.Add(new Line()
                    {
                        Stroke = br,
                        StrokeThickness = 2,
                        Name = "v_" + i,
                        X1 = tX,
                        X2 = tX,
                        Y1 = vY,
                        Y2 = vY - 20
                    });
                }
                tX += 30 + interval;
                tY += 30 + interval;
            }

        }
    }
}

