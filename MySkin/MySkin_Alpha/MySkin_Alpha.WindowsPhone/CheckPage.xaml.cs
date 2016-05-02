using MySkin_Alpha.Common;
using MySkin_Alpha.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MySkin_Alpha
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CheckPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        NevParams par;

        public CheckPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            await DataSource.init();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (mainImage.Source != null)
                processButton.Visibility = Visibility.Visible;
            else processButton.Visibility = Visibility.Collapsed;
            this.navigationHelper.OnNavigatedTo(e);
            par = (NevParams)e.Parameter;
            LoadImage(par.file);
        }

        private async void LoadImage(StorageFile file)
        {
            //IRandomAccessStreamWithContentType imgSource = await file.OpenReadAsync();
            //BitmapImage img = new BitmapImage();
            //img.SetSource(imgSource);
            var imageData = await file.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(imageData);
            uint width = decoder.OrientedPixelWidth;
            uint height = decoder.OrientedPixelHeight;
            WriteableBitmap image = new WriteableBitmap(Convert.ToInt32(width), Convert.ToInt32(height));
            imageData.Seek(0);
            await image.SetSourceAsync(imageData);
            mainImage.Source = image;
            if (mainImage.Source != null)
                processButton.Visibility = Visibility.Visible;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion


        private async Task<StorageFile> saveImage(WriteableBitmap WB, string name)
        {
            string FileName = name;
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                          (uint)WB.PixelWidth,
                          (uint)WB.PixelHeight,
                          96.0,
                          96.0,
                          pixels);
                await encoder.FlushAsync();
            }
            return file;
        }

        private async void processButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "Details",
                MaxWidth = this.ActualWidth,
                MaxHeight = this.ActualHeight / 2
            };

            var panel = new StackPanel();
            TextBox tb = new TextBox
            {
                Header = "Nevus description",
                TextWrapping = TextWrapping.Wrap,
            };
            panel.Children.Add(tb);
            
            dialog.Content = panel;

            // The CanExecute of the Command does not enable/disable the button :-(
            dialog.PrimaryButtonText = "OK";
            var cmd = new Common.RelayCommand(() =>
            {
                par.description = tb.Text;
            });

            dialog.PrimaryButtonCommand = cmd;

            dialog.SecondaryButtonText = "Cancel";
            //dialog.SecondaryButtonCommand = new Common.RelayCommand(() =>
            //{
            //    Frame.Navigate(typeof)
            //});
            processButton.Visibility = Visibility.Collapsed;
            cancelButton.Visibility = Visibility.Collapsed;
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                progressRing.IsActive = true;
                progressRing.Visibility = Visibility.Visible;
                //mainImage.Source = null;
                ImageProcessor proc = new ImageProcessor(par);
                await proc.go();
                progressRing.IsActive = false;
                processButton.Visibility = Visibility.Visible;
                cancelButton.Visibility = Visibility.Visible;
                if (proc.resizedImage != null)
                {
                    par.file = await saveImage(proc.resizedImage, par.file.Name);
                    //LoadImage(par.file);
                    //string id = proc.UniqueId;
                    progressRing.Visibility = Visibility.Collapsed;
                    Frame.Navigate(typeof(ImagePage), proc.UniqueId);
                }
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        private void OnSizeChanged(Object sender, SizeChangedEventArgs args)
        {
            mainImage.Width = scrl.ViewportWidth;
            mainImage.Height = scrl.ViewportHeight;
        }
    }
}
