using System;
using System.Collections.Generic;
using UIKit;
using Foundation;
using MonoTouch.Dialog;
using Newtonsoft.Json;
using System.Drawing;

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
			Title = "Welcome to DynaPad";
		}


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = NSBundle.MainBundle.LocalizedString("Menu", "Form Sections");

			ClearsSelectionOnViewWillAppear &= UIDevice.CurrentDevice.UserInterfaceIdiom != UIUserInterfaceIdiom.Pad;

			DetailViewController = (DetailViewController)((UINavigationController)SplitViewController.ViewControllers[1]).TopViewController;
			DetailViewController.Style = UITableViewStyle.Plain;

			// TODO pull to refresh: (problem scrolling with it..)
			//DetailViewController.RefreshRequested += delegate
			//{
			//	DetailViewController.ReloadComplete();
			//};

			var dds = new DynaPadService.DynaPadService();

			//var menu = dds.BuildDynaMenu("123");
			//var menuObj = JsonConvert.DeserializeObject<Menu>(dds.BuildDynaMenu("123"));

			myDynaMenu = JsonConvert.DeserializeObject<Menu>(dds.BuildDynaMenu("123"));
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
						sectionMenu.Add(new StringElement(mItem.MenuItemCaption, delegate { LoadReportView(mItem.MenuItemValue, "Report"); }));
					//rootMenu.createOnSelected = GetReportService;
					//Section sectionReport = new Section();

					//sectionReport.Add(new StringElement(rootMenu.MenuValue, delegate { LoadReportView("Report", rootMenu.MenuValue); }));

					//rootMenu.Add(sectionReport);
						break;
				}
				if (mItem.MenuItemAction != "GetReport")
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
			if (DetailViewController.QuestionsView != null)
			{
				DetailViewController.Title = "";
				DetailViewController.QuestionsView = null; //.Clear();
			}

			var dfElemet = (DynaFormRootElement)rElement;
			SelectedAppointment.ApptFormId = dfElemet.FormID;
			SelectedAppointment.ApptFormName = dfElemet.FormName;
			SelectedAppointment.ApptPatientId = dfElemet.PatientID;
			SelectedAppointment.ApptDoctorId = dfElemet.DoctorID;
			SelectedAppointment.ApptLocationId = dfElemet.LocationID;
			SelectedAppointment.ApptId = dfElemet.ApptID;

			return new DynaDialogViewController(rElement, true);
		}


		public UIViewController GetReportService(RootElement rElement)
		{
			if (DetailViewController.QuestionsView != null)
			{
				DetailViewController.Title = "";
				DetailViewController.QuestionsView = null; //.Clear();
			}

			var dfElemet = (DynaFormRootElement)rElement;
			//var ShortForm = SelectedAppointment.ApptShortForms.Find((QShortForm obj) => obj.FormId == dfElemet.MenuValue);
			var dds = new DynaPadService.DynaPadService();
			string origJson = dds.GetShortForms(dfElemet.FormID, dfElemet.DoctorID, false);
			JsonHandler.OriginalFormJsonString = origJson;
			var rootReports = new RootElement(dfElemet.FormName);
			var sectionReports = new Section();

			foreach (Report esf in SelectedAppointment.ApptReports)
			{
				sectionReports.Add(new StringElement(esf.ReportName, delegate { LoadSectionView(esf.ReportId, "Report", null, false); }));
			}

			rootReports.Add(sectionReports);

			var formDVC = new DynaDialogViewController(rootReports, true);

			return formDVC;
		}


		public UIViewController GetFormService(RootElement rElement)
		{
			if (DetailViewController.QuestionsView != null)
			{
				DetailViewController.Title = "";
				DetailViewController.QuestionsView = null; //.Clear();
			}

			//var bounds = UIScreen.MainScreen.Bounds;
			var bounds = base.TableView.Frame;
			// show the loading overlay on the UI thread using the correct orientation sizing
			loadingOverlay = new LoadingOverlay(bounds);
			//mvc = (DialogViewController)((UINavigationController)SplitViewController.ViewControllers[0]).TopViewController;
			SplitViewController.Add(loadingOverlay);
			var dds = new DynaPadService.DynaPadService();
			var dfElemet = (DynaFormRootElement)rElement;
			string origJson = dds.GetFormQuestions(dfElemet.FormID, dfElemet.PatientID, SelectedAppointment.ApptId, dfElemet.IsDoctorForm);
			JsonHandler.OriginalFormJsonString = origJson;
			SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

			var sectionsGroup = new RadioGroup("sections", -1);
			var rootFormSections = new RootElement(SelectedAppointment.SelectedQForm.FormName, sectionsGroup);

			var sectionFormSections = new Section();

			bool IsDoctorForm = dfElemet.IsDoctorForm;
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
				formPresetsRoot.IsPreset = true;


				var noPresetRadio = new MyRadioElement("No Preset", "FormPresetAnswers");
				noPresetRadio.OnSelected += delegate (object sender, EventArgs e)
				{
					JsonHandler.OriginalFormJsonString = origJson;
					SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

					LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
				};
				formPresetSection.Add(noPresetRadio);


				foreach(string[] arrPreset in FormPresetNames)
				{
					var radioPreset = new MyRadioElement(arrPreset[1], "FormPresetAnswers");
					radioPreset.OnSelected += delegate (object sender, EventArgs e)
					{
						string presetJson = arrPreset[2];
						JsonHandler.OriginalFormJsonString = presetJson;
						SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(presetJson);
						LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
					};

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
					SavePresetPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SaveFormPreset(SavePresetPrompt.TextFields[0].Text)));
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
				var section = new SectionStringElement(fSection.SectionName, delegate { 
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
					var shhh = sectionFormSections.GetContainerTableView();
					sectionFormSections.GetContainerTableView().ReloadData();
				});

				sectionFormSections.Add(section);
				//sectionFormSections.Add(new StringElement(fSection.SectionName, delegate { LoadSectionView(fSection.SectionId, fSection.SectionName, fSection, IsDoctorForm); }));
			}

			var finalizeSection = new SectionStringElement("Finalize", delegate {
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

			if (!IsDoctorForm)
			{
				messageLabel = new UILabel();
				formDVC.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIImage.FromBundle("Lock"), UIBarButtonItemStyle.Bordered, delegate (object sender, EventArgs e)
			  	{
				  //Create Alert
				  var BackPrompt = UIAlertController.Create("Exit Form", "Administrative use only. Please enter password to continue", UIAlertControllerStyle.Alert);
				  BackPrompt.AddTextField((field) =>
				  {
					  field.SecureTextEntry = true;
					  field.Placeholder = "Password";
				  });

				  BackPrompt.Add(messageLabel);
				  BackPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => PopBack(BackPrompt.TextFields[0].Text)));
				  BackPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

				  //Present Alert
				  PresentViewController(BackPrompt, true, null);
			  	});
				//formDVC.NavigationItem.LeftBarButtonItem.Title = "Back";
			}

			loadingOverlay.Hide();

			return formDVC;
		}


		void PopBack(string password)
		{
			bool isValid = password == Constants.Password;
			if (isValid)
			{
				if (DetailViewController.QuestionsView != null)
				{
					DetailViewController.Title = "";
					DetailViewController.QuestionsView = null; //.Clear();
				}

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


		void SaveFormPreset(string presetName)
		{
			// doctorid = 123 / 321
			// locationid = 321 / 123

			string presetJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
			var dds = new DynaPadService.DynaPadService();
			dds.SaveAnswerPreset(SelectedAppointment.SelectedQForm.FormId, null, SelectedAppointment.ApptDoctorId, true, presetName, presetJson, SelectedAppointment.ApptLocationId); 
		}


		void LoadSectionView(string sectionId, string sectionName, FormSection OrigSection, bool IsDoctorForm, Section sections = null)
		{
			GlassButton btnNextSection = null;
			if (sectionName != "Report" || sectionName != "Finalize")
			{
				btnNextSection = new GlassButton(new RectangleF(0, 0, (float)DetailViewController.View.Frame.Width, 50));
				//btnNextSection.Font = UIFont.BoldSystemFontOfSize(17);
				btnNextSection.TitleLabel.Font = UIFont.BoldSystemFontOfSize(17);
				btnNextSection.SetTitleColor(UIColor.Black, UIControlState.Normal);
				btnNextSection.NormalColor = UIColor.FromRGB(224, 238, 240);
				btnNextSection.SetTitle("Next Section", UIControlState.Normal);
				btnNextSection.TouchUpInside += (sender, e) =>
				{
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

						//DetailViewController.SetDetailItem(new Section(nextSectionQuestions.SectionName), nextSectionQuestions.SectionName, nextSectionQuestions.SectionId, nextSectionJson, IsDoctorForm, btnNextSection);
						LoadSectionView(nextSectionQuestions.SectionId, nextSectionQuestions.SectionName, nextSectionQuestions, IsDoctorForm, sections);
					}
				};
			}

			string origSectionJson = JsonConvert.SerializeObject(OrigSection);
			DetailViewController.SetDetailItem(new Section(sectionName), sectionName, sectionId, origSectionJson, IsDoctorForm, btnNextSection);
		}


		void LoadReportView(string valueId, string sectionName) 		{ 			DetailViewController.SetDetailItem(new Section(sectionName), "Report", valueId, "", false, null); 		}


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
