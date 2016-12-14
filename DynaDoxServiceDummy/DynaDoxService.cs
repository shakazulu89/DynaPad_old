using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
//using DynaPad;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;

using System.Net;
using System.Diagnostics;


namespace DynaDoxServiceDummy
{
	public class DynaDoxService
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			ExportToPdf("stum");
		}

		public DynaDoxService()
		{
		}

		public string ExportToPdf(string answers)
		{
			DataTable table = new DataTable();
			table.Columns.Add("Question", typeof(string));
			table.Columns.Add("Answer", typeof(string));

			// Here we add five DataRows.
			table.Rows.Add("Indocin", "David");
			table.Rows.Add("Enebrel", "Sam");
			table.Rows.Add("Hydralazine", "Christoff");
			table.Rows.Add("Combivent", "Janet");
			table.Rows.Add("Dilantin", "Melanie");

			ExportToPdf(table);

			return answers;
		}

		public void ExportToPdf(DataTable dt)
		{
			Document document = new Document();
			string path = HttpContext.Current.Server.MapPath("sample.pdf");
			PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(path, FileMode.Create));
			document.Open();
			iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 5);

			PdfPTable table = new PdfPTable(dt.Columns.Count);
			PdfPRow row = null;
			float[] widths = new float[] { 4f, 4f, 4f, 4f };

			table.SetWidths(widths);

			table.WidthPercentage = 100;
			int iCol = 0;
			string colname = "";
			PdfPCell cell = new PdfPCell(new Phrase("Products"));

			cell.Colspan = dt.Columns.Count;

			foreach (DataColumn c in dt.Columns)
			{

				table.AddCell(new Phrase(c.ColumnName, font5));
			}

			foreach (DataRow r in dt.Rows)
			{
				if (dt.Rows.Count > 0)
				{
					table.AddCell(new Phrase(r[0].ToString(), font5));
					table.AddCell(new Phrase(r[1].ToString(), font5));
				}
			}
			document.Add(table);
			document.Close();
		}
	}
}
