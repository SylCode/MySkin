using MySkin_Alpha.Common;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel;
using System.IO;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;

//using AForge.Imaging;
//using AForge.Imaging.Filters;
//using AForge.Math;
using Accord.Imaging.Converters;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.MachineLearning;
using Accord.Math;
using System.Drawing;
using Accord.Statistics.Visualizations;
using Accord;
using Windows.UI.Popups;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using Windows.Graphics.Display;
using System.Linq;
using System.Runtime.Serialization.Json;
using Windows.Data.Json;
using MySkin_Alpha.Data;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace MySkin_Alpha
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    /// 
    //public class NevParams
    //{
    //    public Size captureElementSize;
    //    public StorageFile file;
    //}
    public sealed partial class ImagePage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        NevParams par;
        private WriteableBitmap image, resizedImage;
        private Bitmap Bimage;
        private StorageFile file;
        private Nevus nevus;
        private byte[] pixelData;
        private Methods processor;
        uint width, height;
        int originalWidth, originalHeigth;
        double scale = 0.25;
        double scaleFactor;
        string path,name;
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        /// 
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
        /// 
        #endregion


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
            progressRing.IsActive = true;
            navigationHelper.OnNavigatedTo(e);
            par = (NevParams)e.Parameter;
            LoadImage(par.file);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }
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

        #endregion

        #region Init

        private async void LoadImage(StorageFile file)
        {
            path = file.Path;
            name = file.Name;
            Bimage = await getBitmapFromFile(file);
            var imageData = await file.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(imageData);
            width = decoder.OrientedPixelWidth;
            height = decoder.OrientedPixelHeight;
            image = new WriteableBitmap(Convert.ToInt32(width), Convert.ToInt32(height));
            originalWidth = image.PixelWidth;
            originalHeigth = image.PixelHeight;
            imageData.Seek(0);
            await image.SetSourceAsync(imageData);
            int xL, yL, w, h;
            xL = ((int)width - 640) / 2;
            yL = ((int)height - 640) / 2;
            w = 640;
            h = 640;
            image = image.Crop(xL, yL, w, h);
            width = (uint)image.PixelWidth;
            height = (uint)image.PixelHeight;
            pixelData = getPixelData(image);
            processor = new Methods(Convert.ToInt32(width), Convert.ToInt32(height));
            go();
        }


        private byte[] getPixelData(WriteableBitmap image)
        {
            byte[] pix = new byte[image.PixelWidth * image.PixelHeight * 4];

            var srcPixelStream = image.PixelBuffer.AsStream();
            int l = srcPixelStream.Read(pix, 0, image.PixelWidth * image.PixelHeight * 4);


            return pix;
        }

        private double CalculateScaleFactor(double capWidth, Windows.Foundation.Size imageSize, double interLine)
        {
            double rel = 10.0 / interLine;
            return (rel / ((double)imageSize.Width / capWidth));
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

        private async Task<Bitmap> WritableBitmapToBitmap(WriteableBitmap bitmap)
        {
            Bitmap im;
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                //im = (Bitmap)System.Drawing.Image.FromStream(stream);
                im = (Bitmap)(await bitmap.FromStream(stream));
            }
            return im;
        }
        #endregion

        #region Aforge_proc
        //private Bitmap analyzeBlobs(Bitmap image)
        //{
        //    Bitmap tBimage, tColorImage;
        //    Rectangle aimedRectangle = new Rectangle();
        //    int area, nBlobs;
        //    double colorDeviation = 0.0f;
        //    System.Drawing.Color stDev;

        //    BlobCounter blobCounter = new BlobCounter();
        //    blobCounter.FilterBlobs = true;
        //    blobCounter.MinWidth = 20;
        //    blobCounter.MinHeight = 20;

        //    ConservativeSmoothing smooth = new ConservativeSmoothing(200);
        //    BrightnessCorrection brightness = new BrightnessCorrection(80);
        //    ContrastCorrection contrast = new ContrastCorrection(55);
        //    //ImageToMatrix im2matr = new ImageToMatrix();
        //    // byte[,] cMatrix;
        //    Grayscale gs = new Grayscale(0, 0, 0);
        //    OtsuThreshold otsuThr = new OtsuThreshold();
        //    SISThreshold brad = new SISThreshold();
        //    Threshold simpleThreshold = new Threshold();
        //    Invert inv = new Invert();
        //    Closing close = new Closing();
        //    FillHoles fillHoles = new FillHoles();
        //    fillHoles.MaxHoleHeight = image.Width / 3;
        //    fillHoles.MaxHoleWidth = image.Width / 3;
        //    CannyEdgeDetector canny = new CannyEdgeDetector();
        //    Crop crop = new Crop(new Rectangle(0, 0, 0, 0));





        //    int xL, yL, w, h;
        //    xL = ((int)image.Width - 640) / 2;
        //    yL = ((int)image.Height - 640) / 2;
        //    w = 640;
        //    h = 640;
        //    crop = new Crop(new Rectangle(xL, yL, w, h));

        //    //if (image.Width < image.Height)
        //    //{
        //    //    crop = new Crop(new Rectangle(image.Width / 3, (int)(image.Height / 2.5), image.Width / 3, image.Height / 5));
        //    //}
        //    //else
        //    //{
        //    //    crop = new Crop(new Rectangle(image.Width / 3, (int)(image.Height / 4), image.Width / 3, image.Height / 2));
        //    //}
        //    tBimage = crop.Apply(image);
        //    //im2matr.Convert(tBimage, out cMatrix);


        //    //tBimage = gs.Apply(tBimage);
        //    //for (int i = 1; i <= 3; i++)
        //    //    for (int j = 1; j <= 3; j++)
        //    //    {
        //    //        if (i != 2 || j != 2)
        //    //        {
        //    //            smooth.ApplyInPlace(tBimage, new Rectangle(i * (tBimage.Width / 3), j * (tBimage.Height / 3), tBimage.Width / 3, tBimage.Height / 3));
        //    //        }
        //    //    }
        //    //double meab = tBimage.Mean();
        //    //if (tBimage.Mean()<166)
        //    //{
        //    //    brightness.ApplyInPlace(tBimage);
        //    //}
        //    tBimage = gs.Apply(tBimage);
        //    contrast.ApplyInPlace(tBimage);
        //    int threshold = otsuThr.CalculateThreshold(tBimage, new Rectangle(0, 0, tBimage.Width, tBimage.Height));
        //    simpleThreshold.ThresholdValue = threshold - 40;
        //    simpleThreshold.ApplyInPlace(tBimage);
        //    //brad.ApplyInPlace(tBimage);
        //    inv.ApplyInPlace(tBimage);
        //    close.ApplyInPlace(tBimage);
        //    fillHoles.ApplyInPlace(tBimage);
        //    //dilate(tBimage, 3);

        //    //canny.ApplyInPlace(tBimage);
        //    ////setBackImage(tBimage);

        //    ////int threshold = otsuThr.CalculateThreshold(tBimage, new Rectangle(0, 0, tBimage.Width, tBimage.Height));

        //    ////simpleThreshold.ThresholdValue = threshold - 40;
        //    ////simpleThreshold.ApplyInPlace(tBimage);

        //    ////inv.ApplyInPlace(tBimage);
        //    ////close.ApplyInPlace(tBimage);
        //    ////fillHoles.ApplyInPlace(tBimage);
        //    ////dilate(tBimage, 3);

        //    ////canny.ApplyInPlace(tBimage);
        //    ////setBackImage(tBimage);
        //    blobCounter.ProcessImage(tBimage);

        //    int index = 0;
        //    aimedRectangle = getMaxRectangle(blobCounter.GetObjectsRectangles(), index, tBimage.Width, tBimage.Height);

        //    List<IntPoint> points = blobCounter.GetBlobsEdgePoints(blobCounter.GetObjectsInformation()[index]);

        //    Blob nev = blobCounter.GetObjectsInformation()[index];
        //    blobCounter.ExtractBlobsImage(tBimage, nev, true);
        //    Accord.Point center = nev.CenterOfGravity;
        //    area = nev.Area;

        //    stDev = nev.ColorStdDev; //nev.Image.ToManagedImage().StandardDeviation(nev.Image.ToManagedImage().Mean());
        //    colorDeviation = nev.Image.ToManagedImage().StandardDeviation(nev.Image.ToManagedImage().Mean());

        //    crop = new Crop(aimedRectangle);



        //    tColorImage = crop.Apply(image);
        //    ImageStatistics stats = new ImageStatistics(tColorImage);
        //    //Histogram red = stats.Red;
        //    //Histogram green = stats.Green;
        //    //Histogram blue = stats.Blue;
        //    ////im2matr.Convert(tColorImage, out cMatrix);

        //    //tColorImage = tBimage;
        //    //tBimage = crop.Apply(tBimage);

        //    //colorDeviation = (red.StdDev + green.StdDev + blue.StdDev) / 3;
        //    BlobCounter microBlobCounter = new BlobCounter();
        //    microBlobCounter.ProcessImage(tBimage);
        //    nBlobs = microBlobCounter.ObjectsCount;
        //    ////wrBitmap.DrawRectangle(aimedRectangle.Left, aimedRectangle.Top, aimedRectangle.Right, aimedRectangle.Bottom,Windows.UI.Color.FromArgb(255,255,0,100));

        //    ////setBackImage(nev.Image.ToManagedImage());
        //    areaText.Text = string.Format("{0:0.##}", area);
        //    gravityCenterText.Text = "X:" + string.Format("{0:0.##}", center.X) + " Y:" + string.Format("{0:0.##}", center.Y);
        //    colorDeviationText.Text = string.Format("{0:0.##}", colorDeviation);
        //    innerBlobsText.Text = string.Format("{0:0.##}", nBlobs);
        //    writeInfo("File: " + file.Name + "\nNevus area: " + area + "\nNevus gravity center: " + gravityCenterText.Text + "\nColor Deviation: " + colorDeviation + "\nInner Blobs: " + nBlobs);
        //    var R = new Windows.UI.Xaml.Shapes.Rectangle();
        //    R.Height = tBimage.Height + 50;
        //    R.Width = tBimage.Width + 50;
        //    //R.Margin = new Windows.UI.Xaml.Thickness(aimedRectangle.Left, aimedRectangle.Top, aimedRectangle.Right, aimedRectangle.Bottom);
        //    R.Stroke = new SolidColorBrush(Colors.Red);
        //    R.StrokeThickness = 3;
        //    R.Fill = new SolidColorBrush(Colors.Transparent);
        //    Canvas.SetLeft(R, -R.Width / 2 + R.Width * scale);
        //    Canvas.SetTop(R, -R.Height / 2);
        //    infoCanvas.Children.Add(R);
        //    //setBackImage(tBimage);
        //    return tColorImage;
        //}

        private int getMaxRectangle(Rectangle[] rects, int imageWidth, int imageHeight)
        {
            int ct = 0, index = 0;
            float min = imageWidth * imageHeight;
            IntPoint p1 = new IntPoint(imageWidth / 2, imageHeight / 2);

            foreach (Rectangle rect in rects)
            {
                if (p1.DistanceTo(new IntPoint(rect.Center().X, rect.Center().Y)) < min)
                {
                    min = p1.DistanceTo(new IntPoint(rect.Center().X, rect.Center().Y));
                    index = ct;
                }
                ct++;
            }
            return index;
        }

        
        #endregion

        #region IO

        private async Task<StorageFile> saveImage(WriteableBitmap WB, string name)
        {
            string FileName = name + ".";
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;

            FileName += "bmp";
            BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;

            var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(FileName, CreationCollisionOption.GenerateUniqueName);
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

        private void setBackImage(Bitmap image)
        {
            dispImage.Source = (BitmapSource)image;
        }

        private void setBackImage(BitmapImage image)
        {
            dispImage.Source = image;
        }

        private void setMainImage(Bitmap image)
        {
            mainImage.Source = (BitmapSource)image;
        }

        private void setMainImage(WriteableBitmap image)
        {
            mainImage.Source = image;
            mainImage.UpdateLayout();
        }

        private void setBackImage(WriteableBitmap image)
        {
            dispImage.Source = image;
            dispImage.UpdateLayout();
        }

        private void setBackImage1(WriteableBitmap image)
        {
            dispImage1.Source = image;
            dispImage1.UpdateLayout();
        }

        private void setBackImage2(WriteableBitmap image)
        {
            dispImage2.Source = image;
            dispImage2.UpdateLayout();
        }

        private void setBackImage3(WriteableBitmap image)
        {
            dispImage3.Source = image;
            dispImage3.UpdateLayout();
        }

        private async void writeInfo(Nevus nevus)
        {
            await DataSource.AddNevusToDatabaseAsync(nevus);
            //DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Nevus>));
            //try
            //{
            //    StorageFile f = await ApplicationData.Current.LocalFolder.GetFileAsync("Data.json");
            //    //await FileIO.AppendTextAsync(file, ",");
            //    string content = string.Empty;
            //    List<Nevus> list = new List<Nevus>();
            //    list.Add(nevus);
            //    using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("Data.json", CreationCollisionOption.OpenIfExists))
            //    {
            //        ser.WriteObject(stream, list);
            //    }
            //    //ser = new DataContractJsonSerializer(typeof(List<Nevus>));
            //    var streamer = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync("Data.json");
            //    list = (List<Nevus>)ser.ReadObject(streamer);
            //    using (StreamReader reader = new StreamReader(streamer))
            //    {
            //        content = await reader.ReadToEndAsync();

            //    }

            //    JsonObject o = new JsonObject();
            //    bool t = JsonObject.TryParse(content, out o);
            //}
            //catch (FileNotFoundException)
            //{
            //    using (var stream = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("Data.json", CreationCollisionOption.OpenIfExists))
            //    {
            //        List<Nevus> list = new List<Nevus>();
            //        list.Add(nevus);
            //        ser.WriteObject(stream, list);
                    
            //    }
            //}
            
            
            //foreach (Nevus ne in list)
            //{
            //    double area = ne.area;
            //}
            //await FileIO.AppendTextAsync(file, text + "\n");
        }

        #endregion

        #region Accord_proc
        private async Task<WriteableBitmap> process(byte[] pixelData)
        {
            WriteableBitmap bitmap, Contrast, BinaryGen, BinaryLoc, EdgeGen, EdgeLoc, Grayscale;
            Rectangle aimedRectangle = new Rectangle();
            Nevus nevus;
            int area;
            double colorDeviation = 0.0f;

            bitmap = new WriteableBitmap((int)width, (int)height);
            Contrast = new WriteableBitmap((int)width, (int)height);
            BinaryGen = new WriteableBitmap((int)width, (int)height);
            BinaryLoc = new WriteableBitmap((int)width, (int)height);
            EdgeGen = new WriteableBitmap((int)width, (int)height);
            EdgeLoc = new WriteableBitmap((int)width, (int)height);
            Grayscale = new WriteableBitmap((int)width, (int)height);



            byte[] gray = processor.addContrast(pixelData, 4);
            gray = processor.Grayscale(gray);
            byte[] edgeGen;

            edgeGen = processor.Edge(gray, Mask.Gaussian3x3, 0.2, 0);

            edgeGen = processor.invert(edgeGen);

            int threshold = processor.getOtsuThreshold(gray) - 45;
            Windows.Foundation.Point seed = new Windows.Foundation.Point((int)width / 2, (int)height / 2);

            await writeBitmap(EdgeGen, edgeGen);
            //await writeBitmap(EdgeLoc, edgeLoc);
            //await writeBitmap(Grayscale, gray);

            Bitmap segmentImage = (Bitmap)EdgeGen;
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinWidth = 40;
            blobCounter.MinHeight = 40;
            blobCounter.MaxHeight = (int)height - 10;
            blobCounter.MaxWidth = (int)width - 10;
            blobCounter.ObjectsOrder = ObjectsOrder.Size;
            blobCounter.ProcessImage(segmentImage);




            int index = 0;

            if (blobCounter.ObjectsCount != 0)
            {
                Blob nev = blobCounter.GetObjectsInformation()[index];
                if (nev.Rectangle.Contains((int)(width / 2), (int)(height / 2)) && new IntPoint((int)(width / 2), (int)(height / 2)).DistanceTo(new Accord.IntPoint(nev.Rectangle.Center().X, nev.Rectangle.Center().Y)) < 50)
                {
                    aimedRectangle = nev.Rectangle;
                }
                else
                {
                    index = getMaxRectangle(blobCounter.GetObjectsRectangles(), (int)width - 1, (int)height - 1);
                    nev = blobCounter.GetObjectsInformation()[index];
                    aimedRectangle = nev.Rectangle;
                }
                List<IntPoint> points = blobCounter.GetBlobsEdgePoints(blobCounter.GetObjectsInformation()[index]);
                List<IntPoint> pointsL, pointsR, pointsU, pointsD;
                pointsL = new List<IntPoint>();
                pointsR = new List<IntPoint>();
                pointsU = new List<IntPoint>();
                pointsD = new List<IntPoint>();
                blobCounter.GetBlobsLeftAndRightEdges(nev, out pointsL, out pointsR);
                blobCounter.GetBlobsTopAndBottomEdges(nev, out pointsU, out pointsD);
                double assymmetryRate;
                byte[] border, color;
                pixelData = processor.analyzeBlob(pixelData, edgeGen, aimedRectangle, pointsL, pointsR, pointsU, pointsD, out assymmetryRate, out border, out color);
                colorDeviation = processor.colorVariation;
                area = nev.Area;
                double blackness = processor.blackness;
                double blueness = processor.blueness;
                double redness = processor.redness;
                await writeBitmap(bitmap, pixelData);

                Accord.Point center = nev.CenterOfGravity;

                scaleFactor = CalculateScaleFactor(par.captureElementSize.Width, new Windows.Foundation.Size(originalWidth, originalHeigth), par.interLine);

                bitmap.DrawRectangle(aimedRectangle.Left, aimedRectangle.Top, aimedRectangle.Right, aimedRectangle.Bottom, Windows.UI.Color.FromArgb(255, 255, 0, 100));

                double scaledArea = (aimedRectangle.Width * scaleFactor) * (aimedRectangle.Height * scaleFactor);
                double borderEvenRate = getAsymmentryRate(center, blobCounter.GetBlobsEdgePoints(nev));
                bool asymetric = false;
                bool big = false;
                asymetric = borderEvenRate > 100 ? true : false;
                big = scaledArea > 25 ? true : false;

                await saveImage(bitmap, "sfs.jpg");
                //ProgressRing.IsActive = false;
                //Accord.IO.CsvWriter csvWriter = new Accord.IO.CsvWriter()
                //writeInfo(string.Format("{0:0.####}", scaledArea) + "," + string.Format("{0:0.####}", borderEvenRate) + "," + string.Format("{0:0.####}", colorDeviation) + "," + string.Format("{0:0.####}", blackness) + "," + string.Format("{0:0.####}", blueness) + "," + string.Format("{0:0.####}", redness) + ",");
                nevus = new Nevus(name,"", scaledArea, borderEvenRate, assymmetryRate, colorDeviation, blackness, blueness, redness,path);
                FuzzyDataItem item = new FuzzyDataItem();
                Fuzzy.GetMembership(nevus.assymmetryRate, nevus.borderVariation, nevus.colorVariation, nevus.area, nevus.blueness, nevus.redness, out item);

                writeInfo(nevus);
                areaText.Text = string.Format("{0:0.##}", scaledArea);
                borderVariationText.Text = string.Format("{0:0.##}", borderEvenRate);
                colorDeviationText.Text = string.Format("{0:0.##}", colorDeviation);
                bluenessText.Text = string.Format("{0:0.##}", blackness);
                bluenessText.Text = string.Format("{0:0.##}", blueness);
                rednessText.Text = string.Format("{0:0.##}", redness);
                //MessageDialog dialog = new MessageDialog("Area = " + string.Format("{0:0.##}", scaledArea) + " mm. " + "\nBorderVar = " + string.Format("{0:0.##}", BorderEvenRate) + "; \nColorDev = " + string.Format("{0:0.##}", colorDeviation) + "; \nDarkness = " + string.Format("{0:0.##}", blackness) + "; \nBlueness = " + string.Format("{0:0.##}", blueness) + "; \nRedness = " + string.Format("{0:0.##}", redness) + " \nAsymmetric = " + asymetric + " \nBigger than norm = " + big);
                //await dialog.ShowAsync();

            }
            else
            {
                MessageDialog dialog = new MessageDialog("No moles were found, please, try another photo.");
                await dialog.ShowAsync();
            }

            return bitmap;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private double getAsymmentryRate(Accord.Point gravityCenter, List<IntPoint> blobContour)
        {
            List<double> dists = new List<double>();
            foreach (IntPoint point in blobContour)
            {
                dists.Add(gravityCenter.DistanceTo(point));
            }
            double[] d = dists.ToArray<double>();

            return Accord.Statistics.Measures.Variance(d);
        }


        private async Task writeBitmap(WriteableBitmap bitmap, byte[] pixelData)
        {
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(pixelData, 0, pixelData.Length);
            }
        }
        #endregion

        private async void go()
        {
            if (pixelData != null)
            {
                mainImage.Source = null;
                WriteableBitmap gray = await process(pixelData);
                progressRing.IsActive = false;
                //Bitmap test = await WritableBitmapToBitmap(gray);
                //Bitmap test1 = analyzeBlobs(Bimage);
                //setBackImage(test1);
                setMainImage(gray);
                //setBackImage(gray);
            }
        }
    }
}
