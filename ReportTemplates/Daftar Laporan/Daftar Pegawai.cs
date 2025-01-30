using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using iText.Layout;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using System.Text;

namespace Pura_Gaji_Viewer.ReportTemplates
{
    public class Daftar_Pegawai : IReportTemplate
    {
        public byte[] GeneratePDF(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            // Query data from the database using input values
            DataTable dataTable = FetchDataFromDatabase(inputValues, connection);

            // Create a memory stream to store the PDF
            using (var memoryStream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                PageSize landscape = PageSize.A4.Rotate();
                Document document = new Document(pdfDocument, landscape);
                document.SetMargins(20, 20, 20, 20);

                // Use fonts
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.COURIER);

                // Add title
                Paragraph title = new Paragraph("DAFTAR PEGAWAI")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT);
                document.Add(title);

                // Create table with dynamic width
                // Define table with fixed column widths
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(UnitValue
                    .CreatePercentArray(new float[] { 2, 4, 10, 8, 5, 5, 5, 3, 3, 4, 2, 2, 3, 3, 4 })); // Column widths
                table.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.LEFT); // Align to left
                table.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE); // Align to middle
                table.SetWidth(UnitValue.CreatePercentValue(80)); // Table width grows dynamically but capped at 80% of page
                table.SetBorder(Border.NO_BORDER);

                // Add headers
                // Main headers
                table.AddHeaderCell(CreateHeaderCell("NO", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("NIK", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("NAMA", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("BAGIAN", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("TGL MASUK", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("TGL ANGKAT", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("TGL LAHIR", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("STAT", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("PNDDK", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("UMR", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("CUTI", boldFont, 1, 2)); // Spans 2 columns
                table.AddHeaderCell(CreateHeaderCell("BPJS", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("THT", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("PENSIUN", boldFont, 2, 1));

                // Sub-Headers under CUTI
                table.AddHeaderCell(CreateHeaderCell("HARI", boldFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("", boldFont, 1, 1)); // Blank sub-header

                // Add data rows
                int rowNumber = 1;
                foreach (DataRow row in dataTable.Rows)
                {
                    table.AddCell(CreateDataCell(rowNumber.ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["NIK"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["NAMA"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["BAGIAN"].ToString(), regularFont));

                    // Format the date fields to display only dd/MM/yyyy
                    string tglMsk = FormatDate(row["TGL_MSK"].ToString());
                    string tglAngkat = FormatDate(row["TGL_ANGKAT"].ToString());
                    string tglLhr = FormatDate(row["TGL_LHR"].ToString());

                    table.AddCell(CreateDataCell(tglMsk, regularFont));
                    table.AddCell(CreateDataCell(tglAngkat, regularFont));
                    table.AddCell(CreateDataCell(tglLhr, regularFont));

                    table.AddCell(CreateDataCell(row["STAT"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["PENDIDIKAN"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["UMRNEW"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["BLNCT"].ToString(), regularFont)); // HARI
                    table.AddCell(CreateDataCell(row["CT2"].ToString(), regularFont)); // Blank column
                    table.AddCell(CreateDataCell(row["BPJS"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["THT"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["PENSIUN"].ToString(), regularFont));
                    rowNumber++;
                }

                document.Add(table);
                document.Close();

                return memoryStream.ToArray();
            }
        }
        public Cell CreateHeaderCell(string content, PdfFont font, int rowSpan, int colSpan)
        {
            return new Cell(rowSpan, colSpan)
                .Add(new Paragraph(content)
                    .SetFont(font)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER))
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(0)
                .SetMargin(0);
        }
        public Cell CreateDataCell(string content, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(content)
                    .SetFont(font)
                    .SetFontSize(6)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER))
                .SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingTop(1)
                .SetPaddingBottom(1);
        }
        public DataTable FetchDataFromDatabase(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            // Get values from the dictionary using the input prefixed names
            string CNBAGIAN = inputValues["inputBAGIAN"];
            string CNSTAT = inputValues["inputSTAT"];
            string CNPDDK = inputValues["inputPDDK"];
            int CNLOKASI = !string.IsNullOrEmpty(inputValues["inputLOKASI"]) ?
                           int.Parse(inputValues["inputLOKASI"]) : 0;

            DataTable dataTable = new DataTable();

            using (var command = new OracleCommand("PABSEN.PEGAWAI", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters with the correct names expected by the stored procedure
                command.Parameters.Add("CNBAGIAN", OracleDbType.Varchar2).Value = CNBAGIAN;
                command.Parameters.Add("CNSTAT", OracleDbType.Varchar2).Value = CNSTAT;
                command.Parameters.Add("CNPDDK", OracleDbType.Varchar2).Value = CNPDDK;
                command.Parameters.Add("CNLOKASI", OracleDbType.Int32).Value = CNLOKASI;
                command.Parameters.Add("RET", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var adapter = new OracleDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }

            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    System.Diagnostics.Debug.WriteLine($"Column: {col.ColumnName}, Value: {row[col]}, Type: {row[col]?.GetType().Name ?? "null"}");
                }
            }

            return dataTable;
        }
        public string GenerateHTMLPreview(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            DataTable dataTable = FetchDataFromDatabase(inputValues, connection);

            if (dataTable == null)
            {
                System.Diagnostics.Debug.WriteLine("FetchDataFromDatabase returned null.");
            }

            if (dataTable.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("FetchDataFromDatabase returned an empty DataTable.");
            }

            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn col in dataTable.Columns)
                {
                    System.Diagnostics.Debug.WriteLine($"{col.ColumnName}: {row[col]}");
                }
            }

            StringBuilder tableHtml = new StringBuilder();
            tableHtml.Append("<table class='table table-bordered table-striped'><thead><tr>");

            // Add headers with the same structure as the PDF
            tableHtml.Append("<th rowspan='2'>NO</th>");
            tableHtml.Append("<th rowspan='2'>NIK</th>");
            tableHtml.Append("<th rowspan='2'>NAMA</th>");
            tableHtml.Append("<th rowspan='2'>BAGIAN</th>");
            tableHtml.Append("<th rowspan='2'>TGL MASUK</th>");
            tableHtml.Append("<th rowspan='2'>TGL ANGKAT</th>");
            tableHtml.Append("<th rowspan='2'>TGL LAHIR</th>");
            tableHtml.Append("<th rowspan='2'>STAT</th>");
            tableHtml.Append("<th rowspan='2'>PNDDK</th>");
            tableHtml.Append("<th rowspan='2'>UMR</th>");
            tableHtml.Append("<th colspan='2'>CUTI</th>");
            tableHtml.Append("<th rowspan='2'>BPJS</th>");
            tableHtml.Append("<th rowspan='2'>THT</th>");
            tableHtml.Append("<th rowspan='2'>PENSIUN</th>");
            tableHtml.Append("</tr><tr>");
            tableHtml.Append("<th>HARI</th>");
            tableHtml.Append("<th></th>");
            tableHtml.Append("</tr></thead><tbody>");

            // Add rows
            int rowNumber = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                tableHtml.Append("<tr>");
                tableHtml.Append($"<td>{rowNumber}</td>");
                tableHtml.Append($"<td>{row["NIK"]}</td>");
                tableHtml.Append($"<td>{row["NAMA"]}</td>");
                tableHtml.Append($"<td>{row["BAGIAN"]}</td>");
                tableHtml.Append($"<td>{row["TGL_MSK"]}</td>");
                tableHtml.Append($"<td>{row["TGL_ANGKAT"]}</td>");
                tableHtml.Append($"<td>{row["TGL_LHR"]}</td>");
                tableHtml.Append($"<td>{row["STAT"]}</td>");
                tableHtml.Append($"<td>{row["PENDIDIKAN"]}</td>");
                tableHtml.Append($"<td>{row["UMRNEW"]}</td>");
                tableHtml.Append($"<td>{row["BLNCT"]}</td>");
                tableHtml.Append($"<td>{row["CT2"]}</td>");
                tableHtml.Append($"<td>{row["BPJS"]}</td>");
                tableHtml.Append($"<td>{row["THT"]}</td>");
                tableHtml.Append($"<td>{row["PENSIUN"]}</td>");
                tableHtml.Append("</tr>");
                rowNumber++;
            }

            tableHtml.Append("</tbody></table>");
            return tableHtml.ToString();
        }
        private string FormatDate(string dateStr)
        {
            DateTime date;
            if (DateTime.TryParse(dateStr, out date))
            {
                return date.ToString("dd/MM/yyyy"); // Format to dd/MM/yyyy
            }
            return dateStr; // If not a valid date, return the original string
        }
    }
}