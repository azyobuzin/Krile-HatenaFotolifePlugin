using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Azyobuzi.FutonWriter.ExtensionsForRx;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    public class WsseAtomClient
    {
        public WsseAtomClient()
        {
            this.Parameters = new Dictionary<string, object>();
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Uri { get; set; }
        public string MethodType { get; set; }
        public Action<HttpWebRequest> ApplyBeforeRequest { get; set; }
        public Dictionary<string, object> Parameters { get; private set; }
        public XDocument Content { get; set; }
        public Encoding Encoding { get; set; }

        private Random random = new Random();

        private string CreateWsseHeader()
        {
            var nonce = new byte[40];
            this.random.NextBytes(nonce);
            var created = DateTime.UtcNow.ToString("o");
            var sha1 = new SHA1Managed();
            sha1.Initialize();
            var digest = Convert.ToBase64String(
                sha1.ComputeHash(
                    (new[] { nonce }.Concat(new[] { created, this.Password }.Select(Encoding.ASCII.GetBytes)))
                     .SelectMany(_ => _)
                     .ToArray()
                ));
            return string.Format(
                @"UsernameToken Username=""{0}"", PasswordDigest=""{1}"", Nonce=""{2}"", Created=""{3}""",
                this.UserName, digest, Convert.ToBase64String(nonce), created);
        }

        private HttpWebRequest CreateRequest()
        {
            if (string.IsNullOrEmpty(this.UserName) || string.IsNullOrEmpty(this.Password))
                throw new InvalidOperationException("UserNameまたはPasswordを設定してください。");
            if (string.IsNullOrEmpty(this.Uri))
                throw new InvalidOperationException("Uriを設定してください。");

            var req = (HttpWebRequest)WebRequest.Create(
                this.Parameters.Any()
                ? this.Uri + "?" + string.Join("&", this.Parameters.Select(_ => _.Key.UrlEncode() + "=" + _.Value.ToString().UrlEncode()))
                : this.Uri);

            req.Method = this.MethodType ?? "GET";
            req.Headers.Add("X-WSSE", this.CreateWsseHeader());
            if (this.MethodType.IfRange("POST", "PUT") && this.Content != null)
                req.ContentType = "application/x.atom+xml";
            if (this.ApplyBeforeRequest != null) this.ApplyBeforeRequest(req);

            return req;
        }

        public IObservable<WebResponse> GetResponse()
        {
            this.Encoding = this.Encoding ?? Encoding.UTF8;

            var req = this.CreateRequest();

            switch (req.Method)
            {
                case "GET":
                case "DELETE":
                    return Observable.Defer(() => req.GetResponseAsObservable());

                case "POST":
                case "PUT":
                    if (this.Content != null)
                    {
                        using (var mem = new MemoryStream())
                        using (var sw = new StreamWriter(mem, this.Encoding))
                        {
                            this.Content.Save(sw);
                            sw.Flush();
                            return req.UploadDataAsync(mem.ToArray());
                        }
                    }
                    else
                    {
                        return Observable.Defer(() => req.GetResponseAsObservable());
                    }

                default:
                    throw new InvalidOperationException();
            }
        }

        public IObservable<string> GetResponseText()
        {
            return GetResponse().SelectMany(res => res.DownloadStringAsync(this.Encoding));
        }

        public IObservable<XDocument> GetResponseXml()
        {
            return GetResponseText().Select(_ => XDocument.Parse(_));
        }

        #region XmlNamespaces
        public static readonly XNamespace AtomNs = "http://www.w3.org/2005/Atom";
        public static readonly XNamespace AtomPubNs = "http://www.w3.org/2007/app";
        public static readonly XNamespace HatenaNs = "http://www.hatena.ne.jp/info/xmlns#";
        public static readonly XNamespace DublinCoreNs = "http://purl.org/dc/elements/1.1/";
        public static readonly XNamespace Atom02SpecNs = "http://purl.org/atom/ns#";
        #endregion
    }
}
