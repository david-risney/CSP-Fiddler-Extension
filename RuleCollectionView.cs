using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fiddler;

namespace FiddlerCSP
{
    public partial class RuleCollectionView : UserControl
    {
        private CSPRuleCollector collector;

        public RuleCollectionView(CSPRuleCollector collectorIn)
        {
            collector = collectorIn;
            collector.OnRuleAddedOrModified += AddToListViewOnUIThread;

            InitializeComponent();
        }

        private void RuleCollectionView_Load(object sender, EventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Copy", new EventHandler(delegate(object o, EventArgs copyEventArgs) {
                string text = "";
                foreach (ListViewItem item in RuleCollectionListView.SelectedItems)
                {
                    text += item.Text + " " + item.SubItems[1].Text + "\n";
                }
                Clipboard.SetText(text);
            }));
            RuleCollectionListView.ContextMenu = contextMenu;

            RuleCollectionListView.ItemSelectionChanged += RuleCollectionListView_ItemSelectionChanged;

            VerboseLoggingCheckBox.Checked = FiddlerExtension.Settings.verboseLogging;
            EnableRuleCollectionCheckBox.Checked = FiddlerExtension.Settings.enabled;
        }

        void RuleCollectionListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            string uri = e.Item.Text;
            string rule = e.Item.SubItems[1].Text;
            string formattedRule = rule.Replace("Content-Security-Policy: ", "").Replace("; ", "\r\n\r\n").Replace(" ", "\r\n\t");
            SelectedRuleText.Text = "Document: " + uri + "\r\n\r\n" + rule + "\r\n\r\n" + formattedRule;
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            FiddlerExtension.Settings.enabled = EnableRuleCollectionCheckBox.Checked;
        }

        private void VerboseLoggingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FiddlerExtension.Settings.verboseLogging = VerboseLoggingCheckBox.Checked;
        }
    }
}
