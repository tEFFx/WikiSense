using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WikiSense4GH {
    class WikiPage {
        public WikiPage(string _title) {
            title = _title;
        }

        public string title = "";
        public XmlNode xmlNode = null;
        public List<WikiPage> children = new List<WikiPage>();
        public WikiPage parent = null;
    }
}
