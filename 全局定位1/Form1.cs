using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

namespace 全局定位1
{
    public partial class Form1 : Form
    {

        Stopwatch sw = new Stopwatch();

        //!PID暂存
        double x = 0;
        double y = 0;
        double p = 0;
        //float w=0;

        public Image imgofCar = Image.FromFile("车.PNG");
        public Image imgofGroud = Image.FromFile("比赛场地.PNG");

        //!绘图原点
        int[] Origin_Position = new int[2];//{pictureBox1.Width, pictureBox1.Height};

        public Form1()
        {
            InitializeComponent();
            //获取端口列
            serialPort1.DataReceived += DataReceivedHandler;
            sw.Start();

            Origin_Position[0] = pictureBox1.Width / 2; ///原点x坐标
            Origin_Position[1] = pictureBox1.Height;///原点y坐标

            Control.CheckForIllegalCrossThreadCalls = false;  //防止跨线程出错
            pictureBox1.MouseClick += PictureBox1_MouseClick;
            textBox3.TextChanged += TextBox3_TextChanged;
            textBox4.TextChanged += TextBox3_TextChanged;
        }

        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            textBox2.Text = "targetX:" + textBox3.Text + "targetY:" + textBox4.Text;
        }

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (checkBox1.Checked)
            {
                int _x = MousePosition.X - this.Location.X - pictureBox1.Location.X - groupBox1.Location.X;
                float x_mm = (float)_x / pictureBox1.Width * 5000;
                int _y = MousePosition.Y - this.Location.Y - pictureBox1.Location.Y - groupBox1.Location.Y - 40;
                float y_mm = (float)(pictureBox1.Height - _y) / pictureBox1.Height * 5000;
                textBox3.Text = x_mm + "";
                textBox4.Text = y_mm + "";
                Graphics g = pictureBox1.CreateGraphics();
                Pen pen = new Pen(Color.Red, 3);
                g.DrawEllipse(pen, _x, _y, 5, 5);
                g.Dispose();

            }
            if (checkBox3.Checked)
            {
                serialPort1.WriteLine(textBox2.Text);
            }
        }

        static int buffersize = 18;   //十六进制数的大小（假设为9Byte,可调整数字大小）
        byte[] buffer = new Byte[buffersize];   //创建缓冲区


        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Add("1200");
            comboBox1.Items.Add("2400");
            comboBox1.Items.Add("4800");
            comboBox1.Items.Add("9600");
            comboBox1.Items.Add("14400");
            comboBox1.Items.Add("19200");
            comboBox1.Items.Add("28800");
            comboBox1.Items.Add("38400");
            comboBox1.Items.Add("115200");//常用的波特率
            try
            {
                string[] ports = SerialPort.GetPortNames();//得到接口名字
                //将端口列表添加到comboBox
                this.comboBox2.Items.AddRange(ports);
                ///设置波特率
                serialPort1.BaudRate = Convert.ToInt32(comboBox1.Text);

            }
            catch (Exception ex)
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)//接收/暂停数据按钮
        {
            if (serialPort1.IsOpen)////(更新)如果按下按钮之前串口是开的，就断开//如果按下按钮之前 flag的内容是false 按下之后 内容改成true 然后打开串口
            {
                serialPort1.Close();
                button2.Text = "打开串口";
                groupBox2.Enabled = false;
            }
            else
            {
                //要打开串口要看波特率 串口等有没有设置对
                try
                {
                    serialPort1.BaudRate = Convert.ToInt32(comboBox1.SelectedItem);
                }
                catch (ArgumentException e1)
                {
                    this.errorProvider1.SetError(this.comboBox1, "不能为空");
                }
                try
                {
                    serialPort1.PortName = Convert.ToString(comboBox2.SelectedItem);
                }
                catch (ArgumentException e2)
                {
                    this.errorProvider1.SetError(this.comboBox2, "不能为空");
                }
                try
                {
                    serialPort1.Open();
                }
                catch
                {
                    MessageBox.Show("端口错误", "警告");
                }
                if (serialPort1.IsOpen)
                {
                    button2.Text = "断开连接";
                    groupBox2.Enabled = true;
                    this.errorProvider1.Clear();
                }
            }
        }

        //串口接收完成事件——接收所有数据
        string data_warehouse = "";
        public void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            //读取串口中的所有数据
            string readfromport = serialPort1.ReadExisting();

            //保存读取到的所有数据
            data_warehouse += readfromport;
            //把读到的数据显示在textbox
            if (checkBox2.Checked)
            { }//textBox1.ScrollToCaret(); }// textBox1.Text += readfromport; }
            else
                textBox1.AppendText(readfromport);

            //从读取到的数据中提取XYP
            if (huitu_flag) { tiquXYP(); }
        }

        string[] separators = { "X:", "Y:", "P:" };
        int pos = 0;
        private void tiquXYP()
        {
            while (data_warehouse.Length > 35)
            {
                //第一个换行符的位置
                pos = data_warehouse.IndexOf('\n');
                //断句第一个换行符
                //Debug.WriteLine("" + pos);
                string s = data_warehouse.Remove(pos).ToUpper();//错
                //删掉读到的第一句话
                data_warehouse = data_warehouse.Remove(0, pos + 1);
                //开始断句
                string[] words = s.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                //读取断句中的数据

                try
                {
                    x = Convert.ToDouble(words[0]);
                    y = Convert.ToDouble(words[1]);
                    p = Convert.ToDouble(words[2]);

                    //Debug.WriteLine(words[0]);
                    //Debug.WriteLine(words[1]);
                    //Debug.WriteLine(words[2]);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                }
            }
        }

        private bool huitu_flag = false;

        private void button4_Click(object sender, EventArgs e)//开始画图按钮
        {
            huitu_flag = !huitu_flag;
            if (huitu_flag)
            {
                //开始绘图
                if (data_warehouse.Length > 0)
                {

                }
                else
                {
                    MessageBox.Show("数据接受失败！", "警告");
                }

                pictureBox2.Image = imgofCar;
                pictureBox1.Image = imgofGroud;

                button4.Text = "停止绘图";

                timer1.Interval = 100;
                timer1.Start();

                pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
            }
            else
            {
                //停止绘图
                pictureBox2.Image = null;
                pictureBox1.Image = null;

                button4.Text = "开始绘图";

                timer1.Stop();
                pictureBox1.Paint -= new PaintEventHandler(pictureBox1_Paint);
            }
        }

        private void button3_Click(object sender, EventArgs e)//读取串口以及串口刷新
        {
            string[] ports = SerialPort.GetPortNames();
            this.comboBox2.Items.Clear();
            this.comboBox2.Items.AddRange(ports);
        }

        Image i = Image.FromFile("车.PNG");

        //!每隔100ms执行一次
        private void timer1_Tick(object sender, EventArgs e)//关于小车运动的计时器
        {
            //!按XYP数据绘制 pictureBox位置姿态
            pictureBox2.Left = Origin_Position[0] + Convert.ToInt16(x / 5000 * pictureBox1.Width);
            pictureBox2.Top = Origin_Position[1] - Convert.ToInt16(y / 5000 * pictureBox1.Height) - pictureBox2.Height / 2;//赋予小车坐标位置

            RotateFormCenter(pictureBox2, p);

            //!更新实际位置的groupbox数据
            label6.Text = x + "";
            label7.Text = y + "";
            label8.Text = p + "";

            pictureBox1.Refresh();

        }
 
        private void button6_Click(object sender, EventArgs e)
        {
            serialPort1.WriteLine(textBox2.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RotateFormCenter(Image image, double angle)//任意角度旋转的方法
        {
            Graphics g = Graphics.FromImage(image);
            Matrix x = new Matrix();
            PointF point = new PointF(image.Width / 2f, image.Height / 2f);
            x.RotateAt((float)angle, point);
            g.Transform = x;
            g.DrawImage(image, 0, 0);
            g.Dispose();
        }

        private void RotateFormCenter(PictureBox pb, double angle)//任意角度旋转的方法
        {
            pb.Image = null;
            i.Dispose();
            i = (Image)imgofCar.Clone();
            RotateFormCenter(i, angle);
            pb.Image = i;
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
        int dd = 0;
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            Point point2 = new Point();
            point2.X = pictureBox2.Location.X* pictureBox1.Image.Width / 500;
            point2.Y = pictureBox2.Location.Y* pictureBox1.Image.Height / 500;
            g.FillEllipse(Brushes.Red, point2.X, point2.Y, 5, 5);
        }
    }
}
