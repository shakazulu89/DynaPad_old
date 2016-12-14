using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
//using System.Web.Script.Services;
using System.Web.Services;
using DynaPad;
//using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;

using System.Net;
using System.Diagnostics;
using iTextSharp.text.pdf.draw;
using HtmlAgilityPack;

/// <summary>
/// Summary description for DynaDoxService
/// </summary>
[WebService(Namespace = "http://www.dynadox.pro/webservices/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class DynaDoxService //: System.Web.Services.WebService
{

	[WebMethod]
	public string HelloWorld()
	{
		return "Hello World";
	}

	[WebMethod]
	public List<string[]> GetAnswerPresets(string formId, string sectionId, string doctorId, bool isDocInput,	string locationId)
	{

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))
		{
			myConnection.Open();

			SqlCommand myCommand = new SqlCommand("SELECT AnswersPresets.flgDoctorInput, AnswersPresets.flgSectionAnswerPreset, AnswersPresets.numAnswerPresetID, AnswersPresets.numDoctorID, AnswersPresets.numFormID, AnswersPresets.numLocationID, AnswersPresets.numSectionID, AnswersPresets.txtAnswerPresetName FROM AnswersPresets WHERE AnswersPresets.numFormID = @numFormID AND AnswersPresets.numDoctorID = @numDoctorID ORDER BY flgSectionAnswerPreset ASC", myConnection);
			myCommand.Parameters.AddWithValue("@numFormID", formId);
			myCommand.Parameters.AddWithValue("@numDoctorID", doctorId);
			var dtAnswerPresets = new DataTable();

			var daAnswerPresets = new SqlDataAdapter(myCommand);
			daAnswerPresets.Fill(dtAnswerPresets);

			List<string[]> presetsList = new List<String[]>();

			foreach (DataRow drPreset in dtAnswerPresets.Rows)
			{
				string presetSectionId = drPreset["numSectionID"].ToString();
				string presetName = drPreset["txtAnswerPresetName"].ToString();
				string presetPath = "AnswersPresets/" + formId + "/" + doctorId + "/" + sectionId + "/" +
								   drPreset["numAnswerPresetID"] + ".txt";
				string presetJson = File.ReadAllText(presetPath);
				presetsList.Add(new String[] { presetSectionId, presetName, presetJson });
			}

			return presetsList;
		}
	}

	[WebMethod]
	public string SaveAnswerPreset(string formId, string sectionId, string doctorId, bool isDocInput, string presetName, string presetJson,	string locationId)
	{

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			List<SqlParameter> spPreset = new List<SqlParameter>()
							{
								new SqlParameter() { ParameterName = "flgDoctorInput", Value =  isDocInput},
								new SqlParameter() { ParameterName = "flgSectionAnswerPreset", Value =  string.IsNullOrEmpty(sectionId)? false : true},
								new SqlParameter() { ParameterName = "numFormID", Value = formId },
								new SqlParameter() { ParameterName = "txtAnswerPresetName", Value = presetName }
							};
			if (!string.IsNullOrEmpty(sectionId))
			{
				spPreset.Add(new SqlParameter() { ParameterName = "numLocationID", Value = locationId });
				//new SqlParameter() { ParameterName = "numDoctorID", Value = doctorId },
				//spPreset.Add(new SqlParameter() { ParameterName = "numSectionID", Value = sectionId });
			}

			var presetId = SqlFunctions.SqlInsert("", "AnswersPresets", "numAnswerPresetID", "", myConnection, spPreset);

			string presetPath = string.IsNullOrEmpty(sectionId) ? "AnswersPresets/" + formId + "/" + doctorId + "/" + presetId + ".txt" : "AnswersPresets/" + formId + "/" + doctorId + "/" + sectionId + "/" + presetId + ".txt";

			File.WriteAllText(presetPath, presetJson);

			return "success";
		}

	}


	[WebMethod]
	public string SaveDictation(string formId, string sectionId, string doctorId, bool isDocInput, string locationId, string dictationTitle, byte[] arrDictation)
	{

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			List<SqlParameter> spDictation = new List<SqlParameter>()
							{
								new SqlParameter() { ParameterName = "flgDoctorInput", Value =  isDocInput},
								new SqlParameter() { ParameterName = "flgSectionDictation", Value =  string.IsNullOrEmpty(sectionId)? false : true},
								new SqlParameter() { ParameterName = "numFormID", Value = formId },
								new SqlParameter() { ParameterName = "txtDictationTitle", Value = dictationTitle }
							};
			if (!string.IsNullOrEmpty(sectionId))
			{
				spDictation.Add(new SqlParameter() { ParameterName = "numDoctorID", Value = doctorId });
				spDictation.Add(new SqlParameter() { ParameterName = "numLocationID", Value = locationId });
				spDictation.Add(new SqlParameter() { ParameterName = "numSectionID", Value = sectionId });
			}

			var dictatioId = SqlFunctions.SqlInsert("", "Dictations", "numDictationID", "", myConnection, spDictation);

			string dictationDir = string.IsNullOrEmpty(sectionId) ? "Dictations/" + formId + "/" + doctorId + "/": "Dictations/" + formId + "/" + doctorId + "/" + sectionId + "/";

			if (!Directory.Exists(dictationDir))
			{
				Directory.CreateDirectory(dictationDir);
			}

			string dictationPath = dictationDir + dictatioId + ".aac";

			File.WriteAllBytes(dictationPath, arrDictation);

			return Directory.GetParent(dictationPath).FullName;
		}

	}


	//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	[WebMethod]
	public string GetMenuJson(string formID, string clientID, string ApptDate, bool isDocInput)
	{
		StringBuilder json = new StringBuilder();

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			SqlCommand myCommand = myConnection.CreateCommand();

			myCommand.CommandText = "";

			using (SqlCommand cmd = myCommand)
			{
				var dtQuestions = new DataTable();

				var daComplaints = new SqlDataAdapter(myCommand);
				daComplaints.Fill(dtQuestions);

				string getit = buildclass(dtQuestions, formID);

				myConnection.Close();

				return getit;
			}
		}
	}


	public string[,] GetSectionPresets(string formid, string doctorid, string nopreset, string sectionid)
	{
		string[,] grp = {
			{"No Preset", nopreset},
			{"Spanish", File.ReadAllText("preset_section_Spanish.json")},
			{"Russian", File.ReadAllText("preset_section_Russian.json")},
			{"Chinese", File.ReadAllText("preset_section_Chinese.json")}
		};


		return grp;
	}


	public string[,] GetFormPresets(string formid, string doctorid, string nopreset)
	{
		string[,] fgrp = {
			{"No Preset", nopreset },
			{"Spanish", File.ReadAllText("preset_form_Spanish.json") },
			{"Russian", File.ReadAllText("preset_form_Russian.json") },
			{"Chinese", File.ReadAllText("preset_form_Chinese.json") } };

		return fgrp;
	}


	public string ExportToPdf()
	{
		//SelectedQForm.answeredQForm = JsonConvert.DeserializeObject<QForm>(answers);


		// TODO doctor and form info in SelectedQForm.answeredQform




		//string clientLogo = System.Web.HttpContext.Current.Session["CompanyName"].ToString();
		//clientLogo = clientLogo.Replace(" ", "");
		//string clogo = clientLogo + ".jpg";
		//string imageFilePath = System.Web.HttpContext.Current.Server.MapPath("../ClientLogo/" + clogo + "");
		//iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageFilePath);
		////Resize image depend upon your need   
		//jpg.ScaleToFit(80f, 60f);
		////Give space before image   
		//jpg.SpacingBefore = 0f;
		////Give some space after the image   
		//jpg.SpacingAfter = 1f;
		//jpg.Alignment = Element.HEADER;
		//pdfDoc.Add(jpg);






		var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		var filename = Path.Combine(documents, "summary.pdf");
		Process.Start(filename);

		Document document = new Document();
		PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
		document.Open();


		DataTable dtFormHeader = new DataTable();
		dtFormHeader.Columns.Add();
		dtFormHeader.Rows.Add("Form Information");

		AddTableToPdf(dtFormHeader, document, false, true);

		//Header and appt info
		DataTable dtHeader = new DataTable();
		dtHeader.Columns.Add();
		dtHeader.Columns.Add();
		dtHeader.Rows.Add("Patient Name: ", "Miser Joe");
		dtHeader.Rows.Add("Doctor Name: ", "Brian Wolin");
		dtHeader.Rows.Add("Date Created: ", DateTime.Today.ToShortDateString());
		dtHeader.Rows.Add("Location: ", "Sant Blai");

		AddTableToPdf(dtHeader, document, true, false);


		foreach (FormSection section in SelectedQForm.answeredQForm.FormSections)
		{
			DataTable dtSectionHeader = new DataTable();
			dtSectionHeader.Columns.Add();
			dtSectionHeader.Rows.Add(section.SectionName);

			AddTableToPdf(dtSectionHeader, document, false, true);



			DataTable dt = new DataTable();
			dt.Columns.Add();
			dt.Columns.Add();
			dt.Columns.Add();
			// TODO section logi
			//Paragraph pg = new Paragraph(section.SectionName, iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 10, 1));
			//pg.SpacingAfter = 5;

			//document.Add(pg);h
			//document.Add(new Paragraph(section.SectionName, FontFactory.GetFont(FontFactory.HELVETICA, 6, 1)) { SpacingAfter = 5, SpacingBefore = 5});
			//, iTextSharp.text.Font.UNDERLINE

			//Create Chunk for underline
			//Chunk chkHeader = new Chunk(section.SectionName, FontFactory.GetFont(FontFactory.HELVETICA, 6, 0));
			////chkHeader.SetUnderline(1f, 1f);
			////Add Chunk to paragraph
			//Paragraph pHeader = new Paragraph(chkHeader) { SpacingBefore = 5, SpacingAfter = 0 };
			//Chunk linebreak = new Chunk(new LineSeparator(1f, 100f, GrayColor.DARK_GRAY, Element.ALIGN_CENTER, 5));
			////pHeader.Add(linebreak);
			//document.Add(pHeader);
			//document.Add(linebreak);


			int qIndex = 1;

			foreach (SectionQuestion question in section.SectionQuestions)
			{

				if (question.QuestionOptions != null && question.QuestionOptions.Count > 0)
				{
					bool rowAdded = false;
					foreach (QuestionOption option in question.QuestionOptions)
					{
						if (option.Chosen)
						{
							if (!rowAdded)
							{
								dt.Rows.Add(qIndex + ".", question.QuestionText, option.OptionText);
								rowAdded = true;

								qIndex++;
							}
							else
							{
								dt.Rows.Add("", "", option.OptionText);
							}
						}
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(question.AnswerText))
					{
						dt.Rows.Add(qIndex + ".", question.QuestionText, question.AnswerText);

						qIndex++;
					}
				}

				//qIndex++;
			}

			AddTableToPdf(dt, document, false, false);
		}
		document.Close();

		//disabled grey out (label and input) - like boolean
		//text alligned right no bueno
		//bold radio, check
		// check confirm button
		//extra section causing extra grey




		return filename;
	}


	private void AddTableToPdf(DataTable dt, Document document, bool isHeader, bool isSectionHeader)
	{
		PdfPTable table = new PdfPTable(dt.Columns.Count);
		Font font5 = FontFactory.GetFont(FontFactory.HELVETICA, 5);
		//PdfPRow row = null;

		float[] widths;
		if (isHeader)
		{
			widths = new float[] { 20f, 200f };
			table.DefaultCell.Border = Rectangle.NO_BORDER;
			table.DefaultCell.BorderWidth = 0;
			table.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;
			table.DefaultCell.BackgroundColor = BaseColor.LIGHT_GRAY;
		}
		else if (isSectionHeader)
		{
			widths = new float[] { 100f };
			table.DefaultCell.Border = Rectangle.BOTTOM_BORDER;
			table.SpacingAfter = 3;
			table.SpacingBefore = 5;
		}
		else
		{
			widths = new float[] { 8f, 200f, 200f };
			table.DefaultCell.Border = Rectangle.NO_BORDER;
		}

		table.SetWidths(widths);
		//table.DefaultCell.Border = Rectangle.NO_BORDER;
		//table.LockedWidth = true;
		//table.TotalWidth = 400f;
		//table.WidthPercentage = 100;
		table.HorizontalAlignment = 0;
		//int iCol = 0;
		//string colname = "";
		//PdfPCell cell = new PdfPCell(new Phrase("Products"));

		//cell.Colspan = dt.Columns.Count;

		//foreach (DataColumn c in dt.Columns)
		//{

		//	table.AddCell(new Phrase(c.ColumnName, font5));
		//}

		foreach (DataRow r in dt.Rows)
		{
			if (dt.Rows.Count > 0)
			{
				for (int i = 0; i < r.ItemArray.Length; i++)
				{
					table.AddCell(new Phrase(r[i].ToString(), font5));
				}
				//table.AddCell(new Phrase(r[0].ToString(), font5));
				//table.AddCell(new Phrase(r[1].ToString(), font5));
			}
		}
		document.Add(table);
	}






	//[ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = true)]
	[WebMethod]
	public string GetAllQuestionnaires(string userId)
	{
		//GlobalFuncsClass.sendErrorEmail("QUESTIONATOR SERVICE TEST", formID);

		SqlConnection myConnectionLocal = new SqlConnection("server=localhost;integrated security = SSPI;database=Questionator;MultipleActiveResultSets=True;Connection Timeout=160");
		StringBuilder json = new StringBuilder();

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			SqlCommand myCommand = new SqlCommand("SELECT QuestionForms.numFormID, REPLACE(QuestionForms.txtFormName, '''', '') AS [txtFormName], QuestionForms.dteDateCreated, QuestionForms.dteDateModified FROM QuestionForms", myConnection);

			using (SqlCommand cmd = myCommand)
			{
				var dtQuestionnaires = new DataTable();

				var daQuestionnaires = new SqlDataAdapter(myCommand);
				daQuestionnaires.Fill(dtQuestionnaires);
				string jsonResult = JsonConvert.SerializeObject(dtQuestionnaires);

				string result;
				StringWriter sw = new StringWriter();
				JsonToken jt = new JsonToken();

				using (JsonWriter jsonWriter = new JsonTextWriter(sw))
				{
					jsonWriter.Formatting = (Newtonsoft.Json.Formatting)Formatting.Indented;

					jsonWriter.WriteRaw("[");
					for (int i = 0; i < dtQuestionnaires.Rows.Count; i++)
					{
						if (i > 0)
						{
							jsonWriter.WriteEndObject();
							jsonWriter.WriteRaw(",");
						}
						jsonWriter.WriteStartObject();

						jsonWriter.WritePropertyName("numFormID");
						jsonWriter.WriteValue(dtQuestionnaires.Rows[i]["numFormID"].ToString());
						jsonWriter.WritePropertyName("txtFormName");
						jsonWriter.WriteValue(dtQuestionnaires.Rows[i]["txtFormName"].ToString().Replace("''", ""));
						jsonWriter.WritePropertyName("dteDateCreated");
						jsonWriter.WriteValue(dtQuestionnaires.Rows[i]["dteDateCreated"].ToString());
						jsonWriter.WritePropertyName("dteDateModified");
						jsonWriter.WriteValue(dtQuestionnaires.Rows[i]["dteDateModified"].ToString());

						if (i == dtQuestionnaires.Rows.Count - 1)
						{
							jsonWriter.WriteEndObject();
						}
					}

					jsonWriter.WriteRaw("]");

					result = sw.ToString();
				}
				myConnection.Close();

				return result;
			}
		}
	}

	//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	[WebMethod]
	public bool SubmitFormAnswers(string answers, bool update, bool isDoctorInput)
	{

		//answers = "{\"FormName\":null,\"FormId\":null,\"PatientId\":null,\"DoctorId\":null,\"DoctorLocationId\":null,\"AppointmentId\":null,\"DateCompleted\":null,\"DateUpdated\":null,\"FormSections\":[{\"SectionName\":\"Basic Info\",\"SectionId\":\"9\",\"SectionQuestions\":[{\"SectionId\":\"9\",\"QuestionId\":\"431\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Claimant Name\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"659\",\"AnswerText\":\"Test\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"432\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Claim Number\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"433\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Date\",\"QuestionText\":\"Today''s Date\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"434\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Date\",\"QuestionText\":\"Date of Birth\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"435\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Age\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"442\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Height\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"443\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Weight\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"444\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Please describe which photo ID was presented.\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"444\",\"OptionText\":\"New York State Driver's License\",\"OptionId\":\"505\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"444\",\"OptionText\":\"Passport\",\"OptionId\":\"506\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"444\",\"OptionText\":\"Employment Card\",\"OptionId\":\"507\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"444\",\"OptionText\":\"Photo ID\",\"OptionId\":\"508\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"9\",\"QuestionId\":\"445\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Radio\",\"QuestionText\":\"Right-handed or left-handed?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"445\",\"OptionText\":\"Right\",\"OptionId\":\"509\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"445\",\"OptionText\":\"Left\",\"OptionId\":\"510\",\"ConditionTriggerId\":\"\",\"Chosen\":true}]},{\"SectionId\":\"9\",\"QuestionId\":\"446\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Hair color\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"446\",\"OptionText\":\"brown\",\"OptionId\":\"511\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"dark brown\",\"OptionId\":\"512\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"light brown\",\"OptionId\":\"513\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"black\",\"OptionId\":\"514\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"blonde\",\"OptionId\":\"515\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"red\",\"OptionId\":\"516\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"gray\",\"OptionId\":\"517\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"446\",\"OptionText\":\"no hair\",\"OptionId\":\"518\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"9\",\"QuestionId\":\"447\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Eye color\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"447\",\"OptionText\":\"hazel\",\"OptionId\":\"519\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"447\",\"OptionText\":\"green\",\"OptionId\":\"520\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"447\",\"OptionText\":\"blue\",\"OptionId\":\"521\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"447\",\"OptionText\":\"brown\",\"OptionId\":\"522\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"447\",\"OptionText\":\"gray\",\"OptionId\":\"523\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"9\",\"QuestionId\":\"448\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Photo of claimant\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"449\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Vendor number\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"9\",\"QuestionId\":\"450\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Date\",\"QuestionText\":\"Date of Accident\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null}]},{\"SectionName\":\"Accident Info\",\"SectionId\":\"10\",\"SectionQuestions\":[{\"SectionId\":\"10\",\"QuestionId\":\"460\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Date\",\"QuestionText\":\"Date of Accident\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"461\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Radio\",\"QuestionText\":\"Type of Accident\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"461\",\"OptionText\":\"motor vehicle accident\",\"OptionId\":\"546\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"461\",\"OptionText\":\"worker's comp\",\"OptionId\":\"547\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"461\",\"OptionText\":\"Other\",\"OptionId\":\"548\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"672\",\"QuestionParentId\":\"0\",\"QuestionType\":\"Check\",\"QuestionText\":\"Do you have a history of any serious illnesses including:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"672\",\"OptionText\":\"No history of illness\",\"OptionId\":\"682\",\"ConditionTriggerId\":\"\",\"Chosen\":true},{\"ParentQuestionId\":\"672\",\"OptionText\":\"High Blood Pressure\",\"OptionId\":\"683\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"672\",\"OptionText\":\"Asthma\",\"OptionId\":\"684\",\"ConditionTriggerId\":\"\",\"Chosen\":true},{\"ParentQuestionId\":\"672\",\"OptionText\":\"Diabetes\",\"OptionId\":\"685\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"672\",\"OptionText\":\"Cancer\",\"OptionId\":\"686\",\"ConditionTriggerId\":\"\",\"Chosen\":true},{\"ParentQuestionId\":\"672\",\"OptionText\":\"Arthritis\",\"OptionId\":\"687\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"672\",\"OptionText\":\"Other\",\"OptionId\":\"688\",\"ConditionTriggerId\":\"270\",\"Chosen\":true}]},{\"SectionId\":\"10\",\"QuestionId\":\"673\",\"QuestionParentId\":\"672\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Describe:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"270\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"Other issue\",\"IsAnswered\":false,\"IsEnabled\":true,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"674\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Are you currently taking any medication?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":\"271\",\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"674\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"271\",\"Chosen\":true},{\"ParentQuestionId\":\"674\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"675\",\"QuestionParentId\":\"674\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Type of medication:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"271\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":true,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"676\",\"QuestionParentId\":\"674\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Dosage and frequency:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"271\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":true,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"677\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Are you allergic to any medications?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"677\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"272\",\"Chosen\":false},{\"ParentQuestionId\":\"677\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"678\",\"QuestionParentId\":\"677\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Type of medication:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"272\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"679\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Have you had any surgery in the past (NOT RELATED TO THIS ACCIDENT)\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"679\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"273\",\"Chosen\":false},{\"ParentQuestionId\":\"679\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"680\",\"QuestionParentId\":\"679\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Type of surgery:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"273\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"681\",\"QuestionParentId\":\"679\",\"QuestionType\":\"Date\",\"QuestionText\":\"Date of surgery:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"273\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"682\",\"QuestionParentId\":\"679\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Reason for surgery:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"273\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"10\",\"QuestionId\":\"683\",\"QuestionParentId\":\"0\",\"QuestionType\":\"BodyParts\",\"QuestionText\":\"What are your current complaints as a result of the accident?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"683\",\"OptionText\":\"No initial complaints\",\"OptionId\":\"549\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Headaches\",\"OptionId\":\"550\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Neck \",\"OptionId\":\"551\",\"ConditionTriggerId\":\"274\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Mid Back\",\"OptionId\":\"552\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Low Back\",\"OptionId\":\"553\",\"ConditionTriggerId\":\"275\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Shoulder\",\"OptionId\":\"554\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Shoulder\",\"OptionId\":\"555\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Elbow\",\"OptionId\":\"556\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Elbow\",\"OptionId\":\"557\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Wrist\",\"OptionId\":\"558\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Wrist\",\"OptionId\":\"559\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Hand\",\"OptionId\":\"560\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Hand\",\"OptionId\":\"561\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Hip\",\"OptionId\":\"562\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Hip\",\"OptionId\":\"563\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Knee\",\"OptionId\":\"564\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Knee\",\"OptionId\":\"565\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Ankle\",\"OptionId\":\"566\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Ankle\",\"OptionId\":\"567\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Right Foot\",\"OptionId\":\"568\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Left Foot\",\"OptionId\":\"569\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"683\",\"OptionText\":\"Other (Please list)\",\"OptionId\":\"570\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"684\",\"QuestionParentId\":\"683\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Does your neck pain radiate to your arms or hands?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"274\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"684\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"684\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"685\",\"QuestionParentId\":\"683\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Does your back pain radiate to your arms or hands?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"275\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"685\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"685\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"686\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Have you ever had any of the above complaints prior to the accident?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"686\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"276\",\"Chosen\":false},{\"ParentQuestionId\":\"686\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"10\",\"QuestionId\":\"687\",\"QuestionParentId\":\"686\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Please list\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"276\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null}]},{\"SectionName\":\"History\",\"SectionId\":\"11\",\"SectionQuestions\":[{\"SectionId\":\"11\",\"QuestionId\":\"688\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Were you employed at the time of the accident?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"688\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"277\",\"Chosen\":false},{\"ParentQuestionId\":\"688\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"689\",\"QuestionParentId\":\"688\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"What was your occupation?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"277\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"690\",\"QuestionParentId\":\"688\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Your duties included:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"277\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"691\",\"QuestionParentId\":\"688\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Did you miss any time from work as a result of the accident?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"277\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"691\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"278\",\"Chosen\":false},{\"ParentQuestionId\":\"691\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"692\",\"QuestionParentId\":\"691\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"How much time from work did you miss? (Days/Weeks/Months)\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"278\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"693\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Are you currently working?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":\"280\",\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"693\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"280\",\"Chosen\":true},{\"ParentQuestionId\":\"693\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"279\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"694\",\"QuestionParentId\":\"693\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Explain why:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"279\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"695\",\"QuestionParentId\":\"693\",\"QuestionType\":\"Radio\",\"QuestionText\":\"Are you working full-time or part-time?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"280\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":true,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"695\",\"OptionText\":\"Full Time\",\"OptionId\":\"689\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"695\",\"OptionText\":\"Part Time\",\"OptionId\":\"690\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"696\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"How much walking do you perform per day?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"697\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"How much sitting do you do per day?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"698\",\"QuestionParentId\":\"0\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"How much standing do you do per day?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null},{\"SectionId\":\"11\",\"QuestionId\":\"699\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Do you do any climbing (stairs/ladders)\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":\"\",\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"699\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"\",\"Chosen\":true},{\"ParentQuestionId\":\"699\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"700\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Do you perform household chores, cooking, cleaning, etc?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"700\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"\",\"Chosen\":false},{\"ParentQuestionId\":\"700\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"701\",\"QuestionParentId\":\"0\",\"QuestionType\":\"YesNo\",\"QuestionText\":\"Do you perform any heavy lifting?\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"\",\"ActiveTriggerId\":null,\"IsConditional\":false,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":[{\"ParentQuestionId\":\"701\",\"OptionText\":\"Yes\",\"OptionId\":\"7\",\"ConditionTriggerId\":\"281\",\"Chosen\":false},{\"ParentQuestionId\":\"701\",\"OptionText\":\"No\",\"OptionId\":\"8\",\"ConditionTriggerId\":\"\",\"Chosen\":false}]},{\"SectionId\":\"11\",\"QuestionId\":\"702\",\"QuestionParentId\":\"701\",\"QuestionType\":\"TextInput\",\"QuestionText\":\"Explain:\",\"QuestionKeyboardType\":null,\"ParentConditionTriggerId\":\"281\",\"ActiveTriggerId\":null,\"IsConditional\":true,\"AnswerId\":\"\",\"AnswerText\":\"\",\"IsAnswered\":false,\"IsEnabled\":false,\"AnswerOptionIndex\":null,\"QuestionOptions\":null}]}]}";
		SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;");//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString);
		myConnection.Open();


		SelectedQForm.answeredQForm = JsonConvert.DeserializeObject<QForm>(answers);
		SelectedQForm.answeredQForm.DoctorId = "1";
		SelectedQForm.answeredQForm.DoctorLocationId = "1";
		SelectedQForm.answeredQForm.DateCompleted = DateTime.Today.ToShortDateString();
		SelectedQForm.answeredQForm.FormId = "76";
		SelectedQForm.answeredQForm.PatientId = "123";

		string dateCompleted = (Convert.ToDateTime(SelectedQForm.answeredQForm.DateCompleted)).ToShortDateString();

		SqlCommand myCheckExistingCommand;

		if (isDoctorInput)
		{
			myCheckExistingCommand = new SqlCommand("SELECT numAnsweredFormID FROM QAnsweredForms_D WHERE numFormID = " + SelectedQForm.answeredQForm.FormId + " AND numClientID = " + SelectedQForm.answeredQForm.PatientId + " AND numDoctorID = " + SelectedQForm.answeredQForm.DoctorId + " AND numDoctorLocationID = " + SelectedQForm.answeredQForm.DoctorLocationId + " AND dteCompleted BETWEEN '" + dateCompleted + " 00:00:00' AND '" + dateCompleted + " 23:59:59'") { Connection = myConnection };
		}
		else
		{
			myCheckExistingCommand = new SqlCommand("SELECT numAnsweredFormID FROM QAnsweredForms_D WHERE numFormID = " + SelectedQForm.answeredQForm.FormId + " AND numClientID = " + SelectedQForm.answeredQForm.PatientId + " AND (flgDoctorInput = '0' or flgDoctorInput is null)") { Connection = myConnection };
		}

		string answeredFormID = "";

		SqlDataReader existReader = myCheckExistingCommand.ExecuteReader();


		//answeredFormID = myCheckExistingCommand.ExecuteScalar().ToString();
		bool doInsert = false;
		bool doInsertForm = true;

		if (!existReader.HasRows)
		{
			doInsert = true;
			//answeredFormID = SelectedQForm.answeredQForm.FormId;
		}
		else
		{
			while (existReader.Read())
			{
				answeredFormID = existReader[0].ToString();
			}
		}
		existReader.Close();

		if (update && !string.IsNullOrEmpty(answeredFormID))
		{
			//doInsert = true;

			SqlCommand myDeleteCommand = new SqlCommand("DELETE FROM QAnswers_D WHERE numAnsweredFormID = " + answeredFormID + "; UPDATE QAnsweredForms_D SET dteChanged = '" + DateTime.Now + "' WHERE numAnsweredFormID = " + answeredFormID) { Connection = myConnection };

			myDeleteCommand.ExecuteNonQuery();
		}


		string numAnsweredFormID;
		if (doInsert)
		{
			SqlCommand myFormCommand;

			if (isDoctorInput)
			{
				myFormCommand = new SqlCommand("INSERT INTO QAnsweredForms_D (numFormID, numClientID, flgCompleted, flgDoctorInput, dteCompleted, numDoctorID, numDoctorLocationID, numListingID) VALUES (" + SelectedQForm.answeredQForm.FormId + ", " + SelectedQForm.answeredQForm.PatientId + ", 1, 1, '" + DateTime.Now + "', " + SelectedQForm.answeredQForm.DoctorId + ", " + SelectedQForm.answeredQForm.DoctorLocationId + ", " + SelectedQForm.answeredQForm.AppointmentId + " ); SELECT @@IDENTITY") { Connection = myConnection };
			}
			else
			{
				myFormCommand = new SqlCommand("INSERT INTO QAnsweredForms_D (numFormID, numClientID, flgCompleted, dteCompleted) VALUES (" + SelectedQForm.answeredQForm.FormId + ", " + SelectedQForm.answeredQForm.PatientId + ", 1, '" + DateTime.Now + "' ); SELECT @@IDENTITY") { Connection = myConnection };
			}

			numAnsweredFormID = myFormCommand.ExecuteScalar().ToString();
		}
		else
		{
			numAnsweredFormID = answeredFormID;
		}

		List<SqlParameter> sp = new List<SqlParameter>();

		StringBuilder sb = new StringBuilder();
		sb.Append("INSERT INTO QAnswers_D (numAnsweredFormID, flgDoctorInput, numQuestionID, numOptionID, txtAnswerText) VALUES ");
		// TODO doctor and form info in SelectedQForm.answeredQform

		int pIndex = 0;
		foreach (FormSection section in SelectedQForm.answeredQForm.FormSections)
		{
			// TODO section logic
			foreach (SectionQuestion question in section.SectionQuestions)
			{
				//string pIndex = section.SectionQuestions.IndexOf(question).ToString();
				//sb.Append("(" + numAnsweredFormID + ", '" + isDoctorInput + "', ");
				if (question.QuestionOptions != null && question.QuestionOptions.Count > 0)
				{
					foreach (QuestionOption option in question.QuestionOptions)
					{
						if (option.Chosen)
						{
							sb.Append("(@numAnsweredFormID" + pIndex + ", @flgDoctorInput" + pIndex + ", @numQuestionID" + pIndex + ", @numOptionID" + pIndex + ", @txtAnswerText" + pIndex + "),");
							sp.Add(new SqlParameter("@numAnsweredFormID" + pIndex, numAnsweredFormID));
							sp.Add(new SqlParameter("@flgDoctorInput" + pIndex, isDoctorInput));
							sp.Add(new SqlParameter("@numQuestionID" + pIndex, question.QuestionId));
							sp.Add(new SqlParameter("@numOptionID" + pIndex, option.OptionId));
							sp.Add(new SqlParameter("@txtAnswerText" + pIndex, option.OptionText));
							pIndex++;
							// TODO multiple choice logic
						}
					}
				}
				else if((question.IsEnabled || !question.IsConditional) && !string.IsNullOrEmpty(question.AnswerText))
				{
					sb.Append("(@numAnsweredFormID" + pIndex + ", @flgDoctorInput" + pIndex + ", @numQuestionID" + pIndex + ", @numOptionID" + pIndex + ", @txtAnswerText" + pIndex + "),");
					sp.Add(new SqlParameter("@numAnsweredFormID" + pIndex, numAnsweredFormID));
					sp.Add(new SqlParameter("@flgDoctorInput" + pIndex, isDoctorInput));
					sp.Add(new SqlParameter("@numQuestionID" + pIndex, question.QuestionId));
					sp.Add(new SqlParameter("@numOptionID" + pIndex, DBNull.Value));
					sp.Add(new SqlParameter("@txtAnswerText" + pIndex, question.AnswerText));
					pIndex++;
				}
			}
		}

		SqlCommand myAnswersCommand = new SqlCommand(sb.ToString().Trim(',')) { Connection = myConnection };
		myAnswersCommand.Parameters.AddRange(sp.ToArray());
		myAnswersCommand.ExecuteNonQuery();

		return true;

	}




	private string buildclass(DataTable dtForm, string formID)
	{
		QForm myForm = new QForm();
		myForm.FormName = "Chiro Form";
		myForm.FormId = formID;
		myForm.FormSections = new List<FormSection>();
		DataTable dtSections = dtForm.DefaultView.ToTable(true, "numSectionId", "txtSectionHeader");
		foreach (DataRow drSection in dtSections.Rows)
		{
			myForm.FormSections.Add(new FormSection()
			{
				SectionId = drSection["numSectionID"].ToString(),
				SectionName = drSection["txtSectionHeader"].ToString(),
				SectionQuestions = new List<SectionQuestion>()
			});
		}

		DataTable dtQuestions = dtForm.DefaultView.ToTable(true, "questionid", "txtSectionHeader", "numSectionID", "qtext",
		                                                   "ConditionTrigger", "qtype", "numQParentID", "numQOrder", "numAnswerID", "txtAnswerText", "numOptionID");

		if (!dtQuestions.Columns.Contains("flgProcessed"))
		{
			DataColumn dc = new DataColumn("flgProcessed", typeof(bool)) { DefaultValue = false };
			dtQuestions.Columns.Add(dc);
		}

		var rootQuestions = dtQuestions.Select("numQParentID = '0'", "numQOrder ASC");

		string finalJson = HttpUtility.HtmlDecode(JsonConvert.SerializeObject(BuildQFormRecursive(rootQuestions, dtQuestions, dtForm, myForm)));

		return finalJson;



		//string finalJson = JsonConvert.SerializeObject(myForm);

		//return finalJson;
	}



	private QForm BuildQFormRecursive(DataRow[] questions, DataTable dtQuestions, DataTable dtForm, QForm myForm)
	{
		for (int i = 0; i < questions.Length; i++)
		{
			DataRow drQuestion = questions[i];
			var processed = questions[i]["flgProcessed"];

			if (!Convert.ToBoolean(questions[i]["flgProcessed"]))
			{
				// ADD JSON OF THIS QUESTION

				FormSection qSection = myForm.FormSections.Find(r => r.SectionId == drQuestion["numSectionID"].ToString());

				if (qSection == null)
				{
					qSection = new FormSection()
					{
						SectionId = drQuestion["numSectionID"].ToString(),
						SectionName = drQuestion["txtSectionHeader"].ToString(),
						SectionQuestions = new List<SectionQuestion>()
					};
					myForm.FormSections.Add(qSection);
				}

				if (qSection.SectionQuestions.Find((SectionQuestion obj) => obj.QuestionId == drQuestion["questionid"].ToString()) != null) continue;
				qSection.SectionQuestions.Add(new SectionQuestion()
				{
					SectionId = drQuestion["numSectionID"].ToString(),
					QuestionId = drQuestion["questionid"].ToString(),
					QuestionText = drQuestion["qtext"].ToString(),
					ParentConditionTriggerId = drQuestion["ConditionTrigger"].ToString(),
					QuestionType = drQuestion["qtype"].ToString(),
					QuestionParentId = drQuestion["numQParentID"].ToString(),
					IsConditional = string.IsNullOrEmpty(drQuestion["ConditionTrigger"].ToString()) ? false : true,
					AnswerId = drQuestion["numAnswerID"].ToString(),
					AnswerText = drQuestion["txtAnswerText"].ToString()
				});

				DataRow[] dtQOptions = dtForm.Select("questionid = " + drQuestion["questionid"]);

				if (dtQOptions.Length > 1)
				{
					qSection.SectionQuestions[qSection.SectionQuestions.Count - 1].QuestionOptions =
						new List<QuestionOption>();

					foreach (DataRow qOption in dtQOptions)
					{

						qSection.SectionQuestions[qSection.SectionQuestions.Count - 1].QuestionOptions.Add(new QuestionOption()
						{
							OptionId = qOption["numQTypeOptionID"].ToString(),
							OptionText = qOption["options"].ToString(),
							ConditionTriggerId = qOption["ConnectedCondition"].ToString(),
							ParentQuestionId = qOption["questionid"].ToString(),
							//Chosen = !string.IsNullOrEmpty(drQuestion["numOptionID"].ToString()) && drQuestion["numOptionID"].ToString() == qOption["numQTypeOptionID"].ToString() ? true : false,
							Chosen = !string.IsNullOrEmpty(qOption["numAnswerID"].ToString()) ? true : false
						});
					}
				}
				else
				{
				}

				questions[i]["flgProcessed"] = true;
			}

			var childQuestions = dtQuestions.Select("numQParentID = '" + drQuestion["questionid"] + "'", "numQOrder ASC");
			if (childQuestions.Length > 0)
			{
				myForm = BuildQFormRecursive(childQuestions, dtQuestions, dtForm, myForm);
			}
		}

		return myForm;
	}



	//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	[WebMethod]
	public string GetFormQuestions(string formID, string clientID, string ApptDate, bool isDocInput)
	{
		//GlobalFuncsClass.sendErrorEmail("QUESTIONATOR SERVICE TEST", formID);

		//SqlConnection myConnectionLocal = new SqlConnection("server=localhost;integrated security = SSPI;database=Questionator;MultipleActiveResultSets=True;Connection Timeout=160");
		StringBuilder json = new StringBuilder();

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			string docInputSQL;
			if (isDocInput)
			{
				docInputSQL = " AND Questions_D.flgDoctorInput = 1";
			}
			else
			{
				docInputSQL = " AND Questions_D.flgDoctorInput = 0";
			}

			string completeDateSQL = "";
			if (!string.IsNullOrEmpty(ApptDate))
			{
				DateTime dteApptDate = (Convert.ToDateTime(ApptDate));
				ApptDate = dteApptDate.ToShortDateString();

				completeDateSQL = " AND QAnsweredForms_D.dteCompleted BETWEEN '" + ApptDate + " 00:00:00' AND '" +
								  ApptDate + " 23:59:59'";
			}

			string answeredFormID = "54";
			//SqlCommand myCommand = new SqlCommand("SELECT Questions_D.numQID AS [questionid], Questions_D.txtQText AS [qtext], Questions_D.txtType AS [qtype], Questions_D.numVariant, Questions_D.flgVariant, QTypeOptions_D.txtOptionText AS [options], Questions_D.numQOrder, Questions_D.numQConditionID AS [ConditionTrigger], Questions_D.flgConditional AS [IsConditional], QTypeOptions_D.numQTypeOptionID, Questions_D.flgConditional, Questions_D.numQParentID, Questions_D.flgRequired, drvtbl_D.numQID, drvtbl_D.numQConditionID AS [ConnectedCondition], drvtbl_D.numQConditionOptionID, drvtbl_D.numQTypeOptionID, drvtbl_D.txtConditionType, drvtbl_D.txtConditionOperator, drvAnswers_D.numDoctorID, drvAnswers_D.numDoctorLocationID, drvAnswers_D.dteCompleted, drvAnswers_D.dteChanged, drvAnswers_D.txtAnswerText, drvAnswers_D.numOptionID, drvAnswers_D.numAnswerID FROM Questions_D LEFT OUTER JOIN QTypes_D ON Questions_D.numQTypeID = QTypes_D.numQTypeID LEFT OUTER JOIN QTypeOptions_D ON Questions_D.numQID = QTypeOptions_D.numQuestionID OR QTypes_D.numVariant = QTypeOptions_D.numVariant LEFT OUTER JOIN (SELECT QConditions_D.numQConditionID, QConditions_D.numQID, QConditions_D.txtConditionType, QConditions_D.flgElse, QConditions_D.txtConditionOperator, QConditionOptions_D.numQConditionOptionID, QConditionOptions_D.numQTypeOptionID FROM QConditions_D LEFT OUTER JOIN QConditionOptions_D ON QConditions_D.numQConditionID = QConditionOptions_D.numQConditionID) AS drvtbl_D ON (QTypeOptions_D.numQTypeOptionID = drvtbl_D.numQTypeOptionID AND Questions_D.numQID = drvtbl_D.numQID) LEFT OUTER JOIN (SELECT QAnsweredForms_D.numDoctorID, QAnsweredForms_D.numDoctorLocationID, QAnsweredForms_D.dteCompleted, QAnsweredForms_D.dteChanged, QAnswers_D.txtAnswerText, QAnswers_D.numOptionID, QAnswers_D.numQuestionID, QAnswers_D.numAnswerID FROM QAnsweredForms_D INNER JOIN QAnswers_D ON QAnswers_D.numAnsweredFormID = QAnsweredForms_D.numAnsweredFormID WHERE QAnsweredForms_D.numClientID = " + clientID + completeDateSQL + " AND QAnsweredForms_D.numAnsweredFormID = 44) AS drvAnswers_D ON (QTypeOptions_D.numQTypeOptionID = drvAnswers_D.numOptionID AND (Questions_D.numQID = drvAnswers_D.numQuestionID OR QTypeOptions_D.numQuestionID = null)) WHERE (Questions_D.flgDeleted <> 'True' OR Questions_D.flgDeleted <> '1' OR Questions_D.flgDeleted is null) AND Questions_D.numQFormID = " + formID + docInputSQL + " ORDER BY Questions_D.numQOrder", myConnection);
			//SqlCommand myCommand = new SqlCommand("SELECT TOP 20 ListingNotes.*, Login.UserName, ClientFiles.txtFileName FROM ListingNotes LEFT OUTER JOIN ClientFiles ON ListingNotes.numFileID = ClientFiles.numFileID LEFT OUTER JOIN Login ON ListingNotes.numUserID = Login.UserID", myConnection);
			SqlCommand myCommand = myConnection.CreateCommand();
			//myCommand.CommandText = "SELECT Questions_D.numQID AS [questionid], Questions_D.txtQText AS [qtext], Questions_D.txtType AS [qtype], Questions_D.numVariant, Questions_D.flgVariant, QTypeOptions_D.txtOptionText AS [options], Questions_D.numQOrder, Questions_D.numQConditionID AS [ConditionTrigger], Questions_D.flgConditional AS [IsConditional], QTypeOptions_D.numQTypeOptionID, Questions_D.flgConditional, Questions_D.numQParentID, Questions_D.flgRequired, drvtbl_D.numQID, drvtbl_D.numQConditionID AS [ConnectedCondition], drvtbl_D.numQConditionOptionID, drvtbl_D.numQTypeOptionID, drvtbl_D.txtConditionType, drvtbl_D.txtConditionOperator FROM Questions_D LEFT OUTER JOIN QTypes_D ON Questions_D.numQTypeID = QTypes_D.numQTypeID LEFT OUTER JOIN QTypeOptions_D ON Questions_D.numQID = QTypeOptions_D.numQuestionID OR QTypes_D.numVariant = QTypeOptions_D.numVariant LEFT OUTER JOIN (SELECT QConditions_D.numQConditionID, QConditions_D.numQID, QConditions_D.txtConditionType, QConditions_D.flgElse, QConditions_D.txtConditionOperator, QConditionOptions_D.numQConditionOptionID, QConditionOptions_D.numQTypeOptionID FROM QConditions_D LEFT OUTER JOIN QConditionOptions_D ON QConditions_D.numQConditionID = QConditionOptions_D.numQConditionID) AS drvtbl_D ON (QTypeOptions_D.numQTypeOptionID = drvtbl_D.numQTypeOptionID AND Questions_D.numQID = drvtbl_D.numQID) WHERE (Questions_D.flgDeleted <> 'True' OR Questions_D.flgDeleted <> '1' OR Questions_D.flgDeleted is null) AND Questions_D.numQFormID = 35 AND Questions_D.flgDoctorInput = 0 ORDER BY Questions_D.numQOrder";
			myCommand.CommandText = "SELECT QSections.txtSectionHeader, QSections.numSectionID, Questions_D.numQID AS [questionid], QSections.numSectionPositionNumber, Questions_D.txtQText AS [qtext], Questions_D.txtType AS [qtype], " +
				"Questions_D.numVariant, Questions_D.flgVariant, QTypeOptions_D.txtOptionText AS [options], Questions_D.numQOrder, Questions_D.numQConditionID AS [ConditionTrigger], " +
				"Questions_D.flgConditional AS [IsConditional], QTypeOptions_D.numQTypeOptionID, Questions_D.flgConditional, Questions_D.numQParentID, Questions_D.flgRequired, " +
				"drvtbl_D.numQID, drvtbl_D.numQConditionID AS [ConnectedCondition], drvtbl_D.numQConditionOptionID, drvtbl_D.numQTypeOptionID, drvtbl_D.txtConditionType, " +
				"drvtbl_D.txtConditionOperator, drvAnswers_D.numDoctorID, drvAnswers_D.numDoctorLocationID, drvAnswers_D.dteCompleted, drvAnswers_D.dteChanged, " +
				"drvAnswers_D.txtAnswerText, drvAnswers_D.numOptionID, drvAnswers_D.numAnswerID " +
				"FROM QSections INNER JOIN Questions_D ON QSections.numSectionID = Questions_D.numParentSectionID LEFT OUTER JOIN QTypes_D ON Questions_D.numQTypeID = QTypes_D.numQTypeID " +
				"LEFT OUTER JOIN QTypeOptions_D ON Questions_D.numQID = QTypeOptions_D.numQuestionID OR QTypes_D.numVariant = QTypeOptions_D.numVariant " +
				"LEFT OUTER JOIN (SELECT QConditions_D.numQConditionID, QConditions_D.numQID, QConditions_D.txtConditionType, QConditions_D.flgElse, QConditions_D.txtConditionOperator, QConditionOptions_D.numQConditionOptionID, QConditionOptions_D.numQTypeOptionID " +
				"FROM QConditions_D LEFT OUTER JOIN QConditionOptions_D ON QConditions_D.numQConditionID = QConditionOptions_D.numQConditionID) AS drvtbl_D " +
				"ON (QTypeOptions_D.numQTypeOptionID = drvtbl_D.numQTypeOptionID AND Questions_D.numQID = drvtbl_D.numQID) " +
				"LEFT OUTER JOIN (SELECT QAnsweredForms_D.numDoctorID, QAnsweredForms_D.numDoctorLocationID, QAnsweredForms_D.dteCompleted, QAnsweredForms_D.dteChanged, QAnswers_D.txtAnswerText, QAnswers_D.numOptionID, QAnswers_D.numQuestionID, QAnswers_D.numAnswerID FROM QAnsweredForms_D " +
				"INNER JOIN QAnswers_D ON QAnswers_D.numAnsweredFormID = QAnsweredForms_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormID + ") AS drvAnswers_D " +
				"ON ((QTypeOptions_D.numQTypeOptionID = drvAnswers_D.numOptionID and QTypeOptions_D.numQuestionID = drvAnswers_d.numQuestionID ) " +
				"OR ( Questions_D.numQID = drvAnswers_D.numQuestionID and drvAnswers_D.numoptionid IS NULL)) " +
				"WHERE (Questions_D.flgDeleted <> 'True' OR Questions_D.flgDeleted <> '1' OR Questions_D.flgDeleted is null) " +
				"AND Questions_D.numQFormID = " + formID +  docInputSQL + " ORDER BY QSections.numSectionID, Questions_D.numQOrder";
			//new including mailingissue! //


			using (SqlCommand cmd = myCommand)
			{
				var dtQuestions = new DataTable();

				var daComplaints = new SqlDataAdapter(myCommand);
				daComplaints.Fill(dtQuestions);

				string getit = buildclass(dtQuestions, formID);

				myConnection.Close();

				return getit;
			}
		}
	}

	//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	[WebMethod]
	public string GetDoctorInput(string formID)
	{
		//GlobalFuncsClass.sendErrorEmail("QUESTIONATOR SERVICE TEST", formID);

		SqlConnection myConnectionLocal = new SqlConnection("server=localhost;integrated security = SSPI;database=Questionator;MultipleActiveResultSets=True;Connection Timeout=160");
		StringBuilder json = new StringBuilder();

		using (SqlConnection myConnection = new SqlConnection())//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			//SELECT Questions.numQID AS [questionid], Questions.numQFormID, Questions.txtType, Questions.txtQText AS [qtext], Questions.numQHeaderID, Questions.flgConditional, Questions.numQParentID, Questions.numQTypeID, drvtbl.numQConditionID, drvtbl.numQID AS Expr1, drvtbl.txtConditionType, drvtbl.flgElse, drvtbl.txtConditionOperator, drvtbl.numQConditionOptionID, drvtbl.numQTypeOptionID, drvtbl.numQConditionID AS Expr4, QTypeOptions.numQTypeOptionID AS Expr2, QTypeOptions.txtOptionText AS [options], QTypeOptions.numQTypeID AS Expr5, QTypes.numQTypeID AS Expr3, QTypes.txtQTypeText, QTypes.flgAllowConditions FROM Questions LEFT OUTER JOIN QTypes ON Questions.numQTypeID = QTypes.numQTypeID INNER JOIN QTypeOptions ON QTypes.numQTypeID = QTypeOptions.numQTypeID LEFT OUTER JOIN (SELECT QConditions.numQConditionID, QConditions.numQID, QConditions.txtConditionType, QConditions.flgElse, QConditions.txtConditionOperator, QConditionOptions.numQConditionOptionID, QConditionOptions.numQTypeOptionID FROM QConditions LEFT OUTER JOIN QConditionOptions ON QConditions.numQConditionID = QConditionOptions.numQConditionID) AS drvtbl ON QTypeOptions.numQTypeOptionID = drvtbl.numQTypeOptionID WHERE Questions.numQFormID = " + formID + " ORDER BY Questions.numQID

			SqlCommand myCommand = new SqlCommand("SELECT Questions_D.numQID AS [questionid], Questions_D.txtQText AS [qtext], QTypeOptions_D.txtOptionText AS [options], Questions_D.txtType AS [qtype] FROM Questions_D LEFT OUTER JOIN QTypes_D ON Questions_D.numQTypeID = QTypes_D.numQTypeID INNER JOIN QTypeOptions_D ON Questions.numQID = QTypeOptions_D.numQuestionID LEFT OUTER JOIN (SELECT QConditions_D.numQConditionID, QConditions_D.numQID, QConditions_D.txtConditionType, QConditions_D.flgElse, QConditions_D.txtConditionOperator, QConditionOptions_D.numQConditionOptionID, QConditionOptions_D.numQTypeOptionID FROM QConditions_D LEFT OUTER JOIN QConditionOptions_D ON QConditions_D.numQConditionID = QConditionOptions_D.numQConditionID) AS drvtbl_D ON QTypeOptions_D.numQTypeOptionID = drvtbl_D.numQTypeOptionID WHERE Questions_D.numQFormID = " + formID + " AND Questions_D.flgDoctorInput = 1 ORDER BY Questions_D.numQID", myConnection);
			//SqlCommand myCommand = new SqlCommand("SELECT TOP 20 ListingNotes.*, Login.UserName, ClientFiles.txtFileName FROM ListingNotes LEFT OUTER JOIN ClientFiles ON ListingNotes.numFileID = ClientFiles.numFileID LEFT OUTER JOIN Login ON ListingNotes.numUserID = Login.UserID", myConnection);

			//new including mailingissue! //


			using (SqlCommand cmd = myCommand)
			{
				var dtQuestions = new DataTable();

				var daComplaints = new SqlDataAdapter(myCommand);
				daComplaints.Fill(dtQuestions);
				string jsonResult = JsonConvert.SerializeObject(dtQuestions);

				string result;
				StringWriter sw = new StringWriter();
				JsonToken jt = new JsonToken();

				using (JsonWriter jsonWriter = new JsonTextWriter(sw))
				{
					jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;
					int currentID = 0;

					jsonWriter.WriteRaw("[");
					for (int i = 0; i < dtQuestions.Rows.Count; i++)
					{
						int lastID = Convert.ToInt16(dtQuestions.Rows[i]["questionid"]);
						if (currentID != lastID)
						{
							if (i > 0)
							{
								jsonWriter.WriteEndArray();
								jsonWriter.WriteEndObject();
								jsonWriter.WriteRaw(",");
							}
							jsonWriter.WriteStartObject();

							jsonWriter.WritePropertyName("questionid");
							jsonWriter.WriteValue(dtQuestions.Rows[i]["questionid"].ToString());
							jsonWriter.WritePropertyName("qtext");
							jsonWriter.WriteValue(dtQuestions.Rows[i]["qtext"].ToString().Replace("''", ""));
							jsonWriter.WritePropertyName("options");
							jsonWriter.WriteStartArray();
							jsonWriter.WriteValue(dtQuestions.Rows[i]["options"] + "," + dtQuestions.Rows[i]["qtype"]);

							currentID = lastID;
						}
						else
						{
							jsonWriter.WriteValue(dtQuestions.Rows[i]["options"].ToString());
						}

						if (i == dtQuestions.Rows.Count - 1)
						{
							jsonWriter.WriteEndArray();
							jsonWriter.WriteEndObject();
						}
					}

					jsonWriter.WriteRaw("]");

					result = sw.ToString();
				}
				myConnection.Close();

				return result;
			}
		}
	}


	//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	[WebMethod]
	public string GetShortForms(string qFormID, string docID, bool showcase)   //, List<string> formAnswers
	{

		SqlConnection myConnectionLocal = new SqlConnection("server=localhost;integrated security = SSPI;database=Questionator;MultipleActiveResultSets=True;Connection Timeout=160");
		StringBuilder json = new StringBuilder();

		using (SqlConnection myConnection = new SqlConnection())//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			string query = "";
			if (showcase)
			{
				query = "SELECT * FROM QShortForms";
			}
			else
			{
				query = "SELECT * FROM QShortForms WHERE numDoctorID = " + docID + " AND numQShortFormID = " + qFormID;
			}

			SqlCommand myCommand = new SqlCommand(query, myConnection);

			//new including mailingissue! //


			using (SqlCommand cmd = myCommand)
			{
				var dtShortForms = new DataTable();

				var daShortForms = new SqlDataAdapter(myCommand);
				daShortForms.Fill(dtShortForms);
				string jsonResult = JsonConvert.SerializeObject(dtShortForms);

				string result;
				StringWriter sw = new StringWriter();
				JsonToken jt = new JsonToken();

				using (JsonWriter jsonWriter = new JsonTextWriter(sw))
				{
					jsonWriter.Formatting = Formatting.Indented;
					int currentID = 0;

					string answeredID = "";
					string answeredOptionIndex = "";

					jsonWriter.WriteRaw("[");
					for (int i = 0; i < dtShortForms.Rows.Count; i++)
					{
						string sFormName = dtShortForms.Rows[i]["txtshortFormName"].ToString();
						string sFormId = dtShortForms.Rows[i]["numShortFormID"].ToString();
						string qFormId = dtShortForms.Rows[i]["numQFormID"].ToString();
						string doctorId = dtShortForms.Rows[i]["numDoctorID"].ToString();
						string dateCreated = dtShortForms.Rows[i]["dteDateCreated"].ToString();
						string dateModified = dtShortForms.Rows[i]["dteDateModified"].ToString();

						if (i > 0)
						{
							jsonWriter.WriteEndObject();
							jsonWriter.WriteRaw(",");
						}
						jsonWriter.WriteStartObject();

						jsonWriter.WritePropertyName("sformname");
						jsonWriter.WriteValue(sFormName);
						jsonWriter.WritePropertyName("sformid");
						jsonWriter.WriteValue(sFormId);
						jsonWriter.WritePropertyName("qformid");
						jsonWriter.WriteValue(qFormID);
						jsonWriter.WritePropertyName("docid");
						jsonWriter.WriteValue(doctorId);
						jsonWriter.WritePropertyName("datecreated");
						jsonWriter.WriteValue(dateCreated);
						jsonWriter.WritePropertyName("datemodified");
						jsonWriter.WriteValue(dateModified);

						if (i == dtShortForms.Rows.Count - 1)
						{
							jsonWriter.WriteEndObject();
						}
					}

					jsonWriter.WriteRaw("]");


					result = sw.ToString();

					return result;
				}
			}
		}
	}

	//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
	[WebMethod]
	public string CreateShortForm(string formID)   //, List<string> formAnswers
	{
		//GlobalFuncsClass.sendErrorEmail("QUESTIONATOR SERVICE TEST", formID);

		SqlConnection myConnectionLocal = new SqlConnection("server=localhost;integrated security = SSPI;database=Questionator;MultipleActiveResultSets=True;Connection Timeout=160");
		StringBuilder json = new StringBuilder();

		using (SqlConnection myConnection = new SqlConnection())//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			//SELECT Questions.numQID AS [questionid], Questions.numQFormID, Questions.txtType, Questions.txtQText AS [qtext], Questions.numQHeaderID, Questions.flgConditional, Questions.numQParentID, Questions.numQTypeID, drvtbl.numQConditionID, drvtbl.numQID AS Expr1, drvtbl.txtConditionType, drvtbl.flgElse, drvtbl.txtConditionOperator, drvtbl.numQConditionOptionID, drvtbl.numQTypeOptionID, drvtbl.numQConditionID AS Expr4, QTypeOptions.numQTypeOptionID AS Expr2, QTypeOptions.txtOptionText AS [options], QTypeOptions.numQTypeID AS Expr5, QTypes.numQTypeID AS Expr3, QTypes.txtQTypeText, QTypes.flgAllowConditions FROM Questions LEFT OUTER JOIN QTypes ON Questions.numQTypeID = QTypes.numQTypeID INNER JOIN QTypeOptions ON QTypes.numQTypeID = QTypeOptions.numQTypeID LEFT OUTER JOIN (SELECT QConditions.numQConditionID, QConditions.numQID, QConditions.txtConditionType, QConditions.flgElse, QConditions.txtConditionOperator, QConditionOptions.numQConditionOptionID, QConditionOptions.numQTypeOptionID FROM QConditions LEFT OUTER JOIN QConditionOptions ON QConditions.numQConditionID = QConditionOptions.numQConditionID) AS drvtbl ON QTypeOptions.numQTypeOptionID = drvtbl.numQTypeOptionID WHERE Questions.numQFormID = " + formID + " ORDER BY Questions.numQID

			//SqlCommand myCommand = new SqlCommand("SELECT Questions.numQID AS [questionid], Questions.txtQText AS [qtext], QTypeOptions.txtOptionText AS [options] FROM Questions LEFT OUTER JOIN QTypes ON Questions.numQTypeID = QTypes.numQTypeID INNER JOIN QTypeOptions ON QTypes.numQTypeID = QTypeOptions.numQTypeID LEFT OUTER JOIN (SELECT QConditions.numQConditionID, QConditions.numQID, QConditions.txtConditionType, QConditions.flgElse, QConditions.txtConditionOperator, QConditionOptions.numQConditionOptionID, QConditionOptions.numQTypeOptionID FROM QConditions LEFT OUTER JOIN QConditionOptions ON QConditions.numQConditionID = QConditionOptions.numQConditionID) AS drvtbl ON QTypeOptions.numQTypeOptionID = drvtbl.numQTypeOptionID WHERE Questions.numQFormID = " + formID + " ORDER BY Questions.numQID", myConnection);
			SqlCommand myCommand = new SqlCommand("SELECT Questions_D.numQID AS [questionid], Questions_D.txtQText AS [qtext], QTypeOptions_D.txtOptionText AS [options] FROM Questions_D LEFT OUTER JOIN QTypes_D ON Questions_D.numQTypeID = QTypes_D.numQTypeID INNER JOIN QTypeOptions_D ON QTypes_D.numQTypeID = QTypeOptions_D.numQTypeID LEFT OUTER JOIN (SELECT QConditions_D.numQConditionID, QConditions_D.numQID, QConditions_D.txtConditionType, QConditions_D.flgElse, QConditions_D.txtConditionOperator, QConditionOptions_D.numQConditionOptionID, QConditionOptions_D.numQTypeOptionID FROM QConditions_D LEFT OUTER JOIN QConditionOptions_D ON QConditions_D.numQConditionID = QConditionOptions_D.numQConditionID) AS drvtbl_D ON QTypeOptions_D.numQTypeOptionID = drvtbl_D.numQTypeOptionID WHERE Questions_D.numQFormID = " + formID + " ORDER BY Questions_D.numQID", myConnection);

			//new including mailingissue! //


			using (SqlCommand cmd = myCommand)
			{
				var dtQuestions = new DataTable();

				var daComplaints = new SqlDataAdapter(myCommand);
				daComplaints.Fill(dtQuestions);

				var stringShortForm =
					"Mr. <*Bart James*> reports that he was involved in a motor vehicle accident on <*3/20/2012*> as the seat-belted <*driver/a seatbelted front/rear-seat passenger/bicyclist/pedestrian*>, his vehicle was struck in the <*front/rear-ended or on the front/rear right/left*> side or (if pedestrian) the claimant was struck by the vehicle. There was a reported loss of consciousness or there was no reported loss of consciousness.  ";

				//formAnswers.Add("1,Driver");
				//formAnswers.Add("2,Yes");
				//formAnswers.Add("3,No");
				//formAnswers.Add("4,Yes");

				/*foreach(string answer in formAnswers)
                {
                    string id = answer.Substring(0 , answer.LastIndexOf(','));
                    string text = answer.Substring(answer.LastIndexOf(','));

                    stringShortForm = stringShortForm.Replace("<*" + i + "*>", "test" + i);
                }*/

				//var stringShortForm = "Doctor <*1*> has done <*2*> on <*3*> who was a <*4*>";

				for (int i = 1; i <= 4; i++) // dtQuestions.Rows.Count; i++)
				{
					//JsonArray jarray = new JsonArray ("Driver", "Passenger", "Bicyclist", "Pedestrian");
					//stringShortForm = stringShortForm.Replace("<*" + i + "*>", "test" + i);

				}


				string result = "";
				StringWriter sw = new StringWriter();
				JsonToken jt = new JsonToken();

				using (JsonWriter jsonWriter = new JsonTextWriter(sw))
				{
					jsonWriter.Formatting = Newtonsoft.Json.Formatting.Indented;

					//write to json shortform final text //
					jsonWriter.WriteRaw("[");
					jsonWriter.WriteStartObject();
					jsonWriter.WritePropertyName("formattedshortform");
					jsonWriter.WriteValue(stringShortForm);
					jsonWriter.WriteEndObject();
					jsonWriter.WriteRaw("]");

					result = sw.ToString();
				}
				myConnection.Close();

				return result;
			}
		}
	}



	public string GenerateReport(string listingid, string clientid, string datecompleted, string fileName, string sFormId)
	{

		using (SqlConnection myConnection = new SqlConnection("Data Source=172.16.201.231;Initial Catalog=DynaDox_Test;Persist Security Info=True;User ID=roySql;Password=Makulu2011;"))//ConfigurationManager.ConnectionStrings["sqlCon"].ConnectionString))
		{
			myConnection.Open();

			string dateCompleted = (Convert.ToDateTime(datecompleted)).ToShortDateString();

			//SqlCommand myCommand = new SqlCommand("SELECT QFormRulesOutput_D.txtOutput, QFormRulesOutput_D.txtElseOutput, QFormRulesOutput_D.txtOperator, QFormRuleOptions_D.txtOptionsOperator, QFormRuleOptions_D.numShortFormRuleOptionID, QFormRuleOptions_D.numShortFormRuleOutputID, QFormRuleOptions_D.numQuestionID, QFormRuleOptions_D.numOptionID, QFormRuleOptions_D.txtAnswerText, QFormRules_D.numShortFormRuleID, QFormRules_D.txtDescription, drvAnswers.txtAnswerText AS [AnswerText], drvAnswers.numOptionID AS [AnswerOptionID] FROM QFormRules_D INNER JOIN QFormRulesOutput_D ON QFormRules_D.numShortFormRuleID = QFormRulesOutput_D.numShortFormRuleID INNER JOIN QFormRuleOptions_D ON QFormRulesOutput_D.numShortFormRuleOutputID = QFormRuleOptions_D.numShortFormRuleOutputID LEFT OUTER JOIN (SELECT QAnswers_D.txtAnswerText, QAnswers_D.numOptionID, QAnswers_D.numQuestionID FROM QAnswers_D INNER JOIN QAnsweredForms_D ON QAnswers_D.numAnsweredFormID = QAnsweredForms_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + ") AS drvAnswers ON QFormRuleOptions_D.numQuestionID = drvAnswers.numQuestionID AND QFormRuleOptions_D.numOptionID = drvAnswers.numOptionID WHERE QFormRulesOutput_D.numShortFormID = 3; SELECT QAnsweredForms_D.numClientID, QAnsweredForms_D.numDoctorID, QAnsweredForms_D.numDoctorLocationID, CONVERT(varchar, Clients.dteDOI, 101) AS [dteDOI], Clients.txtClientFirstName, Clients.txtClientLastName, Clients.txtSCSNumber, Clients.txtClaimNumber,  CONVERT(varchar, DoctorListing.dteAppointmentDate, 101) AS [dteAppointmentDate], DoctorListing.txtAppointmentTime, DoctorListing.numListingID, Doctors.txtDoctorFirstName, Doctors.txtDoctorLastName, DoctorsLocations.txtDoctorAddressLine1, DoctorsLocations.txtDoctorAddressLine2, DoctorsLocations.txtDoctorCity, DoctorsLocations.txtCounty AS [DoctorAddress], DoctorsLocations.txtDoctorState, DoctorsLocations.txtDoctorZipCode, DoctorsLocations.txtDoctorFaxNumber, DoctorsLocations.txtDoctorPhoneNumber FROM DoctorListing LEFT OUTER JOIN QAnsweredForms_D ON DoctorListing.numListingID = QAnsweredForms_D.numListingID INNER JOIN Clients ON DoctorListing.numClientID = Clients.numClientID INNER JOIN DoctorsLocations ON DoctorListing.numDoctorLocationID = DoctorsLocations.numDoctorLocationID  INNER JOIN Doctors ON DoctorsLocations.numDoctorID = Doctors.numDoctorID WHERE DoctorListing.numListingID = 1234428; SELECT QAnsweredForms_D.flgDoctorInput, QAnsweredForms_D.numFormID, QAnswers_D.numAnswerID, QAnswers_D.numOptionID, QAnswers_D.numQuestionID, QAnswers_D.txtAnswerText FROM QAnsweredForms_D INNER JOIN QAnswers_D ON QAnsweredForms_D.numAnsweredFormID = QAnswers_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + " ;", myConnection);


			SqlCommand myAnsweredFormCommand = new SqlCommand("SELECT TOP 1 numAnsweredFormID FROM QShortForms INNER JOIN QAnsweredForms_D ON QShortForms.numQFormID = QAnsweredForms_D.numFormID WHERE numShortFormID = " + sFormId, myConnection);
			string answeredFormId = myAnsweredFormCommand.ExecuteScalar().ToString();

			SqlCommand myCommand = new SqlCommand("SELECT QFormRulesOutput_D.txtOutput, QFormRulesOutput_D.txtElseOutput, QFormRulesOutput_D.txtOperator, QFormRuleOptions_D.txtOptionsOperator, QFormRuleOptions_D.numShortFormRuleOptionID, QFormRuleOptions_D.numShortFormRuleOutputID, QFormRuleOptions_D.numQuestionID, QFormRuleOptions_D.numOptionID, QFormRuleOptions_D.txtAnswerText, QFormRules_D.numShortFormRuleID, QFormRules_D.txtDescription, drvAnswers.txtAnswerText AS [AnswerText], drvAnswers.numOptionID AS [AnswerOptionID] FROM QFormRules_D INNER JOIN QFormRulesOutput_D ON QFormRules_D.numShortFormRuleID = QFormRulesOutput_D.numShortFormRuleID INNER JOIN QFormRuleOptions_D ON QFormRulesOutput_D.numShortFormRuleOutputID = QFormRuleOptions_D.numShortFormRuleOutputID LEFT OUTER JOIN (SELECT QAnswers_D.txtAnswerText, QAnswers_D.numOptionID, QAnswers_D.numQuestionID FROM QAnswers_D INNER JOIN QAnsweredForms_D ON QAnswers_D.numAnsweredFormID = QAnsweredForms_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + ") AS drvAnswers ON QFormRuleOptions_D.numQuestionID = drvAnswers.numQuestionID AND QFormRuleOptions_D.numOptionID = drvAnswers.numOptionID WHERE QFormRulesOutput_D.numShortFormID = " + sFormId + "; SELECT QAnsweredForms_D.flgDoctorInput, QAnsweredForms_D.numFormID, QAnswers_D.numAnswerID, QAnswers_D.numOptionID, QAnswers_D.numQuestionID, QAnswers_D.txtAnswerText FROM QAnsweredForms_D INNER JOIN QAnswers_D ON QAnsweredForms_D.numAnsweredFormID = QAnswers_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + " ; SELECT QAnsweredForms_D.numClientID, QAnsweredForms_D.numDoctorID, QAnsweredForms_D.numDoctorLocationID FROM QAnsweredForms_D WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + "; ", myConnection);

			//SqlCommand myCommand = new SqlCommand("SELECT TOP 20 ListingNotes.*, Login.UserName, ClientFiles.txtFileName FROM ListingNotes LEFT OUTER JOIN ClientFiles ON ListingNotes.numFileID = ClientFiles.numFileID LEFT OUTER JOIN Login ON ListingNotes.numUserID = Login.UserID", myConnection);


			//new including mailingissue! //


			//using (SqlCommand cmd = myCommand)
			//{
				var dsInformation = new DataSet();

				var daComplaints = new SqlDataAdapter("SELECT QFormRulesOutput_D.txtOutput, QFormRulesOutput_D.txtElseOutput, QFormRulesOutput_D.txtOperator, QFormRuleOptions_D.txtOptionsOperator, QFormRuleOptions_D.numShortFormRuleOptionID, QFormRuleOptions_D.numShortFormRuleOutputID, QFormRuleOptions_D.numQuestionID, QFormRuleOptions_D.numOptionID, QFormRuleOptions_D.txtAnswerText, QFormRules_D.numShortFormRuleID, QFormRules_D.txtDescription, drvAnswers.txtAnswerText AS [AnswerText], drvAnswers.numOptionID AS [AnswerOptionID] FROM QFormRules_D INNER JOIN QFormRulesOutput_D ON QFormRules_D.numShortFormRuleID = QFormRulesOutput_D.numShortFormRuleID INNER JOIN QFormRuleOptions_D ON QFormRulesOutput_D.numShortFormRuleOutputID = QFormRuleOptions_D.numShortFormRuleOutputID LEFT OUTER JOIN (SELECT QAnswers_D.txtAnswerText, QAnswers_D.numOptionID, QAnswers_D.numQuestionID FROM QAnswers_D INNER JOIN QAnsweredForms_D ON QAnswers_D.numAnsweredFormID = QAnsweredForms_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + ") AS drvAnswers ON QFormRuleOptions_D.numQuestionID = drvAnswers.numQuestionID AND QFormRuleOptions_D.numOptionID = drvAnswers.numOptionID WHERE QFormRulesOutput_D.numShortFormID = " + sFormId + "; SELECT QAnsweredForms_D.flgDoctorInput, QAnsweredForms_D.numFormID, QAnswers_D.numAnswerID, QAnswers_D.numOptionID, QAnswers_D.numQuestionID, QAnswers_D.txtAnswerText FROM QAnsweredForms_D INNER JOIN QAnswers_D ON QAnsweredForms_D.numAnsweredFormID = QAnswers_D.numAnsweredFormID WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + " ; SELECT QAnsweredForms_D.numClientID, QAnsweredForms_D.numDoctorID, QAnsweredForms_D.numDoctorLocationID FROM QAnsweredForms_D WHERE QAnsweredForms_D.numAnsweredFormID = " + answeredFormId + "; ", myConnection);
				daComplaints.Fill(dsInformation);

				SqlCommand mySectionsCommand = new SqlCommand("SELECT * FROM QShortForms INNER JOIN QShortFormSections ON QShortForms.numShortFormID = QShortFormSections.numShortFormID WHERE QShortForms.numShortFormID = " + sFormId + " AND numQFormID = " + dsInformation.Tables[1].Rows[0]["numFormID"] + " ORDER BY numSectionPosition", myConnection);

				var dtSections = new DataTable();

				var daSections = new SqlDataAdapter(mySectionsCommand);
				daSections.Fill(dtSections);

				string htmlString = "";
				string pathy = "";
				try
				{
					foreach (DataRow sRow in dtSections.Rows)
					{///2004/SCS/dotnet/DynaForm

						//string path = Server.MapPath("/2004/SCS/dotnet/DynaForm/ShortForms/" + sRow["numQFormID"] + "/" + sRow["numDoctorID"] + "/ShortForms/Sections");
						string path = "/ShortForms/" + sRow["numQFormID"] + "/" + sRow["numDoctorID"] + "/ShortForms/Sections";
						//if (Server.MachineName.Contains("ROYHAREL"))
						//{
						//	path = Server.MapPath("/ShortForms/" + sRow["numQFormID"] + "/" + sRow["numDoctorID"] + "/ShortForms/Sections");
						//}
						//string test = Server.MachineName;
						//string path = Server.MapPath("/ShortForms/" + sRow["numQFormID"] + "/" + sRow["numDoctorID"] + "/ShortForms/Sections"); //"D:/supportclaimservices.com/2004/SCS/dotnet/DynaForm/ShortForms/" + sRow["numQFormID"] + "/" + sRow["numDoctorID"] + "/ShortForms/Sections"; //Server.MapPath("DynaForm/ShortForms/" + sRow["numQFormID"] + "/" + sRow["numDoctorID"] + "/ShortForms/Sections"); //2004/SCS/dotnet/DynaForm/
						pathy = path;

						//var sFiles = Directory.GetFiles(path).OrderBy(f => new FileInfo(f).CreationTime);
						//foreach (string file in sFiles)
						//{
						//GlobalFuncsClass.sendErrorEmail("Create Short form file", "PATH: " + path + " FILE: " + file);
						string sectionFile = path + "/" + sRow["numShortFormID"] + "_" + sRow["numShortFormSectionID"] + ".txt";
						htmlString += File.ReadAllText(sectionFile);
						//}
					}
				}
				catch (Exception ex)
				{
					//GlobalFuncsClass.sendErrorEmail("Create Short form Path Error", "PATH: " + pathy + " ERROR MESSAGE: " + ex + " STACK TRACE:  " + ex.StackTrace);
					//Response.Write("ERROR: " + ex.Message);
				}

				HtmlDocument reportHtmlDoc = new HtmlDocument();
				reportHtmlDoc.LoadHtml(htmlString);
				//GlobalFuncsClass.sendErrorEmail("Create Short form HTML", "HTML: " + htmlString + "      *******also number of rows:"  + dtSections.Rows.Count);
				string result = "";

				var ruleNodes = reportHtmlDoc.DocumentNode.Descendants(".//img[@class='rule']"); ///@alt
				if (ruleNodes != null)
				{
					string lastRuleValue = "";
					foreach (HtmlNode ruleNode in ruleNodes)
					{
						HtmlAttribute rule = ruleNode.Attributes["alt"];

						string ruleValue = rule.Value.Replace("&lt;*", "").Replace("*&gt;", "").Replace("<*", "").Replace("*>", "");
						if (ruleValue != lastRuleValue)
						{
							string outputValue = GetValue(dsInformation.Tables[0], dsInformation, ruleValue, "FRU", false);
							var newOutputNode = HtmlNode.CreateNode(outputValue);
							ruleNode.ParentNode.ReplaceChild(newOutputNode, ruleNode);
						}
						else
						{
							ruleNode.ParentNode.RemoveChild(ruleNode);
						}
						lastRuleValue = ruleValue;
						//string imageUrl = src.Value;
					}
					result = reportHtmlDoc.DocumentNode.InnerHtml;
				}
				var dataNodes = reportHtmlDoc.DocumentNode.Descendants(".//img[@class='data']"); ///@alt
				if (dataNodes != null)
				{
					foreach (HtmlNode dataNode in dataNodes.ToList())
					{
						HtmlAttribute data = dataNode.Attributes["alt"];

						//GlobalFuncsClass.sendErrorEmail("Create Short form DATA VALUE FIELD", "VALUE: " + data.Value);

						string dataValue = data.Value.Replace("&lt;*", "").Replace("*&gt;", "").Replace("<*", "").Replace("*>", "");
						string dataOutput = "";
						if (dataValue.Contains("SCS"))
						{
							dataOutput = GetValue(dsInformation.Tables[2], null, dataValue, "SCS", false);
						}
						else if (dataValue.Contains("QAN"))
						{
							dataOutput = GetValue(dsInformation.Tables[1], null, dataValue, "QAN", false);

						}
						else if (dataValue.Contains("DIN")) // its DIN
						{
							dataOutput = GetValue(dsInformation.Tables[1], null, dataValue, "DIN", false);
						}
						else if (dataValue.Contains("GRAMMAR"))
						{
							dataOutput = GetValue(dsInformation.Tables[2], null, dataValue, "GRAMMAR", false);
						}

						var newDataNode = HtmlNode.CreateNode(dataOutput);

						dataNode.ParentNode.ReplaceChild(newDataNode.ParentNode, dataNode);
						//reportHtmlDoc.DocumentNode.ReplaceChild(newDataNode, reportHtmlDoc.DocumentNode.SelectSingleNode());
						//string imageUrl = src.Value;
					}
					result = reportHtmlDoc.DocumentNode.OuterHtml;
				}

				//htmlString = File.ReadAllText("//psf/Home/Documents/Magic Briefcase/Work/Mac Project/ShortFormTestHTML.htm"); 
				//File.ReadAllText("d:/supportclaimservices.com/2004/SCS/dotnet/temp/QSigs/ShortFormTestHTML.htm");
				//htmlString = htmlString.Replace("&lt;*", "<");
				//htmlString = htmlString.Replace("&gt;", ">");

				//string result = "";

				//File.WriteAllText("//psf/Home/Documents/Magic Briefcase/Work/Mac Project/" + fileName + ".htm", (reportHtmlDoc.DocumentNode.OuterHtml));
				//File.WriteAllText("d:/supportclaimservices.com/2004/SCS/dotnet/temp/QSigs/" + fileName + ".htm", htmlOutputString.ToString());

				//GlobalFuncsClass.sendErrorEmail("Create Short form result", result); 
				return result;
			//}
		}
	}

	private string GetValue(DataTable dTable, DataSet dSet, string searchString, string infoCase, bool elseOutput)
	{
		string result = "";
		string expression = "";
		string qID = "";
		DataRow[] foundRows;

		switch (infoCase)
		{
			case "SCS":
				result = dTable.Rows[0][searchString.Substring(searchString.IndexOf('_') + 1)].ToString();

				break;
			case "GRAMMAR":
				string[] grammer = searchString.Substring(searchString.IndexOf('_') + 1).Split('_');
				string gender = "Male";//dTable.Rows[0]["txtGender"].ToString();
				if (gender == "Female")
				{
					result = grammer[1];
				}
				else
				{
					result = grammer[0];
				}
				break;
			case "DIN":

				qID = searchString.Substring(4);
				expression = "flgDoctorInput = 1 AND numQuestionID = " + qID;

				// Use the Select method to find all rows matching the filter.
				foundRows = dTable.Select(expression);
				if (foundRows.Length > 0)
				{
					result = foundRows[0]["txtAnswerText"].ToString();
				}
				else
				{
					result = " NO ANSWER WAS FOUND ";
				}

				break;
			case "QAN":

				qID = searchString.Substring(4);
				expression = "numQuestionID = " + qID;   // ******** flgDoctorInput <> 1 AND  ******** //////////

				// Use the Select method to find all rows matching the filter.
				foundRows = dTable.Select(expression);
				if (foundRows.Length > 0)
				{
					foreach (DataRow aRow in foundRows)
					{
						result += aRow["txtAnswerText"] + ", ";
					}
					result = result.Trim().Trim(',');
				}
				else
				{
					result = " NO ANSWER WAS FOUND ";
				}

				break;
			case "FRU":
				int start = searchString.LastIndexOf('_') + 1;
				int end = searchString.LastIndexOf('*');
				string formRuleId = searchString.Substring(start);


				StringBuilder formRuleFinalOutput = new StringBuilder();
				expression = "numShortFormRuleID = '" + formRuleId + "'";

				// Use the Select method to find all rows matching the filter.
				foundRows = dTable.Select(expression, "numShortFormRuleOutputID ASC");

				if (foundRows.Length > 0)
				{

					string output = "";
					string outputRule = foundRows[0]["txtOutput"].ToString();
					string outputElseRule = foundRows[0]["txtElseOutput"].ToString();

					int currentOutputRuleID = Convert.ToInt32(foundRows[0]["numShortFormRuleOutputID"]);
					int outputRuleID = Convert.ToInt32(foundRows[0]["numShortFormRuleOutputID"]);
					string ruleOpertator = foundRows[0]["txtOptionsOperator"].ToString();

					string outputOptionID = foundRows[0]["numOptionID"].ToString();
					string answerOptionID = foundRows[0]["AnswerOptionID"].ToString();
					string answerText = foundRows[0]["AnswerText"].ToString();

					bool sofarsogood = false;
					bool itsallgood = false;
					bool doContinue = true;


					for (int r = 0; r < foundRows.Length; r++)
					{

						outputOptionID = foundRows[r]["numOptionID"].ToString();
						answerOptionID = foundRows[r]["AnswerOptionID"].ToString();
						answerText = foundRows[r]["AnswerText"].ToString();

						outputRule = foundRows[r]["txtOutput"].ToString();
						outputElseRule = foundRows[r]["txtElseOutput"].ToString();

						outputRuleID = Convert.ToInt32(foundRows[r]["numShortFormRuleOutputID"]);
						ruleOpertator = foundRows[r]["txtOperator"].ToString();

						if (currentOutputRuleID != outputRuleID)
						{
							if (sofarsogood)
							{
								itsallgood = true;
								doContinue = false;
							}
							else
							{
								doContinue = true;
							}
							currentOutputRuleID = outputRuleID;
						}
						if (doContinue)
						{
							switch (ruleOpertator)
							{
								case "And":

									//DataTable dt1 = foundRows.CopyToDataTable();

									//DataView view = new DataView(dt1);
									//DataTable distinctValues = view.ToTable(true, "numShortFormRuleOutputID", "numQuestionId");

									bool qParametersTrue = true;

									//foreach(DataRow outputIdRow in distinctValues.Rows)
									//{
									string qId = foundRows[r]["numQuestionId"].ToString();
									DataRow[] foundQuestion = dTable.Select("numShortFormRuleOutputID = '" + currentOutputRuleID + "' AND numQuestionId = '" + qId + "'");

									if (foundQuestion.Length > 0)
									{
										if (foundQuestion[0]["txtOptionsOperator"].ToString() == "And")
										{
											foreach (DataRow qRow in foundQuestion)
											{
												string questionOptionID = qRow["numOptionID"].ToString();
												string questionAnswerOptionID = qRow["AnswerOptionID"].ToString();
												if (questionOptionID == questionAnswerOptionID)
												{
													qParametersTrue = true;
												}
												else
												{
													qParametersTrue = false;
													break;
												}
											}
										}
										else
										{
											foreach (DataRow qRow in foundQuestion)
											{
												string questionOptionID = qRow["numOptionID"].ToString();
												string questionAnswerOptionID = qRow["AnswerOptionID"].ToString();
												if (questionOptionID == questionAnswerOptionID)
												{
													qParametersTrue = true;
													break;
												}
												qParametersTrue = false;
											}
										}
									}
									//}
									if (qParametersTrue)//(outputOptionID == answerOptionID)
									{
										sofarsogood = true;
										if (r + 1 < foundRows.Length)
										{
											if (currentOutputRuleID != Convert.ToInt32(foundRows[r + 1]["numShortFormRuleOutputID"]))
											{
												itsallgood = true;
											}
										}
										else
										{
											itsallgood = true;
										}
										//itsallgood = false;
									}
									else
									{
										//itsallgood = false;
										doContinue = false;
										sofarsogood = false;
									}
									break;
								case "Or":
									if (outputOptionID == answerOptionID && !sofarsogood)
									{
										itsallgood = true;
									}
									break;
								case "else":
									if (outputOptionID != answerOptionID)
									{
										sofarsogood = true;
										elseOutput = true;
										if (r + 1 < foundRows.Length)
										{
											if (currentOutputRuleID != Convert.ToInt32(foundRows[r + 1]["numShortFormRuleOutputID"]))
											{
												itsallgood = true;
											}
										}
										else
										{
											itsallgood = true;
										}
									}
									else
									{
										itsallgood = true;
										elseOutput = false;
									}
									//itsallgood = true;
									break;
								case "else if":

									break;
							}

							/*if(!doContinue)
							{
								for (int y = r; y < foundRows.Length; y++)
								{
									// LOOP TO SKIP ROWS UNTIL NEXT RULE ** //
								}
							}*/

						}


						if (itsallgood)
						{
							if (elseOutput)
							{
								output = outputElseRule;
							}
							else
							{
								output = outputRule;
							}
							break;
						}


					}

					if (string.IsNullOrEmpty(output))
					{
						output = outputElseRule;
					}

					elseOutput = false;



					HtmlDocument ruleOutputHtmlDoc = new HtmlDocument();
					ruleOutputHtmlDoc.LoadHtml(output);

					var dataNodes = ruleOutputHtmlDoc.DocumentNode.Descendants(".//img[@class='data']/@alt");
					if (dataNodes != null)
					{
						foreach (HtmlNode dataNode in dataNodes)
						{
							HtmlAttribute data = dataNode.Attributes["alt"];

							string dataValue = data.Value.Replace("&lt;*", "").Replace("*&gt;", "").Replace("<*", "").Replace("*>", "");
							string dataOutput;
							if (dataValue.Contains("SCS"))
							{
								dataOutput = GetValue(dSet.Tables[2], null, dataValue, "SCS", false);
							}
							else if (dataValue.Contains("QAN"))
							{
								dataOutput = GetValue(dSet.Tables[1], null, dataValue, "QAN", false);
								if (outputElseRule != null && dataOutput == " NO ANSWER WAS FOUND ")
								{
									elseOutput = true;

								}

							}
							else // its DIN
							{
								dataOutput = GetValue(dSet.Tables[1], null, dataValue, "DIN", false);
								if (outputElseRule != null && dataOutput == " NO ANSWER WAS FOUND ")
								{
									elseOutput = true;

								}
							}

							var newDataNode = HtmlNode.CreateNode(dataOutput);

							dataNode.ParentNode.ReplaceChild(newDataNode.ParentNode, dataNode);

							//string imageUrl = src.Value;
						}
					}
					result = ruleOutputHtmlDoc.DocumentNode.OuterHtml; //formRuleFinalOutput.ToString();
				}
				else
				{
					if (elseOutput)
					{
						result = "";
					}
					else
					{
						result = " NO RULE WAS FOUND ";
					}
				}

				break;
		}


		return result;
	}
}




//foreach (DataRow drQuestion in dtQuestions.Rows)
//		{

//			FormSection qSection = myForm.FormSections.Find(r => r.SectionId == drQuestion["numSectionID"].ToString());

//			if (qSection == null)
//			{
//				qSection = new FormSection()
//{
//	SectionId = drQuestion["numSectionID"].ToString(),
//					SectionName = drQuestion["txtSectionHeader"].ToString(),
//					SectionQuestions = new List<SectionQuestion>()
//				};
//myForm.FormSections.Add(qSection);
//			}

//			if (qSection.SectionQuestions.Find((SectionQuestion obj) => obj.QuestionId == drQuestion["questionid"].ToString()) != null) continue;
//			qSection.SectionQuestions.Add(new SectionQuestion()
//{
//	SectionId = drQuestion["numSectionID"].ToString(),
//				QuestionId = drQuestion["questionid"].ToString(),
//				QuestionText = drQuestion["qtext"].ToString(),
//				ParentConditionTriggerId = drQuestion["ConditionTrigger"].ToString(),
//				QuestionType = drQuestion["qtype"].ToString(),
//				QuestionParentId = drQuestion["numQParentID"].ToString(),
//				IsConditional = string.IsNullOrEmpty(drQuestion["ConditionTrigger"].ToString()) ? false : true,
//				AnswerId = drQuestion["numAnswerID"].ToString(),
//				AnswerText = drQuestion["txtAnswerText"].ToString()
//			});

//			DataRow[] dtQOptions = dtForm.Select("questionid = " + drQuestion["questionid"]);

//			if (dtQOptions.Length > 1)
//			{
//				qSection.SectionQuestions[qSection.SectionQuestions.Count - 1].QuestionOptions =
//					new List<QuestionOption>();

//				foreach (DataRow qOption in dtQOptions)
//				{

//					qSection.SectionQuestions[qSection.SectionQuestions.Count - 1].QuestionOptions.Add(new QuestionOption()
//{
//	OptionId = qOption["numQTypeOptionID"].ToString(),
//						OptionText = qOption["options"].ToString(),
//						ConditionTriggerId = qOption["ConnectedCondition"].ToString(),
//						ParentQuestionId = qOption["questionid"].ToString(),
//						//Chosen = !string.IsNullOrEmpty(drQuestion["numOptionID"].ToString()) && drQuestion["numOptionID"].ToString() == qOption["numQTypeOptionID"].ToString() ? true : false,
//						Chosen = !string.IsNullOrEmpty(qOption["numAnswerID"].ToString()) ? true : false
//					});
//				}
//			}
//			else
//			{
//			}
//		}