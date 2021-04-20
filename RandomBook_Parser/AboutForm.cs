using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RandomBook_Parser {
    public partial class AboutForm : Form {
        public AboutForm() {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://readly.ru/books/i_am_lucky");
        }
    }
}
