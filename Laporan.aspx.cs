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

            // Store the selected procedure and generate inputs
            ViewState["SelectedProcedure"] = selectedProcedure;
            GenerateInputs(schemaName, procedureName);

            // Update selection messages
            warningPilihLaporan.Style["display"] = "none";
            reportPreviewPanel.Style["display"] = "none";
            successPilihLaporan.InnerText = "Laporan Berhasil dipilih!";
            successPilihLaporan.Style["display"] = "block";
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
                    using (OracleCommand cmd = new OracleCommand("PABSEN.DapatProcedureVariables", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;


                        // Add input parameter
                        cmd.Parameters.Add("proc_name", OracleDbType.Varchar2).Value = procedureName.ToUpper();

                        // Add output parameter for RETCURSOR
                        OracleParameter cursorParam = new OracleParameter
                        {
                            ParameterName = "variables_cursor",
                            OracleDbType = OracleDbType.RefCursor,
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(cursorParam);

                        // Execute and get the reader from the RefCursor
                        using (OracleDataReader dr = cmd.ExecuteReader())
                        {
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
                    warningForm.InnerText = $"Gagal mendapatkan detail prosedur: {ex.Message}";
                }
            }
        }
        private DataTable ExecuteProcedure(string fullProcedureName, Dictionary<string, string> parameters)
        {
            string[] parts = fullProcedureName.Split('.');
            string procedureName = parts[1]; // Get the procedure name without schema

            string oracleDb = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;
            using (OracleConnection conn = new OracleConnection(oracleDb))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand($"{fullProcedureName}", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add input parameters based on the textbox values
                    foreach (var param in parameters)
                    {
                        string paramName = param.Key.Replace("input", ""); // Remove "input" prefix
                        cmd.Parameters.Add(paramName, OracleDbType.Varchar2).Value = param.Value;
                    }

                    // Add the output cursor parameter
                    OracleParameter cursorParam = new OracleParameter();
                    cursorParam.ParameterName = "RET";
                    cursorParam.OracleDbType = OracleDbType.RefCursor;
                    cursorParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(cursorParam);

                    // Execute and fill DataTable
                    DataTable dt = new DataTable();
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    return dt;
                }
            }
        }
        /*protected void btnLihat_Click(object sender, EventArgs e)
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

            // Get the selected procedure
            string procedureName = ViewState["SelectedProcedure"] as string;
            if (string.IsNullOrEmpty(procedureName))
            {
                warningForm.Style["display"] = "block";
                warningForm.InnerText = "Procedure tidak ditemukan!";
                successForm.Style["display"] = "none";
                return;
            }

            try
            {
                // Execute the stored procedure and fetch the result
                DataTable resultTable = ExecuteProcedure(procedureName, inputValues);

                if (resultTable != null && resultTable.Rows.Count > 0)
                {
                    // Generate table HTML
                    StringBuilder tableHtml = new StringBuilder();
                    tableHtml.Append("<table class='table table-bordered table-striped'><thead><tr>");

                    // Add headers
                    foreach (DataColumn column in resultTable.Columns)
                    {
                        tableHtml.Append($"<th>{column.ColumnName}</th>");
                    }
                    tableHtml.Append("</tr></thead><tbody>");

                    // Add rows
                    foreach (DataRow row in resultTable.Rows)
                    {
                        tableHtml.Append("<tr>");
                        foreach (var cell in row.ItemArray)
                        {
                            tableHtml.Append($"<td>{cell}</td>");
                        }
                        tableHtml.Append("</tr>");
                    }
                    tableHtml.Append("</tbody></table>");

                    // Set table content
                    tablePreviewLiteral.Text = tableHtml.ToString();

                    // Set title and show preview
                    previewTitle.InnerText = dropdownLaporan.SelectedItem.Text;
                    reportPreviewPanel.Visible = true;

                    // Success message
                    warningForm.Style["display"] = "none";
                    successForm.Style["display"] = "block";
                    successForm.InnerText = "Data berhasil ditampilkan!";
                }
                else
                {
                    // No data found
                    tablePreviewLiteral.Text = string.Empty;
                    reportPreviewPanel.Visible = false;
                    successForm.Style["display"] = "none";
                    warningForm.Style["display"] = "block";
                    warningForm.InnerText = "Tidak ada data yang ditemukan.";
                }
            }
            catch (Exception ex)
            {
                // Handle errors
                reportPreviewPanel.Visible = false;
                warningForm.Style["display"] = "block";
                warningForm.InnerText = $"Error: {ex.Message}";
            }
        }
        */

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

        /*
        private string GeneratePDF(DataTable dataTable, string reportTitle)
        {
            string folderPath = Server.MapPath("~/GeneratedReports/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string pdfFileName = $"{reportTitle}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string pdfPath = Path.Combine(folderPath, pdfFileName);

            using (var writer = new PdfWriter(pdfPath))
            {
                using (var pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);

                    // Add the title
                    document.Add(new Paragraph(reportTitle).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER).SetFontSize(20));

                    // Create a table
                    iTextTable table = new iTextTable(dataTable.Columns.Count);
                    table.SetWidth(UnitValue.CreatePercentValue(100));

                    // Add table headers
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        table.AddHeaderCell(new Cell().Add(new Paragraph(column.ColumnName)));
                    }

                    // Add table data
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(cell?.ToString() ?? "")));
                        }
                    }

                    document.Add(table);
                }
            }

            return pdfPath;
        }

        private void ShowProgress(int percentage, string message)
        {
            // Ensure the progress bar is visible
            downloadProgressBar.Style["display"] = "block";

            // Update the progress bar width and value
            var progressBarInner = downloadProgressBar.Controls[0] as HtmlGenericControl;
            if (progressBarInner != null)
            {
                progressBarInner.Style["width"] = $"{percentage}%";
                progressBarInner.Attributes["aria-valuenow"] = percentage.ToString();
                progressBarInner.InnerText = $"{percentage}%";
            }

            // Update the progress label
            progressBarLabel.Style["display"] = "block";
            progressBarLabel.InnerText = $"{message} ({percentage}%)";

            // Send the updates to the client
            Response.Flush();
        }

        private void ServeFileToClient(string pdfPath, string reportTitle)
        {
            string fileName = $"{reportTitle.Replace(" ", "_")}.pdf";

            // Set up response headers
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", $"attachment; filename={fileName}");
            Response.TransmitFile(pdfPath);
            Response.End();
        }


        /*
        private void GeneratePdf(string filePath, string Pilihan, string DariTanggal, string SampaiTanggal, string NIK, List<Dictionary<string, string>> data)
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create a PDF writer and document
            using (PdfWriter writer = new PdfWriter(filePath))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    float topMargin = 20f;
                    float rightMargin = 20f;
                    float bottomMargin = 20f;
                    float leftMargin = 20f;
                    Document document = new Document(pdf, PageSize.A4);
                    document.SetMargins(topMargin, rightMargin, bottomMargin, leftMargin);

                    // Load standard fonts
                    PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Add a title
                    document.Add(new Paragraph("PURA GROUP - UNIT OFFSET")
                        .SetFont(boldFont)
                        .SetFontSize(12)
                        .SetMultipliedLeading(1)
                        .SetMarginTop(0)
                        .SetMarginBottom(0));

                    document.Add(new Paragraph(Pilihan)
                        .SetFont(boldFont)
                        .SetFontSize(16)
                        .SetMultipliedLeading(1)
                        .SetMarginBottom(0));

                    document.Add(new Paragraph("Periode Tgl : " + DariTanggal + " - " + SampaiTanggal)
                        .SetFont(boldFont)
                        .SetFontSize(10)
                        .SetMultipliedLeading(1)
                        .SetMarginBottom(10));

                    // Prepare the table
                    if (data.Count > 0)
                    {
                        // Get the column count and calculate proportional widths
                        int columnCount = data[0].Count;
                        float[] columnWidths = new float[columnCount];
                        for (int i = 0; i < columnCount; i++)
                        {
                            columnWidths[i] = 1; // Equal width for all columns
                        }

                        // Create table with fixed widths
                        iTextTable table = new iTextTable(UnitValue.CreatePercentArray(columnWidths))
                            .SetWidth(UnitValue.CreatePercentValue(100)); // Table width is 100% of the page

                        // Add table headers
                        foreach (string columnName in data[0].Keys)
                        {
                            table.AddHeaderCell(new Cell()
                                .Add(new Paragraph(columnName)
                                    .SetFont(boldFont)
                                    .SetFontSize(10))
                                .SetPadding(5)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetMaxWidth(100)
                                .SetVerticalAlignment(VerticalAlignment.MIDDLE));
                        }

                        // Add table rows
                        foreach (var row in data)
                        {
                            foreach (var value in row.Values)
                            {
                                table.AddCell(new Cell()
                                    .Add(new Paragraph(value)
                                        .SetFont(regularFont)
                                        .SetFontSize(9)
                                        .SetTextAlignment(TextAlignment.LEFT)
                                        .SetMarginBottom(0)
                                        .SetMultipliedLeading(1))
                                    .SetPadding(2)
                                    .SetMaxWidth(100) // Set maximum width for data cells
                                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .SetKeepTogether(true) // Ensure content is wrapped within the cell
                                    .SetBorderTop(Border.NO_BORDER)   // Remove only the top border
                                    .SetBorderBottom(Border.NO_BORDER));
                            }
                        }

                        for (int i = 0; i < data.Count; i++)
                        {
                            var row = data[i];
                            foreach (var value in row.Values)
                            {
                                var cell = new Cell()
                                    .Add(new Paragraph(value)
                                        .SetFont(regularFont)
                                        .SetFontSize(9)
                                        .SetTextAlignment(TextAlignment.LEFT)
                                        .SetMarginBottom(0)
                                        .SetMultipliedLeading(1))
                                    .SetPadding(2)
                                    .SetMaxWidth(100)
                                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                                    .SetTextAlignment(TextAlignment.LEFT)
                                    .SetKeepTogether(true)
                                    .SetBorderTop(Border.NO_BORDER)
                                    .SetBorderBottom(Border.NO_BORDER);

                                // Add bottom border for the last row
                                if (i == data.Count - 1)
                                {
                                    cell.SetBorderBottom(new SolidBorder(1));
                                }
                                table.AddCell(cell);
                            }
                        }

                        // Add the table to the document
                        document.Add(table);

                        document.Add(new Paragraph("\n\nDisetujui:")
                            .SetFont(regularFont)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.RIGHT));

                        document.Add(new Paragraph("Nama")
                            .SetFont(regularFont)
                            .SetFontSize(10)
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetMarginTop(20));
                    }
                    else
                    {
                        document.Add(new Paragraph("No data available").SetFont(regularFont));
                    }

                    // Close the document
                    document.Close();
                }
            }
        }
        private List<Dictionary<string, string>> FetchDataFromProcedure()
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
            string oracleDb = ConfigurationManager.ConnectionStrings["OracleConnection"].ConnectionString;

            using (OracleConnection conn = new OracleConnection(oracleDb))
            {
                conn.Open();
                using (OracleCommand cmd = new OracleCommand("DapatModul", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("list_modul", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Dictionary<string, string> row = new Dictionary<string, string>();
                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                row.Add(dr.GetName(i), dr[i]?.ToString());
                            }
                            data.Add(row);
                        }
                    }
                }
            }

            return data;
        }
        private void ServePdfToUser(string filePath)
        {
            Response.ContentType = "application/pdf";
            Response.AppendHeader("Content-Disposition", "attachment; filename=Laporan.pdf");
            Response.TransmitFile(filePath);
            Response.End();
        } */
    }
}