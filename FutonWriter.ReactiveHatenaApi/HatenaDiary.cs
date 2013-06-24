using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Linq;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    public class HatenaDiary
    {
        public string UserName { get; set; }
        public string Password { get; set; }

        private static XDocument CreatePostEntryXml(string title, string content, DateTime? updated)
        {
            var elm = new XElement(WsseAtomClient.Atom02SpecNs + "entry",
                new XElement(WsseAtomClient.Atom02SpecNs + "title", title),
                new XElement(WsseAtomClient.Atom02SpecNs + "content",
                    new XAttribute("type", "text/plain"), content));
            if (updated.HasValue)
                elm.Add(new XElement(WsseAtomClient.Atom02SpecNs + "updated", updated.Value.ToString("o")));
            return new XDocument(elm);
        }

        #region ブログ コレクション
        public IObservable<DiaryEntry> GetEntrys(int page = 1)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format(
                        "http://d.hatena.ne.jp/{0}/atom/blog?page={1}",
                        this.UserName,
                        page)
                }
                .GetResponseXml()
                .Select(xml => xml.Root)
                .Select(xml => Tuple.Create(
                    xml.Element(WsseAtomClient.AtomNs + "title").Value,
                    xml.Elements(WsseAtomClient.AtomNs + "entry")))
                .SelectMany(tuple => tuple.Item2.Select(entry => new DiaryEntry(entry) { BlogTitle = tuple.Item1 }));
        }
             
        public IObservable<DiaryEntry> PostEntry(string title, string content, DateTime? updated = null)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/blog", this.UserName),
                    MethodType = "POST",
                    Content = CreatePostEntryXml(title, content, updated)
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root) { HatenaSyntax = content });
        }

        public IObservable<DiaryEntry> GetEntry(string dateId)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/blog/{1}", this.UserName, dateId)
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root));
        }

        public IObservable<DiaryEntry> EditEntry(string dateId, string title, string content, DateTime? updated = null)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/blog/{1}", this.UserName, dateId),
                    MethodType = "PUT",
                    Content = CreatePostEntryXml(title, content, updated)
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root) { HatenaSyntax = content });
        }

        public IObservable<Unit> DeleteEntry(string dateId)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/blog/{1}", this.UserName, dateId),
                    MethodType = "DELETE"
                }
                .GetResponse()
                .Do(res => res.Close())
                .Select(_ => new Unit());
        }
        #endregion

        #region 下書き コレクション
        public IObservable<DiaryEntry> GetDrafts(int page = 1)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format(
                        "http://d.hatena.ne.jp/{0}/atom/draft?page={1}",
                        this.UserName,
                        page)
                }
                .GetResponseXml()
                .Select(xml => xml.Root)
                .Select(xml => Tuple.Create(
                    xml.Element(WsseAtomClient.AtomNs + "title").Value,
                    xml.Elements(WsseAtomClient.AtomNs + "entry")))
                .SelectMany(tuple => tuple.Item2.Select(entry => new DiaryEntry(entry) { BlogTitle = tuple.Item1 }));
        }

        public IObservable<DiaryEntry> PostDraft(string title, string content, DateTime? updated = null)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/draft", this.UserName),
                    MethodType = "POST",
                    Content = CreatePostEntryXml(title, content, updated)
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root) { HatenaSyntax = content });
        }

        public IObservable<DiaryEntry> GetDraft(string id)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/draft/{1}", this.UserName, id)
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root));
        }

        public IObservable<DiaryEntry> EditDraft(string id, string title, string content, DateTime? updated = null)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/draft/{1}", this.UserName, id),
                    MethodType = "PUT",
                    Content = CreatePostEntryXml(title, content, updated)
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root) { HatenaSyntax = content });
        }

        public IObservable<Unit> DeleteDraft(string id)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/draft/{1}", this.UserName, id),
                    MethodType = "DELETE"
                }
                .GetResponse()
                .Do(res => res.Close())
                .Select(_ => new Unit());
        }

        public IObservable<DiaryEntry> PublishDraft(string id)
        {
            return new WsseAtomClient()
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    Uri = string.Format("http://d.hatena.ne.jp/{0}/atom/draft/{1}", this.UserName, id),
                    MethodType = "PUT",
                    ApplyBeforeRequest = req => req.Headers.Add("X-HATENA-PUBLISH", "1")
                }
                .GetResponseXml()
                .Select(xml => new DiaryEntry(xml.Root));
        }
        #endregion
    }
}
