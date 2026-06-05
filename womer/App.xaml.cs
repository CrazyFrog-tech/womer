using Microsoft.Extensions.DependencyInjection;

namespace womer
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            InitializeComponent();
            _services = services;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var appShell = _services.GetRequiredService<AppShell>();
            return new Window(appShell);
        }
    }
}