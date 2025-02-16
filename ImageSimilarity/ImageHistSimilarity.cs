using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Threading.Tasks;

using static ImageSimilarity.ImageOperations;

namespace ImageSimilarity
{
    public struct ChannelStats
    {
        public int[] Hist;
        public int Min;
        public int Max;
        public int Med;
        public double Mean;
        public double StdDev;
    }
    public struct ImageInfo
    {
        public string Path;
        public int Width;
        public int Height;
        public ChannelStats RedStats;
        public ChannelStats GreenStats;
        public ChannelStats BlueStats;
    }

    public struct MatchInfo
    {
        public string MatchedImgPath;
        public double MatchScore;
    }
    public class ImageHistSimilarity
    {
        /// <summary>
        /// Calculate the image stats (Max, Min, Med, Mean, StdDev & Histogram) of each color
        /// </summary>
        /// <param name="imgPath">Image path</param>
        /// <returns>Calculated stats of the given image</returns>
        public static ImageInfo CalculateImageStats(string imgPath)
        {
            RGBPixel[,] imageMatrix = OpenImage(imgPath);
            int height = imageMatrix.GetLength(0);
            int width = imageMatrix.GetLength(1);
            double redSum = 0.0, greenSum = 0.0, blueSum = 0.0;
            int redMax = -1, greenMax = -1, blueMax = -1;
            int redMin = 256, greenMin = 256, blueMin = 256;
            double totalPixels = height * width;
            double invTotalPixels = 1.0 / totalPixels;

            ChannelStats redStats = new ChannelStats { Hist = new int[256] };
            ChannelStats greenStats = new ChannelStats { Hist = new int[256] };
            ChannelStats blueStats = new ChannelStats { Hist = new int[256] };

            for (int i = 0; i < height; ++i)
            {
                for (int j = 0; j < width; ++j)
                {
                    int r = imageMatrix[i, j].red;
                    int g = imageMatrix[i, j].green;
                    int b = imageMatrix[i, j].blue;

                    if (r > redMax) redMax = r;
                    if (g > greenMax) greenMax = g;
                    if (b > blueMax) blueMax = b;

                    if (r < redMin) redMin = r;
                    if (g < greenMin) greenMin = g;
                    if (b < blueMin) blueMin = b;

                    redSum += r;
                    greenSum += g;
                    blueSum += b;

                    redStats.Hist[r]++;
                    greenStats.Hist[g]++;
                    blueStats.Hist[b]++;

                }
            }
            redStats.Max = redMax; greenStats.Max = greenMax; blueStats.Max = blueMax;
            redStats.Min = redMin; greenStats.Min = greenMin; blueStats.Min = blueMin;
            redStats.Mean = redSum * invTotalPixels;
            greenStats.Mean = greenSum * invTotalPixels;
            blueStats.Mean = blueSum * invTotalPixels;
            int cumulativeRedFreq = 0; int cumulativeGreenFreq = 0; int cumlativeBlueFreq = 0;
            int medianPos = (int)(totalPixels + 1) / 2;
            double sumRedSqs = 0.0; double sumGreenSqs = 0.0; double sumBlueSqs = 0;
            double redDiff; double greenDiff; double blueDiff;
            redStats.Med = -1; blueStats.Med = -1; greenStats.Med = -1;
            for (int i = 0; i < 256; ++i)
            {
                int redCount = redStats.Hist[i];
                int greenCount = greenStats.Hist[i];
                int blueCount = blueStats.Hist[i];
                cumlativeBlueFreq += blueCount;
                cumulativeRedFreq += redCount;
                cumulativeGreenFreq += greenCount;

                if (cumulativeRedFreq >= medianPos && redStats.Med == -1)
                {
                    redStats.Med = i;
                }
                if (cumlativeBlueFreq >= medianPos && blueStats.Med == -1)
                {
                    blueStats.Med = i;
                }

                if (cumulativeGreenFreq >= medianPos && greenStats.Med == -1)
                {
                    greenStats.Med = i;
                }

                if (redCount > 0)
                {
                    redDiff = i - redStats.Mean;
                    sumRedSqs += redCount * redDiff * redDiff;

                }

                if (blueCount > 0)
                {
                    blueDiff = i - blueStats.Mean;
                    sumBlueSqs += blueCount * blueDiff * blueDiff;

                }

                if (greenCount > 0)
                {
                    greenDiff = i - greenStats.Mean;
                    sumGreenSqs += greenCount * greenDiff * greenDiff;
                }
            }
            redStats.StdDev = Math.Sqrt(sumRedSqs * invTotalPixels);
            blueStats.StdDev = Math.Sqrt(sumBlueSqs * invTotalPixels);
            greenStats.StdDev = Math.Sqrt(sumGreenSqs * invTotalPixels);

            return new ImageInfo
            {
                Height = height,
                Width = width,
                Path = imgPath,
                BlueStats = blueStats,
                GreenStats = greenStats,
                RedStats = redStats,
            };
        }


        /// <summary>
        /// Load all target images and calculate their stats
        /// </summary>
        /// <param name="targetPaths">Path of each target image</param>
        /// <returns>Calculated stats of each target image</returns>
        public static ImageInfo[] LoadAllImages(string[] targetPaths)
        {
            ImageInfo[] imageInfos = new ImageInfo[targetPaths.Length];

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            Parallel.For(0, targetPaths.Length, options, i =>
            {
                imageInfos[i] = CalculateImageStats(targetPaths[i]);
            });

            

            return imageInfos;
        }

        /// <summary>
        /// Match the given query image with the given target images and return the TOP matches as specified
        /// </summary>
        /// <param name="queryPath">Path of the query image</param>
        /// <param name="targetImgStats">Calculated stats of each target image</param>
        /// <param name="numOfTopMatches">Desired number of TOP matches to be returned</param>
        /// <returns>Top matches (image path & distance score) </returns>
        public static MatchInfo[] FindTopMatches(string queryPath, ImageInfo[] targetImgStats, int numOfTopMatches)
        {
            ImageInfo queryImg = CalculateImageStats(queryPath);
            MatchInfo[] matchInfos = new MatchInfo[targetImgStats.Length];
            //calculate cosine distance here
            for (int i = 0; i < targetImgStats.Length; ++i)
            {
                double result = calculateCosineDistance(ref targetImgStats[i], ref queryImg);
                matchInfos[i] = new MatchInfo
                {
                    MatchedImgPath = targetImgStats[i].Path,
                    MatchScore = result,
                };
            }
            Array.Sort(matchInfos, (p1, p2) => p1.MatchScore.CompareTo(p2.MatchScore));
            MatchInfo[] topMatches = new MatchInfo[numOfTopMatches];
            Array.Copy(matchInfos, topMatches, numOfTopMatches);

            return topMatches;
        }

        private static double calculateCosineDistance(ref ImageInfo imageInfos, ref ImageInfo queryImage)
        {
            //qprobdist can be calculated once. instead of everytime FIX THIS 
            int height = imageInfos.Height;
            int width = imageInfos.Width;
            int qheight = queryImage.Height;
            int qwidth = queryImage.Width;
            double redDist;
            double greenDist;
            double blueDist;
            double redSum = 0;
            double normRed1 = 0;
            double greenSum = 0;
            double normGreen1 = 0;
            double blueSum = 0;
            double normBlue1 = 0;
            double normRed2 = 0;
            double normBlue2 = 0;
            double normGreen2 = 0;
            for (int i = 0; i < 256; ++i)
            {
                double redProbDist = (double)imageInfos.RedStats.Hist[i] / (height * width);
                double greenProbDist = (double)imageInfos.GreenStats.Hist[i] / (height * width);
                double blueProbDist = (double)imageInfos.BlueStats.Hist[i] / (height * width);
                double qredProbDist = (double)queryImage.RedStats.Hist[i] / (qheight * qwidth);
                double qgreenProbDist = (double)queryImage.GreenStats.Hist[i] / (qheight * qwidth);
                double qblueProbDist = (double)queryImage.BlueStats.Hist[i] / (qheight * qwidth);
                redSum += (redProbDist * qredProbDist);
                greenSum += (greenProbDist * qgreenProbDist);
                blueSum += (blueProbDist * qblueProbDist);
                normRed1 += (redProbDist * redProbDist);
                normBlue1 += (blueProbDist * blueProbDist);
                normGreen1 += (greenProbDist * greenProbDist);
                normRed2 += (qredProbDist * qredProbDist);
                normGreen2 += (qgreenProbDist * qgreenProbDist);
                normBlue2 += (qblueProbDist * qblueProbDist);
            }
            redDist = Math.Acos(redSum / Math.Sqrt(normRed1 * normRed2)) * (180.0 / Math.PI);
            blueDist = Math.Acos(blueSum / Math.Sqrt(normBlue1 * normBlue2)) * (180.0 / Math.PI);
            greenDist = Math.Acos(greenSum / Math.Sqrt(normGreen1 * normGreen2)) * (180.0 / Math.PI);
            return (redDist + blueDist + greenDist) / 3;
        }
    }
}