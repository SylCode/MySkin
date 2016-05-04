using MySkin_Alpha.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;

//using AForge.Imaging;
//using AForge.Imaging.Filters;
//using AForge.Math;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.Math;
using System.Drawing;
using Accord;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using MySkin_Alpha.Data;
using Windows.ApplicationModel.Appointments;
using Windows.UI.Xaml;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MySkin_Alpha
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImagePage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private DataItem item;
        private Nevus n;
        private string id;
        //NevParams par;
        //private WriteableBitmap image, resizedImage;
        //private Bitmap Bimage;
        //private StorageFile file;
        //int originalWidth, originalHeigth;
        //private byte[] pixelData;
        //private Methods processor;
        //uint width, height;
        //double scale = 0.25;
        //double scaleFactor;


        #region base
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


        public ImagePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
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
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            item = await DataSource.GetItemAsync(id);
            n = DataSource.GetNevusById(id);
            StorageFile file = await StorageFile.GetFileFromPathAsync(item.imagePath);
            LoadImage(file);
        }

        private Windows.Foundation.Rect GetElementRect(FrameworkElement element)
        {
            Windows.UI.Xaml.Media.GeneralTransform buttonTransform = element.TransformToVisual(null);
            Windows.Foundation.Point point = buttonTransform.TransformPoint(new Windows.Foundation.Point());
            return new Windows.Foundation.Rect(point, new Windows.Foundation.Size(element.ActualWidth, element.ActualHeight));
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
            id = (string)e.Parameter;
            //ProgressRing.IsActive = true;
            navigationHelper.OnNavigatedTo(e);
            //par = (NevParams)e.Parameter;
            //LoadImage(par.file);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);

        }

        #endregion
        #endregion

        #region initMethods

        private async void LoadImage(StorageFile file)
        {
            var imageData = await file.OpenReadAsync();

            var decoder = await BitmapDecoder.CreateAsync(imageData);
            uint width = decoder.OrientedPixelWidth;
            uint height = decoder.OrientedPixelHeight;
            WriteableBitmap image = new WriteableBitmap(Convert.ToInt32(width), Convert.ToInt32(height));
            WriteableBitmap Cimage = new WriteableBitmap(Convert.ToInt32(width), Convert.ToInt32(height));
            WriteableBitmap Bimage = new WriteableBitmap(Convert.ToInt32(width), Convert.ToInt32(height));
            imageData.Seek(0);
            await image.SetSourceAsync(imageData);
            MoleImage.Source = image;
            string[] delimiters = {"\\"};
            string oldpath = file.Path;

            string path = ApplicationData.Current.LocalFolder.Path + "\\color_"+file.Path.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Last();
            file = await StorageFile.GetFileFromPathAsync(path);
            var CimageData = await file.OpenReadAsync();
            CimageData.Seek(0);
            await Cimage.SetSourceAsync(CimageData);
            ColourImage.Source = Cimage;
            path = ApplicationData.Current.LocalFolder.Path + "\\border_" +oldpath.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).Last();
            file = await StorageFile.GetFileFromPathAsync(path);
            var BimageData = await file.OpenReadAsync();
            BimageData.Seek(0);
            await Bimage.SetSourceAsync(BimageData);
            BorderImage.Source = Bimage;

            SafeRisk.Text = item.safe;
            RiskProgressBar.Value = item.risk;
            RiskProgressBar.Maximum = 100;
            AreaProgressBar.Value = item.area;
            AreaProgressBar.Maximum = Nevus.dMax;
            BorderProgressBar.Value = item.borderVariation;
            BorderProgressBar.Maximum = Nevus.bMax;
            ColourProgressBar.Value = n.colorVariation;
            ColourProgressBar.Maximum = Nevus.cMax;
            AssymmetryProgressBar.Value = item.assymmetryRate;
            AssymmetryProgressBar.Maximum = Nevus.aMax;
            AssymmetryProgressBar.Minimum = 1;
            
        }



        private async System.Threading.Tasks.Task<Bitmap> getBitmapFromFile(StorageFile file)
        {
            Bitmap bitmap;

            using (var stream = await file.OpenStreamForReadAsync())
            {
                bitmap = (Bitmap)System.Drawing.Image.FromStream(stream);
            }
            return bitmap;
        }



        #endregion

        private async void Data_ItemClick(object sender, ItemClickEventArgs e)
        {
            MessageDialog dialog;
            switch (((Grid)e.ClickedItem).Name)
            {
                case "Diameter":
                    dialog = new MessageDialog("Area = " + n.area + " mm^2");
                    await dialog.ShowAsync();
                    break;
                case "Assymmetry":
                    dialog = new MessageDialog("Assymmetry = " + n.assymmetryRate);
                    await dialog.ShowAsync();
                    break;
                case "Border":
                    dialog = new MessageDialog("Border irregularity = " + n.borderVariation);
                    await dialog.ShowAsync();
                    break;
                case "Colour":
                    dialog = new MessageDialog("Colour irregularity = " + n.colorVariation);
                    await dialog.ShowAsync();
                    break;
                case "Info":
                    dialog = new MessageDialog("Risk = " + n.risk + "%");
                    await dialog.ShowAsync();
                    break;
            }
        }
    }
}
