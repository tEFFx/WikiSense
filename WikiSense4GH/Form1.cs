using System;
using System .Collections .Generic;
using System .ComponentModel;
using System .Data;
using System .Drawing;
using System .Linq;
using System .Text;
using System .Threading .Tasks;
using System .Windows .Forms;
using System .Xml;

namespace WikiSense4GH {
    public partial class Form1 : Form {
        private XmlDocument xmlFile = new XmlDocument();
        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        private WikiPage indexPage = new WikiPage("Wiki Index");

        public Form1() {
            InitializeComponent();

            openFileDialog.Filter = "XML-file (*.xml)|*.xml";
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
        }

        /// <summary>
        /// Set all items in a CheckedListBox checked/unchecked
        /// </summary>
        /// <param name="_listBox">ListBox to perform operation on</param>
        /// <param name="_select">Selected?</param>
        private void CheckListBoxItems(CheckedListBox _listBox, bool _select) {
            if(_listBox.Items.Count == 0) {
                status.Text = _select ? "Nothing to select." : "Nothing to deselect.";
            }

            for(int i = 0 ; i < _listBox.Items.Count ; i++ ) {
                _listBox.SetItemChecked(i, _select);
            }
        }

        /// <summary>
        /// Opens a XML-file and loads the first member names in order to easily exclude/include stuff
        /// </summary>
        private void openIntelliSenseXMLfileToolStripMenuItem_Click(object sender, EventArgs e) {
            if(openFileDialog.ShowDialog() == DialogResult.OK) {
                try {
                    xmlFile.Load(openFileDialog.FileName);
                    typeList.Items.Clear();
                    foreach(XmlNode node in xmlFile.GetElementsByTagName("member")) {
                        string memberName = XmlHelper.GetMemberName(node, 0);

                        if(!typeList.Items.Contains(memberName)) {
                            typeList.Items.Add(memberName, true);
                        }
                    }

                    status.Text = "Found " + typeList.Items.Count + " namespaces/classes!";
                } catch(Exception err) {
                    MessageBox.Show("Unable to load XML-file!\n" + "Exception: " + err, "ERROR!");
                }
            }
        }

        /// <summary>
        /// Sort and generate GitHub wiki-files.
        /// </summary>
        private void exportGitHubWikiToolStripMenuItem_Click(object sender, EventArgs e) {
            if(folderBrowserDialog.ShowDialog() == DialogResult.OK) {
                indexPage.children.Clear();

                //sort all members into a tree, should probably be in a thread/task for larger xml-files
                foreach ( XmlNode node in xmlFile.GetElementsByTagName("member") ) {
                    string member = XmlHelper.GetMemberName(node, 0);
                    if ( typeList.CheckedItems.Contains(member) ) {
                        WikiPage last = indexPage;
                        int members = XmlHelper.CountMemberNames(node);
                        for(int i = 0; i < members; i++) {
                            string name = XmlHelper.GetMemberName(node, i);
                            if(!last.children.Any(x => x.title == name)) {
                                WikiPage created = new WikiPage(name);
                                created.parent = last;
                                if(members - 1 == i) {
                                    created.xmlNode = node;
                                }
                                last.children.Add(created);
                                last = created;
                            } else if(members - 1 == i) {
                                last.children.Find(x => x.title == name).xmlNode = node;
                            } else {
                                last = last.children.Find(x => x.title == name);
                            }
                        }
                    }
                }

                //do a depth-first search in order to generate the pages
                int count = 0;
                Stack<WikiPage> pageQueue = new Stack<WikiPage>();
                pageQueue.Push(indexPage);
                while(pageQueue.Count > 0) {
                    WikiPage next = pageQueue.Pop();
                    bool emptyPage = true;

                    //print page title
                    string pageContent = "##**" + next.title + "**##\n";
                    if(next.parent != null) {
                        string thisTitle = next.title;
                        if(thisTitle.Contains(next.parent.title)) {
                            thisTitle = thisTitle.Remove(thisTitle.IndexOf(next.parent.title), next.parent.title.Length);
                        }
                        pageContent = "##[" + next.parent.title + "](" + System.Net.WebUtility.UrlEncode(next.parent.title) + ")"; //escape for link safety
                        if(next.parent.title == indexPage.title) {
                            pageContent += " - **" + thisTitle + "**##\n";
                        } else {
                            pageContent += "**" + thisTitle + "**##\n";
                        }
                    }
                    pageContent += "\n";
                    
                    //print page info
                    if(next.xmlNode != null && next.xmlNode.HasChildNodes) {
                        try {
                            XmlDocument nodeInfo = new XmlDocument();
                            nodeInfo.LoadXml("<properties>" + next.xmlNode.InnerXml + "</properties>");

                            XmlNodeList summary = nodeInfo.GetElementsByTagName("summary");
                            if ( summary.Count > 0 && summary[0].InnerText.Length > 0 ) {
                                pageContent += "###Summary###\n" + XmlHelper.RemoveWhitespace(summary [ 0 ].InnerText) + "\n";
                                emptyPage = false;
                            }
                            pageContent += "\n";

                            XmlNodeList param = nodeInfo.GetElementsByTagName("param");
                            if ( param.Count > 0 ) {
                                pageContent += "###Params###\n";
                                foreach ( XmlNode node in param ) {
                                    pageContent += "* " + node.Attributes [ "name" ].Value + "\n";
                                    if ( node.InnerText.Length > 0 ) {
                                        pageContent += "    * " + node.InnerText + "\n";
                                    }
                                }
                                emptyPage = false;
                            }
                            pageContent += "\n";

                            XmlNodeList returns = nodeInfo.GetElementsByTagName("returns");
                            if ( returns.Count > 0 && returns[0].InnerText.Length > 0) {
                                pageContent += "###Returns###\n" + XmlHelper.RemoveWhitespace(returns [ 0 ].InnerText) + "\n";
                                emptyPage = false;
                            }
                            pageContent += "\n";
                        } catch(Exception ex) {
                            MessageBox.Show("Invalid XML!\n\"" + next.xmlNode.InnerXml + "\"\nException: " + ex);
                        }
                    }

                    //print page children
                    if(next.children.Count > 0) {
                        pageContent += "###Members###\n";

                        foreach(WikiPage page in next.children ) {
                            pageContent += "* [" + page.title + "](" + System.Net.WebUtility.UrlEncode( page.title) + ")\n"; //let us put on our http condom once again
                        }

                        emptyPage = false;
                        pageContent += "\n";
                    }

                    if ( emptyPage ) {
                        pageContent += "_Nothing to see here..._";
                    }

                    System.IO.File.WriteAllText(folderBrowserDialog.SelectedPath + "\\" + next.title + ".md", pageContent);
                    foreach (WikiPage page in next.children) {
                        pageQueue.Push(page);
                    }

                    count++;
                }

                status.Text = "Successfully exported " + count + " pages!";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            CheckListBoxItems(typeList, true);
        }

        private void deselectAllToolStripMenuItem_Click(object sender, EventArgs e) {
            CheckListBoxItems(typeList, false);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show("WikiSense for GitHub v0.1-alpha\n" + 
                "Parsing IntelliSense XML-files to GitHub wiki formatting.\n" +
                "Licensed under GPLv2 - http://www.gnu.org/licenses/old-licenses/gpl-2.0.html", "About");
        }
    }
}
