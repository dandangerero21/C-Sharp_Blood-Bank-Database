using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blood_Bank_Management_System
{
    internal class DatabaseConnection
    {
        private string sql_string, credstring;
        private string strCon;
        System.Data.SqlClient.SqlDataAdapter mydataadapter;
        System.Data.SqlClient.SqlDataAdapter cred;
        System.Data.SqlClient.SqlConnection con;

        public string Sql
        {
            set
            { sql_string = value; }
        }

        public string connection_string
        {
            set { strCon = value; }
        }

        public System.Data.DataSet GetConnection
        {
            get { return BloodData(); }
        }

        private System.Data.DataSet BloodData()
        {
            con = new System.Data.SqlClient.SqlConnection(strCon);
            con.Open();

            mydataadapter = new System.Data.SqlClient.SqlDataAdapter(sql_string, con);
            System.Data.DataSet newdataset = new System.Data.DataSet();

            mydataadapter.Fill(newdataset, "BloodBankTable");

            credstring = "SELECT * FROM Credentials";

            cred = new System.Data.SqlClient.SqlDataAdapter(credstring, con);

            cred.Fill(newdataset, "Credentials");

            con.Close();

            return newdataset;
        }


        public void UpdateDatabase(System.Data.DataSet placer)
        {
            System.Data.SqlClient.SqlCommandBuilder builder = new System.Data.SqlClient.SqlCommandBuilder(mydataadapter);
            builder.DataAdapter.Update(placer.Tables[0]);
        }
    }
}
