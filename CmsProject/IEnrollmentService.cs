using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmsProject
{
    public interface IEnrollmentService
    {
        List<Enrollment> GetEnrollmentList(string sectionId);

    }


}
