
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanGPSLogger
{
    internal interface IFileWriter
    {
        void writeLine(double time, UInt16[] data);

    }
}
