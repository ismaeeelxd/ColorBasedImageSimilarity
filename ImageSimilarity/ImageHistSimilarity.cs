using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
             * calculate the prob dist in the same n^2 loop
             */
            RGBPixel [,] imageMatrix = OpenImage(imgPath);
            int height = imageMatrix.GetLength(0);
            int width = imageMatrix.GetLength(1);
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
            int redSum = 0, greenSum = 0, blueSum = 0;
            int redMax = -1, greenMax = -1, blueMax = -1;
            int redMin = int.MaxValue, greenMin = int.MinValue, blueMin = int.MaxValue;
            for (int i = 0; i < height; ++i)
            {
                for(int j = 0; j < width; ++j)
                {
                    if (imageMatrix[i, j].red > redMax) redMax = imageMatrix[i, j].red;
                    if (imageMatrix[i, j].blue > blueMax) blueMax = imageMatrix[i, j].blue;
                    if (imageMatrix[i, j].green > greenMax) greenMax = imageMatrix[i, j].green;

                    if (imageMatrix[i, j].red < redMin) redMin = imageMatrix[i, j].red;
                    if (imageMatrix[i, j].green < greenMin) greenMin = imageMatrix[i, j].green;
                    if (imageMatrix[i, j].blue < blueMin) blueMin = imageMatrix[i, j].blue;

                    redSum += imageMatrix[i, j].red;
                    greenSum += imageMatrix[i, j].green;
                    blueSum += imageMatrix[i, j].blue;

                    redStats.Hist[imageMatrix[i, j].red]++;
                    greenStats.Hist[imageMatrix[i, j].green]++;
                    blueStats.Hist[imageMatrix[i, j].blue]++;

                }
            }
            throw new NotImplementedException();
        }
        /// <summary>
        /// Load all target images and calculate their stats
        /// </summary>
        /// <param name="targetPaths">Path of each target image</param>
        /// <returns>Calculated stats of each target image</returns>
        public static ImageInfo[] LoadAllImages(string []targetPaths)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
