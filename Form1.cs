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
        /*进程控制块变量*/
        struct PCB
        {
            int id;//标识符
            int x;//寄存器
            string state;//状态
            int reason;//阻塞原因
        }
        private List<PCB> Block = new List<PCB>();//阻塞队列
        private List<PCB> Ready = new List<PCB>();//就绪队列

        /*磁盘初始化变量*/
        private byte[] fat = new byte[128];
        private byte[] root = new byte[64];
        private List<string> Names = new List<string>();
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

        /*刷新磁盘（每改变一次磁盘就刷新一次)*/
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
                    Names.Add(filename);
                    start = item[6];

                    TreeNode newnode = new TreeNode(filename, 2, 3);
                    newnode.Name = filename;
                    file_option.Traverse(start, newnode,menu2,menu3,menu3e,Names);
                    this.treeView1.Nodes.Add(newnode);
                    newnode.ContextMenuStrip = menu2;
                    newnode.Expand();
                }
            }
            treeView1.TreeViewNodeSorter = new file_option();

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
            else if(Names.Contains(file_name))
            {
                MessageBox.Show("该名称已存在！");
                return;
            }
            Names.Add(file_name);
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
            newnode.Name = file_name;
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
            else if (Names.Contains(file_name))
            {
                MessageBox.Show("该名称已存在！");
                return;
            }
            Names.Add(file_name);

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
            newnode.Name = file_name;
            newnode.ContextMenuStrip = menu2;
            this.treeView1.SelectedNode.Nodes.Add(newnode);
            this.treeView1.SelectedNode.Expand();
            treeView1.TreeViewNodeSorter = new file_option();

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
            if (context.Length>256)
            {
                MessageBox.Show("字数超出限制!");
                return;
            }
            if (Names.Contains(file_name))
            {
                MessageBox.Show("该名称已存在！");
                return;
            }
            Names.Add(file_name);

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
                    Label lab = (Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
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
            if(file_type=="ex")
                newnode.ContextMenuStrip = menu3e;
            else
                newnode.ContextMenuStrip = menu3;
            newnode.Name = file_name;
            this.treeView1.SelectedNode.Nodes.Add(newnode);
            this.treeView1.SelectedNode.Expand();
            treeView1.TreeViewNodeSorter = new file_option();
        }
        private void 删除目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*查找该节点的起始盘块号*/
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            string filename = this.treeView1.SelectedNode.Text;//文件名
            string filetype;//文件类型
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
                    f.Seek(j + 3, SeekOrigin.Begin);
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

            /*递归删除目录项*/
            byte[] disk = new byte[128 * 64];//读磁盘
            f = new FileStream(path, FileMode.Open, FileAccess.Read);
            f.Read(disk, 0, 128 * 64);
            f.Close();
            Stack<string> fname = new Stack<string>();//存文件名称
            Stack<string> ftype = new Stack<string>();//存文件类型
            Stack<int> fstore = new Stack<int>();//存文件起始盘块号
            byte[] file_context = new byte[64];
            fname.Push(filename);
            ftype.Push("d");
            fstore.Push(store);
            while(fstore.Count!=0)
            {
                filename = fname.Pop();
                Names.Remove(filename);
                store = fstore.Pop();
                filetype = ftype.Pop();
                for(int i=store*64;i<(store+1)*64;i++)
                {
                    file_context[i - store * 64] = disk[i];
                }

                if(filetype=="d")//如果是目录的话
                {
                    disk[store] = 128;//更改文件分配表
                    for (int i = store * 64; i < (store + 1) * 64; i++)//删除内容
                    {
                        disk[i] = 32;
                    }
                    for (int jj=0;jj<64;jj=jj+8)
                    {
                        if (file_context[jj] != 32)
                        {
                            filename= Encoding.UTF8.GetString(file_context, jj, 3).Trim();
                            filetype = Encoding.UTF8.GetString(file_context, jj + 5, 1);
                            store = file_context[jj + 6];
                            fname.Push(filename);
                            ftype.Push(filetype);
                            fstore.Push(store);
                        }
                        else
                            break;
                    }
                }
                else if(filetype=="l")//如果是文件的话
                {
                    while(disk[store]!=store)
                    {
                        //清理空间
                        for (int i = store * 64; i < (store + 1) * 64; i++)
                        {
                            disk[i] = 32;
                        }
                        //更改文件分配表
                        int t = disk[store];
                        disk[store] = 128;
                        store = t;
                    }
                    if(disk[store]==store)
                    {
                        //清理空间
                        for (int i = store * 64; i < (store + 1) * 64; i++)
                        {
                            disk[i] = 32;
                        }
                        //更改文件分配表
                        disk[store] = 128;
                    }

                }
            }
            for (int i = j; i < ((j / 64 + 1) * 64 - 8); i++)
            {
                disk[i] = disk[i + 8];
            }
            for (int i = ((j / 64 + 1) * 64 - 8); i < (j / 64 + 1) * 64; i++)
            {
                disk[i] = 32;
            }
            f = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            f.Write(disk, 0, 128 * 64);
            f.Close();
            treeView1.SelectedNode.Nodes.Clear();
            treeView1.Nodes.Remove(treeView1.SelectedNode);
            init_();
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
            string k = filename;//用于Names
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
            if (!Names.Contains(filename))
            {
                Names.Remove(k);
                Names.Add(filename);
                this.treeView1.SelectedNode.Text = filename;//修改节点名字
                this.treeView1.SelectedNode.Name = filename;
            }
            if (filetype == "ex")
                this.treeView1.SelectedNode.ContextMenuStrip = menu3e;
            //更改位示图
            ii = store;
            while(fat[ii]!=ii)
            {
                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                lab1.BackColor = Color.YellowGreen;
                int t = fat[ii];
                fat[ii] = 128;
                ii = t;
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
        private void 删除文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*查找该节点的起始盘块号*/
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            string filename=this.treeView1.SelectedNode.Text;//文件名
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
                f.Read(bytes, 0, 3);
                name = Encoding.UTF8.GetString(bytes, 0, 3).Trim();
                if (name == filename)
                {
                    f.Seek(j + 3, SeekOrigin.Begin);
                    f.Read(bytes2, 0, 2);
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
            //修改文件分配表 修改位示图
            int ii = store;
            Stack<int> p=new Stack<int>();
            while(fat[ii]!=ii)
            {
                p.Push(ii);
                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                lab1.BackColor = Color.YellowGreen;
                int t = fat[ii];
                fat[ii] = 128;
                ii = t;

            }
            if (fat[ii] == ii)
            {
                p.Push(ii);
                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                lab1.BackColor = Color.YellowGreen;
                fat[ii] = 128;
            }
                
            

            //写入磁盘
            f = new FileStream(path, FileMode.Open);
            byte[] disk = new byte[128 * 64];
            f.Read(disk, 0, 128 * 64);
            f.Close();
            //文件分配表
            for (int i = 0; i < 128; i++)
            {
                disk[i] = fat[i];
            }
            //文件内容
            while(p.Count!=0)
            {
                int m = p.Pop();
                for(int i=0;i<64;i++)
                {
                    disk[m * 64 + i] = 32;
                }
            }
            //文件控制块
            for (int i = j; i < ((j/64+1)*64-8); i++)
            {
                disk[i] = disk[i + 8];
            }
            for(int i= ((j / 64 + 1) * 64-8); i< (j / 64 + 1) * 64;i++)
            {
                disk[i] = 32;
            }
            f = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            f.Write(disk, 0, disk.Length);
            f.Close();

            //修改视图
            treeView1.Nodes.Remove(treeView1.SelectedNode);
            Names.Remove(filename);
        }

        /*menu3e右键菜单*/
        private void 运行文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void 编辑文件ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            编辑文件ToolStripMenuItem_Click(sender, e);
        }
        private void 删除文件ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            删除文件ToolStripMenuItem_Click(sender, e);
        }
        /*命令接口*/
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar==(char)Keys.Enter)
            {
                string context1 = textBox1.Text;
                string[] order = context1.Split(' ');
                switch(order[0])
                {
                    case "create": {
                            //是否重名
                            if (Names.Contains(order[1]))
                            {
                                MessageBox.Show("该名称已存在！");
                                return;
                            }
                            Names.Add(order[1]);
                            //计算子节点数量
                            TreeNode tn = treeView1.Nodes.Find(order[3], true).First();
                            int count = tn.GetNodeCount(false);
                            if (count == 8)
                            {
                                MessageBox.Show("文件数量达到上限！");
                                return;
                            }
                            /*查找父节点的起始盘块号*/
                            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
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
                                if (name == order[3])
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
                            int start_num = 0;
                            for (; start < 128; start++)
                            {
                                if (fat[start] == 128)
                                {
                                    start_num = start;
                                    fat[start_num] = (byte)(start);
                                    Label lab = (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
                                    lab.BackColor = System.Drawing.Color.DodgerBlue;
                                    break;
                                }
                                else if (start == 127)
                                {
                                    MessageBox.Show("磁盘空间已满！");
                                    return;
                                }
                            }
                            //向父目录节点内写入文件控制块信息
                            file_option.add_file(path, order[1], order[2], store, start_num, fat, "", count);

                            //写入图标
                            TreeNode newnode = new TreeNode(order[1], 0, 1);
                            newnode.Name = order[1];
                            newnode.ContextMenuStrip = menu3;
                            tn.Nodes.Add(newnode);
                            tn.Expand();
                            treeView1.TreeViewNodeSorter = new file_option();
                        };break;//文件名 文件类型 父节点名称
                    case "mkdir":  {
                            //是否重名
                            if (Names.Contains(order[1]))
                            {
                                MessageBox.Show("该名称已存在！");
                                return;
                            }
                            Names.Add(order[1]);
                            //计算子节点数量
                            TreeNode tn = treeView1.Nodes.Find(order[2], true).First();
                            int count = tn.GetNodeCount(false);
                            if (count == 8)
                            {
                                MessageBox.Show("文件数量达到上限！");
                                return;
                            }
                            /*查找父节点的起始盘块号*/
                            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
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
                                if (name == order[2])
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
                            int start_num = 0;
                            for (; start < 128; start++)
                            {
                                if (fat[start] == 128)
                                {
                                    start_num = start;
                                    fat[start_num] = (byte)(start);
                                    Label lab = (this.Controls.Find("label" + Convert.ToString(start + 1), true).First()) as Label;
                                    lab.BackColor = System.Drawing.Color.DodgerBlue;
                                    break;
                                }
                                else if (start == 127)
                                {
                                    MessageBox.Show("磁盘空间已满！");
                                    return;
                                }
                            }
                            //向父目录节点内写入文件控制块信息
                            file_option.add_tree(path, order[1], store, start_num, count);

                            //写入图标
                            TreeNode newnode = new TreeNode(order[1], 2, 3);
                            newnode.Name = order[1];
                            newnode.ContextMenuStrip = menu2;
                            tn.Nodes.Add(newnode);
                            tn.Expand();
                            treeView1.TreeViewNodeSorter = new file_option();
                        };break;//文件名 父节点名称
                    case "delete": {
                            TreeNode tn = treeView1.Nodes.Find(order[1], true).First();
                            /*查找该节点的起始盘块号*/
                            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
                            string filename = order[1];//文件名
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
                                f.Read(bytes, 0, 3);
                                name = Encoding.UTF8.GetString(bytes, 0, 3).Trim();
                                if (name == filename)
                                {
                                    f.Seek(j + 3, SeekOrigin.Begin);
                                    f.Read(bytes2, 0, 2);
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
                            //修改文件分配表 修改位示图
                            int ii = store;
                            Stack<int> p = new Stack<int>();
                            while (fat[ii] != ii)
                            {
                                p.Push(ii);
                                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                                lab1.BackColor = Color.YellowGreen;
                                int t = fat[ii];
                                fat[ii] = 128;
                                ii = t;

                            }
                            if (fat[ii] == ii)
                            {
                                p.Push(ii);
                                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                                lab1.BackColor = Color.YellowGreen;
                                fat[ii] = 128;
                            }



                            //写入磁盘
                            f = new FileStream(path, FileMode.Open);
                            byte[] disk = new byte[128 * 64];
                            f.Read(disk, 0, 128 * 64);
                            f.Close();
                            //文件分配表
                            for (int i = 0; i < 128; i++)
                            {
                                disk[i] = fat[i];
                            }
                            //文件内容
                            while (p.Count != 0)
                            {
                                int m = p.Pop();
                                for (int i = 0; i < 64; i++)
                                {
                                    disk[m * 64 + i] = 32;
                                }
                            }
                            //文件控制块
                            for (int i = j; i < ((j / 64 + 1) * 64 - 8); i++)
                            {
                                disk[i] = disk[i + 8];
                            }
                            for (int i = ((j / 64 + 1) * 64 - 8); i < (j / 64 + 1) * 64; i++)
                            {
                                disk[i] = 32;
                            }
                            f = new FileStream(path, FileMode.Truncate, FileAccess.Write);
                            f.Write(disk, 0, disk.Length);
                            f.Close();

                            //修改视图
                            treeView1.Nodes.Remove(tn);
                            Names.Remove(filename);
                        }; break;//文件名
                    case "deldir": {
                            TreeNode tn = treeView1.Nodes.Find(order[1], true).First();
                            /*查找该节点的起始盘块号*/
                            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
                            string filename = order[1];//文件名
                            string filetype;//文件类型
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
                                    f.Seek(j + 3, SeekOrigin.Begin);
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

                            /*递归删除目录项*/
                            byte[] disk = new byte[128 * 64];//读磁盘
                            f = new FileStream(path, FileMode.Open, FileAccess.Read);
                            f.Read(disk, 0, 128 * 64);
                            f.Close();
                            Stack<string> fname = new Stack<string>();//存文件名称
                            Stack<string> ftype = new Stack<string>();//存文件类型
                            Stack<int> fstore = new Stack<int>();//存文件起始盘块号
                            byte[] file_context = new byte[64];
                            fname.Push(filename);
                            ftype.Push("d");
                            fstore.Push(store);
                            while (fstore.Count != 0)
                            {
                                filename = fname.Pop();
                                Names.Remove(filename);
                                store = fstore.Pop();
                                filetype = ftype.Pop();
                                for (int i = store * 64; i < (store + 1) * 64; i++)
                                {
                                    file_context[i - store * 64] = disk[i];
                                }

                                if (filetype == "d")//如果是目录的话
                                {
                                    disk[store] = 128;//更改文件分配表
                                    for (int i = store * 64; i < (store + 1) * 64; i++)//删除内容
                                    {
                                        disk[i] = 32;
                                    }
                                    for (int jj = 0; jj < 64; jj = jj + 8)
                                    {
                                        if (file_context[jj] != 32)
                                        {
                                            filename = Encoding.UTF8.GetString(file_context, jj, 3).Trim();
                                            filetype = Encoding.UTF8.GetString(file_context, jj + 5, 1);
                                            store = file_context[jj + 6];
                                            fname.Push(filename);
                                            ftype.Push(filetype);
                                            fstore.Push(store);
                                        }
                                        else
                                            break;
                                    }
                                }
                                else if (filetype == "l")//如果是文件的话
                                {
                                    while (disk[store] != store)
                                    {
                                        //清理空间
                                        for (int i = store * 64; i < (store + 1) * 64; i++)
                                        {
                                            disk[i] = 32;
                                        }
                                        //更改文件分配表
                                        int t = disk[store];
                                        disk[store] = 128;
                                        store = t;
                                    }
                                    if (disk[store] == store)
                                    {
                                        //清理空间
                                        for (int i = store * 64; i < (store + 1) * 64; i++)
                                        {
                                            disk[i] = 32;
                                        }
                                        //更改文件分配表
                                        disk[store] = 128;
                                    }

                                }
                            }
                            for (int i = j; i < ((j / 64 + 1) * 64 - 8); i++)
                            {
                                disk[i] = disk[i + 8];
                            }
                            for (int i = ((j / 64 + 1) * 64 - 8); i < (j / 64 + 1) * 64; i++)
                            {
                                disk[i] = 32;
                            }
                            f = new FileStream(path, FileMode.Truncate, FileAccess.Write);
                            f.Write(disk, 0, 128 * 64);
                            f.Close();
                            tn.Nodes.Clear();
                            treeView1.Nodes.Remove(tn);
                            init_();
                        }; break;//文件名
                    case "edit":   { 
                            TreeNode tn=treeView1.Nodes.Find(order[1],true).First();
                            /*查找该节点的起始盘块号*/
                            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
                            string filename =order[1];//文件名
                            string filetype;//文件类型
                            int count = tn.Parent.GetNodeCount(false);
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
                                    f.Seek(j + 3, SeekOrigin.Begin);
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
                            string context = "";
                            int ii = store;
                            while (fat[ii] != ii)
                            {
                                int next = fat[store];
                                f = new FileStream(path, FileMode.Open, FileAccess.Read);
                                f.Seek(store * 64, SeekOrigin.Begin);
                                byte[] t = new byte[64];
                                f.Read(t, 0, 64);
                                f.Close();
                                context = context + Encoding.UTF8.GetString(t);
                                ii = fat[ii];
                            }
                            f = new FileStream(path, FileMode.Open, FileAccess.Read);
                            f.Seek(ii * 64, SeekOrigin.Begin);
                            byte[] bytes3 = new byte[64];
                            f.Read(bytes3, 0, 64);
                            f.Close();
                            context = context + Encoding.UTF8.GetString(bytes3, 0, 64).Trim();


                            /*修改文件内容*/
                            edit_file window = new edit_file();
                            string k = filename;//用于Names
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
                            if (!Names.Contains(filename))
                            {
                                Names.Remove(k);
                                Names.Add(filename);
                                tn.Text = filename;//修改节点名字
                                tn.Name = filename;
                            }
                            //更改位示图
                            ii = store;
                            while (fat[ii] != ii)
                            {
                                Label lab1 = (this.Controls.Find("label" + Convert.ToString(ii + 1), true).First()) as Label;
                                lab1.BackColor = Color.YellowGreen;
                                int t = fat[ii];
                                fat[ii] = 128;
                                ii = t;
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
                            file_option.add_file(path, filename, filetype, j / 64, start_num, fat, context, count - 1);
                        }; break;//文件名
                }
            }
        }
    }
}
