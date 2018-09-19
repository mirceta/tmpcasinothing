using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.IO;

namespace ThunderpickAspnet
{
    class Program
    {

        #region // configuration //
        static int xsize = 325, ysize = 10;
        static int xoffset = 790;
        static int yoffset = 162;

        static string screenShotPath = @"C:\Users\km\Desktop\tmpimages\image";
        static string piecePath = @"C:\Users\km\Desktop\tmpimages\blimage";
        static int imgcntr = 0;
        #endregion

        #region // parameters //
        static Vector3 GRAY = new Vector3(97, 114, 135);
        static Vector3 BLUE = new Vector3(65, 173, 209);
        static Vector3 RED = new Vector3(219, 110, 107);
        static Vector3 YELLOW = new Vector3(220, 190, 39);
        
        #endregion 

        static void Main(string[] args)
        {
            // create directories
            string basedir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string imagesfolder = basedir + @"\tmpimages";
            screenShotPath = basedir + @"\tmpimages\image";
            piecePath = basedir + @"\tmpimages\blimage";
            if (!System.IO.Directory.Exists(imagesfolder))
            {
                System.IO.Directory.CreateDirectory(imagesfolder);
                System.IO.Directory.CreateDirectory(screenShotPath);
                System.IO.Directory.CreateDirectory(piecePath);
            }

            Thread.Sleep(3000);

            SaveScreenChunk();
            string oldString = ImageToString();
            Console.WriteLine(oldString);
            int maxoccurrence = 0;


            do
            {
                imgcntr++;
                Thread.Sleep(30000);
                SaveScreenChunk();
                string newString = ImageToString();
                string cmpString = oldString.Substring(oldString.Length - 24);

                int idx = newString.IndexOf(cmpString);
                oldString += newString.Substring(idx + 24);

                Console.WriteLine(oldString);

                // average
                List<int> currrun = new List<int>();
                for (int i = 0; i < oldString.Length; i++) {
                    if (oldString[i] == 'Y') {
                        currrun.Add(i);
                    }
                }
                List<double> kurac = new List<double>();
                for (int i = 1; i < currrun.Count; i++) {
                    int curr = currrun[i] - currrun[i - 1];
                    kurac.Add(curr);
                    maxoccurrence = (maxoccurrence > curr) ? maxoccurrence : curr;
                }

                if (kurac.Count > 0)
                    Console.WriteLine("Average: {0}", kurac.Average());

                // last
                string tmp = oldString;
                tmp = new string(tmp.Reverse().ToArray());
                int currmax = tmp.IndexOf("Y");
                Console.WriteLine("Last: {0}", currmax);

                // max
                Console.WriteLine("Max: {0}", maxoccurrence);


            } while (true);
            

        }

        #region // auxiliary //
        static void SaveScreenChunk() {
            
            ScreenCapture sc = new ScreenCapture();
            // capture entire screen, and save it to a file
            Image img = sc.CaptureScreen();

            string file = MakeFileName(screenShotPath);
            img.Save(file);
            Bitmap bmp = new Bitmap(file);



            Bitmap bmpPiece = new Bitmap(xsize, ysize);


            for (int i = 0; i < xsize; i++)
            {
                for (int j = 0; j < ysize; j++)
                {
                    Color c = bmp.GetPixel(i + xoffset, j + yoffset);
                    bmpPiece.SetPixel(i, j, c);
                }
            }

            bmpPiece.Save(MakeFileName(piecePath));
        }

        /*
        Will split the bar into 27 section and take the center pixel of each section. It will then compute the color
        and return it in a string.
        */
        static string ImageToString() {
            Bitmap bmp = new Bitmap(MakeFileName(piecePath));
            Bitmap[] arrBmp = new Bitmap[27];

            int chunkSize = xsize / 27;

            string result = "";

            for (int i = 0; i < 27; i++) {

                double totalPixels = (double) chunkSize * ysize;
                

                Color c = bmp.GetPixel(i * chunkSize + chunkSize / 2, ysize / 2);
                Vector3 centerPx = new Vector3(c.R, c.G, c.B);

                result += GetClosestColor(centerPx);
            }

            return result;
        }

        static string GetClosestColor(Vector3 color) {

            double[] dists = new double[4];
            dists[0] = L2Distance(color, GRAY);
            dists[1] = L2Distance(color, BLUE);
            dists[2] = L2Distance(color, RED);
            dists[3] = L2Distance(color, YELLOW);


            int result = Array.IndexOf(dists, dists.Min());

            switch (result) {
                case 0:
                    return "G";
                case 1:
                    return "B";
                case 2:
                    return "R";
                case 3:
                    return "Y";
            }

            return "EPICFAIL";
        }

        static double L2Distance(Vector3 v1, Vector3 v2) {
            return Math.Sqrt(Math.Pow(v1.X - v2.X, 2) + Math.Pow(v1.Y - v2.Y, 2) + Math.Pow(v1.Z - v2.Z, 2));
        }

        static string MakeFileName(string file) {
            return file + imgcntr.ToString() + ".jpg";
        }
        #endregion
    }
}
