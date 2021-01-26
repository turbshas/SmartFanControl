﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartFanControl.Config
{
    internal class FileConfigNotifier : IConfigNotifier, IDisposable
    {
        private const string CONFIG_FILE_NAME = "SmartFanControlConfig.json";

        private readonly object _lock;
        private readonly Config _config;
        private bool disposedValue;

        public FileConfigNotifier()
        {
            _lock = new object();
            string config_file = File.ReadAllText(CONFIG_FILE_NAME);
            _config = JsonConvert.DeserializeObject<Config>(config_file);

            if (_config == null)
            {
                _config = new Config();
            }
            if (_config.DeviceConfigs == null)
            {
                _config.DeviceConfigs = new Dictionary<string, IDeviceConfig>();
            }
        }

        public event EventHandler<ConfigChangedEventArgs> ConfigChanged;

        public IDeviceConfig GetConfig(string id)
        {
            lock (_lock)
            {
                _config.DeviceConfigs.TryGetValue(id, out IDeviceConfig deviceConfig);
                return deviceConfig;
            }
        }

        public List<IDeviceConfig> GetDeviceConfigs()
        {
            lock (_lock)
            {
                return _config.DeviceConfigs.Values.ToList();
            }
        }

        public void SetConfig(string id, IDeviceConfig config)
        {
            lock (_lock)
            {
                _config.DeviceConfigs[id] = config;
            }
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs { Id = id, Config = config });
        }

        public void RemoveConfig(string id)
        {
            lock (_lock)
            {
                _config.DeviceConfigs.Remove(id);
            }
            ConfigChanged?.Invoke(this, new ConfigChangedEventArgs { Id = id, Config = null });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    File.WriteAllText(CONFIG_FILE_NAME, JsonConvert.SerializeObject(_config, Formatting.Indented));
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
