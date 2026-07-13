using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ASID.Edge.Services
{
    public class ZplGenerator
    {
        public string Create(string dataMatrix)
        {
            return
"^XA\r\n^ FO50,50 ^ A0N,40,40 ^ FDASID TEST ^ FS\r\n^ XZ";
        }
    }
}
