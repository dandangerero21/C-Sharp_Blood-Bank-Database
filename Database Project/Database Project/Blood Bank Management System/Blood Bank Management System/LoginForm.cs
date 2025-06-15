using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blood_Bank_Management_System
{
    public partial class LoginForm : Form
    {

        /*ang following code ay nakita ko sa StackOverflow (best site) dahil naghahanap ako
         * ng paraan para idrag ang isang form na walang form border style*/
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public LoginForm()
        {
            InitializeComponent();
        }

        DatabaseConnection loginConnect;
        string connectionstring;

        public DataSet mydataset;
        public DataRow rows;

        int maxrow;

        

        private void btnLogin_Click(object sender, EventArgs e)
        {

            if (mydataset.Tables.Count >= 2)
            {
                //dito nakalagay login info
                DataTable secondTable = mydataset.Tables[1];

                if (secondTable.Rows.Count > 0)
                {
                    //para mascan lahat kung may tumama na data sa table
                    foreach (DataRow rows in secondTable.Rows)
                    {
                        string username = rows["Username"].ToString(); 
                        string password = rows["Password"].ToString(); 

                        if (tbusername.Text == username && tbpassword.Text == password)
                        {
                            //pag tumama ang password at username (nakalagay sa credentials table), magsshow ang success at lalabas ang splash screen
                            MessageBox.Show("Login successful.");
                            this.Hide();
                            Thread t = new Thread(new ThreadStart(StartForm));
                            t.Start();
                            Thread.Sleep(5000);
                            Form1 f2 = new Form1();
                            t.Abort();
                            //pagkatapos ng splash screen lalabas na ang mainform
                            f2.ShowDialog();

                            this.Close();
                        }
                    }

                    MessageBox.Show("No exact credentials found.");
                }
                else
                {
                    MessageBox.Show("Second table is empty.");
                }
            }
        }

        public void StartForm()
        {

            Application.Run(new splashscreen());

        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            try //pangconnect sa credentials table
            {
                loginConnect = new DatabaseConnection();
                connectionstring = Properties.Settings.Default.myconstring;

                loginConnect.connection_string = connectionstring;

                loginConnect.Sql = Properties.Settings.Default.loginsql;

                mydataset = loginConnect.GetConnection;

                maxrow = mydataset.Tables[1].Rows.Count;

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //close window
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //minimize window
            this.WindowState = FormWindowState.Minimized;
        }

        private void LoginForm_MouseDown(object sender, MouseEventArgs e)
        {
            //iscan kung nakadrag sa left mouse button
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

    }
}
