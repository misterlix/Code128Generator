using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code128Generator
{
    class Program
    {

        static readonly Code128Encoder _code128Encoder = new Code128Encoder();


        [STAThread]
        static void Main(string[] args)
        {
            var input = "Hello\tTabs";

            var encoded = _code128Encoder.Encode(input);
            System.Windows.Forms.Clipboard.SetText(encoded);
        }



    }
}
