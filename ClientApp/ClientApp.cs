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
    public partial class ClientApp : Form
    {
        #region Variables

        #endregion

        #region Properties

        public static string ImageLocation { get; set; }

        public string GenderTrainingSheetPath { get; set; }

        public string GenderoutMatrixPath { get; set; }

        #endregion

        #region C'tor

        public ClientApp()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void getGender()
        {
            Cursor.Current = Cursors.WaitCursor;

            string genderDBdirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Gender_Database";

            GenderTrainingSheetPath = genderDBdirectory + "\\gender_training_data.csv";

            GenderoutMatrixPath = genderDBdirectory + "\\gender_train_matrix.yml";

            Image<Gray, Byte> inputImage = new Image<Gray, byte>(ImageLocation);

            int genderIndex = GenderDetection.DetectGender(inputImage, GenderTrainingSheetPath, GenderoutMatrixPath);

            //if (genderIndex == 0)
            //{
            //    MessageBox.Show("female detected !!!");
            //}
            //else if (genderIndex == 1)
            //{
            //    MessageBox.Show("Male detected !!!");
            //}
            //else
            //{
            //    MessageBox.Show("Application not able to detect !!!");
            //}

            getAge(genderIndex);

            Cursor.Current = Cursors.Default;
        }

        private void getAge(int genderIndex)
        {
            Image<Gray, Byte> inputImage = new Image<Gray, byte>(ImageLocation);

            string ageDBdirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Gender_Database";

            string ageTrainingSheetPath = ageDBdirectory + "\\age_training_data.csv";

            string ageoutMatrixPath = ageDBdirectory + "\\age_train_matrix.yml";

            int ageIndex = AgeDetection.DetectAge(inputImage, ageTrainingSheetPath, ageoutMatrixPath);

            if (genderIndex == 0)
            {
                MessageBox.Show("female detected  with age " + ageIndex.ToString());
            }
            else if (genderIndex == 1)
            {
                MessageBox.Show("Male detected with age " + ageIndex.ToString());
            }
            else
            {
                MessageBox.Show("Application not able to detect !!!");
            }

        }

        #endregion

        #region Events

        //private void btnselectbrowse_Click(object sender, EventArgs e)
        //{
        //    DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
        //    if (result == DialogResult.OK) // Test result.
        //    {
        //        ImageLocation = openFileDialog1.FileName;
        //        textBox1.Text = ImageLocation;
        //    }
        //}

        //private void btnDetect_Click(object sender, EventArgs e)
        //{
        //    getGender();
        //}

        private void btnStartWI_Click(object sender, EventArgs e)
        {
            WindowsImpression _wi = new WindowsImpression();
            _wi.Show();
        }

        #endregion
    }
}
