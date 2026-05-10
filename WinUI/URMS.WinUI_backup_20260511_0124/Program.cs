using Microsoft.UI.Dispatching;
using WinRT;

namespace URMS.WinUI
{
    public static class Program
    {
        [global::System.STAThread]
        static void Main(string[] args)
        {
            global::WinRT.ComWrappersSupport.InitializeComWrappers();
            global::Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var ctx = new DispatcherQueueSynchronizationContext(
                    DispatcherQueue.GetForCurrentThread());
                global::System.Threading.SynchronizationContext.SetSynchronizationContext(ctx);
                new App();
            });
        }
    }
}
