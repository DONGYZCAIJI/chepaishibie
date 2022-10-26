using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSGeo.GDAL;//gdal根本用不上，一个graphic就行
using OSGeo.OGR;
using OSGeo.OSR;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using OpenCvSharp;
using System.Drawing;
using OpenCvSharp.Extensions;



namespace chepaishibie
{
	public partial class Form1 : Form
	{
	
		public Form1()
		{

			InitializeComponent();
		}
		private double ratio = 1;        // 图片的起始显示比例
		private double ratioStep = 0.1;
		private System.Drawing.Size pic_size;
		private int xPos;
		private int yPos;
		//现在就是一放大，中心跑了不说，太小还不能拖动，非常恼火
		private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				ratio += ratioStep;
				if (ratio > 3) // 放大上限？为什么要有上线
					ratio = 3;
				else
				{
					this.changePictureBox1Size(ratio);
				}
			}
			else
			{
				ratio -= ratioStep;
				if (ratio < 0.5)  // 放大下限
					ratio = 0.5;
				else
				{
					this.changePictureBox1Size(ratio);
				}
			}
		}
		private void changePictureBox1Size(double ratio)
		{
			var t = pictureBox1.Size;
			t.Width = Convert.ToInt32(pic_size.Width * ratio);
			t.Height = Convert.ToInt32(pic_size.Height * ratio);
			pictureBox1.Size = t;
			//point location应该就是这个代码了
			System.Drawing.Point location = new System.Drawing.Point();
			//完美解决哈哈哈哈，聪明小董上线
			location.Y = (this.splitContainer1.Panel1.Height - this.pictureBox1.Height) / 2;
			location.X = (this.splitContainer1.Panel1.Width - this.pictureBox1.Width) / 2;
			this.pictureBox1.Location = location;
		}
		private void Form1_Load(object sender, EventArgs e)
		{
			pic_size = pictureBox1.Size;
			this.ActiveControl = this.pictureBox1;
			pictureBox1.MouseWheel += new MouseEventHandler(pictureBox1_MouseWheel);
			pictureBox2.MouseWheel += new MouseEventHandler(pictureBox2_MouseWheel);
		}
		//既然如此，那就把pictureBox2的也写一下，毕竟重点在这里。代码重复度太大
		private void changePictureBox2Size(double ratio)
		{
			var t = pictureBox2.Size;
			t.Width = Convert.ToInt32(pic_size.Width * ratio);
			t.Height = Convert.ToInt32(pic_size.Height * ratio);
			pictureBox2.Size = t;
			//point location应该就是这个代码了
			System.Drawing.Point location = new System.Drawing.Point();
			//完美解决哈哈哈哈，聪明小董上线
			location.Y = (this.splitContainer1.Panel2.Height - this.pictureBox2.Height) / 2;
			location.X = (this.splitContainer1.Panel2.Width - this.pictureBox2.Width) / 2;
			this.pictureBox2.Location = location;
		}
		private void pictureBox2_MouseWheel(object sender, MouseEventArgs e)
		{
			if (e.Delta > 0)
			{
				ratio += ratioStep;
				if (ratio > 3) // 放大上限？为什么要有上线
					ratio = 3;
				else
				{
					this.changePictureBox2Size(ratio);
				}
			}
			else
			{
				ratio -= ratioStep;
				if (ratio < 0.5)  // 放大下限
					ratio = 0.5;
				else
				{
					this.changePictureBox2Size(ratio);
				}
			}
		}
		//先写简单的关闭窗体
		private void toolStripMenuItem8_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			DialogResult dlgR = MessageBox.Show("确实要退出本程序吗？", "温馨提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (dlgR == DialogResult.Cancel)
				e.Cancel = true;
		}
		//打开文件，我觉得我需要一个picturebox

		Bitmap bm;
		//底下是啥变量也不知道
		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			OpenFileDialog diyige = new OpenFileDialog();
			diyige.Filter = "图片|*.gif;*.jpg;*.jpeg;*.bmp;*.jfif;*.png;";
			//pictureBox1.Dock = DockStyle.Fill;//设置图片完全填充
			if (diyige.ShowDialog() == DialogResult.OK)
			{
				try
				{//把图片读进来
					bm = new Bitmap(diyige.FileName.ToString());
					pictureBox1.Image = bm;
					pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
		}
		//这个是不小心点出来的，不用管
		private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
		{

		}
		//再打开一遍
		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			toolStripButton1_Click(sender, e);

		}
		//来保存图片了，那当然要用savefiledialog
		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveDlg = new SaveFileDialog();
			saveDlg.Title = "图片保存";
			saveDlg.Filter = "jpg图片|*.JPG|gif图片|*.GIF|png图片|*.PNG|jpeg图片|*.JPEG|BMP图片|*.BMP";//文件类型过滤，只可选择图片的类型
			saveDlg.FilterIndex = 1;//设置默认文件类型显示顺序 
			saveDlg.RestoreDirectory = true; //OpenFileDialog与SaveFileDialog都有RestoreDirectory属性，这个属性默认是false，打开一个文件后，那么系统默认目录就会指向刚才打开的文件。如果设为true就会使用系统默认目录
			if (pictureBox2.Image != null)
			{
				if (saveDlg.ShowDialog() == DialogResult.OK)
				{
					try
					{
						string fileName = saveDlg.FileName.ToString();
						if (fileName != "" && fileName != null)//不为空才打开
						{
							string fileExtName = fileName.Substring(fileName.LastIndexOf(".") + 1).ToString();
							System.Drawing.Imaging.ImageFormat imgformat = null;
							if (fileExtName != "")
							{
								switch (fileExtName)
								{
									case "jpg":
										imgformat = System.Drawing.Imaging.ImageFormat.Jpeg;
										break;
									case "png":
										imgformat = System.Drawing.Imaging.ImageFormat.Png;
										break;
									case "gif":
										imgformat = System.Drawing.Imaging.ImageFormat.Gif;
										break;
									case "bmp":
										imgformat = System.Drawing.Imaging.ImageFormat.Bmp;
										break;
									default:
										imgformat = System.Drawing.Imaging.ImageFormat.Jpeg;
										break;
								}
								try
								{
									MessageBox.Show("保存路径：" + fileName, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
									pictureBox2.Image.Save(fileName, imgformat);
								}
								catch
								{
									MessageBox.Show("图片保存失败！");
								}

							}
						}
					}
					catch (Exception)
					{

						MessageBox.Show("大哥，pictureBox2里真没有图片，别搁那乱点");//找点乐子
					}
				}
			}
            else
			{
				MessageBox.Show("大哥，pictureBox2里真没有图片，别搁那乱点");//找点乐子
			}
		}

		//红色通道的提取，这个的思路是首先生成一个bitmap类，也就是图像嘛，把想要的颜色全提完后，setpixel一下即可，
		private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
			if (pictureBox1.Image != null)
			{
				Bitmap srcBitmap = new Bitmap(pictureBox1.Image);
				Color srcColor;//待会要放东西的，急什么
				int wide = srcBitmap.Width;
				int height = srcBitmap.Height;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < wide; x++)
					{
						//获取像素的ＲＧＢ颜色值
						srcColor = srcBitmap.GetPixel(x, y);
						byte temp = (byte)(srcColor.R);//red
						//设置像素的ＲＧＢ颜色值
						srcBitmap.SetPixel(x, y, Color.FromArgb(temp, temp, temp));
					}
				}
				pictureBox2.Image = srcBitmap;
				pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
			}
			else
			{
				MessageBox.Show("你丫的picturebox1都没图片点毛线通道啊，你怎么和何波似的？");
			}
		}
		//绿色的，何波能不能死啊草拟的嘛
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
			if (pictureBox1.Image != null)
			{
				Bitmap srcBitmap = new Bitmap(pictureBox1.Image);
				Color srcColor;//待会要放东西的，急什么
				int wide = srcBitmap.Width;
				int height = srcBitmap.Height;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < wide; x++)
					{
						//获取像素的ＲＧＢ颜色值
						srcColor = srcBitmap.GetPixel(x, y);
						byte temp = (byte)(srcColor.G);//green
						//设置像素的ＲＧＢ颜色值
						srcBitmap.SetPixel(x, y, Color.FromArgb(temp, temp, temp));
					}
				}
				pictureBox2.Image = srcBitmap;
				pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
			}
			else
			{
				MessageBox.Show("你丫的picturebox1都没图片点毛线通道啊，你怎么和何波似的？");
			}
		}
		//蓝色的，草拟的嘛何波
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
			if (pictureBox1.Image != null)
            {
				Bitmap srcBitmap = new Bitmap(pictureBox1.Image);
				Color srcColor;//待会要放东西的，急什么
				int wide = srcBitmap.Width;
				int height = srcBitmap.Height;
				for (int y = 0; y < height; y++)
				{
					for (int x = 0; x < wide; x++)
					{
						//获取像素的ＲＧＢ颜色值
						srcColor = srcBitmap.GetPixel(x, y);
						byte temp = (byte)(srcColor.B);//blue
						//设置像素的ＲＧＢ颜色值
						srcBitmap.SetPixel(x, y, Color.FromArgb(temp, temp, temp));
					}
				}
				pictureBox2.Image = srcBitmap;
				pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            }
			else
            {
				MessageBox.Show("你丫的picturebox1都没图片点毛线通道啊，你怎么和何波似的？");
            }
			
		}
		//保存文件
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
			toolStripButton2_Click(sender, e);

		}
        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
		}
		//关于鼠标滚轮滑动使pictureBox中的图片方法缩小。
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
			try
			{
				// 鼠标按下拖拽图片
				if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
				{
					// 限制拖拽出框
					if ((pictureBox1.Width - this.Width) >= 0 || (pictureBox1.Height - this.Height) >= 0)
					{
						if ((pictureBox1.Top + Convert.ToInt16(e.Y - yPos)) <= 0
							|| (pictureBox1.Left + Convert.ToInt16(e.X - xPos)) <= 0
							|| (pictureBox1.Right + Convert.ToInt16(e.X - xPos)) >= this.Width
							|| (pictureBox1.Bottom + Convert.ToInt16(e.Y - yPos)) >= this.Height)
						{
							pictureBox1.Left += Convert.ToInt16(e.X - xPos);//设置x坐标.
							pictureBox1.Top += Convert.ToInt16(e.Y - yPos);//设置y坐标.
						}
					}
				}
			}
			catch (Exception dd)
			{
				MessageBox.Show(dd.Message);
			}
		}
		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			xPos = e.X;//当前x坐标.
			yPos = e.Y;//当前y坐标.
		}
        //pictureBox2的拖放
        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
			try
			{
				// 鼠标按下拖拽图片
				if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
				{
					// 限制拖拽出框
					if ((pictureBox2.Width - this.Width) >= 0 || (pictureBox2.Height - this.Height) >= 0)
					{
						if ((pictureBox2.Top + Convert.ToInt16(e.Y - yPos)) <= 0
							|| (pictureBox2.Left + Convert.ToInt16(e.X - xPos)) <= 0
							|| (pictureBox2.Right + Convert.ToInt16(e.X - xPos)) >= this.Width
							|| (pictureBox2.Bottom + Convert.ToInt16(e.Y - yPos)) >= this.Height)
						{
							pictureBox2.Left += Convert.ToInt16(e.X - xPos);//设置x坐标.
							pictureBox2.Top += Convert.ToInt16(e.Y - yPos);//设置y坐标.
						}
					}
				}
			}
			catch (Exception dd)
			{
				MessageBox.Show(dd.Message);
			}
		}
        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
			xPos = e.X;//当前x坐标.
			yPos = e.Y;//当前y坐标.
        }//结束了准备工作，要真正开始算法了

		/// <summary>
		/// 二值化
		/// </summary>
		/// <returns></returns>
		//动真格的了，二值化
		//确实调包了，不掉包也用不来啊
		private void toolStripMenuItem10_Click_1(object sender, EventArgs e)
		{
			if (pictureBox2.Image != null)
			{
				try
				{
					Bitmap bitmap = new Bitmap(pictureBox2.Image);
					Mat mat = BitmapConverter.ToMat(bitmap);
					Mat temp1 = new Mat();
					Mat temp2 = new Mat();
					Cv2.CvtColor(mat, temp1, ColorConversionCodes.RGBA2RGB);
					Cv2.CvtColor(temp1, temp2, ColorConversionCodes.RGB2GRAY);
					Cv2.Threshold(temp2, temp2, 0, 255, ThresholdTypes.Otsu);
					Bitmap bitmap1 = BitmapConverter.ToBitmap(temp2);
					pictureBox2.Image = bitmap1;
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message + '!');
				}
			}
			else MessageBox.Show("能不能看清楚再点");

		}
		private void toolStripMenuItem11_Click(object sender, EventArgs e)
        {
			try
			{
				int Height = this.pictureBox2.Image.Height;
				int Width = this.pictureBox2.Image.Width;
				//新建了一个和pictureBox2一样的image
				Bitmap newBitmap = new Bitmap(Width, Height);
				Bitmap oldBitmap = (Bitmap)this.pictureBox2.Image;
				Color pixel;
				//sobel算子模板
				int[] Sobelx = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
				int[] Sobely = { 1, 2, 1, 0, 0, 0, -1, -2, -1 };
				//
				for (int x = 1; x < Width - 1; x++)
					for (int y = 1; y < Height - 1; y++)
					{
						int rx = 0, gx = 0, bx = 0;
						int ry = 0, gy = 0, by = 0;
						int rz = 0, gz = 0, bz = 0;
						int Index = 0;
						for (int col = -1; col <= 1; col++)
							for (int row = -1; row <= 1; row++)
							{
								pixel = oldBitmap.GetPixel(x + row, y + col);
								//x
								rx += pixel.R * Sobelx[Index];
								gx += pixel.G * Sobelx[Index];
								bx += pixel.B * Sobelx[Index];
								//y
								ry += pixel.R * Sobely[Index];
								gy += pixel.G * Sobely[Index];
								by += pixel.B * Sobely[Index];
								//z
								rz += pixel.R * Sobelx[Index];
								gz += pixel.G * Sobelx[Index];
								bz += pixel.B * Sobelx[Index];
								Index++;
							}
						//处理颜色值溢出x
						rx = Math.Abs(rx) > 255 ? 255 : rx;
						rx = rx < 0 ? 0 : Math.Abs(rx);
						gx = gx > 255 ? 255 : gx;
						gx = gx < 0 ? 0 : gx;
						bx = bx > 255 ? 255 : bx;
						bx = bx < 0 ? 0 : bx;
						//处理颜色值溢出y
						ry = ry > 255 ? 255 : ry;
						ry = ry < 0 ? 0 : ry;
						gy = gy > 255 ? 255 : gy;
						gy = gy < 0 ? 0 : gy;
						by = by > 255 ? 255 : by;
						by = by < 0 ? 0 : by;
						//处理颜色值溢出z
						rz = rz > 255 ? 255 : rz;
						rz = rz < 0 ? 0 : rz;
						gz = gz > 255 ? 255 : gz;
						gz = gz < 0 ? 0 : gz;
						bz = bz > 255 ? 255 : bz;
						bz = bz < 0 ? 0 : bz;
						//计算总的G
						int Gr;// Gb, Gg;
						Gr = (int)Math.Sqrt(rx * rx + ry * ry); //Gb = (int)Math.Sqrt(bx * bx + by * by); Gg = (int)Math.Sqrt(gx * gx + gy * gy);
																//重新设置Gr,Gg,Gb
						Gr = Gr > 255 ? 255 : Gr;
						//Gr = Gr < 0 ? 0 : Gr;
						//Gg = Gg > 255 ? 255 : Gg;
						//Gg = Gg < 0 ? 0 : Gg;
						//Gb = Gb > 255 ? 255 : Gb;
						//Gb = Gb < 0 ? 0 : Gb;
						newBitmap.SetPixel(x - 1, y - 1, Color.FromArgb(Gr, Gr, Gr));
					}
				//展示将图片处理后的大小显示在框中
				this.pictureBox2.Image = newBitmap;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "信息提示");
			}
		}
		//图像压缩
        private void toolStripMenuItem12_Click(object sender, EventArgs e)
        {
			try
            {
				bm = new Bitmap(pictureBox1.Image);
				bm = ZoomImage(bm, pictureBox1.Height, pictureBox1.Width);  //缩放输入图像大小           
				pictureBox1.Image = bm;
				MessageBox.Show("图像压缩成功");
            }
			catch(Exception)
            {
				MessageBox.Show("可能你的图片并未导入");
            }

		}
		//zoomimage函数，是用来压缩图像的
		private Bitmap ZoomImage(Bitmap bitmap, int destHeight, int destWidth)
		{
			try
			{
				System.Drawing.Image sourImage = bitmap;
				int width = 0, height = 0;
				//按比例缩放
				int sourWidth = sourImage.Width;
				int sourHeight = sourImage.Height;
				if (sourHeight > destHeight || sourWidth > destWidth)
				{
					if ((sourWidth * destHeight) > (sourHeight * destWidth))
					{
						width = destWidth;
						height = (destWidth * sourHeight) / sourWidth;
					}
					else
					{
						height = destHeight;
						width = (sourWidth * destHeight) / sourHeight;
					}
				}
				else
				{
					width = sourWidth;
					height = sourHeight;
				}
				Bitmap destBitmap = new Bitmap(destWidth, destHeight);
				Graphics g = Graphics.FromImage(destBitmap);
				g.Clear(Color.Transparent);
				//设置画布的描绘质量
				g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.DrawImage(sourImage, new Rectangle((destWidth - width) / 2, (destHeight - height) / 2, width, height), 0, 0, sourImage.Width, sourImage.Height, GraphicsUnit.Pixel);
				g.Dispose();
				//设置压缩质量
				System.Drawing.Imaging.EncoderParameters encoderParams = new System.Drawing.Imaging.EncoderParameters();
				long[] quality = new long[1];
				quality[0] = 8;
				System.Drawing.Imaging.EncoderParameter encoderParam = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
				sourImage.Dispose();
				return destBitmap;
			}
			catch
			{
				return bitmap;
			}

		}
		//图像裁剪，把图像裁剪一部分来突出车牌
		private Image CropImage(Image originImage, Rectangle region)
		{
			Bitmap result = new Bitmap(region.Width, region.Height);
			Graphics graphics = Graphics.FromImage(result);
			graphics.DrawImage(originImage, new Rectangle(0, 0, region.Width, region.Height), region, GraphicsUnit.Pixel);
			return result;
		}
		private void toolStripMenuItem13_Click(object sender, EventArgs e)
        {
			//创建矩形区域
			int qix, qiy, mox, moy = 0;
			bm = new Bitmap(pictureBox1.Image);
			if (bm.Height > bm.Width)
			{
				qix = bm.Width * 3 / 16;
				qiy = bm.Height * 4 / 9;
				mox = bm.Width * 2 / 3;
				moy = bm.Height * 2 / 3;
				Rectangle cropRegion = new Rectangle(qix, qiy, mox, moy);
				pictureBox1.Image = CropImage(pictureBox1.Image, cropRegion);
				pictureBox1.Dock = DockStyle.Fill;
				pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			}
			else
			{
				qix = bm.Width * 2 / 5;
				qiy = bm.Height * 3 / 12;
				mox = bm.Width * 15 / 30;
				moy = bm.Height * 9 / 12;
				Rectangle cropRegion = new Rectangle(qix, qiy, mox, moy);
				pictureBox1.Image = CropImage(pictureBox1.Image, cropRegion);
				pictureBox1.Dock = DockStyle.Fill;
				pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
			}
		}
    }
}

