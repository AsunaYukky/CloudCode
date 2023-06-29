using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CloudCode_example.Classes
{
    public class ImagePreprocessor
    {
        public static List<Point> DetectCloudCodeCorners(Image<Bgr, byte> inputImage)
        {
            // Конвертируем изображение в черно-белый формат
            Image<Gray, byte> grayImage = inputImage.Convert<Gray, byte>();

            // Применяем пороговое значение
            Image<Gray, byte> binaryImage = grayImage.ThresholdBinary(new Gray(100), new Gray(255));

            // Ищем контуры на изображении
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(binaryImage, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // Ищем контур, который наиболее подходит под форму прямоугольника
            double maxArea = 0;
            Emgu.CV.Util.VectorOfPoint cloudCodeContour = null;
            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if (area > maxArea)
                {
                    maxArea = area;
                    cloudCodeContour = contours[i];
                }
            }

            if (cloudCodeContour == null)
            {
                throw new Exception("CloudCode contour not found");
            }

            // Ищем углы прямоугольника
            RotatedRect rotatedRect = CvInvoke.MinAreaRect(cloudCodeContour);
            return rotatedRect.GetVertices().Select(p => new Point((int)p.X, (int)p.Y)).ToList();
        }

        public static Image<Bgr, byte> PreprocessImage(string imagePath, List<Point> cloudCodeCorners)
        {
            // Загрузка изображения
            Image<Bgr, byte> inputImage = new Image<Bgr, byte>(imagePath);

            // Применение размытия для устранения шума
            Image<Bgr, byte> blurredImage = inputImage.SmoothGaussian(5);

            // Применение порогового значения для выделения более ярких областей
            Image<Bgr, byte> equalizedImage = new Image<Bgr, byte>(blurredImage.Width, blurredImage.Height, new Bgr(0, 0, 0));
            for (int i = 0; i < 3; i++)
            {
                Image<Gray, byte> channel = blurredImage[i];
                CvInvoke.EqualizeHist(channel, channel);
                equalizedImage[i] = channel;
            }

            // Коррекция перспективы
            PointF[] srcPoints = cloudCodeCorners.Select(p => new PointF(p.X, p.Y)).ToArray();
            PointF[] dstPoints = new PointF[]
            {
            new PointF(0, 0),
            new PointF(219, 0),
            new PointF(219, 219),
            new PointF(0, 219)
            };

            var homographyMatrix = CvInvoke.GetPerspectiveTransform(srcPoints, dstPoints);
            Image<Bgr, byte> correctedImage = new Image<Bgr, byte>(220, 220);
            CvInvoke.WarpPerspective(equalizedImage, correctedImage, homographyMatrix, new Size(220, 220));

            return correctedImage;
        }
    }

    public class ImageConverter
    {
        public static Image<Bgr, byte> ConvertFromNetImage(string imagePath)
        {
            Bitmap bitmap = new(imagePath);
            // Преобразуем в Image<Bgr, byte> для Emgu.CV
            Image<Bgr, byte> emguImage = bitmap.ToImage<Bgr, byte>();

            return emguImage;
        }
    }
}
