using System.Xml.Linq;

namespace Azyobuzi.FutonWriter.ReactiveHatenaApi
{
    public class FeedGenerator
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string Version { get; set; }

        public FeedGenerator() { }

        public FeedGenerator(XElement xml)
        {
            this.Name = xml.Value;
            xml.Attribute("uri").Null(
                uri => this.Uri = uri.Value,
                () => xml.Attribute("url").Null(
                    url => this.Uri = url.Value
                ));
            this.Version = xml.Attribute("version").Null(v => v.Value);
        }

        public XElement ToXElement(XNamespace ns)
        {
            var re = new XElement(ns + "generator", this.Name);
            if (!string.IsNullOrEmpty(this.Uri))
                re.Add(new XAttribute("uri", this.Uri), new XAttribute("url", this.Uri));
            if (!string.IsNullOrEmpty(this.Version))
                re.Add(new XAttribute("version", this.Version));
            return re;
        }
    }
}
