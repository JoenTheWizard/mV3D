using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace mV3DViewer
{
    class TextBoxHistory
    {
        List<string> history = new List<string>();
        private int currCmd;

        public TextBoxHistory()
        {
            
        }

        public void AddToHistory(string hist)
        {
            currCmd = history.Count;
            history.Add(hist);
        }

        public void ClearHistory()
        {
            history.Clear();
        }

        private void ClearAndRefresh(TextBox txbx, string otpt)
        {
            string k = txbx.Text;
            txbx.Clear();
            string[] lines = k.Split('\n');
            for (int i = 0; i < lines.Length - 2; i++)
            {
                txbx.Text += lines[i] + "\n";
            }
            txbx.Text += Environment.NewLine + otpt;
        }

        public void TraverseUp(TextBox txtbox, string outputTxt)
        {
            if (currCmd >= 0 && history.Count > 0)
            {
                ClearAndRefresh(txtbox,outputTxt);
                txtbox.Text += history[currCmd];
                txtbox.SelectionStart = txtbox.Text.Length;
                txtbox.SelectionLength = 0;
                txtbox.ScrollToEnd();
                currCmd--;
            }
            txtbox.SelectionStart = txtbox.Text.Length;
            txtbox.SelectionLength = 0;
        }

        private void SendToEnd(TextBox txtbox)
        {
            txtbox.SelectionStart = txtbox.Text.Length;
            txtbox.SelectionLength = 0;
            txtbox.ScrollToEnd();
        }

        public void TraverseDown(TextBox txtbox, string outputTxt)
        {
            if (currCmd + 1 < history.Count && history.Count > 0)
            {
                ClearAndRefresh(txtbox,outputTxt);
                txtbox.Text += history[currCmd + 1];
                SendToEnd(txtbox);
                currCmd++;
            }
            else if (currCmd + 1 == history.Count)
            {
                ClearAndRefresh(txtbox, outputTxt);
                txtbox.Text += " ";
                SendToEnd(txtbox);
            }
            txtbox.SelectionStart = txtbox.Text.Length;
            txtbox.SelectionLength = 0;
        }
    }
}
