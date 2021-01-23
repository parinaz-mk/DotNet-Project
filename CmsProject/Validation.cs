using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace CmsProject
{
    public partial class Student
    {
        public string StudentName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }

    public partial class Teacher
    {
        public string TeacherName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }

    public partial class Section 
    {
        public int Spot { get => Enrollments.Count(); }
    }

    public partial class CmsprojectDbConnection : DbContext
    {

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(" The validation errors are: \n", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }
    }

    public partial class Enrollment : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (FinalGrade < 1 || FinalGrade > 100)
            {
                yield return new ValidationResult(
                    "Grade must be between 1 and 100",
                    new[] { nameof(FinalGrade) });
            }
        }
    }
}