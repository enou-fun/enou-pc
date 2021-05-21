using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRLibrary
{
    class BitmapRegion
    {
        private System.Drawing.Bitmap edgeImage;
        private System.Drawing.Bitmap image;
        private Dictionary<int, List<Point>> regionId2PointsDict = new Dictionary<int, List<Point>>();
        private Dictionary<int, System.Drawing.Color> regionId2ColorAvgDict = new Dictionary<int, System.Drawing.Color>();
        private Dictionary<int, double> regionId2ColorVarDict = new Dictionary<int, double>();
        private Dictionary<Point, int> point2RegionIdDict = new Dictionary<Point, int>();
        private HashSet<Point> scanedPointSet = new HashSet<Point>();
        private HashSet<int> transparentRegionId = new HashSet<int>();
        private int regionIdGenerator = 0;
        private int maxGradientRGB = 20;
        private double maxColorVar = 500;
        private int minPointCount = 3;
        private int edgeMinRPass = 0;
        private int edgeMinR = 0;

        public BitmapRegion(System.Drawing.Bitmap img)
        {
            this.image = new System.Drawing.Bitmap(img);

          //  CalEdgeImage();
        }

        /*
        private void CalEdgeImage()
        {
            this.edgeImage = BitmapUtil.ToEdgeImg(image, out edgeMinR);
            edgeMinRPass = (int)(190 + edgeMinR*0.3);
        }
        */

        public void Regionalize()
        {
            //CalEdgeImage();
            for(int x = 0; x < image.Width; ++x)
            {
                for(int y = 0; y < image.Height; ++y)
                {
                    Regionalize(new Point(x, y));
                }
            }
        }

        public void DitchBig()
        {

        }

        public void CalColorPropForRegions()
        {
            if (regionId2PointsDict.Count == 0)
                return;

            foreach(var idPoints in regionId2PointsDict)
            {
                int regionId = idPoints.Key;
                List<Point> pointList = idPoints.Value;
                double count = pointList.Count;
                double r = 0;
                double g = 0;
                double b = 0;

                foreach(var point in pointList)
                {
                    System.Drawing.Color color = image.GetPixel(point.X, point.Y);
                    r += color.R/count;
                    g += color.G/count;
                    b += color.B/count;
                }
                regionId2ColorAvgDict[regionId] = System.Drawing.Color.FromArgb((int)r, (int)g, (int)b);
            }

            foreach(var idPoints in regionId2PointsDict)
            {
                int regionId = idPoints.Key;
                List<Point> pointList = idPoints.Value;
                System.Drawing.Color avgColor = regionId2ColorAvgDict[regionId];
                double variety = 0;
                foreach(var point in pointList)
                {
                    System.Drawing.Color pointColor = image.GetPixel(point.X, point.Y);

                    variety +=(pointColor.R - avgColor.R) * (pointColor.R - avgColor.R);
                    variety +=(pointColor.G - avgColor.G) * (pointColor.G - avgColor.G);
                    variety +=(pointColor.B - avgColor.B) * (pointColor.B - avgColor.B);
                }
                
                variety = Math.Sqrt(variety);
                regionId2ColorVarDict[regionId] = variety;
            }
        }

        public void Draw()
        {
            foreach (var idPoints in regionId2PointsDict)
            {
                int regionId = idPoints.Key;
                List<Point> pointList = idPoints.Value;
                System.Drawing.Color color = regionId2ColorAvgDict[regionId];
                double variety = regionId2ColorVarDict[regionId];
                if(variety > maxColorVar)
                {
                    //color = System.Drawing.Color.Black;
                }

                if(pointList.Count < minPointCount)
                {
                    transparentRegionId.Add(regionId);
                    //color = System.Drawing.Color.FromArgb(0, 255, 255, 255);
                }

                foreach (var point in pointList)
                {
                    image.SetPixel(point.X, point.Y, color);
                }
            }

        }

        public System.Drawing.Bitmap DrawAndGetImage()
        {
            Draw();
            return new System.Drawing.Bitmap(image);
        }

        public System.Drawing.Bitmap GetRefinedImage()
        {
            System.Drawing.Bitmap refinedImage = new System.Drawing.Bitmap(image);

            for(int x = 0; x < refinedImage.Width; ++x)
            {
                for(int y = 0; y < refinedImage.Height; ++y)
                {
                    System.Drawing.Color color = refinedImage.GetPixel(x, y);
                    System.Drawing.Color edgeColor = edgeImage.GetPixel(x, y);

                    System.Drawing.Color result = color.UseBrightness((edgeColor.R-edgeMinR) / (256f-edgeMinR));

                    refinedImage.SetPixel(x,y,result);

                }
            }

            return refinedImage;
        }

        /*
        private double GetEdgeRangeBlackRate(int x, int y, System.Drawing.Bitmap img)
        {
            double rate = 0;
            System.Drawing.Color edgeColor = edgeImage.GetPixel(x, y);
            System.Drawing.Color defaultColor = edgeColor;

            edgeColor = edgeImage.GetPixelWithDefault(x, y, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x, y+1, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x+1, y+1, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x+1, y, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x+1, y-1, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x, y-1, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x-1, y-1, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x-1, y, defaultColor);
            rate += edgeColor.R /255.0f /9f;
            edgeColor = edgeImage.GetPixelWithDefault(x-1, y+1, defaultColor);
            rate += edgeColor.R /255.0f /9f;

            return 1- rate;
        }

    */
        
        private void Regionalize(Point point)
        {
            if (scanedPointSet.Contains(point))
                return;

            List<Point> pointList = new List<Point>();
            Queue<Point> edgeQueue = new Queue<Point>();
            int regionId = GetNewRegionId();
            regionId2PointsDict.Add(regionId, pointList);

            edgeQueue.Enqueue(point);
            scanedPointSet.Add(point);
            while (edgeQueue.Count > 0)
            {
                Point edgePoint = edgeQueue.Dequeue();
                AddToRegion(edgePoint, regionId);

                Point up = new Point(edgePoint.X, edgePoint.Y + 1);
                AddToEdgeIfValid(up, edgePoint, edgeQueue);
                Point upRight = new Point(edgePoint.X+1, edgePoint.Y+1);
                AddToEdgeIfValid(upRight, edgePoint, edgeQueue);
                Point right = new Point(edgePoint.X+1, edgePoint.Y);
                AddToEdgeIfValid(right, edgePoint, edgeQueue);
                Point rightDown = new Point(edgePoint.X+1, edgePoint.Y-1);
                AddToEdgeIfValid(rightDown, edgePoint, edgeQueue);
                Point down = new Point(edgePoint.X, edgePoint.Y - 1);
                AddToEdgeIfValid(down, edgePoint, edgeQueue);
                Point leftDown = new Point(edgePoint.X - 1, edgePoint.Y - 1);
                AddToEdgeIfValid(leftDown, edgePoint, edgeQueue);
                Point left = new Point(edgePoint.X-1, edgePoint.Y);
                AddToEdgeIfValid(left, edgePoint, edgeQueue);
                Point leftUp = new Point(edgePoint.X-1, edgePoint.Y+1);
                AddToEdgeIfValid(leftUp, edgePoint, edgeQueue);
            }
        }

        private void AddToEdgeIfValid(Point dest, Point from, Queue<Point> edgeQueue)
        {
            if (!image.PosValid(dest.X, dest.Y))
                return;

            if (scanedPointSet.Contains(dest))
                return;


            if (!ColorGradientValid(dest, from))
                return;



            /*
            if (!CanPass(dest))
                return;
                */

            /*
            Point jump = new Point(dest.X * 2 - from.X, dest.Y * 2 - from.Y);
            if (image.PosValid(jump.X, jump.Y) && !ColorGradientValid(jump, from))
                return;
                */

            scanedPointSet.Add(dest);
            edgeQueue.Enqueue(dest);

        }

        private bool CanPass(Point dest)
        {
            return edgeImage.GetPixel(dest.X, dest.Y).R > edgeMinRPass; //这个值应该是动态的，取决与edgeImage的某个特征 todo
        }

        private bool ColorGradientValid(Point dest, Point from)
        {
            if (!image.PosValid(dest.X, dest.Y))
                return false;

            System.Drawing.Color fromColor = image.GetPixel(from.X, from.Y);
            System.Drawing.Color destColor = image.GetPixel(dest.X, dest.Y);

            int gradient = 0;
            gradient += (fromColor.R - destColor.R) * (fromColor.R - destColor.R);
            gradient += (fromColor.G - destColor.G) * (fromColor.G - destColor.G);
            gradient += (fromColor.B - destColor.B) * (fromColor.B - destColor.B);

            return Math.Sqrt(gradient) < maxGradientRGB;
        }

        private void AddToRegion(Point point, int regionId)
        {
            regionId2PointsDict[regionId].Add(point);
            point2RegionIdDict.Add(point, regionId);
        }

        private bool HasMarkedRegion(Point point)
        {
            return point2RegionIdDict.ContainsKey(point);
        }

        private int GetNewRegionId()
        {
            return regionIdGenerator++;
        }

    }
}
