using Blood_Bank_Management_System.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blood_Bank_Management_System
{
    public partial class Form1 : Form
    {

        //pang drag ng borderless forms (explanation sa LoginForm.cs)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public Form1()
        { 
            InitializeComponent();
            
        }
        DatabaseConnection Connect;
        string connectionstring;

        public DataSet placer;
        public DataRow rows;

        int maxrow, inc = 0;


        public void updategridview()
        {
            // TODO: This line of code loads data into the 'database1DataSet.BloodBankTable' table. You can move, or remove it, as needed.

            // ginawa ko na lang function dahil icacallout ko naman every time mag save, update, or delete ako para madali na lang
            this.bloodBankTableTableAdapter.Fill(this.database1DataSet.BloodBankTable);
        }

        //ang code sa baba ay pangconvert ng inupload na photo sa byte, dahil tinry ko gumamit ng Image datatype sa database, nag eerror,
        // kaya ito na lang ginawa ko
        public void imageToByte(DataRow rows) {
            if (donorphoto.Image != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    donorphoto.Image.Save(ms, donorphoto.Image.RawFormat);
                    rows["imagery"] = ms.ToArray();
                }
            }
            else
            {
                rows[5] = DBNull.Value;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            updategridview();

            try
            {
                //typical database connection
                Connect = new DatabaseConnection();
                connectionstring = Properties.Settings.Default.DonorCon;

                Connect.connection_string = connectionstring;

                Connect.Sql = Properties.Settings.Default.SQL;

                placer = Connect.GetConnection;

                maxrow = placer.Tables["BloodBankTable"].Rows.Count;

                AddCounterColumn();
                UpdateNameCounters();

                checkrecords();
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }


        }

        //pang select ng photo na gustong iupload
        public void btnbrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif)|*.jpg; *.jpeg; *.png; *.gif|All files (*.*)|*.*";
            openFileDialog.Title = "Select a Photo";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFile = openFileDialog.FileName;

                donorphoto.ImageLocation = selectedFile;
            }

        }
        
        private void btnNext_Click(object sender, EventArgs e)
        {
            
                //galing sa handout
                if (inc != maxrow - 1)
                {
                    inc++;
                    checkrecords();
                }
                else
                {
                    MessageBox.Show("No more next record.");
                }

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //handout again
            rows = placer.Tables["BloodBankTable"].NewRow();
            rows[1] = tbfirstname.Text;
            rows[2] = tbsurname.Text;
            rows[3] = lbbloodtype.Text;
            rows[4] = tbage.Text;

            //call sa function pangconvert image to byte
            imageToByte(rows);

            //pangkuha value ng datetimepicker
            DateTime selectedDate = datedonated.Value;
                rows[6] = selectedDate;



                placer.Tables["BloodBankTable"].Rows.Add(rows);
            try
            {
                Connect.UpdateDatabase(placer);
                //nag eerror sakin pag nagaadd new data tapos update or kaya delete
                //sabi ay DeleteCommand affected 0 of 1 records, kaya ginamitan ko ng AcceptChanges at ReloadDataSet
                //Courtesy: StackOverflow
                placer.AcceptChanges();
                ReloadDataSet();
                maxrow = placer.Tables["BloodBankTable"].Rows.Count;
                inc = maxrow - 1;
                rows["Counter"] = 1;
                
                MessageBox.Show("Data added.");

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }
            finally
            {
                //para di na makapindot ng kung ano ano, pangit tingnan pag nacclick parin
                btnCancel.Enabled = false;
                btnSave.Enabled = false;
                btnAdd.Enabled = true;
                tbfirstname.Enabled = false;
                tbsurname.Enabled = false;
                lbbloodtype.Enabled = false;
                tbage.Enabled = false;
                btnPrevious.Enabled = true;
                btnNext.Enabled = true;
                datedonated.Enabled = false;
                //iupdate ang gridview para makita realtime
                updategridview();
                UpdateNameCounters();
                checkrecords();
            }
        }

        private void AddCounterColumn()
        {
            if (!placer.Tables["BloodBankTable"].Columns.Contains("Counter"))
            {
                placer.Tables["BloodBankTable"].Columns.Add("Counter", typeof(int));
            }
        }

        Dictionary<string, int> nameCounters = new Dictionary<string, int>();

        //ito ay pangcheck if ilang beses na nakapagdonate si donor.
        private void UpdateNameCounters()
        {

            nameCounters.Clear();
            foreach (DataRow row in placer.Tables["BloodBankTable"].Rows)
            {
                string nameKey = row["First Name"].ToString() + row["Last Name"].ToString() + row["Blood Type"].ToString();

                int counter = 0;
                if (nameCounters.ContainsKey(nameKey))
                {
                    counter = nameCounters[nameKey];
                }

                counter++;
                nameCounters[nameKey] = counter;
                row["Counter"] = counter;
            }

             

        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            
                //handout
                if (inc > 0)
                {
                    inc--;
                    checkrecords();
                }
                else
                {
                    MessageBox.Show("No more previous record.");
                }

        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            //handout again
            tbfirstname.Clear();
            tbsurname.Clear();
            lbbloodtype.Text = "";
            tbage.Clear();
            donorphoto.Image = null;
            datedonated.Enabled = false;
            btnCancel.Enabled = true;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            tbfirstname.Enabled = true;
            tbsurname.Enabled = true;
            lbbloodtype.Enabled = true;
            tbage.Enabled = true;
            btnAdd.Enabled = false;
            btnSave.Enabled = true;
            btnCancel.Enabled = true;
            btnbrowse.Enabled = true;
            datedonated.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            //handout
            checkrecords();
            btnCancel.Enabled = false;
            btnSave.Enabled = false;
            btnAdd.Enabled = true;
            btnPrevious.Enabled = true;
            btnNext.Enabled = true;
            btnbrowse.Enabled = false;
            tbfirstname.Enabled = false;
            tbage.Enabled = false;
            tbsurname.Enabled = false;
            lbbloodtype.Enabled = false;
            btnUpdate.Enabled = false;
            btnInfo.Enabled = true;
        }

        private void tbphoto_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            //pang update incase may babaguhin sa data
            rows = placer.Tables["BloodBankTable"].Rows[inc];

            rows[1] = tbfirstname.Text;
            rows[2] = tbsurname.Text;
            rows[3] = lbbloodtype.Text;
            rows[4] = tbage.Text;
            try
            {
                imageToByte(rows);
            }
            catch
            {

            }

            DateTime selectedDate = datedonated.Value;
            rows[6] = selectedDate;


            try
            {
                Connect.UpdateDatabase(placer);
                placer.AcceptChanges();
                UpdateNameCounters();
                MessageBox.Show("Record updated.");
            } catch (Exception err){
                MessageBox.Show(err.Message);
            } finally
            {
                datedonated.Enabled = true;
                btnNext.Enabled = true;
                btnPrevious.Enabled = true;
                btnAdd.Enabled = true;
                btnUpdate.Enabled = false;
                btnInfo.Enabled = true;
                btnbrowse.Enabled = false;
                tbage.Enabled = false;
                tbfirstname.Enabled = false;
                tbsurname.Enabled = false;
                lbbloodtype.Enabled = false;
                datedonated.Enabled = false;
                btnCancel.Enabled = false;
                updategridview();

            }


        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            //bago makapag update data, need muna iclick itong Edit Info.
            datedonated.Enabled = true;
            btnInfo.Enabled = false;
            tbfirstname.Enabled = true;
            tbsurname.Enabled = true;
            lbbloodtype.Enabled = true;
            tbage.Enabled = true;
            btnUpdate.Enabled = true;
            btnbrowse.Enabled = true;
            btnAdd.Enabled = false;
            btnPrevious.Enabled = false;
            btnNext.Enabled = false;
            btnSave.Enabled = false;
            btnCancel.Enabled = true;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            //confirmation para sure na idedelete
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Are you sure to delete the current data?", "Caution", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    //delete data,AcceptChanges, reload dataset at update ng bagong gridview
                     placer.Tables["BloodBankTable"].Rows[inc].Delete();
                     Connect.UpdateDatabase(placer);
                     placer.AcceptChanges();
                    ReloadDataSet();
                     maxrow = placer.Tables["BloodBankTable"].Rows.Count;
                     inc = maxrow - 1;
                    UpdateNameCounters();
                    checkrecords();
                     MessageBox.Show("Record deleted.");
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
                finally
                {
                    updategridview();
                }
            } else
            {
                MessageBox.Show("Action interrupted.");
            }

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        //pangchecklang ng records for next, previous, and data manipulation
        private void checkrecords()
        {


            rows = placer.Tables["BloodBankTable"].Rows[inc];
            tblabel.Text = (inc + 1).ToString();
            tbfirstname.Text = rows["First Name"].ToString();
            tbsurname.Text = rows["Last Name"].ToString();
            lbbloodtype.Text = rows["Blood Type"].ToString();
            tbage.Text = rows["age"].ToString();

            //icoconvert naman dito yung byte na nakastore sa database natin to image.
            if (placer.Tables["BloodBankTable"].Rows[inc]["imagery"] != DBNull.Value)
                {
                //kukunin nya as byte yungnasa database
                    byte[] imageData = (byte[])placer.Tables["BloodBankTable"].Rows[inc]["imagery"];
                    if (imageData != null && imageData.Length > 0)
                    {
                        try
                        {
                            using (MemoryStream ms = new MemoryStream(imageData))
                            {
                                ms.Position = 0;
                            //ilalagay na sa picturebox yung converted format
                                donorphoto.Image = Image.FromStream(ms);
                            }
                        }
                        catch
                        {
                        donorphoto.Image = Resources.Default_pfp_svg;
                        }
                    }
                    else
                    {
                    //itong tatlong to ay para sa mga di nag upload ng image, may default image sila para di naman pangit tingnan
                        donorphoto.Image = Resources.Default_pfp_svg;
                    }
                }
                else
                {
                donorphoto.Image = Resources.Default_pfp_svg;
                }

                if (placer.Tables["BloodBankTable"].Rows[inc]["Date Donated"] != DBNull.Value)
                {
               //ilagay value sa datetimepicker natin
                    DateTime donationDate = Convert.ToDateTime(rows.ItemArray.GetValue(6));
                    datedonated.Value = donationDate;
                }
                else
                {
                    datedonated.Value = DateTime.Today;
                }

            lbdonate.Text = rows["Counter"].ToString() + " time(s)!";

            
        }

        private void tbsurname_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //exit program
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //minimize
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //again, pangdrag ng borderless forms
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //pangprint! kukunin nya currentdata para ilipat sa kabilang form to be printed
            Form2 print = new Form2();
            print.DisplayData(tbfirstname.Text, tbsurname.Text, lbbloodtype.Text, tbage.Text, donorphoto.Image, datedonated.Value, lbdonate.Text);
            print.ShowDialog();
        }

        private List<DataRow> searchResult = new List<DataRow>();
        private int currentSearchIndex = 0;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            string searchTerm = tbSearch.Text.Trim().ToLower();
            searchResult.Clear();

            foreach (DataRow row in placer.Tables["BloodBankTable"].Rows)
            {
                string firstName = row["First Name"].ToString().ToLower();
                string lastName = row["Last Name"].ToString().ToLower();
                string bloodtype = row["Blood Type"].ToString().ToLower();
                string age = row["age"].ToString().ToLower();

                if (firstName.Contains(searchTerm) || lastName.Contains(searchTerm) || bloodtype.Contains(searchTerm) || age.Contains(searchTerm))
                {
                    searchResult.Add(row);
                }

                currentSearchIndex = 0;
                DisplaySearchResult(currentSearchIndex);

            }

            if (tbSearch.Text != "")
            {
                
                btnNext.Visible = false;
                btnNextOccurence.Visible = true;
                btnPrevious.Visible = false;
                btnPreviousOccurence.Visible = true;
            }
            else
            {
                btnNext.Visible = true;
                btnNextOccurence.Visible = false;
                btnPrevious.Visible = true;
                btnPreviousOccurence.Visible = false;
                checkrecords();
            }

        }


            private void DisplaySearchResult(int index)
        {
            
            if (searchResult.Count > 0 && index >= 0 && index < searchResult.Count)
            {
                DataRow row = searchResult[index];
                int totalrows = placer.Tables["BloodBankTable"].Rows.Count, donornumber = -1;
                
                for( int i = 0; i < totalrows; i++)
                {
                    if (searchResult[index] == placer.Tables["BloodBankTable"].Rows[i])
                    {
                        
                        donornumber = i + 1;
                        break;
                    }
                }

                if (donornumber != -1)
                {
                    tblabel.Text = donornumber.ToString();
                    
                }
                tbfirstname.Text = row["First Name"].ToString();
                tbsurname.Text = row["Last Name"].ToString();
                lbbloodtype.Text = row["Blood Type"].ToString();
                tbage.Text = row["age"].ToString();

                if (row["imagery"] != DBNull.Value)
                {
                    byte[] imageData = (byte[])row["imagery"];
                    if (imageData != null && imageData.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            ms.Position = 0;
                            donorphoto.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        donorphoto.Image = Resources.Default_pfp_svg;
                    }
                }
                else
                {
                    donorphoto.Image = Resources.Default_pfp_svg;
                }

                if (row["Date Donated"] != DBNull.Value)
                {
                    DateTime donationDate = Convert.ToDateTime(row["Date Donated"]);
                    datedonated.Value = donationDate;
                }
                else
                {
                    datedonated.Value = DateTime.Today;
                }
                int donationCount = Convert.ToInt32(row["Counter"]);
                lbdonate.Text = donationCount + " time(s)!";
            }
        }

        private void btnNextOccurence_Click(object sender, EventArgs e)
        {
            if (searchResult.Count > 1)
            {
                currentSearchIndex++;
                if (currentSearchIndex >= searchResult.Count)
                {
                    currentSearchIndex = 0;
                }
                DisplaySearchResult(currentSearchIndex);
            }
        }

        private void btnPreviousOccurence_Click(object sender, EventArgs e)
        {
            if (searchResult.Count > 1)
            {
                currentSearchIndex--;
                if (currentSearchIndex < 0)
                {
                    currentSearchIndex = searchResult.Count - 1; ;
                }
                DisplaySearchResult(currentSearchIndex);
            }
        }

        private void ReloadDataSet()
        {
            //bagong connection para di madelay ang data read natin
            placer = Connect.GetConnection;
            maxrow = placer.Tables["BloodBankTable"].Rows.Count;
        }

    }
}
