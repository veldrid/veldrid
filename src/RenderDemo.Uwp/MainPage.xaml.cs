// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using System.Threading.Tasks;
using Veldrid;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RenderDemo.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

        }

        private void SwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var renderContext = UwpRenderContext.CreateFromSwapChainPanel(SwapChainPanel);
            Task.Factory.StartNew(() =>
            {
                Veldrid.RenderDemo.RenderDemo.RunDemo(renderContext);
            }, TaskCreationOptions.LongRunning);
        }
    }
}
