using System;
using System.Collections.Generic;

using UIKit;
using Foundation;
using CoreGraphics;
using MonoTouch.Dialog;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;

namespace DynaPad
{
	public partial class MasterViewController : DialogViewController
	{
		public DetailViewController DetailViewController { get; set; }

		//DataSource dataSource;

		protected MasterViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Title = NSBundle.MainBundle.LocalizedString("Sections", "Form Sections");

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			{
				//PreferredContentSize = new CGSize(320f, 600f);
				ClearsSelectionOnViewWillAppear = false;
			}

			DetailViewController = (DetailViewController)((UINavigationController)SplitViewController.ViewControllers[1]).TopViewController;
			DetailViewController.Style = UITableViewStyle.Plain;

			//DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
			//string menuJson = dds.GetMenuJson("76", "5", "", false);
			//JsonHandler.OriginalFormJsonString = menuJson;

			//string menuJson = File.ReadAllText("SampleMenu.json");
			string menuJson = File.ReadAllText("MenuD.json");

			Menu myMenu = JsonConvert.DeserializeObject<Menu>(menuJson);
			DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();

			RootElement rootMainMenu = new RootElement(myMenu.MenuCaption);
			Section sectionMainMenu = new Section();
			sectionMainMenu.HeaderView = null;
			//sectionMainMenu.Caption = "Menu";
			BuildMenu(myMenu, sectionMainMenu);
			rootMainMenu.Add(sectionMainMenu);
			Root = rootMainMenu;

		}


		private RootElement BuildMenu(Menu myMenu, Section sectionMenu)
		{
			if (myMenu.MenuItems == null) return null;
			foreach (MenuItem mItem in myMenu.MenuItems)
			{
				//RootElement rootMenu = new RootElement(mItem.MenuItemCaption);
				DynaFormRootElement rootMenu = new DynaFormRootElement(mItem.MenuItemCaption);

				rootMenu.Enabled = true;
				rootMenu.FormID = mItem.MenuItemValue;
				rootMenu.FormName = mItem.MenuItemCaption;
				rootMenu.MenuAction = mItem.MenuItemAction;
				rootMenu.MenuValue = mItem.MenuItemValue;
				rootMenu.PatientID = mItem.PatientId;
				rootMenu.DoctorID = mItem.DoctorId;
				rootMenu.LocationID = mItem.LocationId;
				rootMenu.ApptID = mItem.ApptId;


				if (mItem.MenuItemAction == "GetDoctorForm") { rootMenu.IsDoctorForm = true; } else { rootMenu.IsDoctorForm = false; }

				switch (mItem.MenuItemAction)
				{
					case "GetForm":
					case "GetDoctorForm":
						rootMenu.createOnSelected = GetFormService;
					break;
					case "GetAppt":
						
						rootMenu.createOnSelected = GetApptService;
						//DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
						//string origJson = dds.GetFormQuestions(rootMenu.FormID, "5", "", rootMenu.IsDoctorForm);
						//JsonHandler.OriginalFormJsonString = origJson;
						//SelectedQForm.selectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);
						//var gAppt = new StringElement(mItem.MenuItemCaption, delegate { LoadAppt(mItem.MenuItemValue, "5", "", false, mItem); });
						//sectionMenu.Add(gAppt);
					//	RootElement rootAppt = new RootElement(mItem.MenuItemCaption);

						//sectionAppt.HeaderView = null;
						//sectionMainMenu.Caption = "Menu";
						//foreach (Menu m in mItem.Menus)
						//{
						//	Section sectionAppt = new Section();
						//	BuildMenu(m, sectionAppt);
						//	//rootAppt.Add(sectionAppt);
						//	//Root.Add(rootAppt);
						//	//rootMenu.Add(sectionAppt);
						//}
						//Section sectionAppt = new Section();
						//BuildMenu(mItem.Menus[0], sectionAppt);
						break;
					case "GetReport":
						//var gReport = new StringElement("Report", delegate { LoadSectionView("Report", "Report", null, rootMenu.IsDoctorForm); });
						//sectionMenu.Add(gReport);
						rootMenu.createOnSelected = GetReportService;
						break;
						//continue;
				}

				sectionMenu.Add(rootMenu);
				if (mItem.Menus == null) return null;
				foreach (Menu mRoot in mItem.Menus)
				{
					Section newSection = new Section();
					BuildMenu(mRoot, newSection);
					rootMenu.Add(newSection);
				}

			}
			return null;
		}

		public void LoadAppt(string formid, string clientid, string appt, bool isdocform, MenuItem mi)
		{
			DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
			string origJson = dds.GetFormQuestions(formid, clientid, appt, isdocform);
			JsonHandler.OriginalFormJsonString = origJson;
			SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

			Section sectionAppt = new Section();
			BuildMenu(mi.Menus[0], sectionAppt);
		}

		public UIViewController GetReportService(RootElement rElement)
		{
			DynaFormRootElement dfElemet = (DynaFormRootElement)rElement;
			//bool IsDoctorForm = dfElemet.IsDoctorForm;
			//var ShortForm = SelectedAppointment.ApptShortForms.Find((QShortForm obj) => obj.FormId == dfElemet.MenuValue);
			DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
			string origJson = dds.GetShortForms(dfElemet.FormID, dfElemet.DoctorID, false);
			//string origJson = dds.GetFormQuestions(((DynaFormRootElement)rElement).FormID, ((DynaFormRootElement)rElement).PatientID, ((DynaFormRootElement)rElement).ApptID, ((DynaFormRootElement)rElement).IsDoctorForm);
			JsonHandler.OriginalFormJsonString = origJson;
			RootElement rootShortForms = new RootElement(dfElemet.FormName);
			Section sectionShortForms = new Section();
			foreach (QShortForm esf in SelectedAppointment.ApptShortForms)
			{
				//SelectedAppointment.ApptShortForms.Add(esf);
				sectionShortForms.Add(new StringElement(esf.ShortFormName, delegate { LoadSectionView(esf.ShortFormId, "Report", null, false); }));
			}
			//SelectedAppointment.ApptShortForms.Add = dfElemet.FormID;
			//SelectedAppointment.ApptFormName = dfElemet.FormName;
			//SelectedAppointment.ApptPatientId = dfElemet.PatientID;
			//SelectedAppointment.ApptDoctorId = dfElemet.DoctorID;
			//SelectedAppointment.ApptLocationId = dfElemet.LocationID;
			//SelectedAppointment.ApptId = dfElemet.ApptID;
			//return new DialogViewController(UITableViewStyle.Plain, rElement, true);

			//foreach (QShortForm fShortForm in SelectedAppointment.ApptShortForms)
			//{
			//	sectionShortForms.Add(new StringElement(fShortForm.ShortFormName, delegate { LoadSectionView(fShortForm.ShortFormId, "Report", null, false); }));
			//}
			rootShortForms.Add(sectionShortForms);
			//this.Root.Add(sectionFormSections);
			//DialogViewController newDVC = new DialogViewController(this.Root);
			//DialogViewController formDVC = new DialogViewController(UITableViewStyle.Plain, rootFormSections, true);
			DialogViewController formDVC = new DialogViewController(rootShortForms, true);
			//formDVC.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem()
			return formDVC;
		}

		public UIViewController GetApptService(RootElement rElement)
		{
			//DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
			//string origJson = dds.GetFormQuestions(((DynaFormRootElement)rElement).FormID, "5", "", ((DynaFormRootElement)rElement).IsDoctorForm);
			//JsonHandler.OriginalFormJsonString = origJson;
			//SelectedQForm.selectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);
			DynaFormRootElement dfElemet = (DynaFormRootElement)rElement;

			SelectedAppointment.ApptFormId = dfElemet.FormID;
			SelectedAppointment.ApptFormName = dfElemet.FormName;
			SelectedAppointment.ApptPatientId = dfElemet.PatientID;
			SelectedAppointment.ApptDoctorId = dfElemet.DoctorID;
			SelectedAppointment.ApptLocationId = dfElemet.LocationID;
			SelectedAppointment.ApptId = dfElemet.ApptID;
			//return new DialogViewController(UITableViewStyle.Plain, rElement, true);
			return new DialogViewController(rElement, true);
		}

		public UIViewController GetFormService(RootElement rElement)
		{
			DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
			DynaFormRootElement dfElemet = (DynaFormRootElement)rElement;
			string origJson = dds.GetFormQuestions(dfElemet.FormID, dfElemet.PatientID, dfElemet.ApptID, dfElemet.IsDoctorForm);
			//string origJson = dds.GetFormQuestions(((DynaFormRootElement)rElement).FormID, ((DynaFormRootElement)rElement).PatientID, ((DynaFormRootElement)rElement).ApptID, ((DynaFormRootElement)rElement).IsDoctorForm);
			JsonHandler.OriginalFormJsonString = origJson;
			SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

			//string origJson = File.ReadAllText("SampleForm.json");
			//JsonHandler.OriginalFormJsonString = origJson;

			//this.Style = UITableViewStyle.Plain;
			//SelectedQForm.selectedQForm = JsonConvert.DeserializeObject<QForm>(origJson);

			RootElement rootFormSections = new RootElement(SelectedAppointment.SelectedQForm.FormName);
			Section sectionFormSections = new Section();

			bool IsDoctorForm = dfElemet.IsDoctorForm;

			if (IsDoctorForm)
			{

				/*
				 * TODO: make presets password protected! (maybe component: Passcode)
				 * TODO: add 'clear' selection option! ??? DONE ???
				*/


				//string[,] FormPresetNames = dds.GetFormPresets("35", "5", origJson);
				var FormPresetNames = dds.GetAnswerPresets(SelectedAppointment.ApptFormId, null, SelectedAppointment.ApptPatientId, true, SelectedAppointment.ApptLocationId);

				DynaSection formPresetSection = new DynaSection("Form Preset Answers");
				formPresetSection.Enabled = true;

				RadioGroup formPresetGroup = new RadioGroup("FormPresetAnswers", SelectedAppointment.SelectedQForm.FormSelectedTemplateId);
				DynaRootElement formPresetsRoot = new DynaRootElement("Preset Answers", formPresetGroup);
				formPresetsRoot.IsPreset = true;
				//int fpnCount = FormPresetNames.GetLength(0) - 1;
				//for (int i = 0; i <= fpnCount; i++)
				foreach(string[] arrPreset in FormPresetNames)
				{
					var radioPreset = new MyRadioElement(arrPreset[1], "FormPresetAnswers");
					//string fafa = FormPresetNames[formPresetGroup.Selected, 1];
					radioPreset.OnSelected += delegate (object sender, EventArgs e)
					{
						string presetJson = arrPreset[2];
						JsonHandler.OriginalFormJsonString = presetJson;
						//string ppp = presetJson;
						SelectedAppointment.SelectedQForm = JsonConvert.DeserializeObject<QForm>(presetJson);
						LoadSectionView(SelectedAppointment.SelectedQForm.FormSections[0].SectionId, SelectedAppointment.SelectedQForm.FormSections[0].SectionName, SelectedAppointment.SelectedQForm.FormSections[0], IsDoctorForm);
					};

					formPresetSection.Add(radioPreset);
				}


				GlassButton btnNewFormPreset = new GlassButton(new RectangleF(0, 0, (float)View.Frame.Width, 50));
				btnNewFormPreset.Font = UIFont.BoldSystemFontOfSize(17);
				btnNewFormPreset.SetTitleColor(UIColor.Black, UIControlState.Normal);
				btnNewFormPreset.NormalColor = UIColor.FromRGB(224, 238, 240);
				btnNewFormPreset.SetTitle("Save New Form Preset", UIControlState.Normal);
				btnNewFormPreset.TouchUpInside += (sender, e) =>
				{
				/*
				 * TODO: popup to enter preset name
				*/

				//Create Alert
				var SavePresetPrompt = UIAlertController.Create("New Form Preset", "Necesito name", UIAlertControllerStyle.Alert);
				SavePresetPrompt.AddTextField((field) =>
				{
					field.Placeholder = "Preset Name";
				});

				//Add Actions
				SavePresetPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SavePreset(SavePresetPrompt.TextFields[0].Text)));//alert => Console.WriteLine("Okay was clicked")));
				SavePresetPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));//alert => Console.WriteLine("Cancel was clicked")));

				//Present Alert
				PresentViewController(SavePresetPrompt, true, null);
				};

				formPresetSection.Add(btnNewFormPreset);


				formPresetsRoot.Add(formPresetSection);
				formPresetsRoot.Enabled = true;
				//formPresetSection.GetContainerTableView().BackgroundColor = UIColor.FromRGB(169, 188, 208);
				sectionFormSections.Add(formPresetsRoot);
			}



			foreach (FormSection fSection in SelectedAppointment.SelectedQForm.FormSections)
			{
				sectionFormSections.Add(new StringElement(fSection.SectionName, delegate { LoadSectionView(fSection.SectionId, fSection.SectionName, fSection, IsDoctorForm); }));
			}
			sectionFormSections.Add(new StringElement("Finalize", delegate { LoadSectionView("Finalize", "Finalize", null, IsDoctorForm); }));
			rootFormSections.Add(sectionFormSections);
			//this.Root.Add(sectionFormSections);
			//DialogViewController newDVC = new DialogViewController(this.Root);
			//DialogViewController formDVC = new DialogViewController(UITableViewStyle.Plain, rootFormSections, true);
			DialogViewController formDVC = new DialogViewController(rootFormSections, true);
			//formDVC.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem()
			return formDVC;
		}

		void SavePreset(string presetName)
		{
			string presetJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
			// doctorid = 123 / 321
			// locationid = 321 / 123
			DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
			dds.SaveAnswerPreset(SelectedAppointment.SelectedQForm.FormId, null, SelectedAppointment.SelectedQForm.DoctorId, true, presetName, presetJson, SelectedAppointment.SelectedQForm.LocationId); 
		}


		void LoadSectionView(string sectionId, string sectionName, FormSection OrigSection, bool IsDoctorForm)
		{
			string origSectionJson = JsonConvert.SerializeObject(OrigSection);
			DetailViewController.SetDetailItem(new Section(sectionName), sectionId, origSectionJson, IsDoctorForm);
			//this.Style = UITableViewStyle.Plain;
			//DetailViewController.Style = UITableViewStyle.Plain;

			//var controller = (DetailViewController)((UINavigationController)segue.DestinationViewController).TopViewController;
			////controller.SetDetailItem(item, indexPath.Row);
			//controller.NavigationItem.LeftBarButtonItem = SplitViewController.DisplayModeButtonItem;
			//controller.NavigationItem.LeftItemsSupplementBackButton = true;
			//DetailViewController.SetDetailItem(fSection, sIndex);
			//return "success";
		}


		public void GetReportService(RootElement rElement, bool IsDoctorForm)
		{
			DetailViewController.SetDetailItem(new Section(SelectedAppointment.SelectedQForm.FormName), "Report", null, IsDoctorForm);
		}


		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
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

				//var item = dataSource.Objects[indexPath.Row];
				//var controller = (DetailViewController)((UINavigationController)segue.DestinationViewController).TopViewController;
				////controller.SetDetailItem(item, indexPath.Row);
				//controller.NavigationItem.LeftBarButtonItem = SplitViewController.DisplayModeButtonItem;
				//controller.NavigationItem.LeftItemsSupplementBackButton = true;
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
				//sectionItems = sections;
				//foreach (Section fSection in menuRoot[0].Elements[0] as RootElement)
				//{
				//	sectionItems.Add(fSection);
				//}

				//controller.Style = UITableViewStyle.Plain;

				MenuRoot.Add(menuRoot);
				this.controller = controller;

				//this.controller.Style = UITableViewStyle.Plain;
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


