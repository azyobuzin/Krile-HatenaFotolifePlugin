using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    public class FotolifeEntry
    {
        public string Title { get; set; }
        public string PageUri { get; set; }
        public string Id { get; set; }
        public DateTime Issued { get; set; }
        public FeedGenerator Generator { get; set; }
        public string Folder { get; set; }
        public string ImageUri { get; set; }
        public string ThumbnailUri { get; set; }
        public string HatenaSyntax { get; set; }

        public FotolifeEntry() { }
        public FotolifeEntry(XElement xml)
        {
            this.Title = xml.Element(WsseAtomClient.Atom02SpecNs + "title").Value;
            this.PageUri = xml.Elements(WsseAtomClient.Atom02SpecNs + "link")
                .Where(_ => _.Attribute("rel").Value == "alternate")
                .Select(_ => _.Attribute("href").Value)
                .FirstOrDefault();
            this.Id = Regex.Match(this.PageUri, @"\d+$").ToString();
            var issuedXml = xml.Element(WsseAtomClient.Atom02SpecNs + "issued").Value;
            if (!string.IsNullOrWhiteSpace(issuedXml))
                this.Issued = DateTime.Parse(issuedXml);
            this.Generator = xml.Element(WsseAtomClient.Atom02SpecNs + "generator")
                .Null(_ => new FeedGenerator(_));
            this.Folder = xml.Element(WsseAtomClient.DublinCoreNs + "subject")
                .Null(_ => _.Value);
            this.ImageUri = xml.Element(WsseAtomClient.HatenaNs + "imageurl").Value;
            this.ThumbnailUri = xml.Element(WsseAtomClient.HatenaNs + "imageurlsmall").Value;
            this.HatenaSyntax = xml.Element(WsseAtomClient.HatenaNs + "syntax")
                .Null(_ => _.Value);
        }
    }
}
