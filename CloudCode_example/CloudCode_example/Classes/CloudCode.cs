using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using static CloudCode_example.Classes.CloudCode;
using static CloudCode_example.Classes.HammingCoder;

namespace CloudCode_example.Classes
{
    internal class CloudCode
    {
        private const int PIXEL_DIAMETER = 2;
        private const int FIRST_BIT_DIAMETER = (int)(PIXEL_DIAMETER * 1.5);
        private const int CANVAS_SIZE = 220;
        private const int DATA_AREA_SIZE = 200;
        private const int BORDER_WIDTH = 2;
        private const int EMPTY_SPACE = 4;
        private const int BORDER_START = EMPTY_SPACE;
        private const int DATA_START = BORDER_START + BORDER_WIDTH + EMPTY_SPACE;

        public class Generator
        {
            private Random _rand = new Random();
            //private HammingEncoder _encoder = new HammingEncoder();

            public Bitmap Generate(string message)
            {
                //byte[] messageBytes = _encoder.Encode(Encoding.UTF8.GetBytes(message));
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                int redundancy = (int)(messageBytes.Length * 0.05);
                messageBytes = messageBytes.Concat(new byte[redundancy]).ToArray();

                int numOfPixelsPerRow = DATA_AREA_SIZE / PIXEL_DIAMETER;

                Bitmap bitmap = new Bitmap(CANVAS_SIZE, CANVAS_SIZE, PixelFormat.Format32bppArgb);

                List<Point> points = new List<Point>();
                for (int i = 0; i < numOfPixelsPerRow; i++)
                {
                    for (int j = 0; j < numOfPixelsPerRow; j++)
                    {
                        points.Add(new Point(DATA_START + i * PIXEL_DIAMETER, DATA_START + j * PIXEL_DIAMETER));
                    }
                }

                Shuffle(points);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    Brush whiteBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
                    g.FillRectangle(whiteBrush, new Rectangle(0, 0, CANVAS_SIZE, CANVAS_SIZE));

                    Pen borderPen = new Pen(Color.Black, BORDER_WIDTH);
                    g.DrawRectangle(borderPen, BORDER_START, BORDER_START, DATA_AREA_SIZE + 2 * EMPTY_SPACE, DATA_AREA_SIZE + 2 * EMPTY_SPACE);

                    int dataSize = messageBytes.Length;
                    int halfDataSize = dataSize / 2;

                    for (int i = 0; i < dataSize; i++)
                    {
                        int x = points[i].X;
                        int y = points[i].Y;

                        Color pixelColor;
                        int pixelDiameter = PIXEL_DIAMETER;

                        if (i == 0)
                        {
                            // First bit will be bright purple and larger
                            pixelColor = Color.FromArgb(255, 255, 0, 255);
                            pixelDiameter = FIRST_BIT_DIAMETER;
                        }
                        else if (i < halfDataSize)
                        {
                            pixelColor = Color.FromArgb(255, _rand.Next(256), _rand.Next(256), messageBytes[i]);
                        }
                        else
                        {
                            pixelColor = Color.FromArgb(_rand.Next(128), _rand.Next(256), _rand.Next(256), messageBytes[i]);
                        }

                        Brush pixelBrush = new SolidBrush(pixelColor);
                        g.FillEllipse(pixelBrush, x, y, pixelDiameter, pixelDiameter);
                    }
                }

                return bitmap;
            }

            private void Shuffle<T>(IList<T> list)
            {
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = _rand.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }

        public class Decoder
        {
            //private HammingDecoder _decoder = new HammingDecoder();

            public string Decode(Bitmap bitmap)
            {
                List<byte> messageBytes = new List<byte>();

                int numOfPixelsPerRow = DATA_AREA_SIZE / PIXEL_DIAMETER; // Actual pixels per row in data area

                bool firstBitFound = false;

                for (int i = 0; i < numOfPixelsPerRow; i++)
                {
                    for (int j = 0; j < numOfPixelsPerRow; j++)
                    {
                        int x = DATA_START + i * PIXEL_DIAMETER;
                        int y = DATA_START + j * PIXEL_DIAMETER;

                        Color pixelColor = bitmap.GetPixel(x, y);

                        if (!firstBitFound && pixelColor.R == 255 && pixelColor.G == 0 && pixelColor.B == 255)
                        {
                            // Detect the first bit
                            messageBytes.Add(pixelColor.B);
                            firstBitFound = true;
                        }
                        else if (pixelColor.A >= 128) // Non-transparent pixels
                        {
                            messageBytes.Add(pixelColor.B);
                        }
                    }
                }

                //byte[] decodedBytes = _decoder.Decode(messageBytes.ToArray());
                byte[] decodedBytes = messageBytes.ToArray();

                return Encoding.UTF8.GetString(decodedBytes);
            }
        }
    }
}

