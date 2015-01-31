using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fiddler;

namespace FiddlerCSP
{
    public partial class RuleCollectionView : UserControl
    {
        private CSPRuleCollector collector;

        public RuleCollectionView(CSPRuleCollector collector)
        {
            this.collector = collector;
            collector.OnRuleAddedOrModified += AddToListViewOnUIThread;

            InitializeComponent();
        }

        private void RuleCollectionView_Load(object sender, EventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Copy", new EventHandler(delegate(object o, EventArgs copyEventArgs) {
                if (RuleCollectionListView.SelectedItems.Count > 0)
                {
                    StringBuilder sbToCopy = new StringBuilder();
                    foreach (ListViewItem item in RuleCollectionListView.SelectedItems)
                    {
                        sbToCopy.AppendFormat("{0} {1}\n", item.Text, item.SubItems[1].Text);
                    }
                    Clipboard.SetText(sbToCopy.ToString());
                }
            }));
            RuleCollectionListView.ContextMenu = contextMenu;

            RuleCollectionListView.ItemSelectionChanged += RuleCollectionListView_ItemSelectionChanged;

            VerboseLoggingCheckBox.Checked = FiddlerExtension.Settings.verboseLogging;
            EnableRuleCollectionCheckBox.Checked = FiddlerExtension.Settings.enabled;
        }

        void RuleCollectionListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string[] uris = e.Item.ListView.SelectedItems.Cast<ListViewItem>().Where(x => x != null).Select(x => x.Text).ToArray();
            string result = "";

            if (uris.Count() > 0)
            {
                string rule = collector.Get(uris);
                string formattedRule = rule.Replace("Content-Security-Policy: ", "").Replace("; ", "\r\n\r\n").Replace(" ", "\r\n\t");
                result = "Document: " + uris.Aggregate((l, r) => l + " " + r) + "\r\n\r\n" + rule + "\r\n\r\n" + formattedRule;
            }

            SelectedRuleText.Text = result;    
        }

        private delegate void AddToListView(string uri, string rule);

        private void AddToListViewOnUIThread(string uri, string rule)
        {
            if (RuleCollectionListView.InvokeRequired)
            {
                RuleCollectionListView.Invoke(new AddToListView(AddToListViewOnUIThread), new object[] {uri, rule});
            }
            else
            {
                ListViewItem listViewItem = null;
                foreach (ListViewItem check in RuleCollectionListView.Items)
                {
                    if (check.Text == uri)
                    {
                        listViewItem = check;
                        break;
                    }
                }
                if (listViewItem == null)
                {
                    listViewItem = new ListViewItem(uri);
                    listViewItem.SubItems.Add(rule);
                    RuleCollectionListView.Items.Add(listViewItem);
                }
                else
                {
                    listViewItem.SubItems[1].Text = rule;
                }
            }
        }

        private void VerboseLoggingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FiddlerExtension.Settings.verboseLogging = VerboseLoggingCheckBox.Checked;
        }

        private void EnableRuleCollectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FiddlerExtension.Settings.enabled = EnableRuleCollectionCheckBox.Checked;
        }

        private void lnkHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Utilities.LaunchHyperlink("https://github.com/david-risney/CSP-Fiddler-Extension");
        }
    }
}
