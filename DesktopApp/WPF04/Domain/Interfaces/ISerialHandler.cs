using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF04.Domain.Interfaces
{
    public interface ISerialHandler
    {
        public bool OpenPort();

        public string ExecuteSJS(string statementType, object statementPayload);

    }
}
