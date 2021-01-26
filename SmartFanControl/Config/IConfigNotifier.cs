using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Config
{
    internal interface IConfigNotifier : IDisposable
    {
        event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        IDeviceConfig GetConfig(string id);

        List<IDeviceConfig> GetDeviceConfigs();

        void SetConfig(string id, IDeviceConfig config);

        void RemoveConfig(string id);
    }

    internal class ConfigChangedEventArgs
    {
        public string Id { get; set; }

        public IDeviceConfig Config { get; set; }
    }
}
