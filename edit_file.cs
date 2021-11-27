using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace computer1
{
    public partial class edit_file : Form
    {
        private string text1 = "";
        private string text2 = "";
        private string text3 = "";
        public edit_file()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

        //获取文本框内的值
        private void text1_Box(object sender, EventArgs e)
        {
            this.text1 = this.textBox1.Text;
        }

        private void text2_Box(object sender, EventArgs e)
        {
            this.text2 = this.textBox2.Text;
        }

        private void text3_Box(object sender, EventArgs e)
        {
            this.text3 = this.textBox3.Text;
        }

        //定义属性值
        public string file_name
        {
            get
            {
                return text1;
            }
            set
            {
                this.textBox1.Text = value;
            }
        }
        public string file_type
        {
            get 
            {
                return text2;
            }
            set
            {
                textBox2.Text = value;
            }
        }
        public string context
        {
            get 
            {
                return text3;
            }
            set
            {
                textBox3.Text = value;
            }
        }
        public bool Enable2
        {
            set
            {
                textBox2.Enabled = value;
            }
        }
        public bool Enable3
        {
            set
            {
                this.textBox3.Enabled = value;
            }
        }
        //设置按钮操作
        private void ensure(object sender, EventArgs e)
        {
            if(this.textBox1.Text=="")
            {
                MessageBox.Show("请输入文件名！");
                return;
            }
            if(this.textBox2.Enabled==true && this.textBox2.Text=="")
            {
                MessageBox.Show("请输入文件类型！");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void quit(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
