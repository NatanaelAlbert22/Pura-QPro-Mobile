using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace Pura_Gaji_Viewer
{
    public partial class Login : System.Web.UI.Page
    {
        string oracleDb = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                inputUsername.Focus();
            }       
            
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            using (OracleConnection conn = new OracleConnection(oracleDb))
            {
                string query = "select * from DEPARTEMEN";
                using (OracleCommand cmd = new OracleCommand(query, conn))
                {
                    try
                    {
                        conn.Open();
                        string username = inputUsername.Text;
                        string password = inputPassword.Text;
                        string departemen = inputDepartemen.Text;

                        using (OracleDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                string drDepartemen = rdr["DEPART"].ToString();
                                string drUsername = rdr["USERNAME"].ToString();
                                string drPassword = rdr["PASS"].ToString();

                                if (drUsername == username && drPassword == password && drDepartemen == departemen)
                                {
                                    Response.Redirect("~/Default.aspx");
                                    return;
                                }
                            }
                            warningDiv.InnerText = "Username, Password, atau Departemen Salah!";
                            warningDiv.Visible = true;
                        }
                    }
                    catch
                    {
                        warningDiv.InnerText = "Koneksi ke database gagal!";
                        warningDiv.Visible = true;
                    }
                    
                }
            }
        }
    }
}