﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        public double interLine;
    }

    public sealed partial class MainPage : Page, IFileOpenPickerContinuable
    {
        public static MainPage Current;
        public List<double> captureElementSize;
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
        bool isInitialized = false;

        #region base



        public MainPage()
        {
            this.InitializeComponent();
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
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //await StopPreviewAsync();
           // await CleanupCameraAsync();

        }


        private readonly DisplayRequest displayRequest = new DisplayRequest();

        #endregion

        #region CameraSetup

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

            //if (MyMediaCapture.VideoDeviceController.FlashControl.Supported)
            //{
            //    MyMediaCapture.VideoDeviceController.FlashControl.PowerPercent = 20;
            //    MyMediaCapture.VideoDeviceController.FlashControl.Enabled = true;
            //}

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
            SetResolution();



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
                // Cleanup the UI
                myCaptureElement.Source = null;

                // Allow the device screen to sleep now that the preview is stopped
                displayRequest.RequestRelease();
            });
        }

        private async Task CleanupCameraAsync()
        {

            if (isInitialized)
            {
                // If a recording is in progress during cleanup, stop it to save the recording

                if (isPreviewing)
                {
                    // The call to MediaCapture.Dispose() will automatically stop the preview
                    // but manually stopping the preview is good practice
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

        private async void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            await CleanupCameraAsync();
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
                default:
                    return PhotoOrientation.Normal;
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
                Frame.Navigate(typeof(ImagePage), nevParams);
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

        #endregion

        #region actions

        private double CalculateScaleFactor(Size renderSize)
        {
            double w = refLine1.RenderSize.Width;
            double h = renderSize.Height;
            double dist = refLine2.Y1 - refLine1.Y1;


            return 1;
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
                        nevParams.captureElementSize = myCaptureElement.RenderSize;
                        nevParams.interLine = refLine2.Y1 - refLine1.Y1;
                        Frame.Navigate(typeof(ImagePage), nevParams);
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


    }
}
