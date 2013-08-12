//----------------------------------------------------------------------------
//  Author : Piyush Patel
//  https://github.com/piyushkp
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.GPU;
using System.IO;

namespace PeopleTracking
{
    public static class DetectPeople
    {
        private static int frontfaceCount;
        private static int filterWidth;
        private static int filterHeight;

        private static Double ScaleFactor { get; set; }
        public static int MinNeighbors { get; set; }
        private static List<Feed> Feeds { get; set; }


        #region face Detection

        public static List<Feed> Detect(Image<Bgr, Byte> image, out long detectionTime, int OutRectangleSize, bool isAutofaceSize, int FaceRectanglePercentage, int minNeighbors, Double scaleFactor)
        {
            try
            {
                Feeds = new List<Feed>();
                Stopwatch watch;
                watch = Stopwatch.StartNew();

                if (!isAutofaceSize)
                {
                    filterWidth = OutRectangleSize;
                    filterHeight = OutRectangleSize;
                }
                else
                {
                    SetPercentage(FaceRectanglePercentage, image.Width, image.Height);
                }

                // detect front faces from the image
                DetectFrontFace(image);

                // detect side faces from the image
                DetectsideFace(image);

                DetectsideFace(image.Flip(Emgu.CV.CvEnum.FLIP.HORIZONTAL));

                watch.Stop();

                detectionTime = watch.ElapsedMilliseconds;

                return Feeds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void DetectFrontFace(Image<Bgr, Byte> image)
        {
            //return DetectFace(image, "haarcascade_frontalface_default.xml");

            foreach (var item in DetectFace(image, "haarcascade_frontalface_default.xml"))
            {
                Feed _feed = new Feed();
                _feed.X = item.X;
                _feed.Y = item.Y;
                _feed.FaceType = "Front";
                _feed.Width = item.Width;
                _feed.Height = item.Height;
                _feed.FaceID = Guid.NewGuid();

                // Gender detection
                int genderIndex = getGender(image, item);

                if (genderIndex == 0)
                {
                    _feed.Gender = "Female";
                }
                else if (genderIndex == 1)
                {
                    _feed.Gender = "Male";
                }
                else
                {
                    _feed.Gender = "Unknown";
                }

                // Age detection
                int ageIndex = getAge(image, item);

                _feed.Age = GetAgeClassification(ageIndex);
                
                Feeds.Add(_feed);
            }
        }

        private static void DetectsideFace(Image<Bgr, Byte> image)
        {
            //return DetectFace(image, "lbpcascade_profileface.xml");
            foreach (var item in DetectFace(image, "lbpcascade_profileface.xml"))
            {
                Feed _feed = new Feed();
                _feed.X = item.X;
                _feed.Y = item.Y;
                _feed.FaceType = "Side";
                _feed.Width = item.Width;
                _feed.Height = item.Height;
                _feed.FaceID = Guid.NewGuid();

                // Gender detection
                int genderIndex = getGender(image, item);

                if (genderIndex == 0)
                {
                    _feed.Gender = "Female";
                }
                else if (genderIndex == 1)
                {
                    _feed.Gender = "Male";
                }
                else
                {
                    _feed.Gender = "Unknown";
                }

                // Age detection
                int ageIndex = getAge(image, item);

                _feed.Age = GetAgeClassification(ageIndex);

                Feeds.Add(_feed);
            }
        }

        private static Rectangle[] DetectFace(Image<Bgr, Byte> image, string faceFileName)
        {
            try
            {
                if (GpuInvoke.HasCuda)
                {
                    using (GpuCascadeClassifier face = new GpuCascadeClassifier(faceFileName))
                    {
                        using (GpuImage<Bgr, Byte> gpuImage = new GpuImage<Bgr, byte>(image))
                        using (GpuImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                        {
                            Rectangle[] faceRegion = face.DetectMultiScale(gpuGray, 1.1, 10, Size.Empty);

                            return faceRegion;
                        }
                    }
                }
                else
                {
                    //Read the HaarCascade objects
                    using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                    {

                        using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>()) //Convert it to Grayscale
                        {
                            //normalizes brightness and increases contrast of the image
                            gray._EqualizeHist();

                            //Detect the faces  from the gray scale image and store the locations as rectangle
                            //The first dimensional is the channel
                            //The second dimension is the index of the rectangle in the specific channel
                            Rectangle[] facesDetected = face.DetectMultiScale(
                               gray,
                               1.1,
                               10,
                               new Size(filterWidth, filterHeight),
                               Size.Empty);

                            return facesDetected;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void SetPercentage(int percentage, int width, int height)
        {
            if (percentage == 0)
            {
                filterWidth = 100;
                filterHeight = 100;
            }
            else
            {
                filterWidth = (width * percentage) / 100;
                filterHeight = (height * percentage) / 100;
            }
        }

        private static int getGender(Image<Bgr, Byte> image, Rectangle item)
        {
            Image<Bgr, Byte> _croppedImage = image.Copy();

            _croppedImage.ROI = new Rectangle(item.X, item.Y, item.Width, item.Height);

            _croppedImage.Save("D:\\Projects\\Image Processing\\Human tracking\\Database\\Fac_Rec_Database\\face_database" + "\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");

            Image<Gray, Byte> grayImage = _croppedImage.Convert<Gray, Byte>();

            string genderDBdirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Gender_Database";

            string genderTrainingSheetPath = genderDBdirectory + "\\gender_training_data.csv";

            string genderoutMatrixPath = genderDBdirectory + "\\gender_train_matrix.yml";

            return GenderDetection.DetectGender(grayImage, genderTrainingSheetPath, genderoutMatrixPath);
        }

        private static int getAge(Image<Bgr, Byte> image, Rectangle item)
        {
            Image<Bgr, Byte> _croppedImage = image.Copy();

            _croppedImage.ROI = new Rectangle(item.X, item.Y, item.Width, item.Height);

            Image<Gray, Byte> grayImage = _croppedImage.Convert<Gray, Byte>();

            string genderDBdirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Gender_Database";

            string genderTrainingSheetPath = genderDBdirectory + "\\age_training_data.csv";

            string genderoutMatrixPath = genderDBdirectory + "\\age_train_matrix.yml";

            return AgeDetection.DetectAge(grayImage, genderTrainingSheetPath, genderoutMatrixPath);
        }

        private static string GetAgeClassification(int age)
        {
            if (age >= 5 && age <= 15)
                return "Child";
            else if (age >= 16 && age <= 40)
                return "Young Adult";
            else if (age >= 41 && age <= 60)
                return "Adult";
            else if (age >= 61 && age <= 100)
                return "Senior";
            else
                return "Unknown";
        }

        #endregion

    }
}
