using CmsProject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace UnitTestCmsProject
{
    [TestClass]
    public class UnitTestGradesService
    {
        Mock<IEnrollmentService> mock ;
        List<Enrollment> listEnrollment = new List<Enrollment>();
        ReportGradeService classTobeTested;

        [TestInitialize]
        public void TestInitialize()
        {
             mock = new Mock<IEnrollmentService>();
             classTobeTested = new ReportGradeService(mock.Object);
            listEnrollment.Add(new Enrollment() { Id = 1, StudentId = 1, SectionId = 1, FinalGrade = 18 });
            listEnrollment.Add(new Enrollment() { Id = 2, StudentId = 2, SectionId = 1, FinalGrade = 20 });
            listEnrollment.Add(new Enrollment() { Id = 3, StudentId = 3, SectionId = 1, FinalGrade = 13 });
            listEnrollment.Add(new Enrollment() { Id = 4, StudentId = 4, SectionId = 1, FinalGrade = 16 });
        }

        [TestMethod]
        public void TestMethodGetAverage()
        {

            mock.Setup(report => report.GetEnrollmentList("1")).Returns(listEnrollment);
            double avg = classTobeTested.GradesAvg("1");
            Assert.AreEqual(avg , 16.75);
        }

        [TestMethod]
        public void TestMethodGetMedian()
        {
            mock.Setup(report => report.GetEnrollmentList("1")).Returns(listEnrollment);
            double median = classTobeTested.GradesMedian("1");
            Assert.AreEqual(median, 17);
        }



    }
}
