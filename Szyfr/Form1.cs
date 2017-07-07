using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Szyfr
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Quiz Text
        /// </summary>
        Dictionary<string, string> content = new Dictionary<string, string>();

        LogicMgr lm;// = new LogicMgr();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            content.Add("682", "One correct digit, correct place.");
            content.Add("614", "One correct digit, incorrect place.");
            content.Add("206", "Two correct digits, incorrect places.");
            content.Add("738", "Nothing correct.");
            content.Add("870", "One correct digit, incorrect place.");
            foreach(var k in content.Keys)
                cboDescription.Items.Add(k);
            cboDescription.SelectedIndex = 0;            
            numericUpDown1.Value = 1;
            checkBox1.Checked = true;
        }

        private void cboDescription_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                textDescription.Text = content[cboDescription.Text];
                switch (cboDescription.SelectedIndex)
                {
                    case 0:
                        numericUpDown1.Value = 1;
                        checkBox1.Checked = true;
                        break;
                    case 1:                        
                    case 4:
                        numericUpDown1.Value = 1;
                        checkBox1.Checked = false;
                        break;
                    case 3:
                        numericUpDown1.Value = 0;
                        checkBox1.Checked = false;
                        break;
                    case 2:
                        numericUpDown1.Value = 2;
                        checkBox1.Checked = false;
                        break;
                }
            }
            catch { }
            
        }
        
        private void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                string dir = Application.StartupPath;
                string fil = dir + @"\..\..\records.txt";
                lm = new LogicMgr(fil);
                if (!string.IsNullOrWhiteSpace(lm.GetLastError())) MessageBox.Show(lm.GetLastError());
                text.Text = lm.GetAlgoLog();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        
        
    }
}
