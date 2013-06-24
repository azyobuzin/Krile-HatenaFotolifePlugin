using System.Windows;
using Acuerdo.Plugin;

namespace HatenaFotolifePlugin
{
    public class Configurator : IConfigurator
    {
        public Window GetTransitionWindow()
        {
            return new ConfigWindow();
        }
    }
}
