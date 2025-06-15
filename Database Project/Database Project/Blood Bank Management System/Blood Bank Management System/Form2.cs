using Blood_Bank_Management_System.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Printing;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blood_Bank_Management_System
{
    public partial class Form2 : Form
    {

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        //ito ang sasalo ng data natin galing sa kabilang form para madisplay sa form na ito
        public void DisplayData(string tbfirstnametext, string tbsurnametext, string lbbloodtypetext, string tbagetext, Image donorphotoimage, DateTime selectedDate, string lbdonatetext)
        {
            tbfirstname.Text = tbfirstnametext;
            tbsurname.Text = tbsurnametext;
            lbbloodtype.Text = lbbloodtypetext;
            tbage.Text = tbagetext;
            donorphoto.Image = donorphotoimage;
            datedonated.Value = selectedDate;
            lbdonate.Text = lbdonatetext;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //call sa function kasama ang paneltoprint na ipprint natin
            PrintPanel(paneltoprint);
        }

        private void PrintPanel(Panel mypanel)
        {
            PrintDocument PrintDoc = new PrintDocument();
            PrintDoc.PrintPage += (sender, e) =>
            {
                Bitmap bmp = new Bitmap(paneltoprint.Width, paneltoprint.Height);
                paneltoprint.DrawToBitmap(bmp, new Rectangle(0, 0, paneltoprint.Width, paneltoprint.Height));

                //para makuha printable area ng papel
                Rectangle printArea = e.MarginBounds;

                //pangcalculate ng printable area natin
                float scaleX = (float)printArea.Width / bmp.Width;
                float scaleY = (float)printArea.Height / bmp.Height;
                float scale = Math.Min(scaleX, scaleY);

                //scaled size ng ipprint natin
                int scaledWidth = (int)(bmp.Width * scale);
                int scaledHeight = (int)(bmp.Height * scale);

                //high resolution image gamit scaled size natin
                //although blurred pa rin, mas malinaw sya kesa sa una kong prototype
                Bitmap highResBmp = new Bitmap(scaledWidth, scaledHeight);

                highResBmp.SetResolution(900, 900);
                //900dpi resolution

                using (Graphics graphics = Graphics.FromImage(highResBmp))
                {
                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphics.DrawImage(bmp, 0, 0, scaledWidth, scaledHeight);
                }

                e.Graphics.DrawImage(highResBmp, printArea);
            };

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = PrintDoc;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                PrintDoc.Print();
            }
        }

    }
}
