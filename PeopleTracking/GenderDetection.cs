using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace PeopleTracking
{
    public static class GenderDetection
    {
        static List<Image<Gray, byte>> imageList = new List<Image<Gray, byte>>();
        static List<int> labelList = new List<int>();
        static FisherFaceRecognizer faceRecognizer;

        public static int DetectGender(Image<Gray, Byte> inputImage, string trainingSheetPath, string outMatrixPath)
        {
            try
            {
                if(imageList.Count == 0 && labelList.Count == 0)
                    train(trainingSheetPath, outMatrixPath);
                return predict(inputImage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void train(string trainingSheetPath, string outMatrixPath)
        {
            try
            {
                string genderFilesdirectory = Path.GetDirectoryName(trainingSheetPath);
                int imageSize = 64;
                using (StreamReader sr = new StreamReader(trainingSheetPath))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        string[] data = line.Split(';');
                        string filePath = genderFilesdirectory + data[0];
                        imageList.Add(new Image<Gray, byte>(filePath).Resize(imageSize, imageSize, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC));
                        labelList.Add(int.Parse(data[1]));
                        line = sr.ReadLine();
                    }
                }

                faceRecognizer = new FisherFaceRecognizer(imageList.Count, imageList.Count + 75);
                faceRecognizer.Train(imageList.ToArray(), labelList.ToArray());
                faceRecognizer.Save(outMatrixPath);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static int predict(Image<Gray, Byte> inputImage)
        {
            try
            {
                int imageSize = 64;
                Image<Gray, byte> testImage = inputImage.Resize(imageSize, imageSize, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                testImage._EqualizeHist();
                FaceRecognizer.PredictionResult result = faceRecognizer.Predict(testImage);
                return result.Label;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return -1;
        }
    }
}
