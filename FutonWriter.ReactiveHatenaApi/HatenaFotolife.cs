using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    public class HatenaFotolife
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        private static string GetMimeType(string fileName)
        {
            switch (Path.GetExtension(fileName).TrimStart('.').ToLower())
            {
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                case "bmp":
                    return "image/x-bmp";
                default:
                    throw new ArgumentException("対応していないファイルです。");
            }
        }

        public IObservable<FotolifeEntry> Upload(string title, string fileName, string folder = null, FeedGenerator generator = null)
        {
            var xml = new XElement(WsseAtomClient.Atom02SpecNs + "entry",
                new XAttribute(XNamespace.Xmlns + "dc", WsseAtomClient.DublinCoreNs.NamespaceName),
                new XElement(WsseAtomClient.Atom02SpecNs + "title", title),
                new XElement(WsseAtomClient.Atom02SpecNs + "content",
                    new XAttribute("mode", "base64"),
                    new XAttribute("type", GetMimeType(fileName)),
                    Convert.ToBase64String(File.ReadAllBytes(fileName)))
            );

            if (!string.IsNullOrEmpty(folder))
                xml.Add(new XElement(WsseAtomClient.DublinCoreNs + "subject", folder));
            if (generator != null)
                xml.Add(generator.ToXElement(WsseAtomClient.Atom02SpecNs));

            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = "http://f.hatena.ne.jp/atom/post",
                    MethodType = "POST",
                    Content = new XDocument(xml)
                }
                .GetResponseXml()
                .Select(res => new FotolifeEntry(res.Root));
        }

        [Obsolete("はてなからの返値がおかしいのでうまく動かない")]
        public IObservable<FotolifeEntry> GetEntry(string id)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = "http://f.hatena.ne.jp/atom/edit/" + id
                }
                .GetResponseXml()
                .Select(xml => new FotolifeEntry(xml.Root));
        }

        public IObservable<FotolifeEntry> GetEntrys(int page = 1)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = "http://f.hatena.ne.jp/atom/feed",
                    Parameters = { { "page", page } }
                }
                .GetResponseXml()
                .SelectMany(xml => xml.Root.Elements(WsseAtomClient.Atom02SpecNs + "entry"))
                .Select(entry => new FotolifeEntry(entry));
        }

        public IObservable<Unit> EditEntry(string id, string newTitle)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = "http://f.hatena.ne.jp/atom/edit/" + id,
                    MethodType = "PUT",
                    Content = new XDocument(
                        new XElement(WsseAtomClient.Atom02SpecNs + "entry",
                            new XElement(WsseAtomClient.Atom02SpecNs + "title", newTitle)
                        ))
                }
                .GetResponse()
                .Do(res => res.Close())
                .Select(_ => new Unit());
        }

        public IObservable<Unit> DeleteEntry(string id)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = "http://f.hatena.ne.jp/atom/edit/" + id,
                    MethodType = "DELETE"
                }
                .GetResponse()
                .Do(res => res.Close())
                .Select(_ => new Unit());
        }
    }
}
