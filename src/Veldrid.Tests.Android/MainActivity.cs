using System.Reflection;
using Android.Content.PM;
using Xunit;
using Xunit.Runners.UI;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Veldrid.Tests.Android
{
    [Activity(
        Label = "xUnit Android Runner",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize,
        Theme = "@android:style/Theme.Material.Light"
    )]
    public class MainActivity : RunnerActivity
    {
        public delegate void LifecycleHandler();
        public event LifecycleHandler? Paused;
        public event LifecycleHandler? Resumed;

        public MainActivity()
        {
            AndroidDeviceCreator.Activity = this;
        }

        protected override void OnCreate(Bundle bundle)
        {
            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            // or in any reference assemblies

            //AddTestAssembly(typeof(PortableTests).Assembly);
            // or in any assembly that you load (since JIT is available)

#if false
            // you can use the default or set your own custom writer (e.g. save to web site and tweet it ;-)
            Writer = new TcpTextWriter ("10.0.1.2", 16384);
            // start running the test suites as soon as the application is loaded
            AutoStart = true;
            // crash the application (to ensure it's ended) and return to springboard
            TerminateAfterExecution = true;
#endif
            // you cannot add more assemblies once calling base
            base.OnCreate(bundle);
        }

        protected override void OnPause()
        {
            base.OnPause();
            Paused?.Invoke();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Resumed?.Invoke();
        }
    }
}
