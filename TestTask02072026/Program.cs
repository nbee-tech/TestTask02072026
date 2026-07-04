using System;
using System.Text;
using Services;

//I chose this program entry over the new one as the
//job description mentiones working with legacy code
//hence the .NET 5 and lower program entry was chosen
namespace TestTask02072026
{
    public class Program
    {
        static void Main(string[] args)
        {
            string arg1 = args[0];

            var compressionOutput = CompressorService.Compress(arg1);
            Console.WriteLine(compressionOutput);
            
        }
    }
}
