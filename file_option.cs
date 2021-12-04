using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace computer1
{
    class file_option
    {
        //添加目录
        public static void add_tree(string path,string filename,int store,int start_num ,int tree_num)
        {
            //路径 文件名 存储在第几块 起始盘块号 块内已有表项数目
            //更新文件分配表
            FileStream f = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(f);
            byte[] disk = br.ReadBytes(64*129);
            br.Close();
            f.Close();
            disk[start_num] = (byte)(start_num);

            //更新文件控制块
            while (Encoding.UTF8.GetBytes(filename).Length < 3)
                filename = filename + " ";
            byte[] byte1 = Encoding.UTF8.GetBytes(filename + "  d");
            byte[] byte2 = Encoding.UTF8.GetBytes(" ");
            for (int i=0;i<8;i++)
            {
                if (i < byte1.Length)
                    disk[store * 64 + tree_num * 8 + i] = byte1[i];
                else if (i == byte1.Length)
                    disk[store * 64 + tree_num * 8 + i] = (byte)(start_num);
                else
                    disk[store * 64 + tree_num * 8 + i] = byte2[0];
            }
            f = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            f.Write(disk, 0, disk.Length);
            f.Close();
        }
        //添加文件
        public static void add_file(string path, string filename, string filetype, int store,int start_num,byte[] fat,string context, int tree_num)
        {
            //路径 文件名 文件类型 存储块号 起始盘块号 第二块盘块号 文件内容 块内已有表项

            //更新文件分配表
            FileStream f = new FileStream(path, FileMode.Open);
            byte[] disk=new byte[128*64];
            f.Read(disk,0,128*64);
            f.Close();
            int ii = start_num;
            while(fat[ii]!=ii)
            {
                disk[ii] = fat[ii];
                ii = fat[ii];
            }
            if (fat[ii] == ii)
                disk[ii] = (byte)(ii);
            //更新文件控制块
            while (filename.Length < 3)
                filename = filename + " ";
            while (filetype.Length < 2)
                filetype = filetype + " ";
            byte[] byte1 = Encoding.UTF8.GetBytes(filename +filetype+"l");
            for (int i = 0; i < 8; i++)
            {
                if (i < byte1.Length)
                    disk[store * 64 + tree_num * 8 + i] = byte1[i];
                else if (i == byte1.Length)
                    disk[store * 64 + tree_num * 8 + i] = (byte)(start_num);
                else
                    disk[store * 64 + tree_num * 8 + i] = (byte)(context.Length);
            }
            //写入文件内容
            ii = start_num;
            int ji = 0;
            while(disk[ii]!=ii)
            {
                //如果块内不是空，先清空再写入
                if (disk[ii*64]!=32)
                {
                    for (int i = 0; i < 64; i++)
                        disk[ii * 64 + i] = 32;
                }
                byte[] bytes = new byte[64];
                char[] c = context.ToCharArray();
                bytes=Encoding.UTF8.GetBytes(c,ji*64,64);
                ji++;
                for(int i=0;i<64;i++)
                {
                    disk[ii * 64 + i] = bytes[i];
                }
                ii = disk[ii];
            }
            if(disk[ii]==ii)
            {
                //如果块内不是空，先清空再写入
                if (disk[ii * 64] != 32)
                {
                    for (int i = 0; i < 64; i++)
                        disk[ii * 64 + i] = 32;
                }
                byte[] bytes = byte1 = Encoding.UTF8.GetBytes(context.ToCharArray(), ji * 64,context.Length%64);
                ji++;
                for (int i = 0; i < context.Length % 64; i++)
                {
                    disk[ii * 64 + i] = bytes[i];
                }
            }
            f = new FileStream(path, FileMode.Truncate, FileAccess.Write);
            f.Write(disk, 0, disk.Length);
            f.Close();
        }
        //递归遍历磁盘
        public static void Traverse(int start, TreeNode node,ContextMenuStrip menu_d, ContextMenuStrip menu_l)
        {
            string path = "C:/Users/HP/Desktop/expriment/c++_vs/computer1/resource/disk.txt";
            FileStream f = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            byte[] context = new byte[64];//整个块内容存放处
            byte[] item = new byte[8];//目录项存放处
            string filename = "";
            int start_ = 0;
            string ld = "";
            List<TreeNode> childern=new List<TreeNode>();

            f.Seek(start * 64, SeekOrigin.Begin);
            f.Read(context,0,64);
            f.Close();

            int i = 0;
            int ii = 0;
            while(i<63 && context[i]!=32)
            {
                for(int j=0;j<8;j++)
                {
                    item[j] = context[i + j];
                }
                byte[] a = new byte[1];
                filename = Encoding.UTF8.GetString(item, 0, 3).Trim();
                start_ = item[6];
                a[0] = item[5];
                ld = Encoding.UTF8.GetString(a);

                TreeNode newnode = new TreeNode(filename);
                if (ld == "d")//针对目录,需要递归
                {
                    newnode.ContextMenuStrip = menu_d;
                    newnode.ImageIndex = 2;
                    newnode.SelectedImageIndex = 3;
                    newnode.Expand();
                    childern.Add(newnode);
                    Traverse(start_, childern[ii], menu_d, menu_l);
                    ii++;
                    i = i + 8;
                }
                else//针对文件
                {
                    newnode.ContextMenuStrip = menu_l;
                    newnode.ImageIndex = 0;
                    newnode.SelectedImageIndex = 1;
                    newnode.Expand();
                    childern.Add(newnode);
                    ii++;
                    i = i + 8;
                }
            }
            if (childern.Count==0)
                return;
            TreeNode[] childerns = childern.ToArray();
            node.Nodes.AddRange(childerns);
        }
    }

}
