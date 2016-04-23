﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;

namespace MySkin
{
    public enum ObjectsOrder
    {
        /// <summary>
        /// Unsorted order (as it is collected by algorithm).
        /// </summary>
        None,

        /// <summary>
        /// Objects are sorted by size in descending order (bigger objects go first).
        /// Size is calculated as <b>Width * Height</b>.
        /// </summary>
        Size,

        /// <summary>
        /// Objects are sorted by area in descending order (bigger objects go first).
        /// </summary>
        Area,

        /// <summary>
        /// Objects are sorted by Y coordinate, then by X coordinate in ascending order
        /// (smaller coordinates go first).
        /// </summary>
        YX,

        /// <summary>
        /// Objects are sorted by X coordinate, then by Y coordinate in ascending order
        /// (smaller coordinates go first).
        /// </summary>
        XY
    }


    public abstract class BlobCounterBase
    {
        // found blobs
        List<Blob> blobs = new List<Blob>();

        // objects' sort order
        private ObjectsOrder objectsOrder = ObjectsOrder.None;

        // filtering by size is required or not
        private bool filterBlobs = false;
        private IBlobsFilter filter = null;

        // coupled size filtering or not
        private bool coupledSizeFiltering = false;

        // blobs' minimal and maximal size
        private int minWidth = 1;
        private int minHeight = 1;
        private int maxWidth = int.MaxValue;
        private int maxHeight = int.MaxValue;

        /// <summary>
        /// Objects count.
        /// </summary>
        protected int objectsCount;

        /// <summary>
        /// Objects' labels.
        /// </summary>
        protected int[] objectLabels;

        /// <summary>
        /// Width of processed image.
        /// </summary>
        protected int imageWidth;

        /// <summary>
        /// Height of processed image.
        /// </summary>
        protected int imageHeight;

        /// <summary>
        /// Objects count.
        /// </summary>
        /// 
        /// <remarks><para>Number of objects (blobs) found by <see cref="ProcessImage(Bitmap)"/> method.
        /// </para></remarks>
        /// 
        public int ObjectsCount
        {
            get { return objectsCount; }
        }

        /// <summary>
        /// Objects' labels.
        /// </summary>
        /// 
        /// <remarks>The array of <b>width</b> * <b>height</b> size, which holds
        /// labels for all objects. Background is represented with <b>0</b> value,
        /// but objects are represented with labels starting from <b>1</b>.</remarks>
        /// 
        public int[] ObjectLabels
        {
            get { return objectLabels; }
        }

        /// <summary>
        /// Objects sort order.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies objects' sort order, which are provided
        /// by <see cref="GetObjectsRectangles"/>, <see cref="GetObjectsInformation"/>, etc.
        /// </para></remarks>
        /// 
        public ObjectsOrder ObjectsOrder
        {
            get { return objectsOrder; }
            set { objectsOrder = value; }
        }

        /// <summary>
        /// Specifies if blobs should be filtered.
        /// </summary>
        /// 
        /// <remarks><para>If the property is equal to <b>false</b>, then there is no any additional
        /// post processing after image was processed. If the property is set to <b>true</b>, then
        /// blobs filtering is done right after image processing routine. If <see cref="BlobsFilter"/>
        /// is set, then custom blobs' filtering is done, which is implemented by user. Otherwise
        /// blobs are filtered according to dimensions specified in <see cref="MinWidth"/>,
        /// <see cref="MinHeight"/>, <see cref="MaxWidth"/> and <see cref="MaxHeight"/> properties.</para>
        /// 
        /// <para>Default value is set to <see langword="false"/>.</para></remarks>
        /// 
        public bool FilterBlobs
        {
            get { return filterBlobs; }
            set { filterBlobs = value; }
        }

        /// <summary>
        /// Specifies if size filetering should be coupled or not.
        /// </summary>
        /// 
        /// <remarks><para>In uncoupled filtering mode, objects are filtered out in the case if
        /// their width is smaller than <see cref="MinWidth"/> <b>or</b> height is smaller than 
        /// <see cref="MinHeight"/>. But in coupled filtering mode, objects are filtered out in
        /// the case if their width is smaller than <see cref="MinWidth"/> <b>and</b> height is
        /// smaller than <see cref="MinHeight"/>. In both modes the idea with filtering by objects'
        /// maximum size is the same as filtering by objects' minimum size.</para>
        /// 
        /// <para>Default value is set to <see langword="false"/>, what means uncoupled filtering by size.</para>
        /// </remarks>
        /// 
        public bool CoupledSizeFiltering
        {
            get { return coupledSizeFiltering; }
            set { coupledSizeFiltering = value; }
        }

        /// <summary>
        /// Minimum allowed width of blob.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies minimum object's width acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        /// 
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        /// 
        public int MinWidth
        {
            get { return minWidth; }
            set { minWidth = value; }
        }

        /// <summary>
        /// Minimum allowed height of blob.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies minimum object's height acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        /// 
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        /// 
        public int MinHeight
        {
            get { return minHeight; }
            set { minHeight = value; }
        }

        /// <summary>
        /// Maximum allowed width of blob.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies maximum object's width acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        /// 
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        /// 
        public int MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; }
        }

        /// <summary>
        /// Maximum allowed height of blob.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies maximum object's height acceptable by blob counting
        /// routine and has power only when <see cref="FilterBlobs"/> property is set to
        /// <see langword="true"/> and <see cref="BlobsFilter">custom blobs' filter</see> is
        /// set to <see langword="null"/>.</para>
        /// 
        /// <para>See documentation to <see cref="CoupledSizeFiltering"/> for additional information.</para>
        /// </remarks>
        /// 
        public int MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = value; }
        }

        /// <summary>
        /// Custom blobs' filter to use.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies custom blobs' filtering routine to use. It has
        /// effect only in the case if <see cref="FilterBlobs"/> property is set to <see langword="true"/>.</para>
        /// 
        /// <para><note>When custom blobs' filtering routine is set, it has priority over default filtering done
        /// with <see cref="MinWidth"/>, <see cref="MinHeight"/>, <see cref="MaxWidth"/> and <see cref="MaxHeight"/>.</note></para>
        /// </remarks>
        /// 
        public IBlobsFilter BlobsFilter
        {
            get { return filter; }
            set { filter = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounterBase"/> class.
        /// </summary>
        /// 
        /// <remarks>Creates new instance of the <see cref="BlobCounterBase"/> class with
        /// an empty objects map. Before using methods, which provide information about blobs
        /// or extract them, the <see cref="ProcessImage(Bitmap)"/>,
        /// <see cref="ProcessImage(BitmapData)"/> or <see cref="ProcessImage(WriteableBitmap)"/>
        /// method should be called to collect objects map.</remarks>
        /// 
        protected BlobCounterBase() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounterBase"/> class.
        /// </summary>
        /// 
        /// <param name="image">Binary image to look for objects in.</param>
        /// 
        /// <remarks>Creates new instance of the <see cref="BlobCounterBase"/> class with
        /// initialized objects map built by calling <see cref="ProcessImage(Bitmap)"/> method.</remarks>
        /// 
        protected BlobCounterBase(WriteableBitmap image)
        {

            ProcessImage(image);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounterBase"/> class.
        /// </summary>
        /// 
        /// <param name="imageData">Binary image data to look for objects in.</param>
        /// 
        /// <remarks>Creates new instance of the <see cref="BlobCounterBase"/> class with
        /// initialized objects map built by calling <see cref="ProcessImage(BitmapData)"/> method.</remarks>
        /// 
        //protected BlobCounterBase(BitmapData imageData)
        //{
        //    ProcessImage(imageData);
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobCounterBase"/> class.
        /// </summary>
        /// 
        /// <param name="image">Unmanaged binary image to look for objects in.</param>
        /// 
        /// <remarks>Creates new instance of the <see cref="BlobCounterBase"/> class with
        /// initialized objects map built by calling <see cref="ProcessImage(WriteableBitmap)"/> method.</remarks>
        /// 
        //protected BlobCounterBase(WriteableBitmap image)
        //{
        //    ProcessImage(image);
        //}

        /// <summary>
        /// Build objects map.
        /// </summary>
        /// 
        /// <param name="image">Source binary image.</param>
        /// 
        /// <remarks><para>Processes the image and builds objects map, which is used later to extracts blobs.</para></remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the source image.</exception>
        /// 

        /// <summary>
        /// Build objects map.
        /// </summary>
        /// 
        /// <param name="imageData">Source binary image data.</param>
        /// 
        /// <remarks><para>Processes the image and builds objects map, which is used later to extracts blobs.</para></remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the source image.</exception>
        /// 

        /// <summary>
        /// Build object map from raw image data.
        /// </summary>
        /// 
        /// <param name="image">Source unmanaged binary image data.</param>
        /// 
        /// <remarks><para>Processes the image and builds objects map, which is used later to extracts blobs.</para></remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the source image.</exception>
        /// <exception cref="InvalidImagePropertiesException">Thrown by some inherited classes if some image property other
        /// than the pixel format is not supported. See that class's documentation or the exception message for details.</exception>
        /// 
        public byte[] getPixelData(WriteableBitmap image)
        {
            byte[] pix = new byte[image.PixelWidth * image.PixelHeight * 4];

            var srcPixelStream = image.PixelBuffer.AsStream();
            int l = srcPixelStream.Read(pix, 0, image.PixelWidth * image.PixelHeight * 4);


            return pix;
        }

        public void ProcessImage(WriteableBitmap image)
        {
            imageWidth = image.PixelWidth;
            imageHeight = image.PixelHeight;

            byte[] imageData = getPixelData(image);
            // do actual objects map building
            BuildObjectsMap(image);

            // collect information about blobs
            CollectObjectsInfo(imageData);

            // filter blobs by size if required
            if (filterBlobs)
            {
                // labels remapping array
                int[] labelsMap = new int[objectsCount + 1];
                for (int i = 1; i <= objectsCount; i++)
                {
                    labelsMap[i] = i;
                }

                // check dimension of all objects and filter them
                int objectsToRemove = 0;

                if (filter == null)
                {
                    for (int i = objectsCount - 1; i >= 0; i--)
                    {
                        int blobWidth = blobs[i].Rectangle.Width;
                        int blobHeight = blobs[i].Rectangle.Height;

                        if (coupledSizeFiltering == false)
                        {
                            // uncoupled filtering
                            if (
                                (blobWidth < minWidth) || (blobHeight < minHeight) ||
                                (blobWidth > maxWidth) || (blobHeight > maxHeight))
                            {
                                labelsMap[i + 1] = 0;
                                objectsToRemove++;
                                blobs.RemoveAt(i);
                            }
                        }
                        else
                        {
                            // coupled filtering
                            if (
                                ((blobWidth < minWidth) && (blobHeight < minHeight)) ||
                                ((blobWidth > maxWidth) && (blobHeight > maxHeight)))
                            {
                                labelsMap[i + 1] = 0;
                                objectsToRemove++;
                                blobs.RemoveAt(i);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = objectsCount - 1; i >= 0; i--)
                    {
                        if (!filter.Check(blobs[i]))
                        {
                            labelsMap[i + 1] = 0;
                            objectsToRemove++;
                            blobs.RemoveAt(i);
                        }
                    }
                }

                // update labels remapping array
                int label = 0;
                for (int i = 1; i <= objectsCount; i++)
                {
                    if (labelsMap[i] != 0)
                    {
                        label++;
                        // update remapping array
                        labelsMap[i] = label;
                    }
                }

                // repair object labels
                for (int i = 0, n = objectLabels.Length; i < n; i++)
                {
                    objectLabels[i] = labelsMap[objectLabels[i]];
                }

                objectsCount -= objectsToRemove;

                // repair IDs
                for (int i = 0, n = blobs.Count; i < n; i++)
                {
                    blobs[i].ID = i + 1;
                }
            }

            // do we need to sort the list?
            if (objectsOrder != ObjectsOrder.None)
            {
                blobs.Sort(new BlobsSorter(objectsOrder));
            }
        }

        /// <summary>
        /// Get objects' rectangles.
        /// </summary>
        /// 
        /// <returns>Returns array of objects' rectangles.</returns>
        /// 
        /// <remarks>The method returns array of objects rectangles. Before calling the
        /// method, the <see cref="ProcessImage(Bitmap)"/>, <see cref="ProcessImage(BitmapData)"/>
        /// or <see cref="ProcessImage(WriteableBitmap)"/> method should be called, which will
        /// build objects map.</remarks>
        /// 
        /// <exception cref="ApplicationException">No image was processed before, so objects' rectangles
        /// can not be collected.</exception>
        /// 
        public Rectangle[] GetObjectsRectangles()
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");

            Rectangle[] rects = new Rectangle[objectsCount];

            for (int i = 0; i < objectsCount; i++)
            {
                rects[i] = blobs[i].Rectangle;
            }

            return rects;
        }

        /// <summary>
        /// Get objects' information.
        /// </summary>
        /// 
        /// <returns>Returns array of partially initialized blobs (without <see cref="Blob.Image"/> property initialized).</returns>
        /// 
        /// <remarks><para>By the amount of provided information, the method is between <see cref="GetObjectsRectangles"/> and
        /// <see cref="GetObjects( WriteableBitmap, bool )"/> methods. The method provides array of blobs without initialized their image.
        /// Blob's image may be extracted later using <see cref="ExtractBlobsImage( Bitmap, Blob, bool )"/>
        /// or <see cref="ExtractBlobsImage( WriteableBitmap, Blob, bool )"/> method.
        /// </para></remarks>
        /// 
        /// <example>
        /// <code>
        /// // create blob counter and process image
        /// BlobCounter bc = new BlobCounter( sourceImage );
        /// // specify sort order
        /// bc.ObjectsOrder = ObjectsOrder.Size;
        /// // get objects' information (blobs without image)
        /// Blob[] blobs = bc.GetObjectInformation( );
        /// // process blobs
        /// foreach ( Blob blob in blobs )
        /// {
        ///     // check blob's properties
        ///     if ( blob.Rectangle.Width > 50 )
        ///     {
        ///         // the blob looks interesting, let's extract it
        ///         bc.ExtractBlobsImage( sourceImage, blob );
        ///     }
        /// }
        /// </code>
        /// </example>
        /// 
        /// <exception cref="ApplicationException">No image was processed before, so objects' information
        /// can not be collected.</exception>
        /// 
        public Blob[] GetObjectsInformation()
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");

            Blob[] blobsToReturn = new Blob[objectsCount];

            // create each blob
            for (int k = 0; k < objectsCount; k++)
            {
                blobsToReturn[k] = new Blob(blobs[k]);
            }

            return blobsToReturn;
        }

        /// <summary>
        /// Get blobs.
        /// </summary>
        /// 
        /// <param name="image">Source image to extract objects from.</param>
        /// 
        /// <returns>Returns array of blobs.</returns>
        /// <param name="extractInOriginalSize">Specifies size of blobs' image to extract.
        /// If set to <see langword="true"/> each blobs' image will have the same size as
        /// the specified image. If set to <see langword="false"/> each blobs' image will
        /// have the size of its blob.</param>
        ///
        /// <remarks><para>The method returns array of blobs. Before calling the
        /// method, the <see cref="ProcessImage(Bitmap)"/>, <see cref="ProcessImage(BitmapData)"/>
        /// or <see cref="ProcessImage(WriteableBitmap)"/> method should be called, which will build
        /// objects map.</para>
        /// 
        /// <para>The method supports 24/32 bpp color and 8 bpp indexed grayscale images.</para>
        /// </remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the provided image.</exception>
        /// <exception cref="ApplicationException">No image was processed before, so objects
        /// can not be collected.</exception>
        /// 
        //public Blob[] GetObjects(byte[] imageData, bool extractInOriginalSize)
        //{
        //    Blob[] blobs = null;
        //    // lock source bitmap data

        //    try
        //    {
        //        // process image
        //        blobs = GetObjects(writeBitmap(, extractInOriginalSize);
        //    }
        //    finally
        //    {
        //    }
        //    return blobs;
        //}

        //private async WriteableBitmap writeBitmap(WriteableBitmap bitmap, byte[] pixelData)
        //{
        //    WriteableBitmap bmp;
        //    using (Stream stream = bitmap.PixelBuffer.AsStream())
        //    {
        //        await stream.WriteAsync(pixelData, 0, pixelData.Length);
        //    }
        //}

        /// <summary>
        /// Get blobs.
        /// </summary>
        /// 
        /// <param name="image">Source unmanaged image to extract objects from.</param>
        /// <param name="extractInOriginalSize">Specifies size of blobs' image to extract.
        /// If set to <see langword="true"/> each blobs' image will have the same size as
        /// the specified image. If set to <see langword="false"/> each blobs' image will
        /// have the size of its blob.</param>
        /// 
        /// <returns>Returns array of blobs.</returns>
        /// 
        /// <remarks><para>The method returns array of blobs. Before calling the
        /// method, the <see cref="ProcessImage(Bitmap)"/>, <see cref="ProcessImage(BitmapData)"/>
        /// or <see cref="ProcessImage(WriteableBitmap)"/> method should be called, which will build
        /// objects map.</para>
        /// 
        /// <para>The method supports 24/32 bpp color and 8 bpp indexed grayscale images.</para>
        /// </remarks>
        ///
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the provided image.</exception>
        /// <exception cref="ApplicationException">No image was processed before, so objects
        /// can not be collected.</exception>
        /// 
        public Blob[] GetObjects(WriteableBitmap image, bool extractInOriginalSize)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");



            // image size
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            byte[] src;
            byte[] dst;
            //int srcStride = image.Stride;
            int pixelSize = 4;

            Blob[] objects = new Blob[objectsCount];

            // create each image
            for (int k = 0; k < objectsCount; k++)
            {
                int objectWidth = blobs[k].Rectangle.Width;
                int objectHeight = blobs[k].Rectangle.Height;

                int blobImageWidth = (extractInOriginalSize) ? width : objectWidth;
                int blobImageHeight = (extractInOriginalSize) ? height : objectHeight;

                int xmin = blobs[k].Rectangle.X;
                int xmax = xmin + objectWidth - 1;
                int ymin = blobs[k].Rectangle.Y;
                int ymax = ymin + objectHeight - 1;

                int label = blobs[k].ID;

                // create new image
                WriteableBitmap dstImage = new WriteableBitmap(blobImageWidth, blobImageHeight);

                // copy image


                src = getPixelData(image);
                dst = getPixelData(dstImage);
                int p = ymin * width + xmin;
                int index = p*pixelSize;

                //if (extractInOriginalSize)
                //{
                //    // allign destination pointer also
                //    dst += ymin + xmin * pixelSize;
                //}

                //int srcOffset = srcStride - objectWidth * pixelSize;
                //int dstOffset = dstImage.Stride - objectWidth * pixelSize;
                int labelsOffset = width - objectWidth;

                // for each line
                for (int y = ymin; y <= ymax; y++)
                {
                    // copy each pixel
                    for (int x = xmin; x <= xmax; x++, p++, index += pixelSize/*, dst += pixelSize, src += pixelSize*/)
                    {
                        if (objectLabels[p] == label)
                        {
                            // copy pixel
                            dst[index] = src[index];
                            dst[index + 1] = src[index + 1];
                            dst[index + 2] = src[index + 2];
                            dst[index + 3] = src[index + 3];

                            //if (pixelSize > 1)
                            //{
                            //    dst[1] = src[1];
                            //    dst[2] = src[2];

                            //    if (pixelSize > 3)
                            //    {
                            //        dst[3] = src[3];
                            //    }
                            //}
                        }
                    }
                    //src += srcOffset;
                    //dst += dstOffset;
                    p += labelsOffset;
                }

                objects[k] = new Blob(blobs[k]);
                objects[k].Image = dstImage;
                objects[k].OriginalSize = extractInOriginalSize;
            }

            return objects;
        }

        /// <summary>
        /// Extract blob's image.
        /// </summary>
        /// 
        /// <param name="image">Source image to extract blob's image from.</param>
        /// <param name="blob">Blob which is required to be extracted.</param>
        /// <param name="extractInOriginalSize">Specifies size of blobs' image to extract.
        /// If set to <see langword="true"/> each blobs' image will have the same size as
        /// the specified image. If set to <see langword="false"/> each blobs' image will
        /// have the size of its blob.</param>
        ///
        /// <remarks><para>The method is used to extract image of partially initialized blob, which
        /// was provided by <see cref="GetObjectsInformation"/> method. Before calling the
        /// method, the <see cref="ProcessImage(Bitmap)"/>, <see cref="ProcessImage(BitmapData)"/>
        /// or <see cref="ProcessImage(WriteableBitmap)"/> method should be called, which will build
        /// objects map.</para>
        /// 
        /// <para>The method supports 24/32 bpp color and 8 bpp indexed grayscale images.</para>
        /// </remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the provided image.</exception>
        /// <exception cref="ApplicationException">No image was processed before, so blob
        /// can not be extracted.</exception>
        /// 
        //public void ExtractBlobsImage(WriteableBitmap image, Blob blob, bool extractInOriginalSize)
        //{
        //    // lock source bitmap data
        //    //BitmapData imageData = image.LockBits(
        //    //    new Rectangle(0, 0, image.Width, image.Height),
        //    //    ImageLockMode.ReadOnly, image.PixelFormat);

        //    try
        //    {
        //        // process image
        //        ExtractBlobsImage(image, blob, extractInOriginalSize);
        //    }
        //    finally
        //    {
        //    }
        //}

        /// <summary>
        /// Extract blob's image.
        /// </summary>
        /// 
        /// <param name="image">Source unmanaged image to extract blob's image from.</param>
        /// <param name="blob">Blob which is required to be extracted.</param>
        /// <param name="extractInOriginalSize">Specifies size of blobs' image to extract.
        /// If set to <see langword="true"/> each blobs' image will have the same size as
        /// the specified image. If set to <see langword="false"/> each blobs' image will
        /// have the size of its blob.</param>
        ///
        /// <remarks><para>The method is used to extract image of partially initialized blob, which
        /// was provided by <see cref="GetObjectsInformation"/> method. Before calling the
        /// method, the <see cref="ProcessImage(Bitmap)"/>, <see cref="ProcessImage(BitmapData)"/>
        /// or <see cref="ProcessImage(WriteableBitmap)"/> method should be called, which will build
        /// objects map.</para>
        /// 
        /// <para>The method supports 24/32 bpp color and 8 bpp indexed grayscale images.</para>
        /// </remarks>
        /// 
        /// <exception cref="UnsupportedImageFormatException">Unsupported pixel format of the provided image.</exception>
        /// <exception cref="ApplicationException">No image was processed before, so blob
        /// can not be extracted.</exception>
        /// 
        public void ExtractBlobsImage(WriteableBitmap image, Blob blob, bool extractInOriginalSize)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");

            //if ((image.PixelFormat != PixelFormat.Format24bppRgb) &&
            //    (image.PixelFormat != PixelFormat.Format8bppIndexed) &&
            //    (image.PixelFormat != PixelFormat.Format32bppRgb) &&
            //    (image.PixelFormat != PixelFormat.Format32bppArgb) &&
            //    (image.PixelFormat != PixelFormat.Format32bppRgb) &&
            //    (image.PixelFormat != PixelFormat.Format32bppPArgb)
            //    )
            //    throw new UnsupportedImageFormatException("Unsupported pixel format of the provided image.");

            // image size
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            //int srcStride = image.Stride;
            int pixelSize = 4 /*Bitmap.GetPixelFormatSize(image.PixelFormat) / 8*/;

            int objectWidth = blob.Rectangle.Width;
            int objectHeight = blob.Rectangle.Height;

            int blobImageWidth = (extractInOriginalSize) ? width : objectWidth;
            int blobImageHeight = (extractInOriginalSize) ? height : objectHeight;

            int xmin = blob.Rectangle.Left;
            int xmax = xmin + objectWidth - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + objectHeight - 1;

            int label = blob.ID;

            // create new image
            blob.Image = new WriteableBitmap(blobImageWidth, blobImageHeight);
            blob.OriginalSize = extractInOriginalSize;

            // copy image
            //unsafe
            //{
            byte[] src = getPixelData(image);
            byte[] dst = getPixelData(image);
            int p = ymin * width + xmin;
            int index = p;

            //if (extractInOriginalSize)
            //{
            //    // allign destination pointer also
            //    dst += ymin + xmin * pixelSize;
            //}

            //int srcOffset = srcStride - objectWidth * pixelSize;
            //int dstOffset = blob.Image.Stride - objectWidth * pixelSize;
            int labelsOffset = width - objectWidth;

            // for each line
            for (int y = ymin; y <= ymax; y++)
            {
                // copy each pixel
                for (int x = xmin; x <= xmax; x++, p++, index+= pixelSize /*dst += pixelSize, src += pixelSize*/)
                {
                    if (objectLabels[p] == label)
                    {
                        // copy pixel
                        dst[index] = src[index];
                        dst[index + 1] = src[index + 1];
                        dst[index + 2] = src[index + 2];
                        dst[index + 3] = src[index + 3];

                        //if (pixelSize > 1)
                        //{
                        //    dst[1] = src[1];
                        //    dst[2] = src[2];

                        //    if (pixelSize > 3)
                        //    {
                        //        dst[3] = src[3];
                        //    }
                        //}
                    }
                }
                //src += srcOffset;
                //dst += dstOffset;
                p += labelsOffset;
            }
            //}
        }

        /// <summary>
        /// Get list of points on the left and right edges of the blob.
        /// </summary>
        /// 
        /// <param name="blob">Blob to collect edge points for.</param>
        /// <param name="leftEdge">List of points on the left edge of the blob.</param>
        /// <param name="rightEdge">List of points on the right edge of the blob.</param>
        /// 
        /// <remarks><para>The method scans each line of the blob and finds the most left and the
        /// most right points for it adding them to appropriate lists. The method may be very
        /// useful in conjunction with different routines from <see cref="Accord.Math.Geometry"/>,
        /// which allow finding convex hull or quadrilateral's corners.</para>
        /// 
        /// <para><note>Both lists of points are sorted by Y coordinate - points with smaller Y
        /// value go first.</note></para>
        /// </remarks>
        /// 
        /// <exception cref="ApplicationException">No image was processed before, so blob
        /// can not be extracted.</exception>
        /// 
        public void GetBlobsLeftAndRightEdges(Blob blob, out List<Windows.Foundation.Point> leftEdge, out List<Windows.Foundation.Point> rightEdge)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");

            leftEdge = new List<Windows.Foundation.Point>();
            rightEdge = new List<Windows.Foundation.Point>();

            int xmin = blob.Rectangle.Left;
            int xmax = xmin + blob.Rectangle.Width - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + blob.Rectangle.Height - 1;

            int label = blob.ID;

            // for each line
            for (int y = ymin; y <= ymax; y++)
            {
                // scan from left to right
                int p = y * imageWidth + xmin;
                for (int x = xmin; x <= xmax; x++, p++)
                {
                    if (objectLabels[p] == label)
                    {
                        leftEdge.Add(new Windows.Foundation.Point(x, y));
                        break;
                    }
                }

                // scan from right to left
                p = y * imageWidth + xmax;
                for (int x = xmax; x >= xmin; x--, p--)
                {
                    if (objectLabels[p] == label)
                    {
                        rightEdge.Add(new Windows.Foundation.Point(x, y));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get list of points on the top and bottom edges of the blob.
        /// </summary>
        /// 
        /// <param name="blob">Blob to collect edge points for.</param>
        /// <param name="topEdge">List of points on the top edge of the blob.</param>
        /// <param name="bottomEdge">List of points on the bottom edge of the blob.</param>
        /// 
        /// <remarks><para>The method scans each column of the blob and finds the most top and the
        /// most bottom points for it adding them to appropriate lists. The method may be very
        /// useful in conjunction with different routines from <see cref="Accord.Math.Geometry"/>,
        /// which allow finding convex hull or quadrilateral's corners.</para>
        /// 
        /// <para><note>Both lists of points are sorted by X coordinate - points with smaller X
        /// value go first.</note></para>
        /// </remarks>
        /// 
        /// <exception cref="ApplicationException">No image was processed before, so blob
        /// can not be extracted.</exception>
        /// 
        public void GetBlobsTopAndBottomEdges(Blob blob, out List<Windows.Foundation.Point> topEdge, out List<Windows.Foundation.Point> bottomEdge)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");

            topEdge = new List<Windows.Foundation.Point>();
            bottomEdge = new List<Windows.Foundation.Point>();

            int xmin = blob.Rectangle.Left;
            int xmax = xmin + blob.Rectangle.Width - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + blob.Rectangle.Height - 1;

            int label = blob.ID;

            // for each column
            for (int x = xmin; x <= xmax; x++)
            {
                // scan from top to bottom
                int p = ymin * imageWidth + x;
                for (int y = ymin; y <= ymax; y++, p += imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        topEdge.Add(new Windows.Foundation.Point(x, y));
                        break;
                    }
                }

                // scan from bottom to top
                p = ymax * imageWidth + x;
                for (int y = ymax; y >= ymin; y--, p -= imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        bottomEdge.Add(new Windows.Foundation.Point(x, y));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Get list of object's edge points.
        /// </summary>
        /// 
        /// <param name="blob">Blob to collect edge points for.</param>
        /// 
        /// <returns>Returns unsorted list of blob's edge points.</returns>
        /// 
        /// <remarks><para>The method scans each row and column of the blob and finds the
        /// most top/bottom/left/right points. The method returns similar result as if results of
        /// both <see cref="GetBlobsLeftAndRightEdges"/> and <see cref="GetBlobsTopAndBottomEdges"/>
        /// methods were combined, but each edge point occurs only once in the list.</para>
        /// 
        /// <para><note>Edge points in the returned list are not ordered. This makes the list unusable
        /// for visualization with methods, which draw polygon or poly-line. But the returned list
        /// can be used with such algorithms, like convex hull search, shape analyzer, etc.</note></para>
        /// </remarks>
        /// 
        /// <exception cref="ApplicationException">No image was processed before, so blob
        /// can not be extracted.</exception>
        /// 
        public List<Windows.Foundation.Point> GetBlobsEdgePoints(Blob blob)
        {
            // check if objects map was collected
            if (objectLabels == null)
                throw new InvalidOperationException("Image should be processed before to collect objects map.");

            List<Windows.Foundation.Point> edgePoints = new List<Windows.Foundation.Point>();

            int xmin = blob.Rectangle.Left;
            int xmax = xmin + blob.Rectangle.Width - 1;
            int ymin = blob.Rectangle.Top;
            int ymax = ymin + blob.Rectangle.Height - 1;

            int label = blob.ID;

            // array of already processed points on left/right edges
            // (index in these arrays represent Y coordinate, but value - X coordinate)
            int[] leftProcessedPoints = new int[blob.Rectangle.Height];
            int[] rightProcessedPoints = new int[blob.Rectangle.Height];

            // for each line
            for (int y = ymin; y <= ymax; y++)
            {
                // scan from left to right
                int p = y * imageWidth + xmin;
                for (int x = xmin; x <= xmax; x++, p++)
                {
                    if (objectLabels[p] == label)
                    {
                        edgePoints.Add(new Windows.Foundation.Point(x, y));
                        leftProcessedPoints[y - ymin] = x;
                        break;
                    }
                }

                // scan from right to left
                p = y * imageWidth + xmax;
                for (int x = xmax; x >= xmin; x--, p--)
                {
                    if (objectLabels[p] == label)
                    {
                        // avoid adding the point we already have
                        if (leftProcessedPoints[y - ymin] != x)
                        {
                            edgePoints.Add(new Windows.Foundation.Point(x, y));
                        }
                        rightProcessedPoints[y - ymin] = x;
                        break;
                    }
                }
            }

            // for each column
            for (int x = xmin; x <= xmax; x++)
            {
                // scan from top to bottom
                int p = ymin * imageWidth + x;
                for (int y = ymin, y0 = 0; y <= ymax; y++, y0++, p += imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        // avoid adding the point we already have
                        if ((leftProcessedPoints[y0] != x) &&
                             (rightProcessedPoints[y0] != x))
                        {
                            edgePoints.Add(new Windows.Foundation.Point(x, y));
                        }
                        break;
                    }
                }

                // scan from bottom to top
                p = ymax * imageWidth + x;
                for (int y = ymax, y0 = ymax - ymin; y >= ymin; y--, y0--, p -= imageWidth)
                {
                    if (objectLabels[p] == label)
                    {
                        // avoid adding the point we already have
                        if ((leftProcessedPoints[y0] != x) &&
                             (rightProcessedPoints[y0] != x))
                        {
                            edgePoints.Add(new Windows.Foundation.Point(x, y));
                        }
                        break;
                    }
                }
            }

            return edgePoints;
        }

        /// <summary>
        /// Actual objects map building.
        /// </summary>
        /// 
        /// <param name="image">Unmanaged image to process.</param>
        /// 
        /// <remarks><note>By the time this method is called bitmap's pixel format is not
        /// yet checked, so this should be done by the class inheriting from the base class.
        /// <see cref="imageWidth"/> and <see cref="imageHeight"/> members are initialized
        /// before the method is called, so these members may be used safely.</note></remarks>
        /// 
        protected abstract void BuildObjectsMap(WriteableBitmap image);


        #region Private Methods - Collecting objects' rectangles

        // Collect objects' rectangles
        private void CollectObjectsInfo(byte[] image)
        {
            int i = 0, label;

            // create object coordinates arrays
            int[] x1 = new int[objectsCount + 1];
            int[] y1 = new int[objectsCount + 1];
            int[] x2 = new int[objectsCount + 1];
            int[] y2 = new int[objectsCount + 1];

            int[] area = new int[objectsCount + 1];
            long[] xc = new long[objectsCount + 1];
            long[] yc = new long[objectsCount + 1];

            long[] meanR = new long[objectsCount + 1];
            long[] meanG = new long[objectsCount + 1];
            long[] meanB = new long[objectsCount + 1];

            long[] stdDevR = new long[objectsCount + 1];
            long[] stdDevG = new long[objectsCount + 1];
            long[] stdDevB = new long[objectsCount + 1];

            for (int j = 1; j <= objectsCount; j++)
            {
                x1[j] = imageWidth;
                y1[j] = imageHeight;
            }

            int offset = 4; /*image.Stride - imageWidth;*/
            byte g; // pixel's grey value

            // walk through labels array
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++, i++)
                {
                    offset = (y * imageWidth + x) * 4;
                    // get current label
                    label = objectLabels[i];

                    // skip unlabeled pixels
                    if (label == 0)
                        continue;

                    // check and update all coordinates

                    if (x < x1[label])
                    {
                        x1[label] = x;
                    }
                    if (x > x2[label])
                    {
                        x2[label] = x;
                    }
                    if (y < y1[label])
                    {
                        y1[label] = y;
                    }
                    if (y > y2[label])
                    {
                        y2[label] = y;
                    }

                    area[label]++;
                    xc[label] += x;
                    yc[label] += y;

                    g = (byte)((image[offset] + image[offset + 1] + image[offset + 2]) / 3);
                    meanG[label] += g;
                    stdDevG[label] += g * g;
                }
            }

            for (int j = 1; j <= objectsCount; j++)
            {
                meanR[j] = meanB[j] = meanG[j];
                stdDevR[j] = stdDevB[j] = stdDevG[j];
            }


            // create blobs
            blobs.Clear();

            for (int j = 1; j <= objectsCount; j++)
            {
                int blobArea = area[j];

                Blob blob = new Blob(j, new Rectangle(x1[j], y1[j], x2[j] - x1[j] + 1, y2[j] - y1[j] + 1));
                blob.Area = blobArea;
                blob.Fullness = (double)blobArea / ((x2[j] - x1[j] + 1) * (y2[j] - y1[j] + 1));
                blob.CenterOfGravity = new Windows.Foundation.Point((float)xc[j] / blobArea, (float)yc[j] / blobArea);
                blob.ColorMean = Windows.UI.Color.FromArgb(255, (byte)(meanR[j] / blobArea), (byte)(meanG[j] / blobArea), (byte)(meanB[j] / blobArea));
                blob.ColorStdDev = Windows.UI.Color.FromArgb(
                    255,
                    (byte)(Math.Sqrt(stdDevR[j] / blobArea - blob.ColorMean.R * blob.ColorMean.R)),
                    (byte)(Math.Sqrt(stdDevG[j] / blobArea - blob.ColorMean.G * blob.ColorMean.G)),
                    (byte)(Math.Sqrt(stdDevB[j] / blobArea - blob.ColorMean.B * blob.ColorMean.B)));

                blobs.Add(blob);
            }
        }

        // Rectangles' and blobs' sorter
        private class BlobsSorter : System.Collections.Generic.IComparer<Blob>
        {
            private ObjectsOrder order;

            public BlobsSorter(ObjectsOrder order)
            {
                this.order = order;
            }

            public int Compare(Blob a, Blob b)
            {
                Rectangle aRect = a.Rectangle;
                Rectangle bRect = b.Rectangle;

                switch (order)
                {
                    case ObjectsOrder.Size: // sort by size

                        // the order is changed to descending
                        return (bRect.Width * bRect.Height - aRect.Width * aRect.Height);

                    case ObjectsOrder.Area: // sort by area
                        return b.Area - a.Area;

                    case ObjectsOrder.YX:   // YX order

                        return ((aRect.Y * 100000 + aRect.X) - (bRect.Y * 100000 + bRect.X));

                    case ObjectsOrder.XY:   // XY order

                        return ((aRect.X * 100000 + aRect.Y) - (bRect.X * 100000 + bRect.Y));
                }
                return 0;
            }
        }

        #endregion
    }
}
