using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmsProject
{
    class EnrollmentService : IEnrollmentService
    {
        public virtual List<Enrollment> GetEnrollmentList(string sectionId)
        {
            return (from enrollment in Globals.ctx.Enrollments.Include("Student") where enrollment.SectionId.ToString() == sectionId select enrollment).ToList<Enrollment>();
        }
    }
}
