using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Praktika_WinForms_5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Обертка над Bitmap для быстрого чтения и изменения пикселов.
        /// Также, класс контролирует выход за пределы изображения: при чтении за границей изображения - возвращает DefaultColor, при записи за границей изображения - игнорирует присвоение.
        /// </summary>
        public class ImageWrapper : IDisposable, IEnumerable<Point>
        {
            /// <summary>
            /// Ширина изображения
            /// </summary>
            public int Width { get; private set; }
            /// <summary>
            /// Высота изображения
            /// </summary>
            public int Height { get; private set; }
            /// <summary>
            /// Цвет по-умолачнию (используется при выходе координат за пределы изображения)
            /// </summary>
            public Color DefaultColor { get; set; }

            private byte[] data;//буфер исходного изображения
            private byte[] outData;//выходной буфер
            private int stride;
            private BitmapData bmpData;
            private Bitmap bmp;

            /// <summary>
            /// Создание обертки поверх bitmap.
            /// </summary>
            /// <param name="copySourceToOutput">Копирует исходное изображение в выходной буфер</param>
            public ImageWrapper(Bitmap bmp, bool copySourceToOutput = false)
            {
                Width = bmp.Width;
                Height = bmp.Height;
                this.bmp = bmp;

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                stride = bmpData.Stride;

                data = new byte[stride * Height];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, data, 0, data.Length);

                outData = copySourceToOutput ? (byte[])data.Clone() : new byte[stride * Height];
            }

            /// <summary>
            /// Возвращает пиксел из исходнго изображения.
            /// Либо заносит пиксел в выходной буфер.
            /// </summary>
            public Color this[int x, int y]
            {
                get
                {
                    var i = GetIndex(x, y);
                    return i < 0 ? DefaultColor : Color.FromArgb(data[i + 3], data[i + 2], data[i + 1], data[i]);
                }

                set
                {
                    var i = GetIndex(x, y);
                    if (i >= 0)
                    {
                        outData[i] = value.B;
                        outData[i + 1] = value.G;
                        outData[i + 2] = value.R;
                        outData[i + 3] = value.A;
                    };
                }
            }

            /// <summary>
            /// Возвращает пиксел из исходнго изображения.
            /// Либо заносит пиксел в выходной буфер.
            /// </summary>
            public Color this[Point p]
            {
                get { return this[p.X, p.Y]; }
                set { this[p.X, p.Y] = value; }
            }

            /// <summary>
            /// Заносит в выходной буфер значение цвета, заданные в double.
            /// Допускает выход double за пределы 0-255.
            /// </summary>
            public void SetPixel(Point p, double r, double g, double b)
            {
                if (r < 0) r = 0;
                if (r >= 256) r = 255;
                if (g < 0) g = 0;
                if (g >= 256) g = 255;
                if (b < 0) b = 0;
                if (b >= 256) b = 255;

                this[p.X, p.Y] = Color.FromArgb((int)r, (int)g, (int)b);
            }

            int GetIndex(int x, int y)
            {
                return (x < 0 || x >= Width || y < 0 || y >= Height) ? -1 : x * 4 + y * stride;
            }

            /// <summary>
            /// Заносит в bitmap выходной буфер и снимает лок.
            /// Этот метод обязателен к исполнению (либо явно, лмбо через using)
            /// </summary>
            public void Dispose()
            {
                System.Runtime.InteropServices.Marshal.Copy(outData, 0, bmpData.Scan0, outData.Length);
                bmp.UnlockBits(bmpData);
            }

            /// <summary>
            /// Перечисление всех точек изображения
            /// </summary>
            public IEnumerator<Point> GetEnumerator()
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        yield return new Point(x, y);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            
        }

        string filename;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            filename = textBox1.Text;
            if (System.IO.File.Exists(filename))
            {
                
                pictureBox1.Image = Image.FromFile(filename);
                textBox1.Text = filename;
                radioButton1_CheckedChanged(sender, e);
            }
            else
            {
                MessageBox.Show("Не удалось открыть изображение");
            }
            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Bitmap image = (Bitmap)Image.FromFile(filename); 

            using (var wr = new ImageWrapper(image))
                foreach (var p in wr)
                    wr.SetPixel(p, 0, 0, wr[p].B);
            pictureBox2.Image = image;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
           Bitmap image = (Bitmap)Image.FromFile(filename);

            using (var wr = new ImageWrapper(image))
                foreach (var p in wr)
                    wr.SetPixel(p, wr[p].R*2, wr[p].G*2, wr[p].B*2);
            pictureBox2.Image = image;
        }

        
    }
}
