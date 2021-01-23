using System;
using System.Windows;
using WebCam_Capture;
using System.Linq;
using System.Windows.Controls;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for CameraCaptureDialog.xaml
    /// </summary>
    public partial class StudentIdCardDialog : Window
    {
        private WebCamCapture webcam;
        private System.Windows.Controls.Image _FrameImage;
        private int FrameNumber = 30;

        public void InitializeWebCam(ref System.Windows.Controls.Image ImageControl)
        {
            webcam = new WebCamCapture();
            webcam.FrameNumber = ((ulong)(0ul));
            webcam.TimeToCapture_milliseconds = FrameNumber;
            webcam.ImageCaptured += new WebCamCapture.WebCamEventHandler(webcam_ImageCaptured);
            _FrameImage = ImageControl;
        }

        public StudentIdCardDialog()
        {
            InitializeComponent();
            InitializeWebCam(ref imgVideo);
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Fatal Error Connecting to Database:\n" + ex.Message, "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }


        private void bntStart_Click(object sender, RoutedEventArgs e)
        {

            webcam.TimeToCapture_milliseconds = FrameNumber;
            webcam.Start(0);
        }

        private void bntStop_Click(object sender, RoutedEventArgs e)
        {
            webcam.Stop();
            imgVideo.Source = null;
        }

        private void bntCapture_Click(object sender, RoutedEventArgs e)
        {
            imgCapture.Source = imgVideo.Source;
            webcam.Stop();
        }

        void webcam_ImageCaptured(object source, WebcamEventArgs e)
        {
            _FrameImage.Source = Utils.LoadBitmap((System.Drawing.Bitmap)e.WebCamImage);
        }


        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            tbDate.Text = "";
            tbId.Text = "";
            tbName.Text = "";
            tbStudentId.Text = "";
            imgCapture.Source = null;
            webcam.Stop();
            imgVideo.Source = null;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedStudent = (from student in Globals.ctx.Students.Include("Enrollments") where student.Id.ToString() == tbStudentId.Text select student).FirstOrDefault<Student>();
                if (selectedStudent == null)
                {
                    MessageBox.Show("Student Id was Not Found in database", "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var lastEnrolled = selectedStudent.Enrollments.ToList().OrderByDescending(enrol => enrol.DateEnrolled).First();
                tbId.Text = selectedStudent.Id.ToString();
                tbName.Text = selectedStudent.StudentName;
                tbDate.Text = lastEnrolled.DateEnrolled.AddYears(1).ToShortDateString();

            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDlg = new PrintDialog();
            printDlg.PrintVisual(gridStudentCard, "Grid Printing.");
        }

    }
}