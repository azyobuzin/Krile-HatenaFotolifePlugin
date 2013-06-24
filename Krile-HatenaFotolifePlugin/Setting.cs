using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Inscribe.Storage;

namespace HatenaFotolifePlugin
{
    public class Setting
    {
        private static string GetSettingFile()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "HatenaFotolife.xml");
        }

        private static Setting instance;
        public static Setting Instance
        {
            get
            {
                if (instance == null)
                {
                    try
                    {
                        using (var sr = new StreamReader(GetSettingFile()))
                        {
                            instance = new XmlSerializer(typeof(Setting)).Deserialize(sr) as Setting;
                        }
                    }
                    catch
                    {
                        instance = new Setting();
                        instance.Confirm = true;
                    }
                }
                return instance;
            }
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string DefaultFolder { get; set; }
        public string DefaultTitle { get; set; }
        public bool Confirm { get; set; }

        public bool IsAuthorized()
        {
            return !(string.IsNullOrWhiteSpace(this.UserName) || string.IsNullOrWhiteSpace(Password));
        }

        public void Save()
        {
            try
            {
                using (var sw = new StreamWriter(GetSettingFile()))
                {
                    new XmlSerializer(typeof(Setting)).Serialize(sw, this);
                }
            }
            catch (Exception ex)
            {
                ExceptionStorage.Register(ex, ExceptionCategory.PluginError, "HatenaFotolife::Pluginの設定を保存できませんでした。", this.Save);
            }
        }
    }
}
