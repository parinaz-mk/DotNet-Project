using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmsProject
{
    public class ReportGradeService
    {
        IEnrollmentService service;

        public ReportGradeService(IEnrollmentService service)
        {
            this.service = service;
        }
        
        public double GradesAvg(string sectionId)
        {
            double sum = 0;
            double avg = 0;
            this.service.GetEnrollmentList(sectionId).ForEach(enrollment =>
            {
                if (this.service.GetEnrollmentList(sectionId).Count > 0)
                {
                    int count = this.service.GetEnrollmentList(sectionId).Count;
                    if (enrollment.FinalGrade != null)
                    {
                        sum += enrollment.FinalGrade.Value;
                    }
                    avg = sum / count;
                }
            });
            return avg;
        }
        public double GradesMedian(string sectionId)
        {
            double median = 0;

            if (this.service.GetEnrollmentList(sectionId).Count > 0)
            {
                List<Enrollment> sortedList = this.service.GetEnrollmentList(sectionId).OrderBy(enroll => enroll.FinalGrade).ToList();

                if (sortedList.Count % 2 == 1)
                { // odd
                    median = GradesList(sortedList)[GradesList(sortedList).Count / 2];
                }
                else
                { // even
                    median = (GradesList(sortedList)[GradesList(sortedList).Count / 2] + GradesList(sortedList)[GradesList(sortedList).Count / 2 - 1]) / 2;
                }
            }
            return median;
        }

        private List<double> GradesList(List<Enrollment> list)
        {
            List<double> finalGradesList = new List<double>();

            foreach (Enrollment enroll in list)
            {
                if (enroll.FinalGrade != null)
                {
                    finalGradesList.Add(enroll.FinalGrade.Value);
                }
            }

            return finalGradesList;

        }
    }
}
