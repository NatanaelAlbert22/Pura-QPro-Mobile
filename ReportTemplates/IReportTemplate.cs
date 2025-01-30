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
    public interface IReportTemplate
    {
        byte[] GeneratePDF(Dictionary<string, string> inputValues, OracleConnection connection);
        string GenerateHTMLPreview(Dictionary<string, string> inputValues, OracleConnection connection);
        Cell CreateHeaderCell(string content, PdfFont font, int rowSpan, int colSpan);
        Cell CreateDataCell(string content, PdfFont font);
        DataTable FetchDataFromDatabase(Dictionary<string, string> inputValues, OracleConnection connection);
    }
}
