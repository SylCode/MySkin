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
using System.Text;
using Accord.Imaging.Converters;
using Accord.Imaging;
using Accord.Imaging.Filters;
using Accord.MachineLearning;
using Accord.Math;
using System.Drawing;
using Accord.Statistics.Visualizations;
using Accord;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.Serialization.Json;
using MySkin_Alpha.Data;
using Windows.Foundation;
using System.Text.RegularExpressions;

namespace MySkin_Alpha
{
    public sealed class ImageProcessor: IDisposable
    {
        private NevParams par;
        private WriteableBitmap image;
        public WriteableBitmap resizedImage, border_image, color_image;
        private Bitmap Bimage;
        private StorageFile file;
        int originalWidth, originalHeigth;
        private byte[] pixelData;
        private Methods processor;
        uint width, height;
        public string UniqueId;
        private string path;
        double scaleFactor;

        public ImageProcessor(NevParams pars)
        {
            par = pars;
        }
        public async Task go()
        {
            resizedImage = await LoadImage(par.file);
        }

        #region initMethods

        private async Task<WriteableBitmap> LoadImage(StorageFile file)
        {
            UniqueId = file.Name;
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
            xL = ((int)width - 840) / 2;
            yL = ((int)height - 840) / 2;
            w = 840;
            h = 840;
            
            image = image.Crop(xL, yL, w, h);
            width = (uint)image.PixelWidth;
            height = (uint)image.PixelHeight;

            pixelData = getPixelData(image);
            processor = new Methods(Convert.ToInt32(width), Convert.ToInt32(height));
            WriteableBitmap res = await process(pixelData);
            return res;
        }

        private double CalculateScaleFactor(double capWidth, Windows.Foundation.Size imageSize, double interLine)
        {
            double rel = 10.0 / interLine;
            return (rel / ((double)imageSize.Width / capWidth)/* * (double)interLine*/);
        }

        private byte[] getPixelData(WriteableBitmap image)
        {
            byte[] pix = new byte[image.PixelWidth * image.PixelHeight * 4];

            var srcPixelStream = image.PixelBuffer.AsStream();
            int l = srcPixelStream.Read(pix, 0, image.PixelWidth * image.PixelHeight * 4);


            return pix;
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

        #region IO

      
        private async Task writeBitmap(WriteableBitmap bitmap, byte[] pixelData)
        {
            using (Stream stream = bitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(pixelData, 0, pixelData.Length);
            }
        }

        private async void writeInfo(Nevus nevus)
        {
            await DataSource.AddNevusToDatabaseAsync(nevus);
        }

        #endregion

        #region manual_proc
        private async Task<WriteableBitmap> process(byte[] pixelData)
        {
            WriteableBitmap bitmap, Contrast, BinaryGen, BinaryLoc, EdgeGen, EdgeLoc, Grayscale;
            Rectangle aimedRectangle = new Rectangle();
            Nevus nevus;
            int area;
            double colorDeviation = 0.0f;

            bitmap = new WriteableBitmap((int)width, (int)height);
            color_image = new WriteableBitmap((int)width, (int)height);
            border_image = new WriteableBitmap((int)width, (int)height);
            //BinaryLoc = new WriteableBitmap((int)width, (int)height);
            EdgeGen = new WriteableBitmap((int)width, (int)height);
            //EdgeLoc = new WriteableBitmap((int)width, (int)height);
            //Grayscale = new WriteableBitmap((int)width, (int)height);



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
                //if (nev.Rectangle.Contains((int)(width / 2), (int)(height / 2)) && new IntPoint((int)(width / 2), (int)(height / 2)).DistanceTo(new Accord.IntPoint(nev.Rectangle.Center().X, nev.Rectangle.Center().Y)) < 50)
                //{
                //    aimedRectangle = nev.Rectangle;
                //}
                //else
                //{
                    index = processor.getMaxRectangle(blobCounter.GetObjectsRectangles(), (int)width - 1, (int)height - 1);
                    nev = blobCounter.GetObjectsInformation()[index];
                    aimedRectangle = nev.Rectangle;
                //}
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
                pixelData = processor.analyzeBlob(pixelData, edgeGen, aimedRectangle, pointsL, pointsR, pointsU, pointsD,out assymmetryRate, out border, out color);
                colorDeviation = processor.colorVariation;
                area = nev.Area;
                double blackness = processor.blackness;
                double blueness = processor.blueness;
                double redness = processor.redness;
                await writeBitmap(bitmap, pixelData);
                await writeBitmap(color_image, color);
                await writeBitmap(border_image, border);
                color_image = color_image.Crop(aimedRectangle.X - 20, aimedRectangle.Y - 20, aimedRectangle.Width + 40, aimedRectangle.Height + 40);
                border_image = border_image.Crop(aimedRectangle.X - 20, aimedRectangle.Y - 20, aimedRectangle.Width + 40, aimedRectangle.Height + 40);

                Accord.Point center = nev.CenterOfGravity;

                scaleFactor = CalculateScaleFactor(par.captureElementSize.Width, new Windows.Foundation.Size(originalWidth, originalHeigth), par.interLine);

                bitmap.DrawRectangle(aimedRectangle.Left, aimedRectangle.Top, aimedRectangle.Right, aimedRectangle.Bottom, Windows.UI.Color.FromArgb(255, 255, 0, 100));

                double scaledArea = (aimedRectangle.Width * scaleFactor) * (aimedRectangle.Height * scaleFactor);
                double borderEvenRate = getAsymmentryRate(center, blobCounter.GetBlobsEdgePoints(nev));
                bool asymetric = false;
                bool big = false;
                asymetric = borderEvenRate > 100 ? true : false;
                big = scaledArea > 25 ? true : false;

                bitmap = bitmap.Crop(aimedRectangle.X - 20, aimedRectangle.Y - 20, aimedRectangle.Width + 40, aimedRectangle.Height + 40);
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(UniqueId, CreationCollisionOption.ReplaceExisting);
                await ApplicationData.Current.LocalFolder.CreateFileAsync("color_"+UniqueId, CreationCollisionOption.ReplaceExisting);
                await ApplicationData.Current.LocalFolder.CreateFileAsync("border_"+UniqueId, CreationCollisionOption.ReplaceExisting);
                //await saveImage(bitmap, UniqueId);

                nevus = new Nevus(UniqueId, par.description, scaledArea, borderEvenRate,assymmetryRate, colorDeviation, blackness, blueness, redness, file.Path);
                writeInfo(nevus);
                //writeInfo(string.Format("{0:0.####}", scaledArea) + "," + string.Format("{0:0.####}", borderEvenRate) + "," + string.Format("{0:0.####}", colorDeviation) + "," + string.Format("{0:0.####}", blackness) + "," + string.Format("{0:0.####}", blueness) + "," + string.Format("{0:0.####}", redness) + ",");
                //MessageDialog dialog = new MessageDialog("Area = " + string.Format("{0:0.##}", scaledArea) + " mm. " + "\nBorderVar = " + string.Format("{0:0.##}", borderEvenRate) + "; \nColorDev = " + string.Format("{0:0.##}", colorDeviation) + "; \nDarkness = " + string.Format("{0:0.##}", blackness) + "; \nBlueness = " + string.Format("{0:0.##}", blueness) + "; \nRedness = " + string.Format("{0:0.##}", redness) + " \nAsymmetric = " + asymetric + " \nBigger than norm = " + big);
                //await dialog.ShowAsync();

            }
            else
            {
                MessageDialog dialog = new MessageDialog("No moles were found, please, try another photo. Perhaps without the flash");
                await dialog.ShowAsync();
                bitmap = null;
            }

            return bitmap;
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



        #endregion
        
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ImageProcessor() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
