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

namespace Pura_Gaji_Viewer.ReportTemplates.Daftar_Laporan
{
    public class Cuti_Jatuh_Tempo : IReportTemplate
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
                PageSize portrait = PageSize.A4;
                Document document = new Document(pdfDocument, portrait);
                document.SetMargins(20, 20, 20, 20);

                // Use fonts
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
                PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.COURIER);

                // Add title
                document.Add(new Paragraph("PURA GROUP - UNIT OFFSET").SetFont(boldFont).SetFontSize(10).SetMarginBottom(0).SetMarginTop(0));
                document.Add(new Paragraph("SISA CUTI SETAHUN").SetFont(boldFont).SetFontSize(8).SetMarginBottom(0).SetMarginTop(0));
                document.Add(new Paragraph($"PERIODE : {DateTime.Now:MM / yyyy}").SetFont(boldFont).SetFontSize(8).SetMarginBottom(0).SetMarginTop(0));
                document.Add(new Paragraph($"{inputValues["inputBAGIAN"]}").SetFont(boldFont).SetFontSize(9).SetMarginBottom(10).SetMarginTop(0));


                
                // Define table with fixed column widths
                iText.Layout.Element.Table table = new iText.Layout.Element.Table(UnitValue
                    .CreatePercentArray(new float[] { 1, 2, 5, 1, 1, 1, 1, 3, 3, 3, 3, 3 })); // Column widths
                table.SetWidth(UnitValue.CreatePercentValue(100)); // Full width table

                // Add table headers
                table.AddHeaderCell(CreateHeaderCell("NO", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("NIK", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("NAMA", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("ST", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("CT", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("HR", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("SISA", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("UMR LAMA", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("JUMLAH", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("DIBULATKAN", regularFont, 1, 1));
                table.AddHeaderCell(CreateHeaderCell("TT TANGAN", regularFont, 1, 2));


                // Add data rows
                int rowNumber = 1;
                decimal totalJumlah = 0, totalDibulatkan = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    decimal ct = Convert.ToDecimal(row["JMLCT"]);
                    decimal hr = Convert.ToDecimal(row["RPCT"]);
                    decimal sisa = ct - hr;
                    decimal umrLama = Convert.ToDecimal(row["UMR"]);
                    decimal jumlah = umrLama * sisa;
                    decimal dibulatkan = Math.Round(jumlah);

                    totalJumlah += jumlah;
                    totalDibulatkan += dibulatkan;

                    table.AddCell(CreateDataCell(rowNumber.ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["NIK"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["NAMA"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(row["STAT"].ToString(), regularFont));
                    table.AddCell(CreateDataCell(ct.ToString(), regularFont));
                    table.AddCell(CreateDataCell(hr.ToString(), regularFont));
                    table.AddCell(CreateDataCell(sisa.ToString(), regularFont));
                    table.AddCell(CreateDataCell(umrLama.ToString("F2"), regularFont));
                    table.AddCell(CreateDataCell(jumlah.ToString("F2"), regularFont));
                    table.AddCell(CreateDataCell(dibulatkan.ToString("F2"), regularFont));

                    // TT TANGAN
                    table.AddCell(CreateDataCell((rowNumber % 2 == 1 ? rowNumber.ToString() : ""), regularFont));
                    table.AddCell(CreateDataCell((rowNumber % 2 == 0 ? rowNumber.ToString() : ""), regularFont));

                    rowNumber++;
                }

                // Add footer row to the table
                table.AddCell(new Cell(1, 3)
                    .Add(new Paragraph(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                        .SetFont(regularFont)
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.LEFT))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderLeft(new SolidBorder(1))
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)));

                table.AddCell(new Cell(1, 5)
                    .Add(new Paragraph("TOTAL")
                        .SetFont(boldFont)
                        .SetFontSize(9)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)));

                // Add totals to the last cells
                table.AddCell(CreateDataCell(totalJumlah.ToString("F2"), boldFont)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)));

                table.AddCell(CreateDataCell(totalDibulatkan.ToString("F2"), boldFont)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1)));

                // Empty cells for TT TANGAN
                table.AddCell(new Cell(1, 2).SetBorder(Border.NO_BORDER)
                    .SetBorder(Border.NO_BORDER)
                    .SetBorderTop(new SolidBorder(1))
                    .SetBorderBottom(new SolidBorder(1))
                    .SetBorderRight(new SolidBorder(1)));

                // Add the table to the document
                document.Add(table);

                // Add any additional finalization steps here
                document.Close();
                return memoryStream.ToArray();

            }
        }
        public Cell CreateHeaderCell(string content, PdfFont font, int rowspan, int colspan)
        {
            Cell cell = new Cell(rowspan, colspan)
                .Add(new Paragraph(content)
                    .SetFont(font)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE));
                //.SetBackgroundColor(new DeviceRgb(200, 200, 200)); // Light gray background
            return cell;
        }
        public Cell CreateDataCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.LEFT))
                .SetBorder(Border.NO_BORDER)
                .SetBorderLeft(new DottedBorder(0.8f))
                .SetBorderRight(new DottedBorder(0.8f));
       
        }
        public DataTable FetchDataFromDatabase(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            // Get values from the dictionary using the input prefixed names
            string CNBAGIAN = inputValues["inputBAGIAN"];
            string CNSTATUS = inputValues["inputSTATUS"];
            string CNNIK = inputValues["inputNIK"];

            DataTable dataTable = new DataTable();

            using (var command = new OracleCommand("PABSEN.CUTI", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Add parameters with the correct names expected by the stored procedure
                command.Parameters.Add("CNBAGIAN", OracleDbType.Varchar2).Value = CNBAGIAN;
                command.Parameters.Add("CNSTATUS", OracleDbType.Varchar2).Value = CNSTATUS;
                command.Parameters.Add("CNNIK", OracleDbType.Varchar2).Value = CNNIK;
                command.Parameters.Add("RET", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var adapter = new OracleDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }
        public string GenerateHTMLPreview(Dictionary<string, string> inputValues, OracleConnection connection)
        {
            DataTable dataTable = FetchDataFromDatabase(inputValues, connection);

            StringBuilder tableHtml = new StringBuilder();
            tableHtml.Append("<table class='table table-bordered table-striped'><thead><tr style='text-align: center;'>");

            // Add headers with the same structure as the PDF
            tableHtml.Append("<th>NO</th>");
            tableHtml.Append("<th>NIK</th>");
            tableHtml.Append("<th>NAMA</th>");
            tableHtml.Append("<th>ST</th>");
            tableHtml.Append("<th>CT</th>");
            tableHtml.Append("<th>HR</th>");
            tableHtml.Append("<th>SISA</th>");
            tableHtml.Append("<th>UMR LAMA</th>");
            tableHtml.Append("<th>JUMLAH</th>");
            tableHtml.Append("<th>DIBULATKAN</th>");
            tableHtml.Append("<th colspan='2'>TT TANGAN</th>");
            tableHtml.Append("</tr></thead><tbody>");

            // Add rows
            int rowNumber = 1;
            foreach (DataRow row in dataTable.Rows)
            {
                tableHtml.Append("<tr>");
                tableHtml.Append($"<td>{rowNumber}</td>");
                tableHtml.Append($"<td>{row["NIK"]}</td>");
                tableHtml.Append($"<td>{row["NAMA"]}</td>");
                tableHtml.Append($"<td>{row["STAT"]}</td>");
                tableHtml.Append($"<td>{row["JMLCT"]}</td>");
                tableHtml.Append($"<td>{row["RPCT"]}</td>");
                tableHtml.Append($"<td>{Convert.ToDecimal(row["JMLCT"]) - Convert.ToDecimal(row["RPCT"])}</td>");
                tableHtml.Append($"<td>{row["UMR"]}</td>");

                decimal jumlah = Convert.ToDecimal(row["UMR"]) * (Convert.ToDecimal(row["JMLCT"]) - Convert.ToDecimal(row["RPCT"]));
                tableHtml.Append($"<td>{jumlah}</td>");
                tableHtml.Append($"<td>{Math.Round(jumlah)}</td>");

                // Handle TT TANGAN structure
                tableHtml.Append($"<td>{(rowNumber % 2 == 1 ? rowNumber.ToString() : "")}</td>"); // Left column
                tableHtml.Append($"<td>{(rowNumber % 2 == 0 ? rowNumber.ToString() : "")}</td>"); // Right column
                tableHtml.Append("</tr>");
                rowNumber++;
            }

            tableHtml.Append("</tbody></table>");
            return tableHtml.ToString();
        }
    }
}