using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    public class DiaryEntry
    {
        public DateTime Edited { get; set; }
        public string BlogPageUri { get; set; }
        public string BlogTitle { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string HatenaSyntax { get; set; }
        public string DateId { get; set; }

        public DiaryEntry() { }
        public DiaryEntry(XElement xml)
        {
            this.Edited = DateTime.Parse(xml.Element(WsseAtomClient.AtomPubNs + "edited").Value);
            this.BlogPageUri = xml.Elements(WsseAtomClient.AtomNs + "link")
                .Where(_ => _.Attribute("rel").Value == "alternate")
                .Select(_ => _.Attribute("href").Value)
                .FirstOrDefault();
            this.Title = xml.Element(WsseAtomClient.AtomNs + "title").Value;
            this.Content = xml.Element(WsseAtomClient.AtomNs + "content").Value;
            this.HatenaSyntax = xml.Elements(WsseAtomClient.HatenaNs + "syntax")
                .Select(_ => _.Value)
                .SingleOrDefault();
            this.DateId = Regex.Match(
                    xml.Elements(WsseAtomClient.AtomNs + "link")
                        .Where(_ => _.Attribute("rel").Value == "edit")
                        .Select(_ => _.Attribute("href").Value)
                        .First(),
                    @"(\d+/[a-zA-Z0-9]+|\d+)$"
                )
                .ToString();
        }
    }
}
