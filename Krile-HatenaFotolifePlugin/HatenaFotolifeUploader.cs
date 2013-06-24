using System;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using Acuerdo.External.Uploader;
using Acuerdo.Plugin;
using Azyobuzi.FutonWriter.ReactiveHatenaApi;
using Dulcet.Twitter.Credential;

namespace HatenaFotolifePlugin
{
    public class HatenaFotolifeUploader : IUploader
    {
        public HatenaFotolifeUploader(IPlugin owner)
        {
            this.Owner = owner;
        }

        private IPlugin Owner { get; set; }

        public string UploadImage(OAuth credential, string path, string comment)
        {
            if (!Setting.Instance.IsAuthorized())
                throw new KrilePluginException(this.Owner, "設定画面からユーザー名とパスワードを設定してください。");

            var title = Setting.Instance.DefaultTitle;
            var folder = Setting.Instance.DefaultFolder;

            if (Setting.Instance.Confirm)
            {
                UploadWindowViewModel vm = null;
                var t = new Thread(() =>
                {
                    var dialog = new UploadWindow();
                    var result = dialog.ShowDialog();
                    if (result.HasValue && result.Value)
                        vm = dialog.DataContext as UploadWindowViewModel;
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();
                
                if (vm == null)
                    throw new KrilePluginException(this.Owner, "アップロードがキャンセルされました。");

                title = vm.Title;
                folder = vm.Folder;
            }

            var entry = new HatenaFotolife()
            {
                UserName = Setting.Instance.UserName,
                Password = Setting.Instance.Password
            }.Upload(title, path, folder).ToTask().Result;

            return entry.PageUri;            
        }

        public string ServiceName
        {
            get { return "はてなフォトライフ"; }
        }

        public bool IsResolvable(string url)
        {
            return Regex.IsMatch(url, @"^http://f\.hatena\.ne\.jp/(\w+)/(\d+)(?:\?.*)?$");
        }

        public string Resolve(string url)
        {
            return "http://img.azyobuzi.net/api/redirect.json?uri=" + Uri.EscapeDataString(url);
        }
    }
}
