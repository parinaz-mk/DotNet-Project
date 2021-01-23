using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CmsProject
{
    internal class InvalidValueException : Exception
    {
        public InvalidValueException(string message) : base(message)
        {
        }

    }
}
