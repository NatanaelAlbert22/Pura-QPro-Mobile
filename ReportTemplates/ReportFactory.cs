using Pura_Gaji_Viewer.ReportTemplates.Daftar_Laporan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pura_Gaji_Viewer.ReportTemplates
{
    public static class ReportFactory
    {
        public static IReportTemplate GetReport(string reportName)
        {
            switch (reportName)
            {
                case "Daftar Pegawai":
                    return new Daftar_Pegawai();
                case "Cuti Jatuh Tempo":
                    return new Cuti_Jatuh_Tempo();
                case "(Adidas-Ursa) - Detail - Lembur":
                    return new Detail_Lembur();
                default:
                   throw new ArgumentException("Invalid report name");
            }
        }
    }

}