using System.Collections.Generic;
using UIKit;

namespace DynaPad
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main(string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main(args, null, "AppDelegate");
		}

		static void JsonCallback(object data)
		{
			//foreach (JsonElement qElement in
			//Console.WriteLine("Invoked");
		}
	}
}


public static class ActiveMenu
{
	public static Menu activeMenu { get; set; }
}


public class MenuItem
{
	public string MenuItemValue { get; set; }
	public string MenuItemAction { get; set; }
	public string MenuItemCaption { get; set; }
	public string PatientId { get; set; }
	public string DoctorId { get; set; }
	public string LocationId { get; set; }
	public string ApptId { get; set; }
	public List<Menu> Menus { get; set; }
}


public class Menu
{
	public string MenuValue { get; set; }
	public string MenuAction { get; set; }
	public string MenuCaption { get; set; }
	public string PatientId { get; set; }
	public string DoctorId { get; set; }
	public string LocationId { get; set; }
	public string ApptId { get; set; }
	public List<MenuItem> MenuItems { get; set; }
}


public class DynaMenu{}


public static class SelectedAppointment
{
	public static string ApptFormId { get; set; }
	public static string ApptFormName { get; set; }
	public static string ApptId { get; set; }
	public static string ApptPatientId { get; set; }
	public static string ApptDoctorId { get; set; }
	public static string ApptLocationId { get; set; }
	public static List<Report> ApptReports { get; set; }
	public static QForm SelectedQForm { get; set; }
	public static QForm AnsweredQForm { get; set; }
}


public class Report
{
	public string ReportId { get; set; }
	public string ReportName { get; set; }
	public string ReportDescription { get; set; }
	public string DoctorId { get; set; }
	public string FormId { get; set; }
}


public class ActiveTriggerId
{
	public string ParentQuestionId { get; set; }
	public string ParentOptionId { get; set; }
	public string TriggerId { get; set; }
	public bool Triggered { get; set; }
}


public class QuestionOption
{
	public string ParentQuestionId { get; set; }
	public string OptionId { get; set; }
	public string OptionText { get; set; }
	public bool Chosen { get; set; }
	public List<string> ConditionTriggerIds { get; set; }
}


public class SectionQuestion
{
	public string SectionId { get; set; }
	public string QuestionId { get; set; }
	public string QuestionParentId { get; set; }
	public string QuestionType { get; set; }
	public string QuestionText { get; set; }
	public string QuestionKeyboardType { get; set; }
	public string ParentConditionTriggerId { get; set; }
	public string AnswerId { get; set; }
	public string AnswerText { get; set; }
	public string AnswerOptionIndex { get; set; }
	public string MinValue { get; set; }
	public string MaxValue { get; set; }
	public bool IsConditional { get; set; }
	public bool IsAnswered { get; set; }
	public bool IsEnabled { get; set; }
	public List<string> ActiveTriggerIds { get; set; }
	public List<QuestionOption> QuestionOptions { get; set; }
}


public class FormSection
{
	public string SectionId { get; set; }
	public string SectionName { get; set; }
	public int SectionSelectedTemplateId { get; set; }
	public List<SectionQuestion> SectionQuestions { get; set; }
	public FormSection() { SectionSelectedTemplateId = 0; }
}


public class QForm
{
	public string FormId { get; set; }
	public string FormName { get; set; }
	public string PatientId { get; set; }
	public string DoctorId { get; set; }
	public string LocationId { get; set; }
	public string ApptId { get; set; }
	public string DateCompleted { get; set; }
	public string DateUpdated { get; set; }
	public int FormSelectedTemplateId { get; set; }
	public List<FormSection> FormSections { get; set; }
	public QForm() { FormSelectedTemplateId = 0; }
}
