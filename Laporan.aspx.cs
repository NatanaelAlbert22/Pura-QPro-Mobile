using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using SystemListItem = System.Web.UI.WebControls.ListItem;
using Path = System.IO.Path;
using iTextPath = iText.Kernel.Geom.Path;
using iTextTable = iText.Layout.Element.Table;
using System.IO;
using iText.IO.Font.Constants;
using System.Globalization;
using System.Data;
using iText.Kernel.Geom;
using iText.Layout.Borders;
using System.Text;
using System.Web.UI.HtmlControls;
using Pura_Gaji_Viewer.ReportTemplates;

namespace Pura_Gaji_Viewer
{
    public partial class Gaji : System.Web.UI.Page
    {
        protected string Pilihan
        {
            get
            {
                return ViewState["Pilihan"] as string ?? string.Empty;
            }
            set
            {
                ViewState["Pilihan"] = value;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                string procedureName = ViewState["SelectedProcedure"] as string;
                if (!string.IsNullOrEmpty(procedureName))
                {
                    string[] parts = procedureName.Split('.');
                    if (parts.Length == 2)
                    {
                        GenerateInputs(parts[0], parts[1]);
                    }
                }
            }
            else
            {
                LoadDropdownLaporan(); // Load dropdown on initial load
            }
        }
        // ============================================ BAGIAN DROPDOWN ==============================================================
        protected void LoadDropdownLaporan()
        {
            string oracleDb = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
            using (OracleConnection conn = new OracleConnection(oracleDb))
            {
                try
                {
                    conn.Open();

                    // Name of the Oracle procedure to fetch module names
                    string procedureMODUL = "PABSEN.DapatModul";
                    using (OracleCommand cmd = new OracleCommand(procedureMODUL, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("RET", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (OracleDataReader dr = cmd.ExecuteReader())
                        {
                            dropdownLaporan.Items.Clear();
                            dropdownLaporan.Items.Add(new System.Web.UI.WebControls.ListItem("-- Pilih Laporan --", ""));

                            while (dr.Read())
                            {
                                string nama = dr["NAMA"].ToString();
                                string kode = dr["KODE"].ToString();
                                string spName = dr["SP"].ToString();

                                // Add dropdown item
                                dropdownLaporan.Items.Add(new System.Web.UI.WebControls.ListItem(kode + " - " +nama, spName));
                            }

                            dropdownLaporan.Enabled = true;
                            btnPilihLaporan.Enabled = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    warningPilihLaporan.Style["display"] = "block";
                    warningPilihLaporan.InnerText = $"Gagal koneksi ke database! Error: {ex.Message}";
                    dropdownLaporan.Enabled = false;
                    btnPilihLaporan.Enabled = false;
                }
            }
        }
        protected void btnPilihLaporan_Click(object sender, EventArgs e)
        {
            string selectedProcedure = dropdownLaporan.SelectedValue;
            if (selectedProcedure == "-- Pilih Laporan --")
            {
                successPilihLaporan.Style["display"] = "none";
                warningPilihLaporan.InnerText = "Harap Pilih Laporan di Bawah Ini!";
                warningPilihLaporan.Style["display"] = "block";
                formLaporan.Style["display"] = "none";
                reportPreviewPanel.Style["display"] = "none";
                return;
            }

            if (string.IsNullOrEmpty(selectedProcedure))
            {
                successPilihLaporan.Style["display"] = "none";
                warningPilihLaporan.InnerText = "Procedure belum dibuatkan!";
                warningPilihLaporan.Style["display"] = "block";
                formLaporan.Style["display"] = "none";
                reportPreviewPanel.Style["display"] = "none";
                return;
            }

            // Split the procedure into schema and procedure name
            string[] parts = selectedProcedure.Split('.');
            if (parts.Length != 2)
            {
                warningPilihLaporan.InnerText = "Format prosedur tidak valid. Harus berupa 'SCHEMA.PROCEDURE'";
                warningPilihLaporan.Style["display"] = "block";
                formLaporan.Style["display"] = "none";
                reportPreviewPanel.Style["display"] = "none";
                return;
            }

            string schemaName = parts[0];
            string procedureName = parts[1];

            // Clear any existing warnings/success messages
            warningForm.Style["display"] = "none";
            successForm.Style["display"] = "none";

            // Show and update the form
            formLaporan.Style["display"] = "block";
            judulForm.Text = dropdownLaporan.SelectedItem.Text;            

            // Update selection messages
            warningPilihLaporan.Style["display"] = "none";
            reportPreviewPanel.Style["display"] = "none";
            successPilihLaporan.InnerText = "Laporan Berhasil dipilih!";
            successPilihLaporan.Style["display"] = "block";

            // Store the selected procedure and generate inputs
            ViewState["SelectedProcedure"] = selectedProcedure;
            GenerateInputs(schemaName, procedureName);
        }

        // ============================================ BAGIAN INPUT ==============================================================
        private void GenerateInputs(string schemaName, string procedureName)
        {
            string oracleDb = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
            dynamicControlsPanel.Controls.Clear();

            using (OracleConnection conn = new OracleConnection(oracleDb))
            {
                try
                {
                    conn.Open();
                    using (OracleCommand cmd = new OracleCommand("PABSEN.Check_Procedure_Inputs", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add input parameter
                        cmd.Parameters.Add("proc_name", OracleDbType.Varchar2).Value = procedureName.ToUpper();

                        // Add output parameter for REF CURSOR
                        OracleParameter cursorParam = new OracleParameter
                        {
                            ParameterName = "RET",
                            OracleDbType = OracleDbType.RefCursor,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(cursorParam);

                        // Execute and get the reader from the RefCursor
                        using (OracleDataReader dr = cmd.ExecuteReader())
                        {
                            if (!dr.HasRows)
                            {
                                successPilihLaporan.Style["display"] = "none";
                                warningPilihLaporan.InnerText = "Procedure belum dibuatkan!";
                                warningPilihLaporan.Style["display"] = "block";
                                formLaporan.Style["display"] = "none";
                                reportPreviewPanel.Style["display"] = "none";
                                return;
                            }

                            while (dr.Read())
                            {
                                string variableName = dr["VARIABLE_NAME"]?.ToString();
                                string dataType = dr["DATA_TYPE"]?.ToString();

                                if (string.IsNullOrEmpty(variableName)) continue; // Skip null or empty names

                                // Determine input type from the first letter of the variable name
                                char inputTypePrefix = variableName[0];



                                string inputType;
                                switch (inputTypePrefix)
                                {
                                    case 'D':
                                        inputType = "date";
                                        break;
                                    case 'N':
                                        inputType = "number";
                                        break;
                                    case 'C':
                                        inputType = "text";
                                        break;
                                    default:
                                        inputType = "text"; // Default to text
                                        break;
                                }


                                // Extract label text
                                string labelText = variableName.Substring(2).ToUpper(); // Remove first two characters

                                // Create label
                                Label label = new Label
                                {
                                    Text = labelText,
                                    CssClass = "control-label"
                                };

                                // Create input based on data type
                                TextBox input = new TextBox
                                {
                                    ID = $"input{labelText}",
                                    CssClass = "form-control text-center center-block"
                                };
                                input.Attributes.Add("type", inputType);

                                // Add label and input to the dynamic controls panel
                                dynamicControlsPanel.Controls.Add(label);
                                dynamicControlsPanel.Controls.Add(new LiteralControl("<br/>"));
                                dynamicControlsPanel.Controls.Add(input);
                                dynamicControlsPanel.Controls.Add(new LiteralControl("<br/>"));
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    warningForm.Style["display"] = "block";
                    warningForm.InnerText = "Error loading input fields: " + ex.Message;
                }
            }
        }
        protected void btnLihat_Click(object sender, EventArgs e)
        {
            // Collect input values
            Dictionary<string, string> inputValues = new Dictionary<string, string>();
            foreach (Control control in dynamicControlsPanel.Controls)
            {
                if (control is TextBox textBox)
                {
                    inputValues[textBox.ID] = textBox.Text.Trim();
                }
            }
            // Retrieve the report name
            string reportNameFull = judulForm.Text;
            string reportName = reportNameFull.Remove(0, 7).Trim();

            // If no inputs were generated, do not proceed
            if (dynamicControlsPanel.Controls.Count == 0)
            {
                warningForm.Style["display"] = "block";
                warningForm.InnerText = "Report cannot be generated because the procedure does not exist.";
                return;
            }

            IReportTemplate reportTemplate = ReportFactory.GetReport(reportName);

            if (reportTemplate == null)
            {
                warningForm.Style["display"] = "block";
                warningForm.InnerText = "Report template not found!";
                successForm.Style["display"] = "none";
                return;
            }

            //try
            //{
                using (OracleConnection conn = new OracleConnection(ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString))
                {
                    conn.Open();
                    judulForm.Style["display"] = "block";

                    string htmlPreview = reportTemplate.GenerateHTMLPreview(inputValues, conn);
                    tablePreviewLiteral.Text = htmlPreview;

                    // Set title and show preview
                    previewTitle.InnerText = dropdownLaporan.SelectedItem.Text;
                    reportPreviewPanel.Visible = true;
                    // Success message
                    warningForm.Style["display"] = "none";
                    successForm.Style["display"] = "block";
                    successForm.InnerText = "Data berhasil ditampilkan!";
                    reportPreviewPanel.Style["display"] = "block";
                }
            //}
            //catch (Exception ex)
            //{
                // Handle errors
            //    reportPreviewPanel.Visible = false;
            //    warningForm.Style["display"] = "block";
            //    warningForm.InnerText = $"Error hahaha: {ex.Message}";
            //}
        }
 
        // ============================================ BAGIAN PDF ==============================================================
        protected void btnDownload_Click(object sender, EventArgs e)
        {
            // Collect input values from the dynamic controls
            Dictionary<string, string> inputValues = new Dictionary<string, string>();
            foreach (Control control in dynamicControlsPanel.Controls)
            {
                if (control is TextBox textBox)
                {
                    inputValues[textBox.ID] = textBox.Text.Trim();
                    System.Diagnostics.Debug.WriteLine($"Found control: {textBox.ID} = {textBox.Text.Trim()}");
                }
            }

            // Retrieve the report name
            string reportNameFull = previewTitle.InnerText;
            string reportName = reportNameFull.Remove(0, 7).Trim();

            try
            {
                string oracleDb = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
                using (OracleConnection conn = new OracleConnection(oracleDb))
                {
                    // Select the appropriate report template
                    IReportTemplate reportTemplate = ReportFactory.GetReport(reportName);
                    if (reportTemplate == null)
                    {
                        throw new Exception("Invalid report template selected.");
                    }

                    // Generate the PDF
                    byte[] pdfBytes = reportTemplate.GeneratePDF(inputValues, conn);

                    // Send the PDF to the browser for download
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", $"attachment; filename={reportNameFull}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                    Response.BinaryWrite(pdfBytes);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                // Display error message
                warningForm.Style["display"] = "block";
                warningForm.InnerText = $"Error generating report: {ex.Message}, {reportName}";
                successForm.Style["display"] = "none";
            }
        }
    }
}