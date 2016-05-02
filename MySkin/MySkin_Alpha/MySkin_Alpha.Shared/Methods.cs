using System;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using System.Collections.Generic;
using System.Drawing;
using Accord;
using Accord.Imaging;

namespace MySkin_Alpha
{
    class Methods
    {
        int width, height;
        private int calcOffset;
        public double colorVariation = 0;
        public double blackness = 0;
        public double blueness = 0;
        public double redness = 0;
        public bool blueParts = false;

        public Methods(int w, int h)
        {
            width = w;
            height = h;
        }



        public byte[] Binary(byte[] pixelData, int low, int threshold)
        {
            byte[] binary = new byte[pixelData.Length];
            float avg = 0;
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                avg += pixelData[i] * 0.11f;
                avg += pixelData[i + 1] * 0.59f;
                avg += pixelData[i + 2] * 0.3f;

                if (avg < threshold && avg>low)
                {
                    binary[i] = 255;
                    binary[i + 1] = 255;
                    binary[i + 2] = 255;
                    binary[i + 3] = 255;
                }
                else
                {
                    binary[i] = 0;
                    binary[i + 1] = 0;
                    binary[i + 2] = 0;
                    binary[i + 3] = 0;
                }
                avg = 0;
            }
            return binary;
        }

        public byte[] Binary(byte[] pixelData, int threshold, bool invert)
        {
            byte[] binary = new byte[pixelData.Length];
            if (invert)
            {
                for (int i = 0; i < pixelData.Length; i += 4)
                {

                    if (pixelData[i] < threshold)
                    {
                        binary[i] = 0;
                        binary[i + 1] = 0;
                        binary[i + 2] = 0;
                        binary[i + 3] = 0;
                    }
                    else
                    {
                        binary[i] = 255;
                        binary[i + 1] = 255;
                        binary[i + 2] = 255;
                        binary[i + 3] = 255;
                    }
                }
            }
            else
            {
                for (int i = 0; i < pixelData.Length; i += 4)
                {

                    if (pixelData[i] < threshold)
                    {
                        binary[i] = 255;
                        binary[i + 1] = 255;
                        binary[i + 2] = 255;
                        binary[i + 3] = 255;
                    }
                    else
                    {
                        binary[i] = 0;
                        binary[i + 1] = 0;
                        binary[i + 2] = 0;
                        binary[i + 3] = 0;
                    }
                }
            }
            return binary;
        }

        public byte[] Grayscale(byte[] pixelData)
        {
            byte[] grayscale = new byte[pixelData.Length];
            float avg = 0;
            for (int i = 0; i < pixelData.Length; i += 4)
            {
                avg += pixelData[i] * 0.11f;
                avg += pixelData[i + 1] * 0.59f;
                avg += pixelData[i + 2] * 0.3f;

                grayscale[i] = Convert.ToByte(avg);
                grayscale[i + 1] = grayscale[i];
                grayscale[i + 2] = grayscale[i];
                grayscale[i + 3] = (byte)255;

                avg = 0;
            }
            return grayscale;
        }

        public int[] getHistogram(byte[] input)
        {
            int[] hist = new int[256];

            for (int i = 0; i < 256; i++)
                hist[i] = 0;
            for (int i = 0; i < input.Length; i += 4)
                hist[input[i]]++;
            return hist;
        }

        private WriteableBitmap plotHistogram(int[] hist)
        {
            WriteableBitmap histogram = new WriteableBitmap(280, 156);
            float qw = 280 / 256;
            int max = 0;
            for (int i = 0; i < 256; i++)
                if (hist[i] > max)
                    max = hist[i];
            float qh = 156 / max;

            for (int i=0; i<256; i++)
            {
                histogram.DrawLine((int)(i * qw), (int)(hist[i] * qh), (int)((i + 1) * qw), (int)(hist[i + 1] * qh),Colors.Black);
            }
            return histogram;
        }

        public byte getOtsuThreshold(byte[] input)
        {
            byte t = 0;
            float p1, p2, p12;
            int[] hist = getHistogram(input);
            float[] vet = new float[256];
            for (int i=0; i<256; i++)
            {
                p1 = Px(0, i, hist);
                p2 = Px(i + 1, 255, hist);
                p12 = p1 * p2;
                p12 = p12 == 0 ? 1 : p12;
                float diff = (Mx(0, i, hist) * p2) - (Mx(i + 1, 255, hist) * p1);
                vet[i] = diff * diff / p12;
            }
            return (byte)findMax(vet, vet.Length);
        }

        private float Px(int init, int end, int[] hist)
        {
            int sum = 0;
            for (int i = init; i <= end; i++)
                sum += hist[i];

            return (float)sum;
        }

        // function is used to compute the mean values in the equation (mu)
        private float Mx(int init, int end, int[] hist)
        {
            int sum = 0;
            for (int i = init; i <= end; i++)
                sum += i * hist[i];

            return (float)sum;
        }

        // finds the maximum element in a vector
        private int findMax(float[] vec, int n)
        {
            float maxVec = 0;
            int idx = 0;
            int i;

            for (i = 1; i < n - 1; i++)
            {
                if (vec[i] > maxVec)
                {
                    maxVec = vec[i];
                    idx = i;
                }
            }
            return idx;
        }

        public byte[] invert(byte[] input)
        {
            byte[] inv = new byte[input.Length];
            for (int i=0; i<input.Length;i+=4)
            {
                inv[i] = (byte)(input[i] % 255);
                inv[i+1] = (byte)(input[i] % 255);
                inv[i+2] = (byte)(input[i] % 255);
            }
            return inv;
        }

        public byte[] addContrast(byte[] input, float value)
        {
            byte[] output = new byte[input.Length];
            int avR, avG, avB;
            avR=avG=avB = 0;
            for (int i = 0; i < input.Length; i += 4)
            {
                avR += input[i];
                avG += input[i+1];
                avB += input[i+2];
            }

            avR /= input.Length / 4;
            avG /= input.Length / 4;
            avB /= input.Length / 4;

            byte newR, newG, newB;
            int R,G,B;
            for (int i = 0; i < input.Length; i += 4)
            {
                R = Convert.ToInt32(value * (input[i] - avR) + avR);
                G = Convert.ToInt32(value * (input[i+1] - avG) + avG);
                B = Convert.ToInt32(value * (input[i+2] - avB) + avB);

                R = norm(R);
                G = norm(G);
                B = norm(B);

                newR = Convert.ToByte(R);
                newG = Convert.ToByte(G);
                newB = Convert.ToByte(B);

                output[i] = newR;
                output[i + 1] = newG;
                output[i + 2] = newB;
                output[i + 3] = (byte)255;
            }
            return output;
        }

        private int norm (int input)
        {
            if (input > 255)
                return 255;
            else if (input < 0)
                return 0;
            else return input;
        }

        public byte[] Edge(byte[] input, double[,] horizontalMask, double[,] verticalMask)
        {
            //threshold = (byte)((int)getOtsuThreshold(sobel)- (int)45);
            //sobel = Binary(sobel,35, threshold);

            int filterOffset = 1;
            int imageOffset = 0;
            double blueX, greenX, redX;
            double blueY, greenY, redY;
            double blueT, greenT, redT;
            byte[] rez = new byte[input.Length];

            for (int imageY = filterOffset; imageY < height - filterOffset; imageY++)
            {
                for (int imageX = filterOffset; imageX < width - filterOffset; imageX++)
                {
                    blueX = greenX = redX = 0.0;
                    blueY = greenY = redY = 0.0;
                    blueT = greenT = redT = 0.0;
                    imageOffset = (imageY * width + imageX) * 4;
                    for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                        {
                            calcOffset = imageOffset + (((filterY * width) + filterX) * 4);

                            blueX += input[calcOffset] * horizontalMask[filterX + filterOffset, filterY + filterOffset];
                            greenX += input[calcOffset + 1] * horizontalMask[filterX + filterOffset, filterY + filterOffset];
                            redX += input[calcOffset + 2] * horizontalMask[filterX + filterOffset, filterY + filterOffset];

                            blueY += input[calcOffset] * verticalMask[filterX + filterOffset, filterY + filterOffset];
                            greenY += input[calcOffset + 1] * verticalMask[filterX + filterOffset, filterY + filterOffset];
                            redY += input[calcOffset + 2] * verticalMask[filterX + filterOffset, filterY + filterOffset];
                        }
                    }
                    blueT = Math.Sqrt((blueX * blueX) + (blueY * blueY));
                    greenT = Math.Sqrt((greenX * greenX) + (greenY * greenY));
                    redT = Math.Sqrt((redX * redX) + (redY * redY));

                    if (blueT > 255)
                    { blueT = 255; }
                    else if (blueT < 0)
                    { blueT = 0; }

                    if (greenT > 255)
                    { greenT = 255; }
                    else if (greenT < 0)
                    { greenT = 0; }

                    if (redT > 255)
                    { redT = 255; }
                    else if (redT < 0)
                    { redT = 0; }

                    rez[imageOffset] = Convert.ToByte(redT);
                    rez[imageOffset + 1] = Convert.ToByte(greenT);
                    rez[imageOffset + 2] = Convert.ToByte(blueT);
                    rez[imageOffset + 3] = Convert.ToByte(255);

                }
            }

            return rez;
        }

        public byte[] Edge(byte[] input, double[,] mask, double factor, int bias)
        {
            //threshold = (byte)((int)getOtsuThreshold(sobel)- (int)45);
            //sobel = Binary(sobel,35, threshold);

            int filterOffset = 1;
            int imageOffset = 0;
            double blueX, greenX, redX;
            double blueY, greenY, redY;
            double blueT, greenT, redT;
            byte[] rez = new byte[input.Length];

            int filterWidth = mask.GetLength(1);
            int filterHeight = mask.GetLength(0);

            filterOffset = (filterWidth - 1) / 2;

            for (int imageY = filterOffset; imageY < height - filterOffset; imageY++)
            {
                for (int imageX = filterOffset; imageX < width - filterOffset; imageX++)
                {
                    blueX = greenX = redX = 0.0;
                    blueY = greenY = redY = 0.0;
                    blueT = greenT = redT = 0.0;
                    imageOffset = (imageY * width + imageX) * 4;
                    for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                    {
                        for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                        {
                            calcOffset = imageOffset + (((filterY * width) + filterX) * 4);

                            blueX += input[calcOffset] * mask[filterX + filterOffset, filterY + filterOffset];
                            greenX += input[calcOffset + 1] * mask[filterX + filterOffset, filterY + filterOffset];
                            redX += input[calcOffset + 2] * mask[filterX + filterOffset, filterY + filterOffset];
                            
                        }
                    }

                    blueT = factor * blueX + bias;
                    greenT = factor * greenX + bias;
                    redT = factor * redX + bias;

                    if (blueT > 255)
                    { blueT = 255; }
                    else if (blueT < 0)
                    { blueT = 0; }

                    if (greenT > 255)
                    { greenT = 255; }
                    else if (greenT < 0)
                    { greenT = 0; }

                    if (redT > 255)
                    { redT = 255; }
                    else if (redT < 0)
                    { redT = 0; }

                    rez[imageOffset] = Convert.ToByte(redT);
                    rez[imageOffset + 1] = Convert.ToByte(greenT);
                    rez[imageOffset + 2] = Convert.ToByte(blueT);
                    rez[imageOffset + 3] = Convert.ToByte(255);

                }
            }

            return rez;
        }

        private byte[] adaptiveThreshold(byte[] input)
        {
            byte[] output = new byte[width * height * 4];
            int[] integral = new int[width * height];
            int ct = 0;
            int sum = 0;
            int S = width / 64;
            float T = 0.15f;
            int index = 0;
            int x1, y1, x2, y2;

            for (int i = 0; i < input.Length; i += 4)
            {
                sum = 0;
                sum += input[i];
                if (i == 0)
                {
                    integral[ct] = sum;
                }
                else
                {
                    integral[ct] = integral[ct - 1] + sum;
                }
                ct++;
            }
            ct = 0;
            sum = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    try
                    {
                        index = j * width + i;
                        // set the SxS region
                        x1 = i - S; x2 = i + S;
                        y1 = j - S; y2 = j + S;

                        // check the border
                        if (x1 < 0) x1 = 0;
                        if (x2 >= width) x2 = width - 1;
                        if (y1 < 0) y1 = 0;
                        if (y2 >= height) y2 = height - 1;

                        ct = (x2 - x1) * (y2 - y1);

                        sum = integral[y2 * width + x2] -
                      integral[y1 * width + x2] -
                      integral[y2 * width + x1] +
                      integral[y1 * width + x1];

                        if ((long)(input[index] * ct) <= (long)(sum * (1.0 - T)))
                        {
                            output[index * 4] = 0;
                            output[index * 4 + 1] = 0;
                            output[index * 4 + 2] = 0;
                            output[index * 4 + 3] = 255;
                        }
                        else
                        {
                            output[index * 4] = 255;
                            output[index * 4 + 1] = 255;
                            output[index * 4 + 2] = 255;
                            output[index * 4 + 3] = 255;
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }

            return output;
        }

        public byte[] analyzeBlob(byte[] gray,byte[] input, Rectangle boundingRectangle, List<IntPoint> pointsL, List<IntPoint> pointsR, List<IntPoint> pointsU, List<IntPoint> pointsD/*, out Nevus nevus*/, out double assymmetryRate)
        {
            int imageOffset = 0;
            int d = 0;
            byte[] fir = new byte[gray.Length];
            byte[] sec = new byte[gray.Length];
            byte[] final = new byte[gray.Length];
            byte[] init = new byte[gray.Length];
            double[] dims = new double[2];
            List<double> blobPointColorsR = new List<double>();
            List<double> blobPointColorsG = new List<double>();
            List<double> blobPointColorsB = new List<double>();

            dims[0] = pointsL[pointsL.Count / 2].DistanceTo(pointsR[pointsR.Count / 2]);
            dims[1] = pointsU[pointsU.Count / 2].DistanceTo(pointsD[pointsD.Count / 2]);
            if (dims[0] > dims[1])
            {
                assymmetryRate = dims[0] / dims[1];
            }
            else assymmetryRate = dims[1] / dims[0];

            for (int i=0; i<gray.Length; i++)
            {
                fir[i] = gray[i];
                sec[i] = gray[i];
                init[i] = gray[i];
                final[i] = gray[i];
            }

            for (int i = 0; i < pointsL.Count; i++)
            {
                d = pointsL[i].X;
                if (pointsL[i].Y == pointsR[i].Y)
                {
                    while (d <= pointsR[i].X)
                    {
                        imageOffset = (pointsL[i].Y * width) * 4 + d * 4;
                        if (input[imageOffset] < 50 && input[imageOffset] >= 0)
                        {
                            fir[imageOffset] = 0;
                            fir[imageOffset + 1] = 0;
                            fir[imageOffset + 2] = 255;
                            fir[imageOffset + 3] = 255;
                        }
                        else if (input[imageOffset] < 170 && input[imageOffset] >= 0)
                        {
                            fir[imageOffset] = 255;
                            fir[imageOffset + 1] = 0;
                            fir[imageOffset + 2] = 0;
                            fir[imageOffset + 3] = 255;
                        }
                        else if (input[imageOffset] < 235 && input[imageOffset] >= 0)
                        {
                            fir[imageOffset] = 0;
                            fir[imageOffset + 1] = 255;
                            fir[imageOffset + 2] = 0;
                            fir[imageOffset + 3] = 255;
                        }
                        d++;
                    }
                }

            }

            int ct = 0;
            for (int i = 0; i < pointsU.Count; i++)
            {
                d = pointsU[i].Y;
                if (pointsU[i].X == pointsD[i].X)
                {
                    while (d <= pointsD[i].Y)
                    {
                        imageOffset = (d * width) * 4 + pointsU[i].X * 4;
                        if (input[imageOffset] < 50 && input[imageOffset] >= 0)
                        {
                            sec[imageOffset] = 0;
                            sec[imageOffset + 1] = 0;
                            sec[imageOffset + 2] = 255;
                            sec[imageOffset + 3] = 255;
                        }
                        else if (input[imageOffset] < 170 && input[imageOffset] >= 0)
                        {
                            sec[imageOffset] = 255;
                            sec[imageOffset + 1] = 0;
                            sec[imageOffset + 2] = 0;
                            sec[imageOffset + 3] = 255;
                        }
                        else if (input[imageOffset] < 235 && input[imageOffset] >= 0)
                        {
                            sec[imageOffset] = 0;
                            sec[imageOffset + 1] = 255;
                            sec[imageOffset + 2] = 0;
                            sec[imageOffset + 3] = 255;
                        }
                        d++;
                    }
                }
            }

            for (int y = boundingRectangle.Y; y < boundingRectangle.Y + boundingRectangle.Height; y ++)
            {
                for (int x = boundingRectangle.X; x < boundingRectangle.X+ boundingRectangle.Width; x++)
                {
                    int i = (y * width + x) * 4;
                    if (sec[i] == fir[i] && sec[i + 1] == fir[i + 1] && sec[i + 2] == fir[i + 2] && sec[i + 3] == fir[i + 3])
                    {
                        final[i] = init[i];
                        final[i + 1] = init[i + 1];
                        final[i + 2] = init[i + 2];
                        final[i + 3] = init[i + 3];
                        if (gray[i] <= 230 && gray[i + 1] <= 230 && gray[i + 2] <= 250)
                        {
                            blobPointColorsB.Add(gray[i]);
                            blobPointColorsG.Add(gray[i + 1]);
                            blobPointColorsR.Add(gray[i + 2]);



                        }
                    }
                }
            }

            for (int i = 0; i < pointsL.Count; i++)
            {
                imageOffset = (pointsL[i].Y * width) * 4 + pointsL[i].X * 4;
                final[imageOffset] = 0;
                final[imageOffset + 1] = 255;
                final[imageOffset + 2] = 0;
                final[imageOffset + 3] = 0;
                imageOffset = (pointsR[i].Y * width) * 4 + pointsR[i].X * 4;
                final[imageOffset] = 0;
                final[imageOffset + 1] = 255;
                final[imageOffset + 2] = 0;
                final[imageOffset + 3] = 0;
            }

            for (int i = 0; i < pointsU.Count; i++)
            {
                imageOffset = (pointsU[i].Y * width) * 4 + pointsU[i].X * 4;
                final[imageOffset] = 0;
                final[imageOffset + 1] = 255;
                final[imageOffset + 2] = 0;
                final[imageOffset + 3] = 0;
                imageOffset = (pointsD[i].Y * width) * 4 + pointsD[i].X * 4;
                final[imageOffset] = 0;
                final[imageOffset + 1] = 255;
                final[imageOffset + 2] = 0;
                final[imageOffset + 3] = 0;
                d = pointsU[i].Y;

                if (pointsU[i].X == pointsD[i].X)
                {
                    while (d <= pointsD[i].Y)
                    {
                        imageOffset = (d * width) * 4 + pointsU[i].X * 4;

                        if (gray[imageOffset] <= 30 && gray[imageOffset + 1] <= 30 && gray[imageOffset + 2] <= 30)
                        {
                            blackness++;
                        }
                        if (Math.Abs(gray[imageOffset] - gray[imageOffset + 1]) <= 5 && Math.Abs(Math.Max(gray[imageOffset], gray[imageOffset + 1]) - gray[imageOffset + 2]) < 18)
                        {
                            blueness++;
                            final[imageOffset] = 220;
                            final[imageOffset + 1] = 163;
                            final[imageOffset + 2] = 73;
                            final[imageOffset + 3] = 255;
                        }
                        if (gray[imageOffset + 2] <= gray[imageOffset] || Math.Abs(gray[imageOffset] - gray[imageOffset + 2]) <= 10)
                        {
                            blueness++;
                            final[imageOffset] = 255;
                            final[imageOffset + 1] = 0;
                            final[imageOffset + 2] = 0;
                            final[imageOffset + 3] = 255;
                        }
                        if (gray[imageOffset] <= 40 && gray[imageOffset + 1] <= 40 && Math.Abs(gray[imageOffset] - gray[imageOffset + 1]) <= 10 && Math.Abs((Math.Max(gray[imageOffset], gray[imageOffset + 1]) + 1) / (gray[imageOffset + 2] + 1)) <= 0.3)
                        {
                            redness++;
                            final[imageOffset] = 0;
                            final[imageOffset + 1] = 0;
                            final[imageOffset + 2] = 255;
                            final[imageOffset + 3] = 255;
                        }
                        ct++;
                        d++;
                    }
                }
            }

            if (blackness != 0)
            {
                blackness /= (double)ct;
                blackness *= 100;
            }
            if (blueness != 0)
            {
                blueness /= (double)ct;
                blueness *= 100;
            }
            if (redness != 0)
            {
                redness /= (double)ct;
                redness *= 100;
            }
            colorVariation = (Accord.Statistics.Measures.Variance(blobPointColorsR.ToArray()) + Accord.Statistics.Measures.Variance(blobPointColorsG.ToArray()) + Accord.Statistics.Measures.Variance(blobPointColorsB.ToArray())) / 3.0;
            //Nevus nev = new Nevus()


            return final;
        }

        public int getMaxRectangle(Rectangle[] rects, int imageWidth, int imageHeight)
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
    }
}
