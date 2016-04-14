using AForge.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

namespace MySkin
{
    //class Blob
    //{

    //    double area;
    //    Point UpperLeft,BottomRight;

    //    public Blob(Point UpperLeft, Point BottomRight)
    //    {
    //        this.UpperLeft = UpperLeft;
    //        this.BottomRight = BottomRight;
    //        calculateArea();
    //    }

    //    private double distance(Point p1, Point p2)
    //    {
    //        return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
    //    }

    //    private void calculateArea()
    //    {
    //        Point UpperRight, BottomLeft;
    //        UpperRight = new Point(BottomRight.X, UpperLeft.Y);
    //        BottomLeft = new Point(UpperLeft.X, BottomRight.Y);
    //        area = distance(UpperLeft, UpperRight) * distance(BottomRight, UpperRight);
    //    }


    //}
    public class Blob
    {
        // blob's image
        private UnmanagedImage image;
        // blob's image size - as original image or not
        private bool originalSize = false;

        // blob's rectangle in the original image
        private Rectangle rect;
        // blob's ID in the original image
        private int id;
        // area of the blob
        private int area;
        // center of gravity
        private Windows.Foundation.Point cog;
        // fullness of the blob ( area / ( width * height ) )
        private double fullness;
        // mean color of the blob
        private Windows.UI.Color colorMean = Colors.Black;
        // color's standard deviation of the blob
        private Windows.UI.Color colorStdDev = Colors.Black;

        /// <summary>
        /// Blob's image.
        /// </summary>
        ///
        /// <remarks><para>The property keeps blob's image. In the case if it equals to <b>null</b>,
        /// the image may be extracted using <see cref="BlobCounterBase.ExtractBlobsImage( Bitmap, Blob, bool )"/>
        /// or <see cref="BlobCounterBase.ExtractBlobsImage( UnmanagedImage, Blob, bool )"/> method.</para></remarks>
        ///
        public UnmanagedImage Image
        {
            get { return image; }
            internal set { image = value; }
        }

        /// <summary>
        /// Blob's image size.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies size of the <see cref="Image">blob's image</see>.
        /// If the property is set to <see langword="true"/>, the blob's image size equals to the
        /// size of original image. If the property is set to <see langword="false"/>, the blob's
        /// image size equals to size of actual blob.</para></remarks>
        /// 
        public bool OriginalSize
        {
            get { return originalSize; }
            internal set { originalSize = value; }
        }

        /// <summary>
        /// Blob's rectangle in the original image.
        /// </summary>
        /// 
        /// <remarks><para>The property specifies position of the blob in the original image
        /// and its size.</para></remarks>
        /// 
        public Rectangle Rectangle
        {
            get { return rect; }
        }

        /// <summary>
        /// Blob's ID in the original image.
        /// </summary>
        public int ID
        {
            get { return id; }
            internal set { id = value; }
        }

        /// <summary>
        /// Blob's area.
        /// </summary>
        /// 
        /// <remarks><para>The property equals to blob's area measured in number of pixels
        /// contained by the blob.</para></remarks>
        /// 
        public int Area
        {
            get { return area; }
            internal set { area = value; }
        }

        /// <summary>
        /// Blob's fullness, [0, 1].
        /// </summary>
        /// 
        /// <remarks><para>The property equals to blob's fullness, which is calculated
        /// as <b>Area / ( Width * Height )</b>. If it equals to <b>1</b>, then
        /// it means that entire blob's rectangle is filled by blob's pixel (no
        /// blank areas), which is true only for rectangles. If it equals to <b>0.5</b>,
        /// for example, then it means that only half of the bounding rectangle is filled
        /// by blob's pixels.</para></remarks>
        /// 
        public double Fullness
        {
            get { return fullness; }
            internal set { fullness = value; }
        }

        /// <summary>
        /// Blob's center of gravity point.
        /// </summary>
        /// 
        /// <remarks><para>The property keeps center of gravity point, which is calculated as
        /// mean value of X and Y coordinates of blob's points.</para></remarks>
        /// 
        public Windows.Foundation.Point CenterOfGravity
        {
            get { return cog; }
            internal set { cog = value; }
        }

        /// <summary>
        /// Blob's mean color.
        /// </summary>
        /// 
        /// <remarks><para>The property keeps mean color of pixels comprising the blob.</para></remarks>
        /// 
        public Windows.UI.Color ColorMean
        {
            get { return colorMean; }
            internal set { colorMean = value; }
        }

        /// <summary>
        /// Blob color's standard deviation.
        /// </summary>
        /// 
        /// <remarks><para>The property keeps standard deviation of pixels' colors comprising the blob.</para></remarks>
        /// 
        public Windows.UI.Color ColorStdDev
        {
            get { return colorStdDev; }
            internal set { colorStdDev = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        /// 
        /// <param name="id">Blob's ID in the original image.</param>
        /// <param name="rect">Blob's rectangle in the original image.</param>
        /// 
        /// <remarks><para>This constructor leaves <see cref="Image"/> property not initialized. The blob's
        /// image may be extracted later using <see cref="BlobCounterBase.ExtractBlobsImage( Bitmap, Blob, bool )"/>
        /// or <see cref="BlobCounterBase.ExtractBlobsImage( UnmanagedImage, Blob, bool )"/> method.</para></remarks>
        /// 
        public Blob(int id, Rectangle rect)
        {
            this.id = id;
            this.rect = rect;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Blob"/> class.
        /// </summary>
        /// 
        /// <param name="source">Source blob to copy.</param>
        /// 
        /// <remarks><para>This copy constructor leaves <see cref="Image"/> property not initialized. The blob's
        /// image may be extracted later using <see cref="BlobCounterBase.ExtractBlobsImage( Bitmap, Blob, bool )"/>
        /// or <see cref="BlobCounterBase.ExtractBlobsImage( UnmanagedImage, Blob, bool )"/> method.</para></remarks>
        /// 
        public Blob(Blob source)
        {
            // copy everything except image
            id = source.id;
            rect = source.rect;
            cog = source.cog;
            area = source.area;
            fullness = source.fullness;
            colorMean = source.colorMean;
            colorStdDev = source.colorStdDev;
        }
    }
}
