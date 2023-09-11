using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows;
using System.Windows.Interop;


namespace WpfApp1
{
    public class CAPTCHAHelper
    {
        public static Bitmap CreateVerificationCode(out string code)
        {
            Bitmap bitmap = new Bitmap(200, 400);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.FillRectangle(new SolidBrush(System.Drawing.Color.Transparent), 0, 0, 200, 400);
            Font font = new Font(System.Drawing.FontFamily.GenericSerif, 48, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
            Random r = new Random();
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder sb = new StringBuilder();


            for(int x = 0; x< 5; x++)
            {
                string letter = letters.Substring(r.Next(0, letters.Length - 1), 1);
                sb.Append(letter);
                graphics.DrawString(letter, font, new SolidBrush(System.Drawing.Color.Black), x * 38, r.Next(0, 15));
            }
            code = sb.ToString();


            System.Drawing.Pen linepen = new System.Drawing.Pen(new SolidBrush(System.Drawing.Color.Black), 2);
            for(int x = 0; x < 6; x++)
            {
                graphics.DrawLine(linepen, new System.Drawing.Point(r.Next(0, 199), r.Next(0, 59)), new System.Drawing.Point(r.Next(0, 199), r.Next(0, 59)));
            }
            return bitmap;
        }
    }


    public class ImageFormatConvertHelper
    {
        [DllImport("gdi32.dll", SetLastError = true)]

        private static extern bool DeleteObject(IntPtr hobject);

        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }
    }
}
