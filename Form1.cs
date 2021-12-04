using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace computer1
{
    public partial class Form1 : Form
    {
        private byte[] fat = new byte[128];
        private byte[] root = new byte[64];
        public Form1()
        {
            //format();
            InitializeComponent();
            //刷新磁盘
            init_();
        }

        /*格式化*/
        private void format()
        {
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            FileStream f = new FileStream(path, FileMode.Truncate, FileAccess.ReadWrite);
            f.WriteByte((byte)(1));
            f.WriteByte((byte)(1));
            f.WriteByte((byte)(2));
            f.WriteByte((byte)(3));
            fat[0] = 1;
            fat[1] = 1;
            fat[2] = 2;//根目录
            fat[3] = 3;//c目录占据
            for (int i = 4; i <128; i++)
            {
                f.WriteByte((byte)(128));
                fat[i] = 128;
            }
            byte[] bytes = Encoding.UTF8.GetBytes("C    d");
            f.Write(bytes,0,bytes.Length);
            f.WriteByte((byte)(3));
            f.Write(Encoding.UTF8.GetBytes(" "),0,1);
            for (int i = 136; i < 64*128; i++)
            {
                f.Write(Encoding.UTF8.GetBytes(" "), 0, 1);
            }
            f.Close();
        }

        /*刷新磁盘（每改变一次磁盘就刷新一次）未完待续*/
        private void init_()
        {  
            for(int i=1;i<=128;i++)
            {
                Label lab = (this.Controls.Find("label" + Convert.ToString(i), true).First()) as Label;
                lab.BackColor = Color.YellowGreen;
            }
            this.treeView1.Nodes.Clear();
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
            f.Read(fat, 0, 128);
            f.Read(root, 0, 64);
            for (int i = 0; i < 128; i++)
            {
                if (fat[i] != (byte)(128))
                {
                    Label lab = (this.Controls.Find("label" + Convert.ToString(i + 1), true).First()) as Label;
                    lab.BackColor = Color.DodgerBlue;
                }
            }
            f.Close();
          
            //初始化根目录
            string filename = "";
            int start = 0;
            for (int i = 0; i < 64; i = i + 8)
            {
                if (root[i] != 32)
                {
                    byte[] item = new byte[8];
                    for (int j = 0; j < 8; j++)
                    {
                        item[j] = root[i + j];
                    }
                    filename = Encoding.UTF8.GetString(item,0,3).Trim();
                    start = item[6];

                    TreeNode newnode = new TreeNode(filename, 2, 3);
                    file_option.Traverse(start, newnode,menu2,menu3);
                    this.treeView1.Nodes.Add(newnode);
                    newnode.ContextMenuStrip = menu2;
                    newnode.Expand();
                }
            }

        }

        /*menu1右键菜单*/
        private void 添加根目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            //判断根目录是否已满
            int count = this.treeView1.GetNodeCount(false);
            if(count>=8)
            {
                MessageBox.Show("根目录已满！");
                return;
            }
            //视图中创建树
            edit_file window = new edit_file();
            window.Enable2 = false;
            window.Enable3 = false;
            window.ShowDialog();
            string file_name=window.file_name;
            if (file_name == "")
                return;
         
            //磁盘中写入文件控制块
            //寻找磁盘块
            int i = 3;
            for(;i<128;i++)
            {
                if (fat[i] == 128)
                    break;
                else if (i == 127)
                {
                    MessageBox.Show("磁盘空间已满！");
                    return;
                }
            }
            //改变文件控制块，改变位示图
            fat[i] = (byte)(i);
            Label lab = (this.Controls.Find("label" + Convert.ToString(i + 1), true).First()) as Label;
            lab.BackColor = Color.DodgerBlue;

            //写入控制块
            file_option.add_tree(path, file_name,2, i,count);//2是根目录目录项存放处，i是其实盘块号,count是目录项数量

            //写入图标
            TreeNode newnode = new TreeNode(file_name, 2, 3);
            newnode.ContextMenuStrip = menu2;
            this.treeView1.Nodes.Add(newnode);

        }
        private void 格式化磁盘ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            format();
            init_();
        }

        /*menu2右键菜单*/
        private void 添加目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //判断子节点的数量是否超过限制
            int count = this.treeView1.SelectedNode.GetNodeCount(false);
            if(count==8)
            {
                MessageBox.Show("目录数量达到上限！");
                return;
            }
            //视图中创建树
            edit_file window = new edit_file();
            window.Enable2 = false;
            window.Enable3 = false;
            window.ShowDialog();
            string file_name = window.file_name;
            if (file_name == "")
                return;

            //查找父节点的起始盘块号
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            string name_parent = this.treeView1.SelectedNode.Text;
            string name = "";
            FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
            f.Seek(128, SeekOrigin.Begin);
            int j = 128;
            int store;
            while(true)
            {
                
                byte[] bytes = new byte[3];
                byte[] bytes1 = new byte[1];
                int i;
                for(i=0;i<3;i++)
                {
                    f.Read(bytes1, 0, 1);
                    if (bytes1[0]==Encoding.UTF8.GetBytes(" ").First())
                    {
                        break;
                    }
                    bytes[i] = bytes1[0];
                }
                name = Encoding.UTF8.GetString(bytes,0,i);
                if (name==name_parent)
                {
                    f.Seek(j + 6, SeekOrigin.Begin);
                    store=f.ReadByte();
                    break;
                }
                else
                {
                    j = j + 8;
                    f.Seek(j, SeekOrigin.Begin);
                }
                
            }
            
            f.Close();

            //查找空闲块
            int start = 3;
            for (; start < 128; start++)
            {
                if (fat[start] == 128)
                    break;
                else if (start == 127)
                {
                    MessageBox.Show("磁盘空间已满！");
                    return;
                }
            }

            //刷新位示图
            fat[start] = (byte)(start);
            Label lab = (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
            lab.BackColor = Color.DodgerBlue;

            //向父目录节点内写入文件控制块信息
            file_option.add_tree(path, file_name, store, start, count);

            //写入图标
            TreeNode newnode = new TreeNode(file_name, 2, 3);
            newnode.ContextMenuStrip = menu2;
            this.treeView1.SelectedNode.Nodes.Add(newnode);
            this.treeView1.SelectedNode.Expand();
            
        }
        private void 添加文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*判断子节点的数量是否超过限制*/
            int count = this.treeView1.SelectedNode.GetNodeCount(false);
            if (count == 8)
            {
                MessageBox.Show("文件数量达到上限！");
                return;
            }
            /*视图中创建树*/
            edit_file window = new edit_file();
            window.Enable3 = true;
            window.ShowDialog();
            if (window.DialogResult == DialogResult.Cancel)
                return;
            string file_name = window.file_name;
            string file_type = window.file_type;
            string context = window.context;
            if(context.Length>256)
            {
                MessageBox.Show("字数超出限制!");
                return;
            }

            /*查找父节点的起始盘块号*/
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            string name_parent = this.treeView1.SelectedNode.Text;
            string name = "";
            FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
            f.Seek(128, SeekOrigin.Begin);
            int j = 128;
            int store;//起始盘块号
            while (true)
            {

                byte[] bytes = new byte[3];
                byte[] bytes1 = new byte[1];
                int i;
                for (i = 0; i < 3; i++)
                {
                    f.Read(bytes1, 0, 1);
                    if (bytes1[0] == Encoding.UTF8.GetBytes(" ").First())
                    {
                        break;
                    }
                    bytes[i] = bytes1[0];
                }
                name = Encoding.UTF8.GetString(bytes, 0, i);
                if (name == name_parent)
                {
                    f.Seek(j + 6, SeekOrigin.Begin);
                    store = f.ReadByte();
                    break;
                }
                else
                {
                    j = j + 8;
                    f.Seek(j, SeekOrigin.Begin);
                }

            }

            f.Close();

            /*查找空闲块并刷新位示图*/
            int start = 4;
            int count_;
            int pre=0;
            int start_num = 0 ;
            if (context.Length % 64 == 0)
                count_ = context.Length / 64;
            else
                count_ = context.Length / 64+1;

            for (; start < 128; start++)
            {
                if (fat[start] == 128)
                {
                    pre = start;
                    start_num = start;
                    fat[pre] = (byte)(start);
                    Label lab = (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
                    lab.BackColor = Color.DodgerBlue;
                    count_--;
                    start++;
                    break;
                }
                else if(start==127)
                {
                    MessageBox.Show("磁盘空间已满！");
                    return;
                }
            }
             while (count_ > 0)
             {
                for (; start < 128; start++)
                {
                    if(fat[start] == 128)
                    {
                        if (count_ <= 0)
                            break;
                        fat[pre] = (byte)(start);
                        fat[start] = (byte)(start);
                        pre = start;
                        count_--;
                        //刷新位示图
                        Label lab= (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
                        lab.BackColor = Color.DodgerBlue;
                    }
                    else if (start == 127)
                    {
                        MessageBox.Show("磁盘空间已满！");
                        return;
                    }
                }
            }
            //向父目录节点内写入文件控制块信息
            file_option.add_file(path, file_name,file_type,store, start_num,fat,context,count);
            
            //写入图标
            TreeNode newnode = new TreeNode(file_name, 0, 1);
            newnode.ContextMenuStrip = menu3;
            this.treeView1.SelectedNode.Nodes.Add(newnode);
            this.treeView1.SelectedNode.Expand();
        }

        /*menu3右键菜单*/
        private void 编辑文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*查找该节点的起始盘块号*/
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            string filename = this.treeView1.SelectedNode.Text;//文件名
            string filetype;//文件类型
            int count = this.treeView1.SelectedNode.Parent.GetNodeCount(false);
            string name = "";
            FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read);
            f.Seek(128, SeekOrigin.Begin);
            int j = 128;
            int store;//起始盘块号
            while (true)
            {

                byte[] bytes = new byte[3];
                byte[] bytes1 = new byte[1];
                byte[] bytes2 = new byte[2];
                int i;
                for (i = 0; i < 3; i++)
                {
                    f.Read(bytes1, 0, 1);
                    if (bytes1[0] == Encoding.UTF8.GetBytes(" ").First())
                    {
                        break;
                    }
                    bytes[i] = bytes1[0];
                }
                name = Encoding.UTF8.GetString(bytes, 0, i);
                if (name == filename)
                {
                    f.Seek(j+3, SeekOrigin.Begin);
                    f.Read(bytes2, 0, 2);
                    filetype = Encoding.UTF8.GetString(bytes2).Trim();
                    f.Seek(j + 6, SeekOrigin.Begin);
                    store = f.ReadByte();
                    break;
                }
                else
                {
                    j = j + 8;
                    f.Seek(j, SeekOrigin.Begin);
                }

            }
            f.Close();

            /*提取内容*/
            string context="";
            int ii = store;
            while(fat[ii]!=ii)
            {
                int next = fat[store];
                f = new FileStream(path, FileMode.Open, FileAccess.Read);
                f.Seek(store * 64, SeekOrigin.Begin);
                byte[] t = new byte[64];
                f.Read(t, 0, 64);
                f.Close();
                context = context+Encoding.UTF8.GetString(t);
                ii = fat[ii];
            }
            f = new FileStream(path, FileMode.Open, FileAccess.Read);
            f.Seek(ii*64, SeekOrigin.Begin);
            byte[] bytes3 = new byte[64];
            f.Read(bytes3, 0, 64);
            f.Close();
            context = context+Encoding.UTF8.GetString(bytes3,0,64).Trim();
            

            /*修改文件内容*/
            edit_file window = new edit_file();
            window.context = context;
            window.file_name = filename;
            window.file_type = filetype;
            window.ShowDialog();
            if (window.DialogResult == DialogResult.Cancel)
                return;
            filename = window.file_name;
            filetype = window.file_type;
            context = window.context;
            if (context.Length > 256)
            {
                MessageBox.Show("字数超出限制!");
                return;
            }
            this.treeView1.SelectedNode.Text = filename;//修改节点名字
            
            //更改位示图
            ii = store;
            while(fat[ii]!=ii)
            {
                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                lab1.BackColor = Color.YellowGreen;
                fat[ii] = 128;
                ii = fat[ii];
            }
            if (fat[ii] == ii)
            {
                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                lab1.BackColor = Color.YellowGreen;
                fat[ii] = 128;
            }


            //根据新的内容重新查找空闲块
            int start = 4;
            int count_;
            int pre = 0;
            int start_num = 0;
            if (context.Length % 64 == 0)
                count_ = context.Length / 64;
            else
                count_ = context.Length / 64 + 1;

            for (; start < 128; start++)
            {
                if (fat[start] == 128)
                {
                    pre = start;
                    start_num = start;
                    fat[pre] = (byte)(start);
                    Label lab1 = (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
                    lab1.BackColor = Color.DodgerBlue;
                    count_--;
                    start++;
                    break;
                }
                else if (start == 127)
                {
                    MessageBox.Show("磁盘空间已满！");
                    return;
                }
            }
            while (count_ > 0)
            {
                for (; start < 128; start++)
                {
                    if (fat[start] == 128)
                    {
                        if (count_ <= 0)
                            break;
                        fat[pre] = (byte)(start);
                        fat[start] = (byte)(start);
                        pre = start;
                        count_--;
                        //刷新位示图
                        Label lab1 = (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
                        lab1.BackColor = Color.DodgerBlue;
                    }
                    else if (start == 127)
                    {
                        MessageBox.Show("磁盘空间已满！");
                        return;
                    }
                }
            }
            //重新写入内容
            file_option.add_file(path, filename, filetype, j/64, start_num, fat, context, count - 1);
        }
    }
}
