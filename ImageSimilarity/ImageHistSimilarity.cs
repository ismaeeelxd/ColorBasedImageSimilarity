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

            //TODO: Must somehow calculate the red values, green values, blue values alone AND calculate the median ,std dev.
            /*
             * Have a worker for each channel to calculate the mean, median, std dev in parrallel to reduce time
             */
            RGBPixel [,] imageMatrix = OpenImage(imgPath);
            int height = imageMatrix.GetLength(0);
            int width = imageMatrix.GetLength(1);
            int redSum = 0, greenSum = 0, blueSum = 0;
            int redMax = -1, greenMax = -1, blueMax = -1;
            int redMin = int.MaxValue, greenMin = int.MaxValue, blueMin = int.MaxValue;
            int totalPixels = height * width;
            ChannelStats redStats = new ChannelStats { Hist = new int[256] };
            ChannelStats greenStats = new ChannelStats { Hist = new int[256] };
            ChannelStats blueStats = new ChannelStats { Hist = new int[256] };
            ImageInfo imageInfo = new ImageInfo{
                Height = height,
                Width = width,
                Path = imgPath,
                BlueStats = blueStats,
                GreenStats = greenStats,
                RedStats = redStats,
            };

            for (int i = 0; i < height; ++i)
            {
                for(int j = 0; j < width; ++j)
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
            //could potentially have 3 workers here to speed up the process.

            ComputeChannelStats(ref redStats, redSum, totalPixels);
            ComputeChannelStats(ref greenStats, greenSum, totalPixels);
            ComputeChannelStats(ref blueStats, blueSum, totalPixels);

            return imageInfo;
        }
        private static void ComputeChannelStats(ref ChannelStats channelStats,int sum, int totalPixels)
        {
            channelStats.Mean = (double)sum / totalPixels;

            
            int medianPos = (totalPixels + 1) / 2;
            int cumulativeFreq = 0;

            for (int i = 0; i < 256; ++i)
            {
                cumulativeFreq += channelStats.Hist[i];
                if (cumulativeFreq >= medianPos)    
                {
                    channelStats.Med = i;
                    break;
                }
            }

            double sumSquares = 0.0;
            for (int i = 0; i < 256; i++)
            {
                int count = channelStats.Hist[i];
                if (count == 0) continue;
                double diff = i - channelStats.Mean;
                sumSquares += count * diff * diff;
            }
            channelStats.StdDev = Math.Sqrt(sumSquares / totalPixels);
        }


        /// <summary>
        /// Load all target images and calculate their stats
        /// </summary>
        /// <param name="targetPaths">Path of each target image</param>
        /// <returns>Calculated stats of each target image</returns>
        public static ImageInfo[] LoadAllImages(string[] targetPaths)
        {
            ImageInfo[] imageInfos = new ImageInfo[targetPaths.Length];

            // Create ParallelOptions and set the maximum degree of parallelism
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            // Use Parallel.For with ParallelOptions
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
            for(int i = 0; i < numOfTopMatches; ++i)
            {
                double result = calculateCosineDistance(ref targetImgStats[i], ref queryImg);
                matchInfos[i] = new MatchInfo
                {
                    MatchedImgPath = targetImgStats[i].Path,
                    MatchScore = result,
                };
            }
            Array.Sort(matchInfos, (p1, p2) => p1.MatchScore.CompareTo(p2.MatchScore));

            return matchInfos.Take(numOfTopMatches).ToArray();
        }

        private static double calculateCosineDistance(ref ImageInfo imageInfos, ref ImageInfo queryImage)
        {
            //qprobdist can be calculated once. instead of everytime FIX THIS 
            int height = imageInfos.Height;
            int width = imageInfos.Width;
            int qheight = queryImage.Height;
            int qwidth = queryImage.Width;
            double redDist = 0;
            double greenDist = 0;
            double blueDist = 0;
            double redSum = 0;
            double normRed1 = 0;
            double greenSum = 0;
            double normGreen1 = 0;
            double blueSum = 0;
            double normBlue1 = 0;
            double normRed2 = 0;
            double normBlue2 = 0;
            double normGreen2 = 0;
            for(int i = 0; i < 256; ++i)
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
