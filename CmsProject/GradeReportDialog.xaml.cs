using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;

namespace CmsProject
{
    /// <summary>
    /// Interaction logic for GradeReportDialog.xaml
    /// </summary>
    public partial class GradeReportDialog : Window
    {
        ReportGradeService gradeService;
        IEnrollmentService enrollmentService;
        public GradeReportDialog()
        {
            InitializeComponent();
            try
            {
                Globals.ctx = new CmsprojectDbConnection();
                enrollmentService = new EnrollmentService();
                gradeService = new ReportGradeService(enrollmentService);
            }
            catch (SystemException ex)
            {
                MessageBox.Show("Fatal Error Connecting to Database:\n" + ex.Message, "Database Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void LoadChart()
        {

            List<Bar> _bar = new List<Bar>();
            GetEnrollmentList(tbSectionId.Text).ForEach(enrollment =>
            {
                if (enrollment.FinalGrade != null)
                {
                    _bar.Add(new Bar() { BarFirstName = enrollment.Student.FirstName, BarLastName= enrollment.Student.LastName , Value = (enrollment.FinalGrade.Value) });
                }
            });
            this.DataContext = new RecordCollection(_bar);
            lblAverage.Content = String.Format("Average: {0:.##}", gradeService.GradesAvg(tbSectionId.Text));
            lblMedian.Content = "Median: " + gradeService.GradesMedian(tbSectionId.Text);
        }

        public List<Enrollment> GetEnrollmentList(string sectionId)
        {
            List<Enrollment> enrollmentList = new List<Enrollment>();
            try
            {
                enrollmentList = enrollmentService.GetEnrollmentList(sectionId);

            }
            catch (SystemException ex)
            {
                MessageBox.Show("Database Error!:\n" + ex.Message, "Database Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return enrollmentList;
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            LoadChart();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            tbSectionId.Text = "";
            LoadChart();
            lblAverage.Content = "Average: ";
            lblMedian.Content = "Median: ";
        }        
    }

    class RecordCollection : ObservableCollection<Record>
    {
        public RecordCollection(List<Bar> barvalues)
        {
            Random rand = new Random();
            BrushCollection brushcoll = new BrushCollection();

            foreach (Bar barval in barvalues)
            {
                int num = rand.Next(brushcoll.Count / 3);
                Add(new Record(barval.Value, brushcoll[num], barval.BarFirstName , barval.BarLastName));
            }
        }

    }

    class BrushCollection : ObservableCollection<Brush>
    {
        public BrushCollection()
        {
            Type _brush = typeof(Brushes);
            PropertyInfo[] props = _brush.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                Brush _color = (Brush)prop.GetValue(null, null);
                if (_color != Brushes.LightSteelBlue && _color != Brushes.White &&
                     _color != Brushes.WhiteSmoke && _color != Brushes.LightCyan &&
                     _color != Brushes.LightYellow && _color != Brushes.Linen)
                    Add(_color);
            }
        }
    }

    class Bar
    {
        public string BarFirstName { set; get; }

        public string BarLastName { set; get; }

        public double Value { set; get; }
    }

    class Record : INotifyPropertyChanged
    {
        public Brush Color { set; get; }

        public string FirstName { set; get; }

        public string LastName { set; get; }


        private double _data;
        public double Data
        {
            set
            {
                if (_data != value)
                {
                    _data = value;

                }
            }
            get
            {
                return _data;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Record(double value, Brush color, string firstName, string lastName)
        {
            Data = value;
            Color = color;
            FirstName = firstName;
            LastName = lastName;
        }

        protected void PropertyOnChange(string propname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }
    }
}

