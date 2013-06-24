using System;
using System.ComponentModel.Composition;
using System.Reflection;
using Acuerdo.Plugin;
using Inscribe.Plugin;

namespace HatenaFotolifePlugin
{
    [Export(typeof(IPlugin))]
    public class EntryPoint : IPlugin
    {
        public string Name
        {
            get { return "HatenaFotolife::Plugin"; }
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public void Loaded()
        {
            UploaderManager.RegisterUploader(new HatenaFotolifeUploader(this));
        }

        private readonly IConfigurator configurationInterface = new Configurator();
        public IConfigurator ConfigurationInterface
        {
            get { return configurationInterface; }
        }
    }
}
