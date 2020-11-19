using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace Stylophone.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new Stylophone.App(), args);
            host.Run();
        }
    }
}
