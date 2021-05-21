using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Tesseract;

namespace OCRLibrary
{
    public class TesseractOCR : IOptChaRec
    {
        public string srcLangCode = "eng";//OCR识别语言 jpn=日语 eng=英语
        private TesseractEngine TessOCR;
        private string errorInfo;

        private IntPtr WinHandle;
        private Rectangle OCRArea;
        private bool isAllWin;

        private static TesseractOCR instance;

        public static TesseractOCR Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new TesseractOCR();
                    instance.OCR_Init();
                }
                return instance;
            }
        }

        public string OCRProcess(Bitmap originImg)
        {
            try {

                Bitmap cleanImg = originImg;
                if(!IsBlackFontWhiteBack(originImg))
                    cleanImg = GetCleanImg(originImg);


                PageSegMode pageSegMode = PageSegMode.Auto;
                if (originImg.Width < 100)
                    pageSegMode = PageSegMode.SingleWord;
                using (var page = TessOCR.Process(cleanImg, pageSegMode))
                {
                    string res = page.GetText();
                    res = res.Trim();
                    if (res.Equals(String.Empty))
                    {
                        saveFailImg(originImg);
                        saveFailImg(cleanImg);
                    }
                    page.Dispose();
                    return res;
                }

            }
            catch (Exception ex) {
                errorInfo = ex.Message;

                File.AppendAllText("log.txt", String.Format("{0} {1}", DateTime.Now.ToString(), ex.Message));
            }

            return String.Empty;
        }

        private void back(Bitmap originImg)
        {
            //如果只有两种颜色，则是文本阅读模式。直接识别
            //如果颜色驳杂，则说明是看动漫模式。用下面这种
            // OpenCVTest(originImg);
            originImg.Save("origin.jpg");

            /*
            Bitmap sharpBlackImg = MoveGreyToWhite(originImg);
            sharpBlackImg.Save("sharpBlack.jpg");
            */

            Bitmap grey1 = ToGrey(originImg);
            Bitmap blackWhiteOrigin = Thresholding(grey1);
            blackWhiteOrigin.Save("blackWhiteOrigin.jpg");

            BitmapRegion bitmapRegion = new BitmapRegion(originImg);
            bitmapRegion.Regionalize();
            bitmapRegion.CalColorPropForRegions();
            Bitmap regionImage = bitmapRegion.DrawAndGetImage();
            regionImage.Save("region.jpg");

            Bitmap greyImg = ToGrey(regionImage);
            greyImg.Save("grey.jpg");


            int minR = 0;
            Bitmap edgeImg = BitmapUtil.ToEdgeImg(originImg, out minR);
            edgeImg.Save("edge.jpg");


            var fontColor = GetFontStrokeColor(originImg, edgeImg, minR);



            Bitmap blackWhiteImg = Thresholding(regionImage);
            Bitmap blackAlphaImg = ReverseColor(blackWhiteImg);
            blackAlphaImg.Save("blackAlpha.jpg");

            Bitmap cleanImg = BitmapUtil.CleanEdgeSpot(blackAlphaImg);
            cleanImg.Save("clean.jpg");
        }

        private Bitmap GetCleanImg(Bitmap originImg)
        {
            int minR = 0;

            Bitmap edgeImg = BitmapUtil.ToEdgeImg(originImg, out minR);

            //todo 强化文字行部分的edge
            var fontStrokeColor = GetFontStrokeColor(originImg, edgeImg, minR);
            var fontColor = fontStrokeColor.Item1;
            var strokeColor = fontStrokeColor.Item2;

            Bitmap strokeBackImg = BitmapUtil.RenderBackgroundToStrokeColor(originImg, strokeColor);

            Bitmap blackFontImg = RemainFontAsBlackColor(strokeBackImg, fontColor);

            originImg.Save("origin.jpg");
            edgeImg.Save("edge.jpg");
            strokeBackImg.Save("strokeBack.jpg");
            blackFontImg.Save("blackFont.jpg");

            return blackFontImg;
        }

        private bool IsBlackFontWhiteBack(Bitmap img)
        {
            int totalPix = img.Width * 2 + img.Height * 2;

            int blackPixCount = 0;
            for(int x = 0; x < img.Width; ++x)
            {
                if (img.GetPixel(x, 0).Sum() < 700)
                    blackPixCount++;

                if (img.GetPixel(x, img.Height-1).Sum() < 700)
                    blackPixCount++;

                if (blackPixCount > totalPix * 0.1)
                    return false;
            }

            for(int y = 0; y < img.Height; ++y)
            {
                if (img.GetPixel(0, y).Sum() < 700)
                    blackPixCount++;

                if (img.GetPixel(img.Width-1, y).Sum() < 700)
                    blackPixCount++;

                if (blackPixCount > totalPix * 0.1)
                    return false;
            }

            return true;
        }

        private void RenderBackgroundBlack(Bitmap originImg)
        {
            HashSet<Point> scanedPointSet = new HashSet<Point>();

            for (int x = 0; x < originImg.Width; ++x)
            {
                RenderBackgroundBlack(originImg, new Point(x,0));
                RenderBackgroundBlack(originImg, new Point(x,1));
                RenderBackgroundBlack(originImg, new Point(x,2));
                RenderBackgroundBlack(originImg, new Point(x,originImg.Height-1));
                RenderBackgroundBlack(originImg, new Point(x,originImg.Height-2));
                RenderBackgroundBlack(originImg, new Point(x,originImg.Height-3));
            }

            for (int y = 0; y < originImg.Height; ++y)
            {
                RenderBackgroundBlack(originImg, new Point(0,y));
                RenderBackgroundBlack(originImg, new Point(1,y));
                RenderBackgroundBlack(originImg, new Point(2,y));
                RenderBackgroundBlack(originImg, new Point(originImg.Width-1,y));
                RenderBackgroundBlack(originImg, new Point(originImg.Width-2,y));
                RenderBackgroundBlack(originImg, new Point(originImg.Width-3,y));
            }
        }

        private void RenderBackgroundBlack(Bitmap originImg, Point point)
        {
        }

        private void OpenCVTest(Bitmap originImage)
        {

            Mat src = originImage.ToMat().CvtColor(ColorConversionCodes.BGR2GRAY);

            Mat result = src.Threshold(0, 255, ThresholdTypes.BinaryInv | ThresholdTypes.Otsu);

            var contours = src.FindContoursAsArray(RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
            result.DrawContours(contours, 1, Scalar.Blue, 10, LineTypes.Link8);

            result.SaveImage("imgResult.jpg");
        }


        private Tuple<Color,Color> GetFontStrokeColor(Bitmap originImage, Bitmap edgeImage, int minR)
        {
            int threshold = minR + 20; //todo 50 可以调整？
            Dictionary<Color, double> fontColorWeightDict = new Dictionary<Color, double>();
            Dictionary<Color, double> strokeColorWeightDict = new Dictionary<Color, double>();
            for(int x = 0; x <edgeImage.Width; ++x)
            {
                for(int y = 0; y < edgeImage.Height; ++y)
                {
                    var edgeValue = (255 - edgeImage.GetPixel(x, y).R);
                    if (edgeImage.GetPixel(x, y).R > threshold)
                        continue;


                    var colorPair = originImage.GetVividColor(x, y);
                    Color minColor = colorPair.Item1;
                    Color maxColor = colorPair.Item2;

                    if (!strokeColorWeightDict.ContainsKey(minColor))
                        strokeColorWeightDict.Add(minColor,0);
                    strokeColorWeightDict[minColor] += edgeValue;

                    if (!fontColorWeightDict.ContainsKey(maxColor))
                        fontColorWeightDict.Add(maxColor,0);
                    fontColorWeightDict[maxColor] += edgeValue;

                }
            }

            // color will become greater
            var fontColorList = fontColorWeightDict.Keys.ToList();
            fontColorList.Sort((color1,color2)=>{ return color1.Sum() - color2.Sum(); });
            for(int i = 0; i < fontColorList.Count/2; ++i)
                fontColorWeightDict.Remove(fontColorList[i]);

            var strokeColorList = strokeColorWeightDict.Keys.ToList();
            strokeColorList.Sort((color1, color2) => { return color1.Sum() - color2.Sum(); });
            for(int i = strokeColorList.Count / 2; i < strokeColorList.Count; ++i)
                strokeColorWeightDict.Remove(strokeColorList[i]);


            Color font = GetAvgColorFromDict(fontColorWeightDict);
            Color stroke = GetAvgColorFromDict(strokeColorWeightDict);

            return new Tuple<Color, Color>(font, stroke);
           
        } 


        private Color GetAvgColorFromDict(Dictionary<Color,double> colorWeightDict)
        {
            Dictionary<Color, double> resultDict = new Dictionary<Color, double>();

            double totalWeight = 0;
            foreach (var keyValue in colorWeightDict)
            {
                if (keyValue.Value > 100)
                {
                    resultDict.Add(keyValue.Key, keyValue.Value);
                    totalWeight += keyValue.Value;
                }
            }


            double r = 0;
            double g = 0;
            double b = 0;
            foreach (var keyValue in resultDict)
            {
                r += keyValue.Key.R * keyValue.Value / totalWeight;
                g += keyValue.Key.G * keyValue.Value / totalWeight;
                b += keyValue.Key.B * keyValue.Value / totalWeight;
            }

            return Color.FromArgb((int)r, (int)g, (int)b);
        }



        private void saveFailImg(Bitmap img)
        {
            String failImgDir = "failImage";
            if(!Directory.Exists(failImgDir))
            {
                Directory.CreateDirectory(failImgDir);
            }

            String fileName = DateTime.Now.Ticks + ".jpg";
            img.Save(failImgDir +"\\"+ fileName);
        }

        public bool OCR_Init(string param1 = "", string param2 = "")
        {
            try
            {
                TessOCR = new TesseractEngine(Environment.CurrentDirectory + "\\tessdata", srcLangCode, EngineMode.Default);
                return true;
            }
            catch(Exception ex)
            {
                errorInfo = ex.Message;
                return false;
            }
        }

        public string GetLastError()
        {
            return errorInfo;
        }

        public string OCRProcess()
        {
            if (OCRArea != null)
            {
                Image img = ScreenCapture.GetWindowRectCapture(WinHandle, OCRArea, isAllWin);
                return OCRProcess(new Bitmap(img));
            }
            else
            {
                errorInfo = "未设置截图区域";
                return null;
            }
        }

        public void SetOCRArea(IntPtr handle, Rectangle rec, bool AllWin)
        {
            WinHandle = handle;
            OCRArea = rec;
            isAllWin = AllWin;
        }

        public Image GetOCRAreaCap()
        {
            return ScreenCapture.GetWindowRectCapture(WinHandle, OCRArea, isAllWin);
        }

        public void SetOCRSourceLang(string lang)
        {
            srcLangCode = lang;
        }


        //todo 先放这里。。
        static Bitmap ToGrey(Bitmap originImg)
        {
            Bitmap resultImg = new Bitmap(originImg);
            for (int i = 0; i < resultImg.Width; i++)
            {
                for (int j = 0; j < resultImg.Height; j++)
                {
                    Color pixelColor = resultImg.GetPixel(i, j);
                    //计算灰度值
                    int grey = (int)(0.333 * pixelColor.R + 0.333 * pixelColor.G + 0.333 * pixelColor.B);
                    if (pixelColor.A == 0) grey = 255;
                    Color newColor = Color.FromArgb(grey, grey, grey);
                    resultImg.SetPixel(i, j, newColor);
                }
            }
            return resultImg;
        }
        static Bitmap ReverseColor(Bitmap originImg)
        {
            Bitmap resultImg = new Bitmap(originImg);
            for (int i = 0; i < resultImg.Width; i++)
            {
                for (int j = 0; j < resultImg.Height; j++)
                {
                    Color pixelColor = resultImg.GetPixel(i, j);
                    Color newColor = Color.FromArgb(255-pixelColor.R, 255-pixelColor.G, 255-pixelColor.B);
                    resultImg.SetPixel(i, j, newColor);
                }
            }
            return resultImg;
        }

        static Bitmap MoveGreyToWhite(Bitmap image)
        {
            double pow = 1.6;
            Bitmap resultImg = new Bitmap(image);
            for (int i = 0; i < resultImg.Width; i++)
            {
                for (int j = 0; j < resultImg.Height; j++)
                {

                    Color color = resultImg.GetPixel(i, j);
                    if (!isGrey(color))
                        continue;

                    int r = Math.Max(0, color.R - 15);
                    r = (int) Math.Min(255, Math.Pow(r, pow));
                    int g = Math.Max(0, color.G - 15);
                    g = (int) Math.Min(255, Math.Pow(g, pow));
                    int b = Math.Max(0, color.B - 15);
                    b = (int) Math.Min(255, Math.Pow(b, pow));
                    Color newColor = Color.FromArgb(r, g, b);
                    resultImg.SetPixel(i, j, newColor);
                }
            }
            return resultImg;
        }

        private static bool isGrey(Color color)
        {
            int avg = (color.R + color.G + color.B)/3;
            int awayValueFromGrey = 0;
            awayValueFromGrey += (avg - color.R) * (avg - color.R);
            awayValueFromGrey += (avg - color.G) * (avg - color.G);
            awayValueFromGrey += (avg - color.B) * (avg - color.B);
            return Math.Sqrt(awayValueFromGrey) < 15;
        }

        static Bitmap RemainFontAsBlackColor(Bitmap originImg, Color fontColor)
        {
            Bitmap resultImg = new Bitmap(originImg);

            for (int x = 0; x < resultImg.Width; x++)
            {
                for (int y = 0; y < resultImg.Height; y++)
                {
                    Color pixelColor = resultImg.GetPixel(x, y);

                    resultImg.SetPixel(x, y, Color.White);

                    // font color
                    if(Math.Abs(fontColor.Sum() - pixelColor.Sum()) < 240)
                    {
                        resultImg.SetPixel(x, y, Color.Black);
                    }

                }
            }

            return resultImg;
        }


        static Bitmap Thresholding(Bitmap originImg)
        {
            Bitmap resultImg = new Bitmap(originImg);

            int[] histogram = new int[256];
            int minGrayValue = 255, maxGrayValue = 0;
            //求取直方图
            for (int i = 0; i < resultImg.Width; i++)
            {
                for (int j = 0; j < resultImg.Height; j++)
                {
                    Color pixelColor = resultImg.GetPixel(i, j);
                    histogram[pixelColor.R]++;
                    if (pixelColor.R > maxGrayValue) maxGrayValue = pixelColor.R;
                    if (pixelColor.R < minGrayValue) minGrayValue = pixelColor.R;
                }
            }
            //迭代计算阀值
            int threshold = -1;
            int newThreshold = (minGrayValue + maxGrayValue) / 2;
            for (int iterationTimes = 0; threshold != newThreshold && iterationTimes < 100; iterationTimes++)
            {
                threshold = newThreshold;
                int lP1 = 0;
                int lP2 = 0;
                int lS1 = 0;
                int lS2 = 0;
                //求两个区域的灰度的平均值
                for (int i = minGrayValue; i < threshold; i++)
                {
                    lP1 += histogram[i] * i;
                    lS1 += histogram[i];
                }
                int mean1GrayValue = (lP1 / lS1);
                for (int i = threshold + 1; i < maxGrayValue; i++)
                {
                    lP2 += histogram[i] * i;
                    lS2 += histogram[i];
                }
                int mean2GrayValue = (lP2 / lS2);
                newThreshold = (mean1GrayValue + mean2GrayValue) / 2;
            }

            //计算二值化
            for (int i = 0; i < resultImg.Width; i++)
            {
                for (int j = 0; j < resultImg.Height; j++)
                {
                    Color pixelColor = resultImg.GetPixel(i, j);

                    if (pixelColor.R >= newThreshold || pixelColor.A == 0) resultImg.SetPixel(i, j, Color.White);
                    else resultImg.SetPixel(i, j, Color.Black);
                }
            }

            return resultImg;
        }

    }
}
