using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using MonoTouch.Dialog;
using Newtonsoft.Json;
using System.Drawing;
using System.IO;
using Newtonsoft.Json.Linq;
using LoginScreen;
using System.Text.RegularExpressions;

namespace DynaPad
{
	public partial class MasterViewController : DynaDialogViewController
	{
		public DetailViewController DetailViewController { get; set; }
		public DialogViewController mvc { get; set; }
		UILabel messageLabel;
		LoadingOverlay loadingOverlay;
		Menu myDynaMenu;


		protected MasterViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
			Title = "";
		}

		bool needLogin = true;

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			if (needLogin)
			{
				LoginScreenControl<CredentialsProvider, DefaultLoginScreenMessages>.Activate(this);
				needLogin = false;
			}
			else
			{
				Title = NSBundle.MainBundle.LocalizedString("Menu", "Form Sections");
				DetailViewController = (DetailViewController)((UINavigationController)SplitViewController.ViewControllers[1]).TopViewController;
				DetailViewController.Root.Clear();
				DetailViewController.Root.Add(new Section("Logged in"));
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = NSBundle.MainBundle.LocalizedString("Login", "Form Sections");

			ClearsSelectionOnViewWillAppear &= UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad;

			DetailViewController = (DetailViewController)((UINavigationController)SplitViewController.ViewControllers[1]).TopViewController;
			DetailViewController.Style = UITableViewStyle.Plain;
			// TODO pull to refresh: (problem scrolling with it..)
			//DetailViewController.RefreshRequested += delegate
			//{
			//	DetailViewController.NavigationController.NavigationBar.Translucent = false;
			//	DetailViewController.ReloadData();
			//	DetailViewController.ReloadComplete();
			//};
			//RefreshRequested += delegate { ReloadComplete(); };

			//var refresh = new UIRefreshControl();
			//DetailViewController.Add(refresh);

			var dds = new DynaPadService.DynaPadService();

			//var menu = dds.BuildDynaMenu("123");
			//var menuObj = JsonConvert.DeserializeObject<Menu>(dds.BuildDynaMenu("123"));

			myDynaMenu = JsonConvert.DeserializeObject<Menu>(dds.BuildDynaMenu("1"));
			DetailViewController.DynaMenu = myDynaMenu;

			var rootMainMenu = new DynaFormRootElement(myDynaMenu.MenuCaption);
			rootMainMenu.UnevenRows = true;
			rootMainMenu.Enabled = true;

			var sectionMainMenu = new Section();
			sectionMainMenu.HeaderView = null;
			BuildMenu(myDynaMenu, sectionMainMenu);
			rootMainMenu.Add(sectionMainMenu);

			Root = rootMainMenu;
		}


		private RootElement BuildMenu(Menu myMenu, Section sectionMenu)
		{
			if (myMenu.MenuItems == null) return null;
			foreach (MenuItem mItem in myMenu.MenuItems)
			{
				var rootMenu = new DynaFormRootElement(mItem.MenuItemCaption);

				rootMenu.UnevenRows = true;

				rootMenu.Enabled = true;
				rootMenu.FormID = mItem.MenuItemValue;
				rootMenu.FormName = mItem.MenuItemCaption;
				rootMenu.MenuAction = mItem.MenuItemAction;
				rootMenu.MenuValue = mItem.MenuItemValue;
				rootMenu.PatientID = mItem.PatientId;
				rootMenu.PatientName = mItem.PatientName;
				rootMenu.DoctorID = mItem.DoctorId;
				rootMenu.LocationID = mItem.LocationId;
				rootMenu.ApptID = mItem.ApptId;
				rootMenu.IsDoctorForm = mItem.MenuItemAction == "GetDoctorForm";

				switch (mItem.MenuItemAction)
				{
					case "GetPatientForm":
					case "GetDoctorForm":
						rootMenu.createOnSelected = GetFormService;
						break;
					case "GetAppt":
						rootMenu.createOnSelected = GetApptService;
						break;
					case "GetReport":
						sectionMenu.Add(new StringElement(mItem.MenuItemCaption, delegate { LoadReportView(mItem.MenuItemValue, "Report", rootMenu); }));
						//rootMenu.createOnSelected = GetReportService;
						//Section sectionReport = new Section();
						//sectionReport.Add(new StringElement(rootMenu.MenuValue, delegate { LoadReportView("Report", rootMenu.MenuValue); }));
						//rootMenu.Add(sectionReport);

						//DetailViewController.Root.Caption = mItem.MenuItemValue;
						//DetailViewController.ReloadData();
						break;
					case "GetSummary":
						sectionMenu.Add(new StringElement(mItem.MenuItemCaption, delegate { LoadSummaryView(mItem.MenuItemValue, "Summary", rootMenu); }));
						break;
				}
				if (mItem.MenuItemAction != "GetReport" && mItem.MenuItemAction != "GetSummary")
				{
					sectionMenu.Add(rootMenu);
				}

				if (mItem.Menus == null) return null;

				foreach (Menu mRoot in mItem.Menus)
				{
					var newSection = new Section();
					BuildMenu(mRoot, newSection);
					rootMenu.Add(newSection);
				}
			}

			return null;
		}


		public UIViewController GetApptService(RootElement rElement)
		{
			//if (DetailViewController.QuestionsView != null)
			//{
			//	DetailViewController.Title = "";
			//	DetailViewController.QuestionsView = null; //.Clear();
			//}

			var dfElemet = (DynaFormRootElement)rElement;
			SelectedAppointment.ApptFormId = dfElemet.FormID;
			SelectedAppointment.ApptFormName = dfElemet.FormName;
			SelectedAppointment.ApptPatientId = dfElemet.PatientID;
			SelectedAppointment.ApptPatientName = dfElemet.PatientName;
			SelectedAppointment.ApptDoctorId = dfElemet.DoctorID;
			SelectedAppointment.ApptLocationId = dfElemet.LocationID;
			SelectedAppointment.ApptId = dfElemet.ApptID;

			return new DynaDialogViewController(rElement, true);
		}


		public UIViewController GetReportService(RootElement rElement)
		{
			//if (DetailViewController.QuestionsView != null)
			//{
			//	DetailViewController.Title = "";
			//	DetailViewController.QuestionsView = null; //.Clear();
			//}

			var dfElemet = (DynaFormRootElement)rElement;
			//var DynaReport = SelectedAppointment.ApptDynaReports.Find((DynaReport obj) => obj.FormId == dfElemet.MenuValue);
			var dds = new DynaPadService.DynaPadService();
			string origJson = dds.GetDynaReports(dfElemet.FormID, dfElemet.DoctorID, false);
			JsonHandler.OriginalFormJsonString = origJson;
			//var rootReports = new RootElement(dfElemet.FormName);
			var rootReports = new RootElement("Reports");
			var sectionReports = new Section();

			foreach (Report esf in SelectedAppointment.ApptReports)
			{
				sectionReports.Add(new StringElement(esf.ReportName, delegate { LoadSectionView(esf.ReportId, "Report", null, false); }));
			}

			rootReports.Add(sectionReports);

			var formDVC = new DynaDialogViewController(rootReports, true);

			DetailViewController.Root.Caption = dfElemet.FormName;
			DetailViewController.ReloadData();

			formDVC.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Back"), UIBarButtonItemStyle.Plain, delegate (object sender, EventArgs e)
		  	{
				  //DetailViewController.Title = "Welcome to DynaPad";
				  DetailViewController.QuestionsView = null; //.Clear();
				  DetailViewController.NavigationItem.RightBarButtonItem = null;
				  DetailViewController.Root.Clear();
				  DetailViewController.Root.Add(new Section("Logged in"));
				  DetailViewController.Root.Caption = "Welcome to DynaPad";
				  DetailViewController.ReloadData();

				  NavigationController.PopViewController(true);
		  	});

			return formDVC;
		}


		//public UIViewController GetMRService(RootElement rElement)
		//{
		//	var bounds = base.TableView.Frame;
		//	// show the loading overlay on the UI thread using the correct orientation sizing
		//	loadingOverlay = new LoadingOverlay(bounds);
		//	SplitViewController.Add(loadingOverlay);

		//	var dds = new DynaPadService.DynaPadService();
		//	var dfElemet = (DynaFormRootElement)rElement;

		//	string origJson = dds.GetMRFolders(dfElemet.PatientID, dfElemet.DoctorID, dfElemet.LocationID, SelectedAppointment.ApptId);
		//	JsonHandler.OriginalFormJsonString = origJson;
		//	SelectedAppointment.MedicalRecords = JsonConvert.DeserializeObject<QForm>(origJson);

		//	DetailViewController.Root.Caption = "Medical Records:" + SelectedAppointment.SelectedQForm.PatientName;
		//	DetailViewController.ReloadData();

		//	var mrGroup = new RadioGroup("mrs", -1);
		//	var rootMR = new RootElement("Medical Records", mrGroup);

		//	var mrSections = new Section();

		//	foreach (MRFolder mrf in SelectedAppointment.MedicalRecord)
		//	{
		//		var mrfolder = new SectionStringElement(mrf.FolderName, delegate
		//		{
		//			LoadFolderView(mrf.FolderId, mrf.FolderName, mrf.FolderURL, mrf, mrSections);
		//			foreach (Element d in mrSections.Elements)
		//			{
		//				var t = d.GetType();
		//				if (t == typeof(SectionStringElement))
		//				{
		//					var di = (SectionStringElement)d;
		//					di.selected = false;
		//				}
		//			}

		//			mrSections.GetContainerTableView().ReloadData();
		//		});

		//		mrSections.Add(mrfolder);
		//	}

		//	rootMR.Add(mrSections);

		//	var formDVC = new DynaDialogViewController(rootMR, true);

		//	// TODO pull to refresh: (problamatic scrolling with it)
		//	//formDVC.RefreshRequested += delegate 
		//	//{ 
		//	//	formDVC.ReloadComplete(); 
		//	//};

		//formDVC.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Back"), UIBarButtonItemStyle.Plain, delegate (object sender, EventArgs e)
		//  	{
		//		  //DetailViewController.Title = "Welcome to DynaPad";
		//		  DetailViewController.QuestionsView = null; //.Clear();
		//		  DetailViewController.NavigationItem.RightBarButtonItem = null;
		//		  DetailViewController.Root.Clear();
		//		  DetailViewController.Root.Add(new Section("Logged in"));
		//		  DetailViewController.Root.Caption = "Welcome to DynaPad";
		//		  DetailViewController.ReloadData();

		//		  NavigationController.PopViewController(true);
		//  	});

		//	loadingOverlay.Hide();

		//	return formDVC;
		//}


		public UIViewController GetFormService(RootElement rElement)
		{
			//if (DetailViewController.QuestionsView != null)
			//{
			//	DetailViewController.Title = "";
			//	DetailViewController.QuestionsView = null; //.Clear();
			//}

			//var bounds = UIScreen.MainScreen.Bounds;
			var bounds = base.TableView.Frame;
			// show the loading overlay on the UI thread using the correct orientation sizing
			loadingOverlay = new LoadingOverlay(bounds);
			//mvc = (DialogViewController)((UINavigationController)SplitViewController.ViewControllers[0]).TopViewController;
			SplitViewController.Add(loadingOverlay);
			var dds = new DynaPadService.DynaPadService();
			var dfElemet = (DynaFormRootElement)rElement;
			string origJson = dds.GetFormQuestions(dfElemet.FormID, dfElemet.DoctorID, dfElemet.LocationID, dfElemet.PatientID, dfElemet.PatientName, SelectedAppointment.ApptId, dfElemet.IsDoctorForm);
			JsonHandler.OriginalFormJsonString = origJson;
			SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

			DetailViewController.Root.Caption = SelectedAppointment.SelectedQForm.FormName;
			DetailViewController.ReloadData();

			bool IsDoctorForm = dfElemet.IsDoctorForm;

			var navTitle = IsDoctorForm ? "Doctor Form" : "Patient Form";
			var sectionsGroup = new RadioGroup("sections", -1);
			//var rootFormSections = new RootElement(SelectedAppointment.SelectedQForm.FormName, sectionsGroup);
			var rootFormSections = new RootElement(navTitle, sectionsGroup);

			var sectionFormSections = new Section();

			if (IsDoctorForm)
			{
				/*
				 * TODO: make presets password protected (maybe not, since for doctors only?)! (maybe component: Passcode)
				*/

				var FormPresetNames = dds.GetAnswerPresets(SelectedAppointment.ApptFormId, null, SelectedAppointment.ApptDoctorId, true, SelectedAppointment.ApptLocationId);
				var formPresetSection = new DynaSection("Form Preset Answers");
				formPresetSection.Enabled = true;
				var formPresetGroup = new RadioGroup("FormPresetAnswers", SelectedAppointment.SelectedQForm.FormSelectedTemplateId);
				var formPresetsRoot = new DynaRootElement("Preset Answers", formPresetGroup);
				formPresetsRoot.IsPreset = true;//background color

				//var noPresetRadio = new MyRadioElement("No Preset", "FormPresetAnswers");
				var noPresetRadio = new PresetRadioElement("No Preset", "FormPresetAnswers");
				noPresetRadio.PresetName = "No Preset";
				noPresetRadio.OnSelected += delegate (object sender, EventArgs e)
				{
					JsonHandler.OriginalFormJsonString = origJson;
					SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

					LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
				};

				formPresetSection.Add(noPresetRadio);

				int fs = SelectedAppointment.SelectedQForm.FormSections.IndexOf(SelectedAppointment.SelectedQForm.FormSections[0]);

				foreach (string[] arrPreset in FormPresetNames)
				{
					var radioPreset = GetPreset(arrPreset[3], arrPreset[1], arrPreset[2], fs, SelectedAppointment.SelectedQForm.FormSections[0].SectionId, formPresetGroup, SelectedAppointment.SelectedQForm.FormSections[0], formPresetSection, origJson, sectionFormSections, IsDoctorForm);

					//var radioPreset = new PresetRadioElement(arrPreset[1], "FormPresetAnswers");
					//radioPreset.PresetName = arrPreset[1];
					//radioPreset.PresetJson = arrPreset[2];
					//radioPreset.OnSelected += delegate (object sender, EventArgs e)
					//{
					//	string presetJson = arrPreset[2];
					//	JsonHandler.OriginalFormJsonString = presetJson;
					//	SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(presetJson);
					//	LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
					//};

					formPresetSection.Add(radioPreset);
				}

				var btnNewFormPreset = new GlassButton(new RectangleF(0, 0, (float)View.Frame.Width, 50));
				//btnNewFormPreset.Font = UIFont.BoldSystemFontOfSize(17);
				btnNewFormPreset.TitleLabel.Font = UIFont.BoldSystemFontOfSize(17);
				btnNewFormPreset.SetTitleColor(UIColor.Black, UIControlState.Normal);
				btnNewFormPreset.NormalColor = UIColor.FromRGB(224, 238, 240);
				btnNewFormPreset.SetTitle("Save New Form Preset", UIControlState.Normal);
				btnNewFormPreset.TouchUpInside += (sender, e) =>
				{
					/*
					 * TODO: popup to enter preset name (DONE?)
					*/

					//Create Alert
					var SavePresetPrompt = UIAlertController.Create("New Form Preset", "Enter preset name: ", UIAlertControllerStyle.Alert);
					SavePresetPrompt.AddTextField((field) =>
					{
						field.Placeholder = "Preset Name";
					});
					//Add Actions
					SavePresetPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SaveFormPreset(null, SavePresetPrompt.TextFields[0].Text, SelectedAppointment.SelectedQForm.FormSections[0].SectionId, formPresetSection, null, formPresetGroup, origJson, sectionFormSections, IsDoctorForm)));
					SavePresetPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
					//Present Alert
					PresentViewController(SavePresetPrompt, true, null);
				};

				formPresetSection.Add(btnNewFormPreset);
				formPresetsRoot.Add(formPresetSection);
				formPresetsRoot.Enabled = true;

				sectionFormSections.Add(formPresetsRoot);
			}

			foreach (FormSection fSection in SelectedAppointment.SelectedQForm.FormSections)
			{
				var section = new SectionStringElement(fSection.SectionName, delegate
				{
					LoadSectionView(fSection.SectionId, fSection.SectionName, fSection, IsDoctorForm, sectionFormSections);
					foreach (Element d in sectionFormSections.Elements)
					{
						var t = d.GetType();
						if (t == typeof(SectionStringElement))
						{
							var di = (SectionStringElement)d;
							di.selected = false;
						}
					}

					//var shhh = sectionFormSections.GetContainerTableView();
					sectionFormSections.GetContainerTableView().ReloadData();
				});

				sectionFormSections.Add(section);
				//sectionFormSections.Add(new StringElement(fSection.SectionName, delegate { LoadSectionView(fSection.SectionId, fSection.SectionName, fSection, IsDoctorForm); }));
			}

			var finalizeSection = new SectionStringElement("Finalize", delegate
			{
				LoadSectionView("", "Finalize", null, IsDoctorForm);

				foreach (Element d in sectionFormSections.Elements)
				{
					var t = d.GetType();
					if (t == typeof(SectionStringElement))
					{
						var di = (SectionStringElement)d;
						di.selected = false;
					}
				}
				sectionFormSections.GetContainerTableView().ReloadData();
			});

			sectionFormSections.Add(finalizeSection);

			//sectionFormSections.Add(new StringElement("Finalize", delegate { LoadSectionView("", "Finalize", null, IsDoctorForm); }));

			rootFormSections.Add(sectionFormSections);

			var formDVC = new DynaDialogViewController(rootFormSections, true);

			// TODO pull to refresh: (problamatic scrolling with it)
			//formDVC.RefreshRequested += delegate 
			//{ 
			//	formDVC.ReloadComplete(); 
			//};

			messageLabel = new UILabel();
			formDVC.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("LockedBack"), UIBarButtonItemStyle.Plain, delegate (object sender, EventArgs e)
		  	{
				  //Create Alert
				  var BackPrompt = UIAlertController.Create("Exit Form", "Administrative use only. Please enter password to continue", UIAlertControllerStyle.Alert);
				  BackPrompt.AddTextField((field) =>
				  {
					  field.SecureTextEntry = true;
					  field.Placeholder = "Password";
				  });

				  BackPrompt.Add(messageLabel);
				  BackPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => PopBack(BackPrompt.TextFields[0].Text, IsDoctorForm)));
				  BackPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

				  //Present Alert
				  PresentViewController(BackPrompt, true, null);
		  	});

			//if (!IsDoctorForm)
			//{
			//	messageLabel = new UILabel();
			//	formDVC.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Lock"), UIBarButtonItemStyle.Bordered, delegate (object sender, EventArgs e)
			//  	{
			//		  //Create Alert
			//		  var BackPrompt = UIAlertController.Create("Exit Form", "Administrative use only. Please enter password to continue", UIAlertControllerStyle.Alert);
			//		  BackPrompt.AddTextField((field) =>
			//		  {
			//			  field.SecureTextEntry = true;
			//			  field.Placeholder = "Password";
			//		  });

			//		  BackPrompt.Add(messageLabel);
			//		  BackPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => PopBack(BackPrompt.TextFields[0].Text)));
			//		  BackPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

			//		  //Present Alert
			//		  PresentViewController(BackPrompt, true, null);
			//  	});
			//	//formDVC.NavigationItem.LeftBarButtonItem.Title = "Back";
			//}

			string jsonEnding = IsDoctorForm ? "doctor" : "patient";
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var directoryname = Path.Combine(documents, "DynaRestore");
			var filename = Path.Combine(directoryname, SelectedAppointment.ApptId + "_" + SelectedAppointment.SelectedQForm.FormId + "_" + jsonEnding + ".json");


			if (File.Exists(filename))
			{
				var restoreFile = File.ReadAllText(filename);
				string sourceJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
				JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(sourceJson);
				JObject targetJObject = JsonConvert.DeserializeObject<JObject>(restoreFile);

				if (!JToken.DeepEquals(sourceJObject, targetJObject))
				{
					messageLabel = new UILabel();
					formDVC.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Restore"), UIBarButtonItemStyle.Bordered, delegate (object sender, EventArgs e)
				  	{
						  //Create Alert
						  var RestorePrompt = UIAlertController.Create("Restore Form", "Administrative use only. Please enter password to restore", UIAlertControllerStyle.Alert);
						  RestorePrompt.AddTextField((field) =>
						  {
							  field.SecureTextEntry = true;
							  field.Placeholder = "Password";
						  });

						  RestorePrompt.Add(messageLabel);
						  RestorePrompt.AddAction(UIAlertAction.Create("Restore", UIAlertActionStyle.Default, action => RestoreForm(RestorePrompt.TextFields[0].Text, restoreFile, IsDoctorForm, sourceJObject, targetJObject)));
						  RestorePrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

						  //Present Alert
						  PresentViewController(RestorePrompt, true, null);
				  	});
				}
			}

			loadingOverlay.Hide();

			return formDVC;
		}


		void RestoreForm(string password, string restoreFile, bool IsDoctorForm, JObject sourceJObject, JObject targetJObject)
		{
			bool isValid = password == Constants.Password;
			if (isValid)
			{
				//if (DetailViewController.QuestionsView != null)
				//{
				//	DetailViewController.Title = "";
				//	DetailViewController.QuestionsView = null; //.Clear();
				//}
				if (!JToken.DeepEquals(sourceJObject, targetJObject))
				{
					JsonHandler.OriginalFormJsonString = restoreFile;
					SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(restoreFile);
					LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
				}
			}
			else
			{
				messageLabel.Text = "Wrong password, administrative use only";
				var FailAlert = UIAlertController.Create("Error", "Wrong password", UIAlertControllerStyle.Alert);
				FailAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
				// Present Alert
				PresentViewController(FailAlert, true, null);
			}
		}


		void PopBack(string password, bool IsDoctorForm)
		{
			bool isValid = password == Constants.Password;
			if (isValid)
			{
				//if (DetailViewController.QuestionsView != null)
				//{
					string jsonEnding = IsDoctorForm ? "doctor" : "patient";
					var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					var directoryname = Path.Combine(documents, "DynaRestore");
					var filename = Path.Combine(directoryname, SelectedAppointment.ApptId + "_" + SelectedAppointment.SelectedQForm.FormId + "_" + jsonEnding + ".json");

					string sourceJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);

					if (File.Exists(filename))
					{
						var restoreFile = File.ReadAllText(filename);
						JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(sourceJson);
						JObject targetJObject = JsonConvert.DeserializeObject<JObject>(restoreFile);

						if (!JToken.DeepEquals(sourceJObject, targetJObject))
						{
							// Serialize object
							//string restoreJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
							//string jsonEnding = IsDoctorForm ? "doctor" : "patient";
							// Save to file
							//var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
							//var directoryname = Path.Combine(documents, "DynaRestore");
							Directory.CreateDirectory(directoryname);
							//var filename = Path.Combine(directoryname, SelectedAppointment.ApptId + "_" + SelectedAppointment.SelectedQForm.FormId + "_" + jsonEnding + ".json");
							File.WriteAllText(filename, sourceJson);
						}

					}
					else
					{
						Directory.CreateDirectory(directoryname);
						File.WriteAllText(filename, sourceJson);
					}

					//DetailViewController.Title = "Welcome to DynaPad";
					DetailViewController.QuestionsView = null; //.Clear();
					DetailViewController.Root.Clear();
					DetailViewController.Root.Add(new Section("Logged in"));
					DetailViewController.Root.Caption = "Welcome to DynaPad";
					//DetailViewController.NavigationItem.SetLeftBarButtonItem(null, true);
					//DetailViewController.NavigationItem.SetRightBarButtonItems(null, true);
					DetailViewController.NavigationItem.LeftBarButtonItem = null;
					DetailViewController.NavigationItem.RightBarButtonItems = null;

					DetailViewController.ReloadData();
				//}

				NavigationController.PopViewController(true);
			}
			else
			{
				messageLabel.Text = "Wrong password, administrative use only";
				var FailAlert = UIAlertController.Create("Error", "Wrong password", UIAlertControllerStyle.Alert);
				FailAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
				// Present Alert
				PresentViewController(FailAlert, true, null);
			}
		}


		//void SaveFormPreset(string presetName)
		//{
		//	// doctorid = 123 / 321
		//	// locationid = 321 / 123

		//	string presetJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
		//	var dds = new DynaPadService.DynaPadService();
		//	dds.SaveAnswerPreset(SelectedAppointment.SelectedQForm.FormId, null, SelectedAppointment.ApptDoctorId, true, presetName, presetJson, SelectedAppointment.ApptLocationId, null);
		//}






		void SaveFormPreset(string presetId, string presetName, string sectionId, Section presetSection, PresetRadioElement pre, RadioGroup presetGroup, string origS, Section sectionFormSections, bool IsDoctorForm = true)
		{
			var sectionQuestions = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
			int fs = SelectedAppointment.SelectedQForm.FormSections.IndexOf(sectionQuestions);

			string presetJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
			var dds = new DynaPadService.DynaPadService();
			dds.SaveAnswerPreset(SelectedAppointment.SelectedQForm.FormId, null, SelectedAppointment.ApptDoctorId, true, presetName, presetJson, SelectedAppointment.ApptLocationId, presetId);

			if (presetId == null)
			{
				var mre = GetPreset(presetId, presetName, presetJson, fs, sectionId, presetGroup, sectionQuestions, presetSection, origS, sectionFormSections, IsDoctorForm);

				presetSection.Insert(presetSection.Count - 1, UITableViewRowAnimation.Fade, mre);
				presetSection.GetImmediateRootElement().RadioSelected = presetSection.Count - 2;

				presetSection.GetImmediateRootElement().Reload(presetSection, UITableViewRowAnimation.Fade);
			}
			else
			{
				presetSection.GetImmediateRootElement().RadioSelected = presetGroup.Selected;
				pre.PresetName = presetName;
				pre.Caption = presetName;
				//pre = GetPreset(presetId, presetName, presetJson, fs, sectionId, presetGroup, sectionQuestions, presetSection, origS, isDoctorInput, nextbtn);

				//pre.GetImmediateRootElement().Reload(pre, UITableViewRowAnimation.Fade);
				//var p = pre.Parent.Parent.Parent;
				//var pp = pre.Parent.Parent.Parent.Parent;
				//presetSection.GetImmediateRootElement().RadioSelected = 0;
				presetSection.GetImmediateRootElement().Reload(pre, UITableViewRowAnimation.Fade);
				//presetSection.GetImmediateRootElement().Reload(presetSection, UITableViewRowAnimation.Fade);
			}

			foreach (Element d in sectionFormSections.Elements)
			{
				var t = d.GetType();
				if (t == typeof(SectionStringElement))
				{
					var di = (SectionStringElement)d;
					if (di.selected == true)
					{
						di.selected = false;
					}
				}
			}

			var q = (SectionStringElement)sectionFormSections[1];
			q.selected = true;
			//presetSection.GetContainerTableView().RemoveFromSuperview();
			//QuestionsView.TableView.ReloadData();
			//SetDetailItem(new Section(sectionQuestions.SectionName), "", sectionId, origS, isDoctorInput, nextbtn);

			LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);

			NavigationController.PopViewController(true);
		}

		void DeleteFormPreset(string presetId, string presetName, string sectionId, Section presetSection, PresetRadioElement pre, RadioGroup presetGroup, string origS, bool IsDoctorForm = true)
		{
			var sectionQuestions = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
			int fs = SelectedAppointment.SelectedQForm.FormSections.IndexOf(sectionQuestions);

			string presetJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
			var dds = new DynaPadService.DynaPadService();
			dds.DeleteAnswerPreset(SelectedAppointment.SelectedQForm.FormId, null, SelectedAppointment.ApptDoctorId, presetId);

			//var mre = GetPreset(presetId, presetName, presetJson, fs, sectionId, presetGroup, sectionQuestions, presetSection, origS, isDoctorInput, nextbtn);

			//presetSection.Insert(presetSection.Count - 1, UITableViewRowAnimation.Automatic, mre);
			if (presetSection.GetImmediateRootElement().RadioSelected == pre.Index)
			{
				presetSection.GetImmediateRootElement().RadioSelected = 0;
			}
			presetSection.Remove(pre);
			//presetSection.GetImmediateRootElement().Reload(pre, UITableViewRowAnimation.Fade);
			presetSection.GetImmediateRootElement().Reload(presetSection, UITableViewRowAnimation.Fade);

			NavigationController.PopViewController(true);
		}

		public PresetRadioElement GetPreset(string presetId, string presetName, string presetJson, int fs, string sectionId, RadioGroup presetGroup, FormSection sectionQuestions, Section presetSection, string origS, Section sectionFormSections, bool IsDoctorForm)
		{
			var PatientId = SelectedAppointment.SelectedQForm.PatientId;
			var PatientName = SelectedAppointment.SelectedQForm.PatientName;
			var DoctorId = SelectedAppointment.SelectedQForm.DoctorId;
			var LocationId = SelectedAppointment.SelectedQForm.LocationId;
			var ApptId = SelectedAppointment.SelectedQForm.ApptId;
			var DateCompleted = SelectedAppointment.SelectedQForm.DateCompleted;
			var DateUpdated = SelectedAppointment.SelectedQForm.DateUpdated;

			var mre = new PresetRadioElement(presetName, "FormPresetAnswers");
			mre.PresetID = presetId;
			mre.PresetName = presetName;
			mre.PresetJson = presetJson;
			mre.OnSelected += delegate (object sender, EventArgs e)
			{
				SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(presetJson);

				SelectedAppointment.SelectedQForm.PatientId = PatientId;
				SelectedAppointment.SelectedQForm.PatientName = PatientName;
				SelectedAppointment.SelectedQForm.DoctorId = DoctorId;
				SelectedAppointment.SelectedQForm.LocationId = LocationId;
				SelectedAppointment.SelectedQForm.ApptId = ApptId;
				SelectedAppointment.SelectedQForm.DateCompleted = DateCompleted;
				SelectedAppointment.SelectedQForm.DateUpdated = DateUpdated;

				//var selectedSection = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
				//if (selectedSection != null)
				//{
				//	selectedSection.SectionSelectedTemplateId = presetGroup.Selected;
				//}
				var selectedSection = SelectedAppointment.SelectedQForm;
				if (selectedSection != null)
				{
					selectedSection.FormSelectedTemplateId = presetGroup.Selected;
				}

				//var ass = SelectedAppointment.SelectedQForm.FormSections[0];
				//var nextSectionIndex = new int();

				foreach (Element d in sectionFormSections.Elements)
				{
					var t = d.GetType();
					if (t == typeof(SectionStringElement))
					{
						var di = (SectionStringElement)d;
						if (di.selected == true)
						{
							di.selected = false;
						}
					}
				}

				var q = (SectionStringElement)sectionFormSections[1];
				q.selected = true;
				//sectionFormSections.GetContainerTableView().SelectRow(sectionFormSections.Elements[1].IndexPath, true, UITableViewScrollPosition.Top);
				//var shhh = sections.GetContainerTableView();
				sectionFormSections.GetContainerTableView().ReloadData();

				//dfElement.RadioSelected = 0;
				//dfElement.GetImmediateRootElement().RadioSelected = 0;
				//dfElement.GetImmediateRootElement().Reload(dfElement, UITableViewRowAnimation.Fade);
				//presetSection.GetImmediateRootElement().Reload(presetSection, UITableViewRowAnimation.Fade);
				LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
			};
			mre.editPresetBtn.TouchUpInside += (sender, e) =>
			{
				var UpdatePresetPrompt = UIAlertController.Create("Update Form Preset", "Overwriting preset '" + mre.PresetName + "', do you wish to continue?", UIAlertControllerStyle.Alert);
				//Add Actions
				UpdatePresetPrompt.AddTextField((field) =>
				{
					field.Placeholder = "Preset Name";
					field.Text = mre.PresetName;
				});
				UpdatePresetPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SaveFormPreset(mre.PresetID, UpdatePresetPrompt.TextFields[0].Text, sectionId, presetSection, mre, presetGroup, origS, sectionFormSections)));
				UpdatePresetPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
				//Present Alert

				PresentViewController(UpdatePresetPrompt, true, null);
			};
			mre.deletePresetBtn.TouchUpInside += (sender, e) =>
			{
				var UpdatePresetPrompt = UIAlertController.Create("Delete Form Preset", "Deleting preset '" + mre.PresetName + "', do you wish to continue?", UIAlertControllerStyle.Alert);
				//Add Actions
				UpdatePresetPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => DeleteFormPreset(mre.PresetID, mre.PresetName, sectionId, presetSection, mre, presetGroup, origS)));
				UpdatePresetPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
				//Present Alert

				PresentViewController(UpdatePresetPrompt, true, null);
			};

			return mre;
		}



		public bool ValidateSection(FormSection OrigSection)
		{
			var valid = true;

			foreach (SectionQuestion question in OrigSection.SectionQuestions)
			{
				switch (question.QuestionType)
				{
					case "BodyParts":
					case "Check":
						
						break;

					case "Radio":
					case "Bool":
					case "YesNo":
						
						break;

					case "TextView":
						
						break;

					case "TextInput":

						break;

					case "Date":

						break;

					case "Height":
					case "Weight":
					case "Amount":
					case "Numeric":
					case "Slider":

						break;
				}
			}

			return valid;
		}



		void LoadSectionView(string sectionId, string sectionName, FormSection OrigSection, bool IsDoctorForm, Section sections = null)
		{
			ReloadData();
			GlassButton btnNextSection = null;
			if (sectionName != "Report" || sectionName != "Finalize" || sectionName != "Photos")
			{
				btnNextSection = new GlassButton(new RectangleF(0, 0, (float)DetailViewController.View.Frame.Width, 50));
				//btnNextSection.Font = UIFont.BoldSystemFontOfSize(17);
				btnNextSection.TitleLabel.Font = UIFont.BoldSystemFontOfSize(17);
				btnNextSection.SetTitleColor(UIColor.Black, UIControlState.Normal);
				btnNextSection.NormalColor = UIColor.FromRGB(224, 238, 240);
				btnNextSection.SetTitle("Next Section", UIControlState.Normal);
				btnNextSection.TouchUpInside += (sender, e) =>
				{
					//if (ValidateSection(OrigSection))
					//{
						var nextSectionIndex = new int();

						foreach (Element d in sections.Elements)
						{
							var t = d.GetType();
							if (t == typeof(SectionStringElement))
							{
								var di = (SectionStringElement)d;
								if (di.selected == true)
								{
									nextSectionIndex = sections.Elements.IndexOf(di) + 1;
								}
								di.selected = false;
							}
						}

						var q = (SectionStringElement)sections.Elements[nextSectionIndex];
						q.selected = true;

						sections.GetContainerTableView().SelectRow(sections.Elements[nextSectionIndex].IndexPath, true, UITableViewScrollPosition.Top);
						//var shhh = sections.GetContainerTableView();
						sections.GetContainerTableView().ReloadData();

						if (IsDoctorForm)
						{
							nextSectionIndex = nextSectionIndex - 1;
						}

						if (q.Caption == "Finalize")
						{
							btnNextSection.SetTitle("Finalize", UIControlState.Normal);
							LoadSectionView("", "Finalize", null, IsDoctorForm);
						}
						else
						{
							var nextSectionQuestions = SelectedAppointment.SelectedQForm.FormSections[nextSectionIndex];
							string nextSectionJson = JsonConvert.SerializeObject(nextSectionQuestions);
							LoadSectionView(nextSectionQuestions.SectionId, nextSectionQuestions.SectionName, nextSectionQuestions, IsDoctorForm, sections);
						}

						string jsonEnding = IsDoctorForm ? "doctor" : "patient";
						var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
						var directoryname = Path.Combine(documents, "DynaRestore");
						var filename = Path.Combine(directoryname, SelectedAppointment.ApptId + "_" + SelectedAppointment.SelectedQForm.FormId + "_" + jsonEnding + ".json");

						string sourceJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);

						if (File.Exists(filename))
						{
							var restoreFile = File.ReadAllText(filename);
							JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(sourceJson);
							JObject targetJObject = JsonConvert.DeserializeObject<JObject>(restoreFile);

							if (!JToken.DeepEquals(sourceJObject, targetJObject))
							{
								// Serialize object
								//string restoreJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
								//string jsonEnding = IsDoctorForm ? "doctor" : "patient";
								// Save to file
								//var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
								//var directoryname = Path.Combine(documents, "DynaRestore");
								Directory.CreateDirectory(directoryname);
								//var filename = Path.Combine(directoryname, SelectedAppointment.ApptId + "_" + SelectedAppointment.SelectedQForm.FormId + "_" + jsonEnding + ".json");
								File.WriteAllText(filename, sourceJson);
							}

						}
						else
						{
							Directory.CreateDirectory(directoryname);
							File.WriteAllText(filename, sourceJson);
						}
					//}
				};

			}

			string origSectionJson = JsonConvert.SerializeObject(OrigSection);

			if (DetailViewController.NavigationController != null)
			{
				DetailViewController.NavigationController.PopViewController(true);
			}

			DetailViewController.SetDetailItem(new Section(sectionName), sectionName, sectionId, origSectionJson, IsDoctorForm, btnNextSection);
		}


		void LoadReportView(string valueId, string sectionName, RootElement rt) 		{
			NavigationController.TopViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Back"), UIBarButtonItemStyle.Plain, delegate (object sender, EventArgs e)
			{
				DetailViewController.NavigationItem.RightBarButtonItem = null;
				DetailViewController.Root.Clear();
				DetailViewController.Root.Add(new Section("Logged in"));
				DetailViewController.Root.Caption = "Welcome to DynaPad";
				DetailViewController.ReloadData();

				NavigationController.PopViewController(true);
			});
 			DetailViewController.SetDetailItem(new Section(sectionName), "Report", valueId, "", false, null); 		}

		void LoadSummaryView(string fileName, string sectionName, RootElement rt)
		{
			NavigationController.TopViewController.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Back"), UIBarButtonItemStyle.Plain, delegate (object sender, EventArgs e)
			{
				DetailViewController.NavigationItem.RightBarButtonItem = null;
				DetailViewController.Root.Clear();
				DetailViewController.Root.Add(new Section("Logged in"));
				DetailViewController.Root.Caption = "Welcome to DynaPad";
				DetailViewController.ReloadData();

				NavigationController.PopViewController(true);
			});

			DetailViewController.SetDetailItem(new Section(sectionName), "Summary", fileName, "", false, null, true, fileName);
		}


		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, Resources, etc that aren't in use.
		}


		void AddNewItem(object sender, EventArgs args)
		{
			//dataSource.Objects.Insert(0, DateTime.Now);
			using (var indexPath = NSIndexPath.FromRowSection(0, 0))
				TableView.InsertRows(new[] { indexPath }, UITableViewRowAnimation.Automatic);
		}


		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showDetail")
			{
				var indexPath = TableView.IndexPathForSelectedRow;
			}
		}


		class DataSource : UITableViewSource
		{
			static readonly NSString CellIdentifier = new NSString("Cell");
			//readonly List<Section> sectionItems = new List<Section>();
			readonly List<RootElement> MenuRoot = new List<RootElement>();
			readonly MasterViewController controller;
			//public DataSource(List<Section> sections, MasterViewController controller)
			public DataSource(RootElement menuRoot, MasterViewController controller)
			{
				MenuRoot.Add(menuRoot);
				this.controller = controller;
			}

			//public IList<Section> Objects
			//{
			//	get { return sectionItems; }
			//}
			public IList<RootElement> Objects
			{
				get { return MenuRoot; }
			}

			// Customize the number of sections in the table view.
			public override nint NumberOfSections(UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return MenuRoot.Count;
			}

			// Customize the appearance of table view cells.
			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell(CellIdentifier, indexPath);
				cell.TextLabel.Text = MenuRoot[indexPath.Row].Caption;

				return cell;
			}

			public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
			{
				// Return false if you do not want the specified item to be editable.
				return true;
			}

			public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete)
				{
					// Delete the row from the data source.
					MenuRoot.RemoveAt(indexPath.Row);
					controller.TableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
				}
				else if (editingStyle == UITableViewCellEditingStyle.Insert)
				{
					// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
				}
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				//if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
				//controller.DetailViewController.SetDetailItem(sectionItems[indexPath.Row]);
			}
		}
	}
}
