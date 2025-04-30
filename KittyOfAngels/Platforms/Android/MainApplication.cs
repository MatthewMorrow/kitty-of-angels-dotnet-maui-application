// Version 1.0
using Android.App;
using Android.Runtime;

namespace KittyOfAngels.Platforms.Android
{
    [Application]
    public class MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : MauiApplication(handle, ownership)
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}