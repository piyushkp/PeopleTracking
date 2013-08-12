using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace PeopleTracking
{
    public class AgeDetection
    {
        static List<Image<Gray, byte>> ageImageList = new List<Image<Gray, byte>>();
        static List<int> ageLabelList = new List<int>();
        static FisherFaceRecognizer faceRecognizer;

        public static int DetectAge(Image<Gray, Byte> inputImage, string trainingSheetPath, string outMatrixPath)
        {
            try
            {
                if (ageImageList.Count == 0 && ageLabelList.Count == 0)
                    Agetrain(trainingSheetPath, outMatrixPath);
                return Agepredict(inputImage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void Agetrain(string trainingSheetPath, string outMatrixPath)
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
                        try
                        {
                            string[] data = line.Split(';');
                            string filePath = genderFilesdirectory + data[0];
                            ageImageList.Add(new Image<Gray, byte>(filePath).Resize(imageSize, imageSize, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC));
                            ageLabelList.Add(int.Parse(data[1]));
                            line = sr.ReadLine();
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

                faceRecognizer = new FisherFaceRecognizer(ageImageList.Count, ageImageList.Count + 75);
                faceRecognizer.Train(ageImageList.ToArray(), ageLabelList.ToArray());
                faceRecognizer.Save(outMatrixPath);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static int Agepredict(Image<Gray, Byte> inputImage)
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
