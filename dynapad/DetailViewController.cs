using System;
using System.Collections.Generic;
using MonoTouch.Dialog;
using Newtonsoft.Json;
using UIKit;
using Foundation;
using System.Drawing;
using CoreGraphics;
using AVFoundation;
using System.Diagnostics;
using System.IO;

namespace DynaPad
{
	public partial class DetailViewController : DialogViewController
	{
		public Section DetailItem { get; set; }
		public DynaMultiRootElement QuestionsView { get; set; }
		public DialogViewController mvc { get; set; }
		UILabel messageLabel;
		LoadingOverlay loadingOverlay;
		AVAudioSession session;
		AVAudioRecorder recorder;
		AVAudioPlayer player;
		Stopwatch stopwatch;
		NSUrl audioFilePath;
		NSObject observer;
		UILabel RecordingStatusLabel = new UILabel();
		UILabel LengthOfRecordingLabel = new UILabel();
		UILabel PlayRecordedSoundStatusLabel = new UILabel();
		UIButton StartRecordingButton = new UIButton();
		UIButton StopRecordingButton = new UIButton();
		UIButton PlayRecordedSoundButton = new UIButton();
		UIButton SaveRecordedSound = new UIButton();
		UIButton CancelRecording = new UIButton();
		public Menu DynaMenu { get; set; }

		protected DetailViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
			//this.TableView.CellLayoutMarginsFollowReadableWidth = false;
		}


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			Root.Caption = "Welcome to DynaPad";
			Root.Add(new Section("Login to the app"));

			base.TableView.CellLayoutMarginsFollowReadableWidth = false;
			// Perform any additional setup after loading the view, typically from a nib.
			base.TableView.ScrollsToTop = true;

			ModalInPopover = true;

			var tap = new UITapGestureRecognizer();
			tap.AddTarget(() =>
			{
				base.View.EndEditing(true);
			});
			base.View.AddGestureRecognizer(tap);
			tap.CancelsTouchesInView = false;
		}


		void SubmitForm(string password, bool isDoctorForm)
		{
			bool isValid = password == Constants.Password;
			if (isValid)
			{
				string finalJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
				//var dt = (DataTable)JsonConvert.DeserializeObject(finalJson, (typeof(DataTable)));
				//var table =  JsonConvert.DeserializeObject<DataTable>(finalJson);
				var dds = new DynaPadService.DynaPadService();
				dds.SubmitFormAnswers(finalJson, true, isDoctorForm);

				SetDetailItem(new Section("Summary"), "Summary", "", null, false);
			}
			else
			{
				messageLabel.Text = "Login failed";
				var FailAlert = UIAlertController.Create("Error", "Wrong password", UIAlertControllerStyle.Alert);
				FailAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
				// Present Alert
				PresentViewController(FailAlert, true, null);
			}
		}


		void Print(string jobname, UIWebView webView)
		{
			var printInfo = UIPrintInfo.PrintInfo;
			printInfo.OutputType = UIPrintInfoOutputType.General;
			printInfo.JobName = jobname;

			var printer = UIPrintInteractionController.SharedPrintController;
			printer.PrintInfo = printInfo;
			printer.PrintFormatter = webView.ViewPrintFormatter;
			printer.ShowsPageRange = true;
			printer.Present(true, (handler, completed, err) =>
			{
				if (!completed && err != null)
				{
					System.Console.WriteLine("error");
				}
			});
		}


		public void SetDetailItem(Section newDetailItem, string context, string valueId, string origSectionJson, bool IsDoctorForm, GlassButton nextbtn = null)
		{
			//var bounds = UIScreen.MainScreen.Bounds;
			var bounds = base.TableView.Frame;
			loadingOverlay = new LoadingOverlay(bounds);
			mvc = (DialogViewController)((UINavigationController)SplitViewController.ViewControllers[1]).TopViewController;
			mvc.Add(loadingOverlay);

			if (DetailItem != newDetailItem)
			{
				ReloadData();
				DetailItem = newDetailItem;
				switch (context)
				{
					case "Summary":
						var summaryElement = new DynaMultiRootElement(SelectedAppointment.ApptFormName);

						var summaryPaddedView = new PaddedUIView<UILabel>();
						summaryPaddedView.Enabled = true;
						summaryPaddedView.Type = "Section";
						summaryPaddedView.Frame = new CGRect(0, 0, 0, 40);
						summaryPaddedView.Padding = 5f;
						summaryPaddedView.NestedView.Text = "SUMARRY";
						summaryPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
						summaryPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
						summaryPaddedView.setStyle();

						var summarySection = new DynaSection("SUMARRY");
						summarySection.HeaderView = summaryPaddedView;
						summarySection.FooterView = new UIView(new CGRect(0, 0, 0, 0));
						summarySection.FooterView.Hidden = true;

						var dds = new DynaPadService.DynaPadService();
						string finalJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm);
						var summaryFileName = dds.ExportToPdf(finalJson);

						var webViews = new UIWebView(View.Bounds);
						webViews.Frame = new CGRect(View.Bounds.X, 0, View.Bounds.Width, View.Bounds.Height);
						//string localHtmlUrl = Path.Combine(NSBundle.MainBundle.BundlePath, summarypdf);
						string localHtmlUrl = Path.Combine("https://test.dynadox.pro/dynawcfservice/summaries/", summaryFileName);
						webViews.LoadRequest(new NSUrlRequest(new NSUrl("https://test.dynadox.pro/dynawcfservice/summaries/summary.pdf")));
						webViews.ScalesPageToFit = true;

						summarySection.Add(webViews);
						summaryElement.Add(summarySection);

						NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(UIImage.FromBundle("Print"), UIBarButtonItemStyle.Plain, (sender, args) =>
						{ Print(summaryFileName, webViews); }), true);

						Root = summaryElement;
						Root.TableView.ScrollEnabled = false;

						break;
					case "Finalize":
						var rootElement = new DynaMultiRootElement(SelectedAppointment.SelectedQForm.FormName);

						var rootPaddedView = new PaddedUIView<UILabel>();
						rootPaddedView.Enabled = true;
						rootPaddedView.Type = "Section";
						rootPaddedView.Frame = new CGRect(0, 0, 0, 40);
						rootPaddedView.Padding = 5f;
						rootPaddedView.NestedView.Text = "FINALIZE FORM";
						rootPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
						rootPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
						rootPaddedView.setStyle();

						var rootSection = new DynaSection("FINALIZE FORM");
						rootSection.HeaderView = rootPaddedView;
						rootSection.FooterView = new UIView(new CGRect(0, 0, 0, 0));
						rootSection.FooterView.Hidden = true;

						var sigPad = new SignaturePad.SignaturePadView(new CGRect(0, 0, View.Frame.Width, 600));
						sigPad.CaptionText = "Sign here:";
						sigPad.BackgroundColor = UIColor.White;

						messageLabel = new UILabel();

						var btnSubmit = new GlassButton(new RectangleF(0, 0, (float)View.Frame.Width, 50));
						//btnSubmit.Font = UIFont.BoldSystemFontOfSize(17);
						btnSubmit.TitleLabel.Font = UIFont.BoldSystemFontOfSize(17);
						btnSubmit.NormalColor = UIColor.Green;
						btnSubmit.DisabledColor = UIColor.Gray;
						btnSubmit.SetTitle("Submit Form", UIControlState.Normal);
						btnSubmit.TouchUpInside += (sender, e) =>
						{
							var SubmitPrompt = UIAlertController.Create("Submit Form", "Please hand back the IPad to submit", UIAlertControllerStyle.Alert);
							SubmitPrompt.AddTextField((field) =>
							{
								field.SecureTextEntry = true;
								field.Placeholder = "Password";
							});
							SubmitPrompt.Add(messageLabel);
							SubmitPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SubmitForm(SubmitPrompt.TextFields[0].Text, IsDoctorForm)));
							SubmitPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
							//Present Alert
							PresentViewController(SubmitPrompt, true, null);
						};

						rootSection.Add(sigPad);
						rootSection.Add(btnSubmit);
						rootElement.Add(rootSection);

						Root = rootElement;
						Root.TableView.ScrollEnabled = false;

						break;
					case "Report":
						//var Report = SelectedAppointment.ApptReports.Find((Report obj) => obj.FormId == sectionId);

						var reportElement = new DynaMultiRootElement(SelectedAppointment.ApptFormName);

						var reportPaddedView = new PaddedUIView<UILabel>();
						reportPaddedView.Enabled = true;
						reportPaddedView.Type = "Section";
						reportPaddedView.Frame = new CGRect(0, 0, 0, 40);
						reportPaddedView.Padding = 5f;
						reportPaddedView.NestedView.Text = "the report"; //Report.ReportName.ToUpper();
						reportPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
						reportPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
						reportPaddedView.setStyle();

						var reportSection = new DynaSection("REPORT");
						reportSection.HeaderView = reportPaddedView;
						reportSection.FooterView = new UIView(new CGRect(0, 0, 0, 0));
						reportSection.FooterView.Hidden = true;

						var bb = View.Frame;
						var webView = new UIWebView(View.Bounds);
						webView.Frame = new CGRect(View.Bounds.X, 0, View.Bounds.Width, View.Bounds.Height);

						var dps = new DynaPadService.DynaPadService();
						string reportUrl = dps.GenerateReport(SelectedAppointment.ApptId, SelectedAppointment.ApptFormId, DateTime.Today.ToShortDateString(), "file", valueId);
						//string report = dps.GenerateReport("123", SelectedQForm.ApptPatientID, DateTime.Today.ToShortDateString(), "file", SelectedQForm.ApptPatientFormID);
						//var asdf = SelectedAppointment.ApptPatientId;

						//var myurl = "https://test.dynadox.pro/dynawcfservice/" + report; // NOTE: https secure request
						//var myurl = "https://test.dynadox.pro/dynawcfservice/test.pdf";// + report; // NOTE: https secure request
						//var url = "https://www.princexml.com/samples/invoice/invoicesample.pdf"; // NOTE: https secure request
						webView.LoadRequest(new NSUrlRequest(new NSUrl(reportUrl)));
						webView.ScalesPageToFit = true;

						reportSection.Add(webView);
						reportElement.Add(reportSection);

						NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(UIImage.FromBundle("Print"), UIBarButtonItemStyle.Plain, (sender, args) =>
						{ Print(SelectedAppointment.ApptFormName, webView); }), true);

						Root = reportElement;
						Root.TableView.ScrollEnabled = false;

						break;
					default:
						// Update the view
						ConfigureView(valueId, origSectionJson, IsDoctorForm, nextbtn);
						break;
				}
			}

			loadingOverlay.Hide();
		}


		void ConfigureView(string sectionId, string origS, bool IsDoctorForm, GlassButton nextbtn)
		{
			// Update the user interface for the detail item
			if (DetailItem != null)
			{
				var sectionQuestions = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);

				var headPaddedView = new PaddedUIView<UILabel>();
				headPaddedView.Enabled = true;
				headPaddedView.Type = "Section";
				headPaddedView.Frame = new CGRect(0, 0, 0, 40);
				headPaddedView.Padding = 5f;
				headPaddedView.NestedView.Text = sectionQuestions.SectionName.ToUpper();
				headPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
				headPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
				headPaddedView.setStyle();

				var headSection = new DynaSection(sectionQuestions.SectionName);
				headSection.HeaderView = headPaddedView;
				headSection.FooterView = new UIView(new CGRect(0, 0, 0, 0));
				headSection.FooterView.Hidden = true;

				if (IsDoctorForm)
				{
					NavigationItem.SetRightBarButtonItem(
					new UIBarButtonItem(UIImage.FromBundle("Dictation"), UIBarButtonItemStyle.Plain, (sender, args) =>
					{
						audioFilePath = null;

						CancelRecording.Frame = new CGRect(0, 0, 350, 50);
						CancelRecording.SetTitle("Close", UIControlState.Normal);
						CancelRecording.SetTitleColor(UIColor.Black, UIControlState.Normal);
						//CancelRecording.TouchUpInside += OnCancelRecording;

						var clab = new UILabel(new CGRect(0, 0, 160, 50));
						clab.TextAlignment = UITextAlignment.Center;
						clab.Text = "Dictation";

						//var segDict = new UISegmentedControl();
						//segDict.Frame = new CGRect(0, 0, 350, 50);
						//segDict.Momentary = true;
						//segDict.InsertSegment(UIImage.FromBundle("Delete"), 0, true);
						//segDict.InsertSegment("Dictation", 1, true);
						//segDict.SetWidth(50, 0);
						//segDict.SetWidth(324, 1);

						var cellHeader = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellHeader.Frame = new CGRect(0, 0, 350, 50);
						//cellHeader.ImageView.Image = UIImage.FromBundle("Close");

						var headclosebtn = new UIButton(new CGRect(0, 0, 50, 50));
						headclosebtn.SetImage(UIImage.FromBundle("Close"), UIControlState.Normal);

						cellHeader.ContentView.Add(headclosebtn);
						cellHeader.ContentView.Add(clab);

						var cellFooter = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellFooter.Frame = new CGRect(0, 0, 350, 50);
						//cellHeader.ImageView.Image = UIImage.FromBundle("Close");
						cellFooter.ContentView.Add(CancelRecording);


						var sec = new Section(cellHeader, cellFooter);
						sec.FooterView.Frame = new CGRect(0, 0, 350, 50);

						RecordingStatusLabel.Text = string.Empty;
						RecordingStatusLabel.Frame = new CGRect(210, 0, 120, 50);

						LengthOfRecordingLabel.Text = string.Empty;
						LengthOfRecordingLabel.Frame = new CGRect(210, 0, 120, 50);

						StartRecordingButton.Frame = new CGRect(20, 0, 160, 50);
						StartRecordingButton.TouchUpInside += OnStartRecording;
						StartRecordingButton.SetTitle("Start Recording", UIControlState.Normal);
						StartRecordingButton.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);

						StopRecordingButton.Frame = new CGRect(20, 0, 160, 50);
						StopRecordingButton.SetTitle("Stop Recording", UIControlState.Normal);
						StopRecordingButton.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						StopRecordingButton.TouchUpInside += OnStopRecording;
						StopRecordingButton.Enabled = false;
						StopRecordingButton.Alpha = (nfloat)0.5;

						PlayRecordedSoundButton.Frame = new CGRect(20, 0, 160, 50);
						PlayRecordedSoundButton.SetTitle("Play Recording", UIControlState.Normal);
						PlayRecordedSoundButton.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						PlayRecordedSoundButton.TouchUpInside += OnPlayRecordedSound;
						PlayRecordedSoundButton.Enabled = false;
						PlayRecordedSoundButton.Alpha = (nfloat)0.5;

						SaveRecordedSound.Enabled = false; 
						SaveRecordedSound.Alpha = (nfloat)0.5;
						SaveRecordedSound.Frame = new CGRect(20, 0, 160, 50);
						SaveRecordedSound.SetTitle("Save Recording", UIControlState.Normal);
						SaveRecordedSound.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						SaveRecordedSound.TouchUpInside += delegate
						{
							OnSaveRecordedSound(sectionId, sec);
						};

						observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnDidPlayToEndTime);

						var cellRecord = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellRecord.Frame = new CGRect(0, 0, 350, 50);
						cellRecord.ImageView.Image = UIImage.FromBundle("Record");
						cellRecord.ContentView.Add(StartRecordingButton);
						cellRecord.ContentView.Add(RecordingStatusLabel);

						sec.Add(cellRecord);

						var cellStop = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellStop.Frame = new CGRect(0, 0, 350, 50);
						cellStop.ImageView.Image = UIImage.FromBundle("Stop");
						cellStop.ContentView.Add(StopRecordingButton);
						cellStop.ContentView.Add(LengthOfRecordingLabel);

						sec.Add(cellStop);

						var cellPlay = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellPlay.Frame = new CGRect(0, 0, 350, 50);
						cellPlay.ImageView.Image = UIImage.FromBundle("Play");
						cellPlay.ContentView.Add(PlayRecordedSoundButton);

						sec.Add(cellPlay);

						var cellSave = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellSave.Frame = new CGRect(0, 0, 350, 50);
						cellSave.ImageView.Image = UIImage.FromBundle("Save");
						cellSave.ContentView.Add(SaveRecordedSound);

						sec.Add(cellSave);

						var dps = new DynaPadService.DynaPadService();
						var dictations = dps.GetFormDictations(SelectedAppointment.SelectedQForm.FormId, sectionId, SelectedAppointment.ApptDoctorId, true, SelectedAppointment.SelectedQForm.LocationId);

						foreach (string[] dictation in dictations)
						{
							byte[] bytes = Convert.FromBase64String(dictation[2]);
							NSData dataDictation = NSData.FromArray(bytes);
							NSError err;
							var dicplayer = new AVAudioPlayer(dataDictation, "aac", out err);

							var duration = TimeSpan.FromSeconds(dicplayer.Duration).ToString(@"hh\:mm\:ss");

							var statusLabel = new UILabel(new CGRect(210, 0, 120, 50));
							statusLabel.Text = duration;

							var PlaySavedDictationButton = new UIButton();
							PlaySavedDictationButton.Frame = new CGRect(0, 0, 160, 50);
							PlaySavedDictationButton.SetTitle(dictation[1], UIControlState.Normal);
							PlaySavedDictationButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
							//PlaySavedDictationButton.SetImage(UIImage.FromBundle("CircledPlay"), UIControlState.Normal);
							PlaySavedDictationButton.TouchUpInside += delegate
							{
								OnPlaySavedDictation(dictation[1], dictation[2], dicplayer, statusLabel);
							};

							var cellDict = new UITableViewCell(UITableViewCellStyle.Default, null);
							cellDict.Frame = new CGRect(0, 0, 350, 50);
							cellDict.BackgroundColor = UIColor.LightGray;
							cellDict.ImageView.Image = UIImage.FromBundle("CircledPlay");
							cellDict.ContentView.Add(PlaySavedDictationButton);
							cellDict.ContentView.Add(statusLabel);

							sec.Add(cellDict);
						}

						var roo = new RootElement("Dictation");
						roo.Add(sec);

						var dia = new DialogViewController(roo);

						var vie = new UIView();

						var con = new UIViewController();

						con.Add(vie);

						var popHeight = sec.Elements.Count > 8 ? 500 : sec.Elements.Count * 50 + 100;

						var pop = new UIPopoverController(dia);
						pop.PopoverContentSize = new CGSize(350, popHeight);
						pop.ShouldDismiss = (popoverController) => false;
						pop.DidDismiss += delegate
						{
							//AVAudioSession.SharedInstance().Dispose();
							session.Dispose();
							session = null;

							observer.Dispose();
							observer = null;
							//recorder.Dispose();
							stopwatch = null;
							recorder = null;
							player = null;
							pop.Dispose();
							pop = null;
							audioFilePath = null;
						};

						//segDict.ValueChanged += (s, e) =>
						//{
						//	if (segDict.SelectedSegment == 0)
						//	{
						//		pop.Dismiss(true);
						//	}
						//};

						CancelRecording.TouchUpInside += delegate
						{ pop.Dismiss(true); };

						headclosebtn.TouchUpInside += delegate
						{ pop.Dismiss(true); };

						pop.PresentFromBarButtonItem(NavigationItem.RightBarButtonItem, UIPopoverArrowDirection.Unknown, true);

					}), true);

					/*
					 * TODO: make presets password protected (maybe not needed since is for doctor only?)! (maybe component: Passcode)
					*/

					var presetPaddedView = new PaddedUIView<UILabel>();
					presetPaddedView.Enabled = true;
					presetPaddedView.Type = "Preset";
					presetPaddedView.Frame = new CGRect(0, 0, 0, 30);
					presetPaddedView.Padding = 5f;
					presetPaddedView.NestedView.Text = "Preset Answers";
					presetPaddedView.setStyle();

					var presetSection = new DynaSection("Preset Answers");
					presetSection.Enabled = true;
					presetSection.HeaderView = presetPaddedView;
					presetSection.FooterView = new UIView(new CGRect(0, 0, 0, 0));
					presetSection.FooterView.Hidden = true;

					int fs = SelectedAppointment.SelectedQForm.FormSections.IndexOf(sectionQuestions);

					var dds = new DynaPadService.DynaPadService();
					var FormPresetNames = dds.GetAnswerPresets(SelectedAppointment.SelectedQForm.FormId, sectionId, SelectedAppointment.ApptDoctorId, true, SelectedAppointment.ApptLocationId);

					var presetGroup = new RadioGroup("PresetAnswers", sectionQuestions.SectionSelectedTemplateId);
					var presetsRoot = new DynaRootElement("Preset Answers", presetGroup);
					presetsRoot.IsPreset = true;

					var noPresetRadio = new MyRadioElement("No Preset", "PresetAnswers");
					noPresetRadio.OnSelected += delegate (object sender, EventArgs e)
					{
						string presetJson = origS;
						SelectedAppointment.SelectedQForm.FormSections[fs] = JsonConvert.DeserializeObject<FormSection>(presetJson);
						var selectedSection = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
						if (selectedSection != null)
						{
							selectedSection.SectionSelectedTemplateId = presetGroup.Selected;
						}

						SetDetailItem(new Section(sectionQuestions.SectionName), "", sectionId, origS, IsDoctorForm, nextbtn);
					};

					presetSection.Add(noPresetRadio);

					foreach (string[] arrPreset in FormPresetNames)
					{
						var mre = new MyRadioElement(arrPreset[1], "PresetAnswers");
						mre.OnSelected += delegate (object sender, EventArgs e)
						{
							string presetJson = arrPreset[2];
							SelectedAppointment.SelectedQForm.FormSections[fs] = JsonConvert.DeserializeObject<FormSection>(presetJson);
							var selectedSection = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
							if (selectedSection != null)
							{
								selectedSection.SectionSelectedTemplateId = presetGroup.Selected;
							}

							SetDetailItem(new Section(sectionQuestions.SectionName), "", sectionId, origS, IsDoctorForm, nextbtn);
						};

						presetSection.Add(mre);
					}

					var btnNewSectionPreset = new GlassButton(new RectangleF(0, 0, (float)View.Frame.Width, 50));
					//btnNewSectionPreset.Font = UIFont.BoldSystemFontOfSize(17);
					btnNewSectionPreset.TitleLabel.Font = UIFont.BoldSystemFontOfSize(17);
					btnNewSectionPreset.SetTitleColor(UIColor.Black, UIControlState.Normal);
					btnNewSectionPreset.NormalColor = UIColor.FromRGB(224, 238, 240);
					btnNewSectionPreset.SetTitle("Save New Section Preset", UIControlState.Normal);
					btnNewSectionPreset.TouchUpInside += (sender, e) =>
					{
						var SavePresetPrompt = UIAlertController.Create("New Section Preset", "Enter preset name: ", UIAlertControllerStyle.Alert);
						SavePresetPrompt.AddTextField((field) =>
						{
							field.Placeholder = "Preset Name";
						});
						//Add Actions
						SavePresetPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SaveSectionPreset(SavePresetPrompt.TextFields[0].Text, sectionId, presetSection, presetGroup, origS, nextbtn)));
						SavePresetPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
						//Present Alert
						PresentViewController(SavePresetPrompt, true, null);

					};

					presetSection.Add(btnNewSectionPreset);

					presetsRoot.Add(presetSection);
					presetsRoot.Enabled = true;

					headSection.Add(presetsRoot);
				}
				else
				{
					NavigationItem.SetRightBarButtonItem(null, false);
				}

				QuestionsView = new DynaMultiRootElement(SelectedAppointment.SelectedQForm.FormName);
				QuestionsView.Add(headSection);

				foreach (SectionQuestion question in sectionQuestions.SectionQuestions)
				{
					bool enabled = !question.IsConditional || (question.IsConditional && question.IsEnabled);
					var qSection = new DynaSection(question.QuestionText);
					qSection.QuestionId = question.QuestionId;
					qSection.Enabled = enabled;

					nfloat qWidth = IsDoctorForm ? View.Frame.Width - 50 : 0;
					UITableViewCell cellQ = null;

					if (IsDoctorForm)
					{
						cellQ = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellQ.Frame = new CGRect(0, 0, View.Frame.Width, 30);
					}

					var qPaddedView = new PaddedUIView<UILabel>();
					qPaddedView.Enabled = enabled;
					qPaddedView.Frame = new CGRect(0, 0, qWidth, 30);
					qPaddedView.Padding = 5f;
					qPaddedView.NestedView.Text = question.QuestionText.ToUpper();
					qPaddedView.Type = "Question";
					qPaddedView.setStyle();

					if (cellQ != null)
					{
						var qDictationButton = new UIButton(new CGRect(View.Frame.GetMaxX() - 50, 0, 50, 30));
						qDictationButton.Enabled = enabled;
						qDictationButton.SetImage(UIImage.FromBundle("QRecord"), UIControlState.Normal);
						if (qDictationButton.Enabled)
						{
							qDictationButton.BackgroundColor = UIColor.FromRGB(230, 230, 250);
						}
						else
						{
							qDictationButton.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
						}

						cellQ.ContentView.Add(qPaddedView);
						cellQ.ContentView.Add(qDictationButton);

						qSection.HeaderView = cellQ;
					}
					else
					{
						qSection.HeaderView = qPaddedView;
					}

					qSection.FooterView = new UIView(new CGRect(0, 0, 0, 0));
					qSection.FooterView.Hidden = true;

					switch (question.QuestionType)
					{
						case "BodyParts":
						case "Check":
							
							foreach (QuestionOption opt in question.QuestionOptions)
							{
								var chk = new DynaCheckBoxElement(opt.OptionText, false, opt.OptionId);
								chk.Enabled = enabled;
								chk.ConditionTriggerId = question.ParentConditionTriggerId;
								chk.Value = opt.Chosen;
								chk.Tapped += delegate
								{
									Chk_Tapped(question, opt, chk.Value, sectionId);
								};

								if (opt.Chosen && opt.ConditionTriggerIds != null && opt.ConditionTriggerIds.Count > 0)
								{
									ConditionalCheck(null, opt.ConditionTriggerIds, sectionId);
								}

								qSection.Add(chk);
							}

							QuestionsView.Add(qSection);

							break;
							
						case "Radio":
						case "Bool":
						case "YesNo":
							
							foreach (QuestionOption opt in question.QuestionOptions)
							{
								var radio = new DynaMultiRadioElement(opt.OptionText, question.QuestionId);
								radio.Enabled = enabled;
								radio.Chosen = opt.Chosen;
								radio.ConditionTriggerId = question.ParentConditionTriggerId;
								radio.ElementSelected += delegate
								{
									Radio_Tapped(question, opt);
									NavigationController.PopViewController(true);
								};
								radio.OnDeselected += delegate
								{
									Radio_UnTapped(question, opt);
									NavigationController.PopViewController(true);
								};

								qSection.Add(radio);

								if (opt.Chosen)
								{
									QuestionsView.Select(question.QuestionId, question.QuestionOptions.IndexOf(opt));

									if (opt.ConditionTriggerIds != null && opt.ConditionTriggerIds.Count > 0)
									{
										if (question.ActiveTriggerIds == null)
										{
											question.ActiveTriggerIds = new List<string>();
										}
										question.ActiveTriggerIds.AddRange(opt.ConditionTriggerIds);
										ConditionalCheck(null, opt.ConditionTriggerIds, sectionId);
									}
								}
							}

							QuestionsView.Add(qSection);

							break;
							
						case "TextInput":
							
							var entryElement = new DynaEntryElement("", "Enter your answer here", question.AnswerText);

							switch (question.QuestionKeyboardType)
							{
								case "numeric":
									entryElement.KeyboardType = UIKeyboardType.NumberPad;
									break;
								case "email":
									entryElement.KeyboardType = UIKeyboardType.EmailAddress;
									break;
								case "decimal":
									entryElement.KeyboardType = UIKeyboardType.DecimalPad;
									break;
								case "phone":
									entryElement.KeyboardType = UIKeyboardType.PhonePad;
									break;
								default:
									entryElement.KeyboardType = UIKeyboardType.Default;
									break;
							}

							entryElement.Enabled = enabled;
							entryElement.QuestionId = question.QuestionId;
							entryElement.ConditionTriggerId = question.ParentConditionTriggerId;
							entryElement.EntryEnded += (sender, e) => { question.AnswerText = entryElement.Value; };

							qSection.Add(entryElement);

							QuestionsView.Add(qSection);

							break;
							
						case "Date":
							
							var dt = new DateTime();
							dt = !string.IsNullOrEmpty(question.AnswerText) ? Convert.ToDateTime(question.AnswerText) : DateTime.Today;
							//dt.ToUniversalTime();

							var dateElement = new NullableDateElementInline("", dt);
							dateElement.Caption = "Tap to select date";
							dateElement.Enabled = enabled;
							dateElement.Alignment = UITextAlignment.Left;
							dateElement.QuestionId = question.QuestionId;
							dateElement.ConditionTriggerId = question.ParentConditionTriggerId;
							dateElement.DateSelected += delegate
							{
								question.AnswerText = dateElement.DateValue.Value.ToShortDateString();
							};

							qSection.Add(dateElement);

							QuestionsView.UnevenRows = true;

							QuestionsView.Add(qSection);

							break;
							
						case "Height":
						case "Weight":
						case "Amount":
						case "Numeric":
						case "Slider":

							/*
							 *  TODO: options: UIStepper, Slider, Segmented Controls
							 * custom: migueldeicaza CounterElement
							 * components: BetterPickers, 
							 * have types: percent, currency, decimal, etc....
							*/

							float questionmin = 0;
							float questionmax = 20;
							float qanswer = 0;

							switch (question.QuestionType)
							{
								case "Height":
									questionmax = 12;
									break;
								case "Weight":
									questionmax = 350;
									break;
							}

							if (!string.IsNullOrEmpty(question.MinValue))
							{
								questionmin = Convert.ToInt32(question.MinValue);
							}

							if (!string.IsNullOrEmpty(question.MaxValue))
							{
								questionmax = Convert.ToInt32(question.MaxValue);
							}

							if (!string.IsNullOrEmpty(question.AnswerText))
							{
								qanswer = Convert.ToInt32(question.AnswerText);
							}

							var sliderElement = new DynaSlider(qanswer, question);
							sliderElement.MinValue = questionmin;
							sliderElement.MaxValue = questionmax;
							sliderElement.ShowCaption = true;
							sliderElement.Caption = qanswer.ToString();
							sliderElement.Enabled = enabled;
							sliderElement.QuestionId = question.QuestionId;
							sliderElement.ConditionTriggerId = question.ParentConditionTriggerId;

							qSection.Add(sliderElement);

							QuestionsView.UnevenRows = true;

							QuestionsView.Add(qSection);

							break;
					}
				}

				var qNext = new DynaSection("Next");
				qNext.HeaderView = new UIView(new CGRect(0, 0, 0, 10));
				qNext.FooterView = new UIView(new CGRect(0, 0, 0, 10));
				qNext.Add(nextbtn);

				QuestionsView.Add(qNext);

				Root = QuestionsView;
				Root.TableView.ScrollEnabled = true;
				Root.TableView.ScrollsToTop = true;
				Root.TableView.ScrollRectToVisible(new CGRect(0, 0, 1, 1), true);
			}
		}


		void Chk_Tapped(SectionQuestion cQuestion, QuestionOption cOption, bool selected, string sectionId)
		{
			//string newTriggerId = cOption.ConditionTriggerIds;
			List<string> newTriggerIds = cOption.ConditionTriggerIds;
			cOption.Chosen = selected;

			MultiConditionalCheck(cQuestion, sectionId);

			QuestionsView.TableView.ReloadData();
		}


		void Radio_Tapped(SectionQuestion rQuestion, QuestionOption rOption)
		{
			//string newTriggerId = rOption.ConditionTriggerIds;
			List<string> newTriggerIds = rOption.ConditionTriggerIds;
			rQuestion.QuestionOptions.ForEach((obj) => obj.Chosen = false);
			rOption.Chosen = true;

			ConditionalCheck(rQuestion.ActiveTriggerIds, newTriggerIds, rQuestion.SectionId);

			rQuestion.ActiveTriggerIds = newTriggerIds;

			QuestionsView.TableView.ReloadData();
		}
		void Radio_UnTapped(SectionQuestion rQuestion, QuestionOption rOption)
		{
			//string newTriggerId = rOption.ConditionTriggerIds;
			List<string> newTriggerIds = new List<string>();
			rQuestion.QuestionOptions.ForEach((obj) => obj.Chosen = false);
			rOption.Chosen = false;

			ConditionalCheck(rQuestion.ActiveTriggerIds, newTriggerIds, rQuestion.SectionId);

			rQuestion.ActiveTriggerIds = newTriggerIds;

			QuestionsView.TableView.ReloadData();
		}


		void Boolean_Changed(SectionQuestion bQuestion, List<string> activeTriggerId, List<string> newTriggerIds, bool selected)
		{
			if (selected)
			{
				bQuestion.QuestionOptions[0].Chosen = true;
				bQuestion.QuestionOptions[1].Chosen = false;
			}
			else
			{
				bQuestion.QuestionOptions[0].Chosen = false;
				bQuestion.QuestionOptions[1].Chosen = true;
			}

			ConditionalCheck(activeTriggerId, newTriggerIds, bQuestion.SectionId);

			bQuestion.ActiveTriggerIds = newTriggerIds;
		}


		private void ConditionalCheck(List<string> activeTriggerIds, List<string> newTriggerIds, string sectionId)
		{
			if (activeTriggerIds != newTriggerIds)
			{
				var sectionQuestions = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);

				if (activeTriggerIds != null && activeTriggerIds.Count > 0 && !string.IsNullOrEmpty(activeTriggerIds[0]))
				{
					var untriggeredQuestions = sectionQuestions.SectionQuestions.FindAll(((obj) => activeTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId) && !string.IsNullOrEmpty(((dynamic)obj).ParentConditionTriggerId)));

					TriggerCheck(untriggeredQuestions, false, sectionId);
				}

				if (newTriggerIds != null && newTriggerIds.Count > 0 && !string.IsNullOrEmpty(newTriggerIds[0]))
				{
					var triggeredQuestions = sectionQuestions.SectionQuestions.FindAll(((obj) => newTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId) && !string.IsNullOrEmpty(((dynamic)obj).ParentConditionTriggerId)));

					TriggerCheck(triggeredQuestions, true, sectionId);
				}
			}
		}


		private void MultiConditionalCheck(SectionQuestion activeQuestion, string sectionId)
		{
			var sectionQuestions = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
			var untriggeredQuestions = new List<SectionQuestion>();
			var triggeredQuestions = new List<SectionQuestion>();
			foreach (QuestionOption qOption in activeQuestion.QuestionOptions)
			{
				if (qOption.Chosen)
				{
					if (qOption.ConditionTriggerIds != null && qOption.ConditionTriggerIds.Count > 0 && !string.IsNullOrEmpty(qOption.ConditionTriggerIds[0]))
					{
						triggeredQuestions.AddRange(sectionQuestions.SectionQuestions.FindAll(((obj) => qOption.ConditionTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId))));
					}
				}
				else
				{
					if (qOption.ConditionTriggerIds != null && qOption.ConditionTriggerIds.Count > 0 && !string.IsNullOrEmpty(qOption.ConditionTriggerIds[0]))
					{
						untriggeredQuestions.AddRange(sectionQuestions.SectionQuestions.FindAll(((obj) => qOption.ConditionTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId))));
					}
				}
			}

			TriggerCheck(untriggeredQuestions, false, sectionId);

			TriggerCheck(triggeredQuestions, true, sectionId);

		}


		private void TriggerCheck(List<SectionQuestion> triggerQuestions, bool triggered, string sectionId)
		{
			foreach (SectionQuestion tQuestion in triggerQuestions)
			{
				if (tQuestion.QuestionOptions != null)
				{
					foreach (QuestionOption tOption in tQuestion.QuestionOptions)
					{
						if (tOption.ConditionTriggerIds != null && tOption.ConditionTriggerIds.Count > 0 && !string.IsNullOrEmpty(tOption.ConditionTriggerIds[0]) && tOption.Chosen)
						{
							List<string> optionTriggerIds = tQuestion.ActiveTriggerIds;

							if (optionTriggerIds != null && optionTriggerIds.Count > 0 && !string.IsNullOrEmpty(optionTriggerIds[0]))
							{
								if (triggered)
								{
									ConditionalCheck(null, tOption.ConditionTriggerIds, sectionId);
								}
								else
								{
									ConditionalCheck(tOption.ConditionTriggerIds, null, sectionId);
								}
							}
						}

					}
				}

				tQuestion.IsEnabled = triggered;

				foreach (DynaSection sec in QuestionsView)
				{
					if (sec.QuestionId == tQuestion.QuestionId)
					{
						var headerLabel = (PaddedUIView<UILabel>)sec.HeaderView;
						if (headerLabel != null)
						{
							headerLabel.Enabled = triggered;
							headerLabel.setStyle();
						}

						foreach (dynamic element in sec.Elements)
						{
							if (element != null)
							{
								element.Enabled = triggered;

								if (element.GetContainerTableView() != null)
								{
									element.GetContainerTableView().ReloadData();
								}
							}
						}

						sec.Enabled = triggered;

						break;
					}
				}
			}
		}


		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}


		void OnStopRecording(object sender, EventArgs e)
		{
			if (recorder == null)
				return;

			recorder.Stop();
			stopwatch.Stop();

			LengthOfRecordingLabel.Text = string.Format("{0:hh\\:mm\\:ss}", stopwatch.Elapsed);
			RecordingStatusLabel.Text = "";
			StartRecordingButton.Enabled = true;
			StartRecordingButton.Alpha = 1;
			StopRecordingButton.Enabled = false;
			StopRecordingButton.Alpha = (nfloat)0.5;
			PlayRecordedSoundButton.Enabled = true;
			PlayRecordedSoundButton.Alpha = 1;
			SaveRecordedSound.Enabled = true;
			SaveRecordedSound.Alpha = 1;
		}


		void OnStartRecording(object sender, EventArgs e)
		{
			System.Console.WriteLine("Begin Recording");

			session = AVAudioSession.SharedInstance();

			NSError error = null;
			session.SetCategory(AVAudioSession.CategoryRecord, out error);
			if (error != null)
			{
				System.Console.WriteLine(error);
				return;
			}

			session.SetActive(true, out error);
			if (error != null)
			{
				System.Console.WriteLine(error);
				return;
			}

			if (!PrepareAudioRecording())
			{
				RecordingStatusLabel.Text = "Error preparing";
				return;
			}

			if (!recorder.Record())
			{
				RecordingStatusLabel.Text = "Error preparing";
				return;
			}

			stopwatch = new Stopwatch();
			stopwatch.Start();

			LengthOfRecordingLabel.Text = "";
			RecordingStatusLabel.Text = "Recording";
			StartRecordingButton.Enabled = false;
			StartRecordingButton.Alpha = (nfloat)0.5;
			StopRecordingButton.Enabled = true;
			StopRecordingButton.Alpha = 1;
			PlayRecordedSoundButton.Enabled = false;
			PlayRecordedSoundButton.Alpha = (nfloat)0.5;
			SaveRecordedSound.Enabled = false;
			SaveRecordedSound.Alpha = (nfloat)0.5;
		}


		NSUrl CreateOutputUrl()
		{
			string fileName = string.Format("Myfile{0}.aac", DateTime.Now.ToString("yyyyMMddHHmmss"));
			string tempRecording = Path.Combine(Path.GetTempPath(), fileName);
			//string tempRecording = Path.Combine(Environment.GetFolderPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), fileName);
			return NSUrl.FromFilename(tempRecording);
		}


		void OnDidPlayToEndTime(object sender, NSNotificationEventArgs e)
		{
			player.Dispose();
			player = null;
		}


		void OnPlayRecordedSound(object sender, EventArgs e)
		{
			try
			{
				System.Console.WriteLine("Playing Back Recording {0}", audioFilePath);

				// The following line prevents the audio from stopping
				// when the device autolocks. will also make sure that it plays, even
				// if the device is in mute
				NSError error = null;
				AVAudioSession.SharedInstance().SetCategory(AVAudioSession.CategoryPlayback, out error);
				if (error != null)
					throw new Exception(error.DebugDescription);
				NSError audioError;
				//player = new AVPlayer(audioFilePath);
				player = new AVAudioPlayer(audioFilePath, "aac", out audioError);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("There was a problem playing back audio: ");
				System.Console.WriteLine(ex.Message);
			}
		}


		void OnPlaySavedDictation(string title, string dictationBytes, AVAudioPlayer pplayer, UILabel statusLabel)
		{
			try
			{
				System.Console.WriteLine("Playing Back Recording {0}", title);

				// The following line prevents the audio from stopping
				// when the device autolocks. will also make sure that it plays, even
				// if the device is in mute
				NSError error = null;
				AVAudioSession.SharedInstance().SetCategory(AVAudioSession.CategoryPlayback, out error);
				if (error != null)
					throw new Exception(error.DebugDescription);
				//byte[] bytes = Convert.FromBase64String(dictationBytes);
				//NSData dataDictation = NSData.FromArray(bytes);
				//NSError audioError;

				statusLabel.Text = "Playing";

				//player = new AVAudioPlayer(dataDictation, "aac", out audioError);
				//player.FinishedPlaying += (se, ea) => { PlayRecordedSoundStatusLabel.Text = string.Format("{0:hh\\:mm\\:ss}", player.Data.Length); };
				//player.Play();

				var duration = TimeSpan.FromSeconds(pplayer.Duration).ToString(@"hh\:mm\:ss");

				pplayer.FinishedPlaying += (se, ea) => { statusLabel.Text = duration; };
				pplayer.Play();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("There was a problem playing back audio: ");
				System.Console.WriteLine(ex.Message);
			}
		}


		bool PrepareAudioRecording()
		{
			audioFilePath = CreateOutputUrl();

			var audioSettings = new AudioSettings
			{
				SampleRate = 44100,
				Format = AudioToolbox.AudioFormatType.MPEG4AAC,
				NumberChannels = 1,
				AudioQuality = AVAudioQuality.High
			};

			//Set recorder parameters
			NSError error;
			recorder = AVAudioRecorder.Create(audioFilePath, audioSettings, out error);
			if (error != null)
			{
				System.Console.WriteLine(error);
				return false;
			}

			//Set Recorder to Prepare To Record
			try
			{
				if (!recorder.PrepareToRecord())
				{
					recorder.Dispose();
					recorder = null;
					return false;
				}
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("record error: " + ex.Message);
			}

			recorder.FinishedRecording += OnFinishedRecording;

			return true;
		}


		void OnFinishedRecording(object sender, AVStatusEventArgs e)
		{
			recorder.Dispose();
			recorder = null;
			System.Console.WriteLine("Done Recording (status: {0})", e.Status);
		}


		protected override void Dispose(bool disposing)
		{
			observer.Dispose();
			base.Dispose(disposing);
		}


		void OnSaveRecordedSound(string sectionId, Section dicSec)
		{
			var dictationData = NSData.FromUrl(audioFilePath); //the path here can be a path to a video on the camera roll
			var dictationArray = dictationData.ToArray();
			try
			{
				var dds = new DynaPadService.DynaPadService();
				//DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();


				string dictationPath = dds.SaveDictation(SelectedAppointment.SelectedQForm.FormId, sectionId, SelectedAppointment.ApptDoctorId, true, SelectedAppointment.SelectedQForm.LocationId, "Roy_" + DateTime.Now.ToShortTimeString(), dictationArray);
				System.Console.WriteLine("Saving Recording {0}", audioFilePath);

				var dps = new DynaPadService.DynaPadService();
				var dictations = dps.GetFormDictations(SelectedAppointment.SelectedQForm.FormId, sectionId, SelectedAppointment.ApptDoctorId, true, SelectedAppointment.SelectedQForm.LocationId);
				dicSec.RemoveRange(4, dicSec.Elements.Count - 4);

				foreach (string[] dictation in dictations)
				{
					byte[] bytes = Convert.FromBase64String(dictation[2]);
					NSData dataDictation = NSData.FromArray(bytes);
					NSError err;
					var dicplayer = new AVAudioPlayer(dataDictation, "aac", out err);

					var duration = TimeSpan.FromSeconds(dicplayer.Duration).ToString(@"hh\:mm\:ss");

					var statusLabel = new UILabel(new CGRect(210, 0, 120, 50));
					statusLabel.Text = duration;

					var PlaySavedDictationButton = new UIButton();
					PlaySavedDictationButton.Frame = new CGRect(0, 0, 160, 50);
					PlaySavedDictationButton.SetTitle(dictation[1], UIControlState.Normal);
					PlaySavedDictationButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
					//PlaySavedDictationButton.SetImage(UIImage.FromBundle("CircledPlay"), UIControlState.Normal);
					PlaySavedDictationButton.TouchUpInside += delegate
					{
						OnPlaySavedDictation(dictation[1], dictation[2], dicplayer, statusLabel);
					};

					var cellDict = new UITableViewCell(UITableViewCellStyle.Default, null);
					cellDict.Frame = new CGRect(0, 0, 350, 50);
					cellDict.BackgroundColor = UIColor.LightGray;
					cellDict.ImageView.Image = UIImage.FromBundle("CircledPlay");
					cellDict.ContentView.Add(PlaySavedDictationButton);
					cellDict.ContentView.Add(statusLabel);

					dicSec.Add(cellDict);
				}

			}
			catch (Exception ex)
			{
				System.Console.WriteLine("There was a problem saving audio: ");
				System.Console.WriteLine(ex.Message);
			}
		}


		void OnCancelRecording(object sender, EventArgs e)
		{
			try
			{
				System.Console.WriteLine("Canceled Recording");
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("There was a problem canceling audio: ");
				System.Console.WriteLine(ex.Message);
			}
		}

		void SaveSectionPreset(string presetName, string sectionId, Section presetSection, RadioGroup presetGroup, string origS, GlassButton nextbtn, bool isDoctorInput = true)
		{
			// doctorid = 123 / 321
			// locationid = 321 / 123

			var sectionQuestions = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
			int fs = SelectedAppointment.SelectedQForm.FormSections.IndexOf(sectionQuestions);

			string presetJson = JsonConvert.SerializeObject(SelectedAppointment.SelectedQForm.FormSections[fs]);
			var dds = new DynaPadService.DynaPadService();
			dds.SaveAnswerPreset(SelectedAppointment.SelectedQForm.FormId, sectionId, SelectedAppointment.ApptDoctorId, true, presetName, presetJson, SelectedAppointment.ApptLocationId);



			var mre = new MyRadioElement(presetName, "PresetAnswers");
			mre.OnSelected += delegate (object sender, EventArgs e)
			{
				SelectedAppointment.SelectedQForm.FormSections[fs] = JsonConvert.DeserializeObject<FormSection>(presetJson);
				var selectedSection = SelectedAppointment.SelectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
				if (selectedSection != null)
				{
					selectedSection.SectionSelectedTemplateId = presetGroup.Selected;
				}

				SetDetailItem(new Section(sectionQuestions.SectionName), "", sectionId, origS, isDoctorInput, nextbtn);
			};

			presetSection.Insert(presetSection.Count - 1, UITableViewRowAnimation.Automatic, mre);
			presetSection.GetImmediateRootElement().RadioSelected = presetSection.Count - 2;

		}
	}
}
