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
using iText.Kernel.Colors;
using System.Globalization;

namespace Pura_Gaji_Viewer.ReportTemplates.Daftar_Laporan
{
    public class Detail_Lembur : IReportTemplate
    {
        /*public byte[] GeneratePDF(Dictionary<string, string> inputValues, Oracle.ManagedDataAccess.Client.OracleConnection connection)
        {
            DataTable dataTable = FetchDataFromDatabase(inputValues, connection);
            using (var memoryStream = new MemoryStream()) 
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                PageSize portrait = PageSize.A4;
                Document document = new Document(pdfDocument, portrait);
                document.SetMargins(20, 20, 20, 20);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Add title
                document.Add(new Paragraph("PURA GROUP - UNIT OFFSET")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0));
                document.Add(new Paragraph("LAPORAN LEMBUR DETAIL")
                    .SetFont(boldFont).SetFontSize(12).SetMarginBottom(0).SetMarginTop(0));
                document.Add(new Paragraph($"Periode Tgl : {inputValues["inputDARI_TGL"]} - {inputValues["inputSMP_TGL"]}")
                    .SetFont(boldFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0));
                // NIK NAMA PERIOS GAJI
                //document.Add(new Paragraph($"{inputValues["inputBAGIAN"]}").SetFont(boldFont).SetFontSize(9).SetMarginBottom(10).SetMarginTop(0));

                if (dataTable.Rows.Count > 0)
                {
                    DataRow firstRow = dataTable.Rows[0];

                    string inputNIK = firstRow["NIK"].ToString();
                    string nama = firstRow["NAMA"].ToString();
                    string bagian = firstRow["BAGIAN"].ToString();
                    string st = firstRow["ST"].ToString();
                    string umr = firstRow["UMR"].ToString();
                    string rupiah = firstRow["RUPIAH"].ToString();

                    // Add the dynamic title below
                    document.Add(new Paragraph($"{inputNIK} . {nama} - {bagian} / {st} - UPAH : {umr} / {rupiah}")
                        .SetFont(boldFont).SetFontSize(9).SetMarginBottom(10).SetMarginTop(5));
                }

                // Define table with fixed column widths
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(UnitValue
                    .CreatePercentArray(new float[] { 4, 2, 4, 6, 4, 4, 5, 6, 4, 4, 4, 3, 6, 4 })); // Column widths
                table.SetWidth(UnitValue.CreatePercentValue(100)); // Full width table
                
                table.AddHeaderCell(CreateHeaderCell("HARI", regularFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("SF", regularFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("GOL", regularFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("ABSEN", regularFont, 1, 4));
                table.AddHeaderCell(CreateHeaderCell("KET", regularFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("LEMBUR", regularFont, 1, 5));
                table.AddHeaderCell(CreateHeaderCell("ISRHT (jam)", regularFont, 2, 1));

                table.AddHeaderCell(CreateHeaderCell("TGL MSK", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("MSK", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("KLR", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("TGL KLR", regularFont, 1, 1));

                table.AddHeaderCell(CreateHeaderCell("MULAI", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("BTS LEMBUR", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("JAM", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("DEC", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("RUPIAH", regularFont, 1, 1));

                // Add data rows
                int rowNumber = 1;
                decimal totalDec = 0, totalRupiah = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    // Extract data from the row
                    string hari = row["HRIX"].ToString();
                    string sf = row["S"].ToString();
                    string gol = row["KODE"].ToString();
                    string tglMsk = row["TANGGAL"].ToString();
                    string msk = row["JMSK"].ToString();
                    string klr = row["JKLR"].ToString();
                    string tglKlr = row["TKLR"].ToString();
                    string ket = row["KET"].ToString();
                    string mulai = row["KLR"].ToString();
                    string btsLembur = row["BTS"].ToString();
                    string jam = row["XLMB"].ToString();
                    decimal dec = Convert.ToDecimal(row["QLMBR"]);
                    decimal rupiah = dec * Convert.ToDecimal(row["RUPIAH"]); // Derived value
                    decimal isrht = Convert.ToDecimal(row["ISK"]) - Convert.ToDecimal(row["ISM"]);

                    // Accumulate totals
                    totalDec += dec;
                    totalRupiah += rupiah;

                    // Add cells to the table
                    table.AddCell(CreateDataCell(rowNumber.ToString(), regularFont));
                    table.AddCell(CreateDataCell(sf, regularFont));
                    table.AddCell(CreateDataCell(gol, regularFont));
                    table.AddCell(CreateDataCell(tglMsk, regularFont));
                    table.AddCell(CreateDataCell(msk, regularFont));
                    table.AddCell(CreateDataCell(klr, regularFont));
                    table.AddCell(CreateDataCell(tglKlr, regularFont));
                    table.AddCell(CreateDataCell(ket, regularFont));
                    table.AddCell(CreateDataCell(mulai, regularFont));
                    table.AddCell(CreateDataCell(btsLembur, regularFont));
                    table.AddCell(CreateDataCell(jam, regularFont));
                    table.AddCell(CreateDataCell(dec.ToString("F2"), regularFont));
                    table.AddCell(CreateDataCell(rupiah.ToString("F2"), regularFont));
                    table.AddCell(CreateDataCell(isrht.ToString("F2"), regularFont));

                    rowNumber++;
                }

                table.AddCell(new Cell(1, 8)
                    .Add(new Paragraph("TOTAL"))
                        .SetFont(boldFont)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new DottedBorder(0.5f))
                    .SetBorderLeft(new SolidBorder(1))
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1));

                table.AddCell(new Cell(1, 3)
                    .Add(new Paragraph(""))
                        .SetFont(boldFont)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new DottedBorder(0.5f))
                    .SetBorderLeft(new SolidBorder(1))
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1));

                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph(totalDec.ToString("F2")))
                        .SetFont(boldFont)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new DottedBorder(0.5f))
                    .SetBorderLeft(new SolidBorder(1))
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1));

                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph(totalRupiah.ToString())
                        .SetFont(boldFont)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new DottedBorder(0.5f))
                    .SetBorderLeft(new SolidBorder(1))
                    .SetBorderRight(new DottedBorder(0.5f))
                    .SetBorderBottom(new SolidBorder(1)));

                table.AddCell(new Cell(1, 1)
                    .Add(new Paragraph(""))
                        .SetFont(boldFont)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.RIGHT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new DottedBorder(0.5f))
                    .SetBorderLeft(new DottedBorder(0.5f))
                    .SetBorderRight(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1));

                document.Add(table);

                if (dataTable.Rows.Count > 0)
                {
                    string approverName = dataTable.Rows[0]["NAMA"].ToString();

                    document.Add(new Paragraph("Disetujui :")
                        .SetFont(boldFont).SetFontSize(8).SetMarginBottom(10).SetMarginTop(30).SetMarginRight(40));
                    document.Add(new Paragraph(approverName)
                        .SetFont(boldFont).SetFontSize(7).SetMarginBottom(10).SetMarginTop(0).SetMarginRight(40));
                }

                document.Close();
                return memoryStream.ToArray();
            }

            throw new NotImplementedException();
        }

        public string GenerateHTMLPreview(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            DataTable dataTable = FetchDataFromDatabase(inputValues, connection);

            StringBuilder tableHtml = new StringBuilder();
            tableHtml.Append("<table class='table table-bordered table-striped'><thead><tr>");

            // Add headers and sub-headers
            tableHtml.Append("<th rowspan='2'>HARI</th>");
            /*tableHtml.Append("<th rowspan='2'>SF</th>");
            tableHtml.Append("<th rowspan='2'>GOL</th>");
            tableHtml.Append("<th colspan='4'>ABSEN</th>");
            tableHtml.Append("<th rowspan='2'>KET</th>");
            tableHtml.Append("<th colspan='5'>LEMBUR</th>");
            tableHtml.Append("<th rowspan='2'>ISRHT (jam)</th>");
            tableHtml.Append("</tr><tr>");
            tableHtml.Append("<th>TGL MSK</th>");
            tableHtml.Append("<th>MSK</th>");
            tableHtml.Append("<th>KLR</th>");
            tableHtml.Append("<th>TGL KLR</th>");
            tableHtml.Append("<th>MULAI</th>");
            tableHtml.Append("<th>BTS LMBUR</th>");
            tableHtml.Append("<th>JAM</th>");
            tableHtml.Append("<th>DEC</th>");
            tableHtml.Append("<th>RUPIAH</th>");
            tableHtml.Append("</tr></thead><tbody>"); */////////
        /*

            // Add rows
            foreach (DataRow row in dataTable.Rows)
            {
                tableHtml.Append("<tr>");
                tableHtml.Append($"<td>{row["HRIX"]}</td>"); // HARI
                /*tableHtml.Append($"<td>{row["S"]}</td>");    // SF
                tableHtml.Append($"<td>{row["KODE"]}</td>"); // GOL

                // ABSEN columns
                tableHtml.Append($"<td>{row["TANGGAL"]}</td>"); // TGL MSK
                tableHtml.Append($"<td>{row["JMSK"]}</td>");    // MSK
                tableHtml.Append($"<td>{row["JKLR"]}</td>");    // KLR
                tableHtml.Append($"<td>{row["TKLR"]}</td>");    // TGL KLR

                tableHtml.Append($"<td>{row["KET"]}</td>");     // KET

                decimal jam = row["XLMB"] != DBNull.Value && decimal.TryParse(row["XLMB"].ToString(), out var parsedJam) ? parsedJam : 0;
                decimal dec = row["QLMBR"] != DBNull.Value && decimal.TryParse(row["QLMBR"].ToString(), out var parsedDec) ? parsedDec : 0;
                decimal rupiah = row["RUPIAH"] != DBNull.Value && decimal.TryParse(row["RUPIAH"].ToString(), out var parsedRupiah) ? parsedRupiah : 0;

                tableHtml.Append($"<td>{jam}</td>");          // JAM
                tableHtml.Append($"<td>{dec}</td>");          // DEC
                tableHtml.Append($"<td>{dec * rupiah}</td>"); // RUPIAH

                // Calculate ISRHT (jam)
                decimal isk = row["ISK"] != DBNull.Value && decimal.TryParse(row["ISK"].ToString(), out var parsedIsk) ? parsedIsk : 0;
                decimal ism = row["ISM"] != DBNull.Value && decimal.TryParse(row["ISM"].ToString(), out var parsedIsm) ? parsedIsm : 0;

                tableHtml.Append($"<td>{isk - ism}</td>");    // ISRHT (jam)*/
        /*
                tableHtml.Append("</tr>");
            }

    tableHtml.Append("</tbody></table>");
            return tableHtml.ToString();
        }

*/
        public byte[] GeneratePDF(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            DataTable dataTable = FetchDataFromDatabase(inputValues, connection);

            using (var memoryStream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdfDocument = new PdfDocument(writer);
                Document document = new Document(pdfDocument, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Add title
                document.Add(new Paragraph("PURA GROUP - UNIT OFFSET")
                    .SetFont(boldFont).SetFontSize(10));
                document.Add(new Paragraph("LAPORAN LEMBUR DETAIL")
                    .SetFont(boldFont).SetFontSize(12));
                document.Add(new Paragraph($"Periode Tgl : {inputValues["inputDARI_TGL"]} - {inputValues["inputSMP_TGL"]}")
                    .SetFont(boldFont).SetFontSize(10));

                if (dataTable.Rows.Count > 0)
                {
                    DataRow firstRow = dataTable.Rows[0];
                    string inputNIK = firstRow["NIK"].ToString();
                    string nama = firstRow["NAMA"].ToString();
                    string bagian = firstRow["BAGIAN"].ToString();
                    string st = firstRow["ST"].ToString();
                    string umr = firstRow["UMR"].ToString();
                    string rupiah = firstRow["RUPIAH"].ToString();

                    document.Add(new Paragraph($"{inputNIK} . {nama} - {bagian} / {st} - UPAH : {umr} / {rupiah}")
                        .SetFont(boldFont).SetFontSize(9));
                }

                // Define table
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(UnitValue
                    .CreatePercentArray(new float[] { 4, 2, 4, 6, 4, 4, 5, 6, 4, 4, 4, 3, 6, 4 }))
                    .SetWidth(UnitValue.CreatePercentValue(100));

                // Add header
                table.AddHeaderCell(CreateHeaderCell("HARI", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("SF", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("GOL", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("ABSEN", boldFont, 1, 4));
                table.AddHeaderCell(CreateHeaderCell("KET", boldFont, 2, 1));
                table.AddHeaderCell(CreateHeaderCell("LEMBUR", boldFont, 1, 5));
                table.AddHeaderCell(CreateHeaderCell("ISRHT (jam)", boldFont, 2, 1));
                // Other headers...

                // Add rows
                foreach (DataRow row in dataTable.Rows)
                {
                    table.AddCell(CreateDataCell(row["HRIX"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["S"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["KODE"].ToString(), regularFont));
                    // Add other cells...
                }

                document.Add(table);
                document.Close();
                return memoryStream.ToArray();
            }
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
            tableHtml.Append("<th rowspan='2'>HARI</th>");
            tableHtml.Append("<th rowspan='2'>SF</th>");
            tableHtml.Append("<th rowspan='2'>GOL</th>");
            tableHtml.Append("<th colspan='4'>ABSEN</th>");
            tableHtml.Append("<th rowspan='2'>KET</th>");
            tableHtml.Append("<th colspan='5'>LEMBUR</th>");
            tableHtml.Append("<th rowspan='2'>ISRHT (jam)</th>");
            tableHtml.Append("</tr><tr>");
            tableHtml.Append("<th>TGL MSK</th>");
            tableHtml.Append("<th>MSK</th>");
            tableHtml.Append("<th>KLR</th>");
            tableHtml.Append("<th>TGL KLR</th>");
            tableHtml.Append("<th>MULAI</th>");
            tableHtml.Append("<th>BTS LMBUR</th>");
            tableHtml.Append("<th>JAM</th>");
            tableHtml.Append("<th>DEC</th>");
            tableHtml.Append("<th>RUPIAH</th>");
            tableHtml.Append("</tr></thead><tbody>");

            foreach (DataRow row in dataTable.Rows)
            {
                tableHtml.Append("<tr>");
                tableHtml.Append($"<td>{row["HRIX"]}</td>");
                tableHtml.Append($"<td>{row["SFT"]}</td>");
                tableHtml.Append($"<td>{row["KODE"]}</td>");
                tableHtml.Append($"<td>{row["TANGGAL"]}</td>");
                tableHtml.Append($"<td>{row["JMSK"]}</td>");
                tableHtml.Append($"<td>{row["JKLR"]}</td>");
                tableHtml.Append($"<td>{row["TKLR"]}</td>");
                tableHtml.Append($"<td>{row["KET"]}</td>");
                tableHtml.Append($"<td>{row["KLR"]}</td>");
                tableHtml.Append($"<td>{row["BTS"]}</td>");
                tableHtml.Append($"<td>{row["XLMB"]}</td>");
                tableHtml.Append($"<td>{row["QLMBR"]}</td>");
                tableHtml.Append($"<td>{Convert.ToDecimal(row["RUPIAH"]) * Convert.ToDecimal(row["QLMBR"])}</td>");
                tableHtml.Append($"<td>{Convert.ToDecimal(row["ISK"]) - Convert.ToDecimal(row["ISM"])}</td>");
                tableHtml.Append("</tr>");
            }

            tableHtml.Append("</tbody></table>");
            return tableHtml.ToString();
        }
        public Cell CreateHeaderCell(string content, PdfFont font, int rowSpan, int colSpan)
        {
            Cell cell = new Cell(rowSpan, colSpan)
                .Add(new Paragraph(content)
                    .SetFont(font)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER));
            return cell;
        }
        public Cell CreateDataCell(string content, PdfFont font)
        {
            Cell cell = new Cell()
                .Add(new Paragraph(content)
                    .SetFont(font)
                    .SetFontSize(6)
                    .SetTextAlignment(TextAlignment.LEFT))
                .SetBorder(Border.NO_BORDER)
                .SetBorderLeft(new DottedBorder(0.5f))
                .SetBorderRight(new DottedBorder(0.5f))
                .SetPaddingTop(1)
                .SetPaddingBottom(1);
            return cell;
        }

        /*
        public DataTable FetchDataFromDatabase(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            // Get values from the dictionary using the input prefixed names
            string CNBAGIAN = inputValues["inputBAGIAN"];
            string CNSTATUS = inputValues["inputSTATUS"];
            string CNNIK = inputValues["inputNIK"];
            string DNDARI_TGL = inputValues["inputDNDARI_TGL"];
            string DNSMP_TGL = inputValues["inputDNSMP_TGL"];

            // Convert date strings to DateTime objects
            DateTime dariTanggal;
            DateTime sampaiTanggal;

            if (!DateTime.TryParseExact(DNDARI_TGL, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dariTanggal))
            {
                throw new Exception("Invalid DNDARI_TGL format. Expected format is dd/MM/yyyy.");
            }

            if (!DateTime.TryParseExact(DNSMP_TGL, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out sampaiTanggal))
            {
                throw new Exception("Invalid DNSMP_TGL format. Expected format is dd/MM/yyyy.");
            }

            DataTable dataTable = new DataTable();

            using (var command = new OracleCommand("PABSEN.ABSENSI2_URSA2", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters with the correct names expected by the stored procedure
                command.Parameters.Add("DNDARI_TGL", OracleDbType.Varchar2).Value = dariTanggal.ToString("dd/MM/yyyy");
                command.Parameters.Add("DNSMP_TGL", OracleDbType.Varchar2).Value = sampaiTanggal.ToString("dd/MM/yyyy");
                command.Parameters.Add("CNDEPT", OracleDbType.Varchar2).Value = CNBAGIAN;
                command.Parameters.Add("CNNIK", OracleDbType.Varchar2).Value = CNNIK;
                command.Parameters.Add("NNSTATUS", OracleDbType.Int32).Value = Convert.ToInt32(CNSTATUS);
                command.Parameters.Add("RET", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var adapter = new OracleDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }
        */
        public DataTable FetchDataFromDatabase(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            
            //string DNDARI_TGL = inputValues["inputDARI_TGL"];
            //string DNSMP_TGL = inputValues["inputSMP_TGL"];
            //string CNDEPT = inputValues.ContainsKey("inputDEPT") ? inputValues["inputDEPT"] : null;
            //string CNNIK = inputValues.ContainsKey("inputNIK") ? inputValues["inputNIK"] : null;
            string CNNIK = inputValues["inputNIK"];
            //string NNSTATUS = inputValues.ContainsKey("inputSTATUS") ? inputValues["inputSTATUS"] : null;

            /*
            // Parse dates
            var parsedDateDari = DateTime.ParseExact(DNDARI_TGL, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var dariTanggal = parsedDateDari.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);

            var parsedDateSampai = DateTime.ParseExact(DNSMP_TGL, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var sampaiTanggal = parsedDateSampai.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            */
            DataTable dataTable = new DataTable();

            /*
            // Define the schema of the DataTable
            dataTable.Columns.Add("HARI", typeof(string));
            dataTable.Columns.Add("SF", typeof(string));
            dataTable.Columns.Add("GOL", typeof(string));
            dataTable.Columns.Add("TGL_MSK", typeof(DateTime));
            dataTable.Columns.Add("MSK", typeof(decimal));
            dataTable.Columns.Add("KLR", typeof(decimal));
            dataTable.Columns.Add("TGL_KLR", typeof(DateTime));
            dataTable.Columns.Add("KET", typeof(string));
            dataTable.Columns.Add("MULAI", typeof(decimal));
            dataTable.Columns.Add("BTS_LMBUR", typeof(decimal));
            dataTable.Columns.Add("JAM", typeof(decimal));
            dataTable.Columns.Add("DEC", typeof(decimal));
            dataTable.Columns.Add("RUPIAH", typeof(decimal));
            dataTable.Columns.Add("ISRHT", typeof(decimal));
            */

            //System.Diagnostics.Debug.WriteLine($"{dariTanggal}, {sampaiTanggal}, {CNDEPT}, {CNNIK}, {NNSTATUS}");

            try
            {
                using (var command = new OracleCommand("PABSEN.absensi2_ursa2", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    //command.Parameters.Add("DNDARI_TGL", OracleDbType.Varchar2).Value = dariTanggal;
                    //command.Parameters.Add("DNSMP_TGL", OracleDbType.Varchar2).Value = sampaiTanggal;
                    
                    //command.Parameters.Add("CNDEPT", OracleDbType.Varchar2).Value = CNDEPT;
                    
                    command.Parameters.Add("CNNIK", OracleDbType.Varchar2).Value = CNNIK;
                    /*
                    if (!string.IsNullOrEmpty(NNSTATUS))
                    {
                        if (int.TryParse(NNSTATUS, out int nnStatusValue))
                        {
                            command.Parameters.Add("NNSTATUS", OracleDbType.Int32).Value = nnStatusValue;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Invalid NNSTATUS value provided. Setting to default (0).");
                            command.Parameters.Add("NNSTATUS", OracleDbType.Int32).Value = 0; // Default value
                        }
                    }
                    else
                    {
                        command.Parameters.Add("NNSTATUS", OracleDbType.Int32).Value = DBNull.Value;  // Default value
                    }
                    */

                    command.Parameters.Add("RET", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                    using (var adapter = new OracleDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching data: {ex.Message}");
            }

            if (dataTable.Rows.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("FetchDataFromDatabase returned an empty DataTable.");
            }

            return dataTable;
        }

    }
}