using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.GPU;
using PeopleTracking;
using System.Threading;
using System.Configuration;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.CvEnum;
using System.Xml;
using System.IO;
using System.Timers;

namespace ClientApp
{
    public partial class WindowsImpression : Form
    {
        #region Variables

        private static Image<Bgr, Byte> image; //Read the files as an 8-bit Bgr image  
        private static long detectionTime;

        #endregion

        #region Properties

        public static string ImageLocation { get; set; }
        public static string OutImageLocation { get; set; }
        public static int TimeDiff { get; set; }
        public static string VideoFileLocation { get; set; }
        private Double ScaleFactor { get; set; }
        public int MinNeighbors { get; set; }
        public int OutRectangleSize { get; set; }
        public bool isAutofaceSize { get; set; }
        public int FaceRectanglePercentage { get; set; }
        public List<Feed> Feeds { get; set; }
        public double TotalVideoFrames { get; set; }
        public double VideoFPS { get; set; }
        // video length in second
        public int VideoLength { get; set; }
        // Average time spent in second
        public int AvgTimeSpentOnWindows { get; set; }

        public string GenderTrainingSheetPath { get; set; }
        public string GenderoutMatrixPath { get; set; }

        #endregion

        #region C'tor

        public WindowsImpression()
        {
            InitializeComponent();

            OutImageLocation = ConfigurationSettings.AppSettings["SaveImageLocation"];
            TimeDiff = Convert.ToInt32(ConfigurationSettings.AppSettings["TimeDifferenceToExtractImageFromVideo"]);
            ScaleFactor = Convert.ToDouble(ConfigurationSettings.AppSettings["ScaleFactor"]);
            MinNeighbors = Convert.ToInt32(ConfigurationSettings.AppSettings["MinNeighbors"]);
            OutRectangleSize = Convert.ToInt32(ConfigurationSettings.AppSettings["OutFaceSize"]);
            isAutofaceSize = Convert.ToBoolean(ConfigurationSettings.AppSettings["IsAutoFaceSize"]);
            VideoFileLocation = ConfigurationSettings.AppSettings["InputVideoPath"];
            FaceRectanglePercentage = Convert.ToInt32(ConfigurationSettings.AppSettings["FaceRectanglePercentage"]);

            Thread WIthread = new Thread(new ThreadStart(LoadWindowImpression));
            WIthread.IsBackground = true;
            WIthread.Start();
        }

        private void LoadWindowImpression()
        {
            LoadVideo();
        }

        #endregion

        #region Methods

        #region Load Video

        public void LoadVideo()
        {
            try
            {
                System.Timers.Timer appTimer = new System.Timers.Timer();

                if (!string.IsNullOrEmpty(VideoFileLocation))
                {
                    DirectoryInfo _videoDirectory = new DirectoryInfo(VideoFileLocation);
                    var videofiles = Array.FindAll(Directory.GetFiles(VideoFileLocation), x => !x.ToLower().Contains("processed"));

                    if (videofiles.Length > 0)
                        appTimer.Enabled = false;

                    foreach (var item in videofiles)
                    {
                        FileInfo _videofile = new FileInfo(item);
                        List<Feed> _feedlist = new List<Feed>();
                        string[] names = _videofile.Name.Split('_');

                        if (names.Length > 0)
                        {
                            string cameraID = names[1];
                            string captureStartDate = names[2];
                            if (names.Length > 3)
                            {
                                string _timestamp = names[3].Remove(names[3].IndexOf('.'));
                                string _hour,_minutes,_seconds;
                                if (_timestamp.Length > 1)
                                    _hour = _timestamp.Substring(0, 2);
                                else
                                    _hour = "00";
                                if(_timestamp.Length > 3)
                                    _minutes = _timestamp.Substring(2, 2);
                                else
                                    _minutes = "00";
                                if(_timestamp.Length > 5)
                                    _seconds = _timestamp.Substring(4, 2);
                                else
                                    _seconds = "00";
                                captureStartDate += ":" + _hour + "." + _minutes + "." + _seconds;
                            }
                            else
                                captureStartDate = captureStartDate.Remove(captureStartDate.IndexOf('.')) + ":00.00.00";
                            string captureEndDate = string.Empty;

                            ExtractVideoFrames(item);

                            _videofile.MoveTo(_videoDirectory + "\\processed_" + _videofile.Name);

                            DirectoryInfo _imageDirectory = new DirectoryInfo(OutImageLocation);
                            var imagefiles = Directory.GetFiles(OutImageLocation);

                            int _imgcount = 0;

                            foreach (var img in imagefiles)
                            {
                                FileInfo _imagefile = new FileInfo(img);
                                _imgcount += 1;
                                foreach (var feed in FaceDetection(img.ToString()))
                                {
                                    feed.Time = ConcateTime(captureStartDate, _imgcount * TimeDiff);
                                    _feedlist.Add(feed);
                                }

                                _imagefile.Delete();
                            }

                            GenerateFeedXML(_feedlist, cameraID, captureStartDate, captureEndDate);
                        }
                    }

                    var videofilesCount = Array.FindAll(Directory.GetFiles(VideoFileLocation), x => !x.ToLower().Contains("processed")).Length;
                    if (videofilesCount > 0)
                    { LoadVideo(); }
                    else
                    {
                        appTimer.Elapsed += new ElapsedEventHandler(LoadVideoTimer);
                        appTimer.Interval = Convert.ToInt32(ConfigurationSettings.AppSettings["LoadVideoTimer"]) * 60 * 1000;
                        appTimer.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                
                this.BeginInvoke((Action)(() =>
                {
                    this.Close();
                }));
            }
        }

        public void LoadVideoTimer(object source, ElapsedEventArgs e)
        {
            LoadVideo();
        }

        #endregion

        public static void StartImageForm()
        {
            //ImageViewer.Show(image, String.Format("Completed face and eye detection using in {0} milliseconds", detectionTime));
        }

        private void ExtractVideoFrames(String Filename)
        {
            Capture _capture = new Capture(Filename);
            try
            {
                TotalVideoFrames = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
                VideoFPS = Math.Round(_capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS));
                VideoLength = Convert.ToInt32(Math.Round(TotalVideoFrames / VideoFPS));
                double frameNumber = 0.0;
                IBGFGDetector<Bgr> _detector = new FGDetector<Bgr>(FORGROUND_DETECTOR_TYPE.FGD);

                bool Reading = true;

                while (Reading)
                {
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES, frameNumber);
                    _capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_BRIGHTNESS, 100);
                    Image<Bgr, Byte> frame = _capture.QueryFrame();
                    if (frame != null)
                    {
                        frame = frame.Resize(Convert.ToDouble(ConfigurationSettings.AppSettings["OutFrameResizeScalefactor"]), Emgu.CV.CvEnum.INTER.CV_INTER_AREA);
                        frame._EqualizeHist();

                        frame.Save(OutImageLocation + "\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".jpg");
                    }
                    else
                    {
                        Reading = false;
                    }

                    frameNumber += (VideoFPS * TimeDiff);
                    if (frameNumber > TotalVideoFrames)
                        Reading = false;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _capture.Dispose();
            }
        }

        private List<Feed> FaceDetection(string imagePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(imagePath))
                {
                    Feeds = new List<Feed>();

                    image = new Image<Bgr, byte>(imagePath);

                    Feeds = DetectPeople.Detect(image, out detectionTime, OutRectangleSize, isAutofaceSize, FaceRectanglePercentage, MinNeighbors, ScaleFactor);

                    return Feeds;
                }
                else
                {
                    MessageBox.Show("Please select the input image.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        #region Feed generation

        private int AverageTimeSpent(int peopleCount)
        {
            if (VideoLength > 0)
                return VideoLength / peopleCount;
            return 0;
        }

        private void GenerateFeedXML(List<Feed> feeds, string cameraID, string captureStartDate, string captureEndDate)
        {
            try
            {
                if (feeds != null && feeds.Count > 0)
                {
                    XmlDocument xDoc = new XmlDocument();
                    XmlNode root;
                    XmlNode node;
                    XmlNode child;

                    // Write down the XML declaration
                    XmlDeclaration xmlDeclaration = xDoc.CreateXmlDeclaration("1.0", "utf-8", null);

                    root = xDoc.CreateElement("Feeds");
                    xDoc.AppendChild(root);

                    child = xDoc.CreateElement("AverageTimeSpent");
                    child.InnerText = AverageTimeSpent(feeds.Count).ToString();
                    root.AppendChild(child);

                    child = xDoc.CreateElement("CameraID");
                    child.InnerText = cameraID;
                    root.AppendChild(child);

                    child = xDoc.CreateElement("StoreID");
                    child.InnerText = ConfigurationSettings.AppSettings["StoreID"];
                    root.AppendChild(child);

                    child = xDoc.CreateElement("CaptureStartDate");
                    child.InnerText = DateTime.ParseExact(captureStartDate, "dd-MM-yyyy:HH.mm.ss",
                                       System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy hh:mm:ss");
                    root.AppendChild(child);

                    child = xDoc.CreateElement("CaptureEndDate");
                    child.InnerText = ConcateTime(captureStartDate, VideoLength);
                    root.AppendChild(child);


                    foreach (var feed in feeds)
                    {
                        node = xDoc.CreateElement("Feed");

                        child = xDoc.CreateElement("FaceID");
                        child.InnerText = Guid.NewGuid().ToString();
                        node.AppendChild(child);

                        child = xDoc.CreateElement("X");
                        child.InnerText = feed.X.ToString();
                        node.AppendChild(child);

                        child = xDoc.CreateElement("Y");
                        child.InnerText = feed.Y.ToString();
                        node.AppendChild(child);

                        child = xDoc.CreateElement("Gender");
                        child.InnerText = feed.Gender;
                        node.AppendChild(child);

                        child = xDoc.CreateElement("Age");
                        child.InnerText = feed.Age;
                        node.AppendChild(child);

                        child = xDoc.CreateElement("FaceType");
                        child.InnerText = feed.FaceType;
                        node.AppendChild(child);

                        child = xDoc.CreateElement("Time");
                        child.InnerText = feed.Time;
                        node.AppendChild(child);


                        root.AppendChild(node);
                    }

                    xDoc.Save(ConfigurationSettings.AppSettings["OutXMLLocation"] + "\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".xml");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string FormatXml(string sUnformattedXml)
        {
            //load unformatted xml into a dom
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);

            //will hold formatted xml
            StringBuilder sb = new StringBuilder();

            //pumps the formatted xml into the StringBuilder above
            StringWriter sw = new StringWriter(sb);

            //does the formatting
            XmlTextWriter xtw = null;

            try
            {
                //point the xtw at the StringWriter
                xtw = new XmlTextWriter(sw);
                //we want the output formatted
                xtw.Formatting = Formatting.Indented;
                //get the dom to dump its contents into the xtw 
                xd.WriteTo(xtw);
            }
            finally
            {
                //clean up even if error
                if (xtw != null)
                    xtw.Close();
            }

            //return the formatted xml
            return sb.ToString();
        }

        /// <summary>
        /// calculate the time
        /// </summary>
        /// <param name="startDate">Video start date</param>
        /// <param name="time">given time in seconds</param>
        /// <returns></returns>
        private string ConcateTime(string startDate, int time)
        {
            DateTime endDate = DateTime.ParseExact(startDate, "dd-MM-yyyy:HH.mm.ss",
                                       System.Globalization.CultureInfo.InvariantCulture);

            return endDate.AddSeconds(time).ToString("dd/MM/yyyy hh:mm:ss");            
        }

        #endregion     

        #endregion

        #region Events

        #endregion
    }
}
