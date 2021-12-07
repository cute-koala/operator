using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace computer1
{
    struct PCB
    {
        public int id;//标识符
        public int x;//寄存器(暂存结果)
        public string state;//状态（ready/running/waiting/blocked)
        public string reason;//阻塞原因
        public int pc;//程序计数器
        public List<int> memory;//所占内存
    };
    class process
    {
        public static Queue<PCB> Block = new Queue<PCB>();//阻塞队列
        public static Queue<PCB> Ready = new Queue<PCB>();//就绪队列
        public static int pcb_count = 0;
        public static string[] Memory = new string[100];//内存
        public static string[] cache = new string[30];//缓存
        public static string IR;//指令寄存器
        public static int PC;//程序计数器
        public static int PSW = 0;//程序状态寄存器
        public static int DR = 0;//数据缓冲寄存器
        public static int time;
        public static PCB now_do;//存储当前正运行的进程的进程控制块
        public static bool flag = false;//标志是否有正在运行的进程
        private static int ID = 1;
        /*进程控制原语*/
        //进程创建
        public static void create()
        {
            if (pcb_count > 10)
            {
                MessageBox.Show("进程控制块已满！");
                return;
            }
            //申请空白进程控制块
            PCB node1 = new PCB();
            node1.memory = new List<int>();
            //申请内存空间(就是内存大小)
            int i = 0;
            for (int j = 0; j < cache.Length && cache[j] != null; j++)
            {
                for (; i < 100; i++)
                {
                    if (Memory[i] == null)
                    {
                        node1.memory.Add(i);
                        Memory[i] = cache[j];
                        cache[j] = null;
                        break;
                    }
                    if (i == 99)
                    {
                        MessageBox.Show("内存已满！");
                    }
                }
            }

            //初始化进程控制块
            node1.id = ID++;
            node1.x = 0;
            node1.state = "ready";
            node1.reason = "";
            node1.pc = node1.memory[0];
            //插入就绪队列
            Ready.Enqueue(node1);
            pcb_count++;
        }
        //进程撤销
        public static void destory()
        {
            //回收进程所占内存资源
            int[] me = now_do.memory.ToArray();
            foreach (int i in me)
            {
                Memory[i] = null;
            }
            now_do.memory.Clear();
            //回收进程控制块
            pcb_count--;
            flag = false;

        }
        //进程阻塞
        public static void block()
        {
            //保存现场环境
            now_do.pc = PC;
            now_do.x = DR;
            //修改状态
            now_do.state = "waiting";
            now_do.reason = "not end";
            //插入阻塞队列
            Block.Enqueue(now_do);
            flag = false;

        }
        //进程唤醒
        public static void awaken()
        {
            PCB p = Block.Dequeue();
            p.state = "Ready";
            Ready.Enqueue(p);
        }
    }
}
