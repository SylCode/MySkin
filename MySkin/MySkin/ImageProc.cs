using System;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using System.Collections.Generic;

namespace MySkin
{
    class ImageProc
    {
        int width, height;
        private int calcOffset;

        public ImageProc(int w, int h)
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

        public byte[] Binary(byte[] pixelData, int threshold)
        {
            byte[] binary = new byte[pixelData.Length];
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

        public List<Blob> analyzeBlobs(byte[] gray,byte[] input, Point seed, int maxWidth, int maxHeight)
        {
            List<Blob> blobs = new List<Blob>();
            List <Point> uL, bR;
            uL = new List<Point>();
            bR = new List<Point>();
            int xInit = (int)seed.X - maxWidth / 2;
            int yInit = (int)seed.Y - maxHeight / 2;
            List<Point> contour = new List<Point>();
            int minW = 1000, maxW = -1, minH = 1000, maxH = -1, imageOffset = 0;
            int beg = xInit * yInit * 4;
            int end = (beg + maxWidth * maxHeight) * 4;
            bool firstH=false, firstW=false;

            

            for (int y = yInit; y < yInit + maxHeight; y++)
            {
                for (int x = xInit; x < xInit + maxWidth; x++)
                {
                    imageOffset = (y * width + x) * 4;
                    if (input[imageOffset] <50)
                    {
                        gray[imageOffset] = 0;
                        gray[imageOffset + 1] = 0;
                        gray[imageOffset + 2] = 255;
                        gray[imageOffset + 3] = 0;
                    }
                    else if (input[imageOffset] < 170)
                    {
                        gray[imageOffset] = 255;
                        gray[imageOffset + 1] = 0;
                        gray[imageOffset + 2] = 0;
                        gray[imageOffset + 3] = 0;
                    }
                    else if (input[imageOffset] < 235)
                    {
                        gray[imageOffset] = 0;
                        gray[imageOffset + 1] = 255;
                        gray[imageOffset + 2] = 0;
                        gray[imageOffset + 3] = 0;
                    }
                    //if (input[imageOffset] <= 235)
                    //{
                    //    int secondaryOffset = imageOffset;
                    //    //while
                    //}


                }
            }

            

            //int minWidth, minHeight, index;
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        if (input[i * width + j] == 255)
            //        {
            //            minWidth = j - (maxWidth / 2) >= 0 ? j - (maxWidth / 2) : 0;
            //            minHeight = i - (maxHeight / 2) >= 0 ? j - (maxHeight / 2) : 0;
            //            uL.Add(new Point(maxWidth, maxHeight));
            //            bR.Add(new Point(minWidth, minHeight));
            //            index = uL.Count - 1;
            //            for (int h=minHeight; h<minHeight+maxHeight; h++)
            //            {
            //                for (int w = minWidth;w<minWidth+maxWidth; w++ )
            //                {
            //                    if (input[h * (minHeight + maxHeight) + w] == 255)
            //                    {
            //                        if (uL[index].X > w)
            //                            uL[index] = new Point(w, uL[index].Y);
            //                        if (uL[index].Y > h)
            //                            uL[index] = new Point(uL[index].X, h);
            //                        if (bR[index].X < w)
            //                            bR[index] = new Point(w, bR[index].Y);
            //                        if (bR[index].Y < h)
            //                            bR[index] = new Point(bR[index].X, h);
            //                    }
            //                }
            //            }
            //        } 
            //    }
            //}

            for (int i=0; i<uL.Count; i++)
            {
                //blobs.Add(new Blob(new Point(uL[i].X, uL[i].Y), new Point(bR[i].X, bR[i].Y)));
            }
           

            return blobs;
        }
        


    }
}
