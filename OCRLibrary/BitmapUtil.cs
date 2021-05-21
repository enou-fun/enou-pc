using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCRLibrary
{
    public static class BitmapUtil
    {

        public static Bitmap RenderBackgroundToStrokeColor(Bitmap originImg, Color strokeColor)
        {
            Bitmap resultImg = new Bitmap(originImg);

            for (int x = 0; x < resultImg.Width; ++x)
            {
                /*
                 * in case the border color is white, we scan 2 lines at a side.
                 */
                RenderPointToStrokeColor(x, 0, resultImg, strokeColor);
                RenderPointToStrokeColor(x, 1, resultImg, strokeColor);
                RenderPointToStrokeColor(x, resultImg.Height - 1, resultImg, strokeColor);
                RenderPointToStrokeColor(x, resultImg.Height - 2, resultImg, strokeColor);
            }

            for (int y = 0; y < resultImg.Height; ++y)
            {
                RenderPointToStrokeColor(0, y, resultImg, strokeColor);
                RenderPointToStrokeColor(1, y, resultImg, strokeColor);
                RenderPointToStrokeColor(resultImg.Width - 1, y, resultImg, strokeColor);
                RenderPointToStrokeColor(resultImg.Width - 2, y, resultImg, strokeColor);
            }

            return resultImg;
        }

        private static void RenderPointToStrokeColor(int x, int y, Bitmap originImg, Color strokeColor)
        {
            Color color = originImg.GetPixel(x, y);
            if (Math.Abs(color.Sum() - strokeColor.Sum()) < 300)
                return;

            LinkedList<Tuple<int, int>> spotEdgeList = new LinkedList<Tuple<int, int>>();
            spotEdgeList.AddLast(Tuple.Create(x, y));
            originImg.SetPixel(x, y, strokeColor);

            while (spotEdgeList.Count != 0)
            {
                var spot = spotEdgeList.First.Value;
                CleanPixelAndAddToListIfNotStroke(spot.Item1 - 1, spot.Item2, spotEdgeList, originImg, strokeColor);
                CleanPixelAndAddToListIfNotStroke(spot.Item1 + 1, spot.Item2, spotEdgeList, originImg, strokeColor);
                CleanPixelAndAddToListIfNotStroke(spot.Item1, spot.Item2 - 1, spotEdgeList, originImg, strokeColor);
                CleanPixelAndAddToListIfNotStroke(spot.Item1, spot.Item2 + 1, spotEdgeList, originImg, strokeColor);
                spotEdgeList.RemoveFirst();

            }

        }


        private static void CleanPixelAndAddToListIfNotStroke(int x, int y, LinkedList<Tuple<int, int>> edgeList, Bitmap img, Color strokeColor)
        {
            if (!img.PosValid(x, y))
                return;

            Color pointColor = img.GetPixel(x, y);
            if (Math.Abs(pointColor.Sum() - strokeColor.Sum()) < 300)
                return;


            edgeList.AddLast(Tuple.Create(x, y));
            img.SetPixel(x, y, strokeColor);
        }

        public static Bitmap CleanEdgeSpot(Bitmap originImg)
        {
            Bitmap resultImg = new Bitmap(originImg); 

            for (int x = 0; x < resultImg.Width; ++x)
            {
                /*
                 * in case the border color is white, we scan 2 lines at a side.
                 */
                TurnSpotColorTo(x, 0, resultImg);
                TurnSpotColorTo(x, 1, resultImg);
                TurnSpotColorTo(x, resultImg.Height-1, resultImg);
                TurnSpotColorTo(x, resultImg.Height-2, resultImg);
            }

            for(int y = 0; y < resultImg.Height; ++y)
            {
                TurnSpotColorTo(0, y, resultImg);
                TurnSpotColorTo(1, y, resultImg);
                TurnSpotColorTo(resultImg.Width-1, y, resultImg);
                TurnSpotColorTo(resultImg.Width-2, y, resultImg);
            }

            return resultImg;
        }

        private static void TurnSpotColorTo(int x, int y, Bitmap originImg)
        {
            Color color = originImg.GetPixel(x, y);
            int pointColor = originImg.GetPixel(x, y).ToArgb();
            if (pointColor.Equals(Color.White.ToArgb()))
                return;

            LinkedList<Tuple<int, int>> spotEdgeList = new LinkedList<Tuple<int, int>>();
            spotEdgeList.AddLast(Tuple.Create(x, y));
            originImg.SetPixel(x, y, Color.White);

            while (spotEdgeList.Count != 0)
            {
                var spot = spotEdgeList.First.Value;
                CleanPixelAndAddToListIfBlack(spot.Item1-1, spot.Item2, spotEdgeList, originImg);
                CleanPixelAndAddToListIfBlack(spot.Item1+1, spot.Item2, spotEdgeList, originImg);
                CleanPixelAndAddToListIfBlack(spot.Item1, spot.Item2-1, spotEdgeList, originImg);
                CleanPixelAndAddToListIfBlack(spot.Item1, spot.Item2+1, spotEdgeList, originImg);
                spotEdgeList.RemoveFirst();

            }

        }

        private static void CleanPixelAndAddToListIfBlack(int x, int y, LinkedList<Tuple<int,int>> edgeList, Bitmap img)
        {
            if (!img.PosValid(x, y))
                return;

            int pointColor = img.GetPixel(x, y).ToArgb();
            if (pointColor.Equals(Color.White.ToArgb()))
                return;

            if (pointColor.Equals(Color.Black.ToArgb()))
            {
                edgeList.AddLast(Tuple.Create(x, y));
                img.SetPixel(x, y, Color.White);
            }
        }

        public static bool PosValid(this Bitmap img, int x, int y)
        {
            bool minValid = x >= 0 && y >= 0;
            bool maxValid = x < img.Width && y < img.Height;

            return minValid && maxValid;
        }

        public static Bitmap Shrink(this Bitmap originImage)
        {
            Bitmap image = new Bitmap(originImage.Width-2, originImage.Height-2);
            for(int x = 0; x < image.Width; ++x)
            {
                for(int y = 0; y < image.Height; ++y)
                {
                    Color color = originImage.GetPixel(x + 1, y + 1);
                    image.SetPixel(x, y, color);
                }
            }
            return image;
        }

        public static Color GetPixelWithDefault(this Bitmap img, int x, int y, Color defaultColor)
        {
            if (!img.PosValid(x, y))
                return defaultColor;

            return img.GetPixel(x, y);
        }

        public static Tuple<Color,Color> GetVividColor(this Bitmap img, int x, int y)
        {
            Color minColor = Color.White;
            Color maxColor = Color.Black;

            GetMinMaxColor(img, x, y, ref minColor, ref maxColor);
            GetMinMaxColor(img, x, y+1, ref minColor, ref maxColor);
            GetMinMaxColor(img, x+1, y+1, ref minColor, ref maxColor);
            GetMinMaxColor(img, x+1, y, ref minColor, ref maxColor);
            GetMinMaxColor(img, x+1, y-1, ref minColor, ref maxColor);
            GetMinMaxColor(img, x, y-1, ref minColor, ref maxColor);
            GetMinMaxColor(img, x-1, y-1, ref minColor, ref maxColor);
            GetMinMaxColor(img, x-1, y, ref minColor, ref maxColor);
            GetMinMaxColor(img, x-1, y+1, ref minColor, ref maxColor);

            return new Tuple<Color, Color>(minColor, maxColor);
        }

        private static void GetMinMaxColor(this Bitmap img, int x, int y, ref Color minColor, ref Color maxColor)
        {
            if (!img.PosValid(x, y))
                return;

            Color color = img.GetPixel(x, y);
            if (color.Sum() < minColor.Sum())
                minColor = color;

            if (color.Sum() > maxColor.Sum())
                maxColor = color;
        }

        public static int Sum(this Color color)
        {
            return color.R + color.G + color.B;
        }

        public static Color Plus(this Color color, Color anotherColor, double rate)
        {
            int r = (int)(color.R + anotherColor.R * rate);
            int g = (int)(color.G + anotherColor.G * rate);
            int b = (int)(color.B + anotherColor.B * rate);
            r = Math.Min(255, r);
            g = Math.Min(255, g);
            b = Math.Min(255, b);
            Color result = Color.FromArgb(r, g, b);
            return result;
        }

        public static Color UseBrightness(this Color color, double rate)
        {
            double multiply = rate /color.GetBrightness();

            int r = (int)(color.R * multiply);
            int g = (int)(color.G * multiply);
            int b = (int)(color.B * multiply);

            r = Math.Min(255, r);
            g = Math.Min(255, g);
            b = Math.Min(255, b);

            return Color.FromArgb(r,g,b);
        }


        /**
        *  clean those spots whose edge color is not so sharp in the origin image
        */
        public static Bitmap CleanNotSubSpot(Bitmap modifiedImg, Bitmap originImg)
        {
            Bitmap resultImg = new Bitmap(modifiedImg);

            /* iterate spots(as long as it's black), find it's edge(the pixels whose neighboring pixel's color is white)
             * compare the two pixels between different sides of the edge(pixels) in the originImg,
             * calculate the difference results and accumulte the absolute value, then get the average value. 
             * if the difference result's abosulte value is far less than the average value, then it's a spot, not a subtitle remove it 
             * 
             * or the difference result of one spot's edge vary so much then it may not be a subtitle.
            */
            

            return resultImg;
        }

        /*
         * 锐化边界
         */
        public static Bitmap ToEdgeImg(Bitmap originImg, out int minR)
        {
            int range = 1;
            Bitmap resultImg = new Bitmap(originImg);

            minR = 255;
            
            for(int x = 0; x < resultImg.Width; ++x)
            {
                for(int y=0; y< resultImg.Height; ++y)
                {
                    double edgeValue = getEdgeValueAt(x, y, originImg, range);
                    Color color = FromEdgeValueToColor(edgeValue);
                    if (color.R < minR) minR = color.R;
                    resultImg.SetPixel(x, y, color);
                }
            }
            return resultImg;
        }

        private static Color FromEdgeValueToColor(double edgeValue)
        {
            double rate = edgeValue / Math.Abs(Color.White.Sum() - Color.Black.Sum());
            int r = (int)(255 * (1-rate));
            return Color.FromArgb(r, r, r);
        }

        private static double getEdgeValueAt(int x, int y, Bitmap img, int range)
        {
            double edgeValue = 0;
            for(int i = 1; i <= range; ++i)
            {
                Color defaultColor = img.GetPixel(x, y);
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x,y+i,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x+i,y+i,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x+i,y,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x+i,y-i,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x,y-i,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x-i,y-i,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x-i,y,img, defaultColor))/8.0f/range;
                edgeValue += Math.Abs(GetPixelColorSum(x, y, img, defaultColor) - GetPixelColorSum(x-i,y+i,img, defaultColor))/8.0f/range;
            }

            return edgeValue;
        }

        private static int GetPixelColorSum(int x, int y, Bitmap img, Color defaultColor)
        {
            if (!img.PosValid(x, y))
                return defaultColor.Sum();

            return img.GetPixel(x, y).Sum();
        }


    }
}
