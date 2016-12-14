using System;
using System.Collections.Generic;
using System.Json;
using MonoTouch.Dialog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.CSharp;
using UIKit;
using Foundation;
using System.Drawing;
using CoreGraphics;
using FloatLabeledEntry;
using System.Data;
using AVFoundation;
using System.Diagnostics;
using System.IO;
using MaterialControls;

namespace DynaPad
{
	public partial class DetailViewController : DialogViewController
	{
		public Section DetailItem { get; set; }
		//public RootElement QuestionsView { get; set; }
		public DynaMultiRootElement QuestionsView { get; set; }

		protected DetailViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
			//this.TableView.CellLayoutMarginsFollowReadableWidth = false;
		}


		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			base.TableView.CellLayoutMarginsFollowReadableWidth = false;
			//this.TableView.CellLayoutMarginsFollowReadableWidth = false;
			// Perform any additional setup after loading the view, typically from a nib.
			//ConfigureView();
		}


		void SubmitForm(string password)
		{
			bool isValid = password == Constants.Password;
			if (isValid)
			{
				string finalJson = JsonConvert.SerializeObject(SelectedAppointment.selectedQForm);
				DataTable dt = (DataTable)JsonConvert.DeserializeObject(finalJson, (typeof(DataTable)));
				var table = JsonConvert.DeserializeObject<DataTable>(finalJson);


				//DynaService.DynaDoxService dds = new DynaPad.DynaService.DynaDoxService();
				DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();

				//dds.GetFormQuestions("35", "5", "", false);

				//dds.ExportToPdf(finalJson);

				dds.SubmitFormAnswers(finalJson, true, false);

				SetDetailItem(new Section("Summary"), "Summary", null, false);
			}
			else {
				messageLabel.Text = "Login failed";
				//passwordEntry.Text = string.Empty;

				//Create Alert
				var FailAlert = UIAlertController.Create("Error", "Wrong password", UIAlertControllerStyle.Alert);

				//Add Action
				FailAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));

				// Present Alert
				PresentViewController(FailAlert, true, null);
			}



		}
		UILabel messageLabel;

		public void SetDetailItem(Section newDetailItem, string sectionId, string origSectionJson, bool IsDoctorForm)
		{
			//this.NavigationItem.SetRightBarButtonItem(null, true);
			//this.NavigationItem.RightBarButtonItem.Dispose();

			if (DetailItem != newDetailItem)
			{
				ReloadData();
				DetailItem = newDetailItem;
				//if (DetailItem.Caption == "Finalize")
				switch (sectionId)
				{
					case "Summary":
						DynaMultiRootElement summaryElement = new DynaMultiRootElement(SelectedAppointment.ApptFormName);
						DynaSection summarySection = new DynaSection("SUMARRY");

						var summaryPaddedView = new PaddedUIView<UILabel>();
						summaryPaddedView.Enabled = true;
						summaryPaddedView.Type = "Section";
						summaryPaddedView.Frame = new CGRect(0, 0, 0, 40);
						summaryPaddedView.Padding = 5f;
						summaryPaddedView.NestedView.Text = "SUMARRY";
						summaryPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
						summaryPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
						summaryPaddedView.setStyle();
						summarySection.HeaderView = summaryPaddedView;

						summarySection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
						summarySection.FooterView.Hidden = true;

						DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
						string finalJson = JsonConvert.SerializeObject(SelectedAppointment.selectedQForm);
						var summarypdf = dds.ExportToPdf(finalJson);

						var webViews = new UIWebView(View.Bounds);
						//View.AddSubview(webView);
						webViews.Frame = new CGRect(View.Bounds.X, 0, View.Bounds.Width, View.Bounds.Height);
						string localHtmlUrl = Path.Combine(NSBundle.MainBundle.BundlePath, summarypdf);
						webViews.LoadRequest(new NSUrlRequest(new NSUrl(localHtmlUrl, false)));
						webViews.ScalesPageToFit = true;

						summarySection.Add(webViews);
						summaryElement.Add(summarySection);

						//rootElement.TableView.ScrollEnabled = false;
						Root = summaryElement;
						Root.TableView.ScrollEnabled = false;
						break;
					case "Finalize":
						DynaMultiRootElement rootElement = new DynaMultiRootElement(SelectedAppointment.selectedQForm.FormName);
						DynaSection rootSection = new DynaSection("FINALIZE FORM");
						//headSection.HeaderView = new UIView(new CGRect(0, 0, 300f, 40));
						//headSection.HeaderView.BackgroundColor = UIColor.FromRGB(169, 188, 208);

						var rootPaddedView = new PaddedUIView<UILabel>();
						rootPaddedView.Enabled = true;
						rootPaddedView.Type = "Section";
						rootPaddedView.Frame = new CGRect(0, 0, 0, 40);
						rootPaddedView.Padding = 5f;
						rootPaddedView.NestedView.Text = "FINALIZE FORM";
						rootPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
						rootPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
						rootPaddedView.setStyle();
						rootSection.HeaderView = rootPaddedView;

						rootSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
						rootSection.FooterView.Hidden = true;

						//Section rootSection = new Section();

						SignaturePad.SignaturePadView sigPad = new SignaturePad.SignaturePadView(new CGRect(0, 0, View.Frame.Width, 300));
						sigPad.CaptionText = "Sign here:";
						sigPad.BackgroundColor = UIColor.White;
						GlassButton btnSubmit = new GlassButton(new RectangleF(0, 0, (float)View.Frame.Width, 50));
						btnSubmit.Font = UIFont.BoldSystemFontOfSize(17);
						btnSubmit.NormalColor = UIColor.Green;
						btnSubmit.DisabledColor = UIColor.Gray;

						messageLabel = new UILabel();

						btnSubmit.SetTitle("Submit Form", UIControlState.Normal);
						btnSubmit.TouchUpInside += (sender, e) =>
						{
							//Create Alert
							var SubmitPrompt = UIAlertController.Create("Submit Form", "Please hand back the IPad to submit", UIAlertControllerStyle.Alert);
							SubmitPrompt.AddTextField((field) =>
							{
								field.SecureTextEntry = true;
								field.Placeholder = "Password";
							});

							SubmitPrompt.Add(messageLabel);
							//Add Actions
							SubmitPrompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, action => SubmitForm(SubmitPrompt.TextFields[0].Text)));
							SubmitPrompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

							//Present Alert
							PresentViewController(SubmitPrompt, true, null);
						};


						rootSection.Add(sigPad);
						rootSection.Add(btnSubmit);
						rootElement.Add(rootSection);

						//rootElement.TableView.ScrollEnabled = false;
						Root = rootElement;
						Root.TableView.ScrollEnabled = false;
						break;
					case "Report":
						DynaMultiRootElement reportElement = new DynaMultiRootElement(SelectedAppointment.ApptFormName);
						DynaSection reportSection = new DynaSection("REPORT");

						var reportPaddedView = new PaddedUIView<UILabel>();
						reportPaddedView.Enabled = true;
						reportPaddedView.Type = "Section";
						reportPaddedView.Frame = new CGRect(0, 0, 0, 40);
						reportPaddedView.Padding = 5f;
						reportPaddedView.NestedView.Text = "REPORT";
						reportPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
						reportPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
						reportPaddedView.setStyle();
						reportSection.HeaderView = reportPaddedView;

						reportSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
						reportSection.FooterView.Hidden = true;

						var bb = View.Frame;
						var webView = new UIWebView(View.Bounds);
						//View.AddSubview(webView);
						webView.Frame = new CGRect(View.Bounds.X, 0, View.Bounds.Width, View.Bounds.Height);

						//DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
						DynaPadService.DynaPadService dps = new DynaPadService.DynaPadService();
						string report = dps.GenerateReport("123", "321", DateTime.Today.ToShortDateString(), "file", "24");
						//string report = dps.GenerateReport("123", SelectedQForm.ApptPatientID, DateTime.Today.ToShortDateString(), "file", SelectedQForm.ApptPatientFormID);
						var asdf = SelectedAppointment.ApptPatientID;

						//webView.LoadHtmlString(report, null);



						//var myurl = "https://test.dynadox.pro/dynawcfservice/" + report; // NOTE: https secure request
						var myurl = "https://test.dynadox.pro/dynawcfservice/test.pdf";// + report; // NOTE: https secure request
						var url = "https://www.princexml.com/samples/invoice/invoicesample.pdf"; // NOTE: https secure request
						webView.LoadRequest(new NSUrlRequest(new NSUrl(myurl)));
						webView.ScalesPageToFit = true;

						reportSection.Add(webView);
						reportElement.Add(reportSection);

						//rootElement.TableView.ScrollEnabled = false;
						Root = reportElement;
						Root.TableView.ScrollEnabled = false;
						break;
					default:
						// Update the view
						ConfigureView(sectionId, origSectionJson, IsDoctorForm);
						break;
				}
			}
		}


		void ConfigureView(string sectionId, string origS, bool IsDoctorForm)
		{
			// Update the user interface for the detail item
			if (DetailItem != null) //IsViewLoaded && 
			{

				//JObject jForm = JObject.Parse(JsonHandler.OriginalFormJsonString);

				//var sectionQuestions = JsonConvert.DeserializeObject<List<SectionQuestion>>(jForm["sections"][0]["elements"].ToString());
				var sectionQuestions = SelectedAppointment.selectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);

				DynaSection headSection = new DynaSection(sectionQuestions.SectionName);
				//headSection.HeaderView = new UIView(new CGRect(0, 0, 300f, 40));
				//headSection.HeaderView.BackgroundColor = UIColor.FromRGB(169, 188, 208);

				var headPaddedView = new PaddedUIView<UILabel>();
				headPaddedView.Enabled = true;
				headPaddedView.Type = "Section";
				headPaddedView.Frame = new CGRect(0, 0, 0, 40);
				headPaddedView.Padding = 5f;
				headPaddedView.NestedView.Text = sectionQuestions.SectionName.ToUpper();
				headPaddedView.NestedView.TextAlignment = UITextAlignment.Center;
				headPaddedView.NestedView.Font = UIFont.BoldSystemFontOfSize(17);
				headPaddedView.setStyle();
				headSection.HeaderView = headPaddedView;

				headSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
				headSection.FooterView.Hidden = true;

				//headSection.HeaderView.AddSubview(new UILabel(new CGRect(0, 0, 300f, 40)) { Text = sectionQuestions.SectionName.ToUpper(), TextAlignment = UITextAlignment.Right, Font = UIFont.BoldSystemFontOfSize(17) });

				if (IsDoctorForm)
				{
					this.NavigationItem.SetRightBarButtonItem(
					new UIBarButtonItem(UIBarButtonSystemItem.Compose, (sender, args) =>
					{
						//(10f + captionSize.Width, UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? 18f : 12f, cell.ContentView.Bounds.Width - 20 - captionSize.Width, 7f)

						audioFilePath = null;
						//RecordingStatusLabel = new UILabel();
						RecordingStatusLabel.Text = string.Empty;
						RecordingStatusLabel.Frame = new CGRect(210, 0, 120, 30);
						//LengthOfRecordingLabel = new UILabel();
						LengthOfRecordingLabel.Text = string.Empty;
						LengthOfRecordingLabel.Frame = new CGRect(210, 0, 120, 30);

						//StartRecordingButton = new UIButton();
						StartRecordingButton.Frame = new CGRect(20, 0, 160, 30);
						StartRecordingButton.TouchUpInside += OnStartRecording;
						StartRecordingButton.SetTitle("Start Recording", UIControlState.Normal);
						StartRecordingButton.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						//StopRecordingButton = new UIButton();
						StopRecordingButton.Frame = new CGRect(20, 0, 160, 30);
						StopRecordingButton.SetTitle("Stop Recording", UIControlState.Normal);
						StopRecordingButton.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						StopRecordingButton.TouchUpInside += OnStopRecording;
						StopRecordingButton.Enabled = false;
						//PlayRecordedSoundButton = new UIButton();
						PlayRecordedSoundButton.Frame = new CGRect(20, 0, 160, 30);
						PlayRecordedSoundButton.SetTitle("Play Recording", UIControlState.Normal);
						PlayRecordedSoundButton.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						PlayRecordedSoundButton.TouchUpInside += OnPlayRecordedSound;
						PlayRecordedSoundButton.Enabled = false;

						//SaveRecordedSound = new UIButton();
						SaveRecordedSound.Enabled = false;
						SaveRecordedSound.Frame = new CGRect(20, 0, 160, 30);
						SaveRecordedSound.SetTitle("Save Recording", UIControlState.Normal);
						SaveRecordedSound.SetTitleColor(UIColor.FromRGB(45, 137, 221), UIControlState.Normal);
						SaveRecordedSound.TouchUpInside += delegate
						{
							OnSaveRecordedSound(sectionId);
						};

						//CancelRecording = new UIButton();
						CancelRecording.Frame = new CGRect(0, 0, 350, 30);
						CancelRecording.SetTitle("Close", UIControlState.Normal);
						CancelRecording.SetTitleColor(UIColor.Black, UIControlState.Normal);
						//CancelRecording.TouchUpInside += OnCancelRecording;


						observer = AVPlayerItem.Notifications.ObserveDidPlayToEndTime(OnDidPlayToEndTime);

						Section sec = new Section();

						var cellRecord = new UITableViewCell(UITableViewCellStyle.Default,null);
						cellRecord.Frame = new CGRect(0, 0, 350, 30);
						cellRecord.ImageView.Image = UIImage.FromFile("images/Record-20.png");
						cellRecord.ContentView.Add(StartRecordingButton);
						cellRecord.ContentView.Add(RecordingStatusLabel);
						sec.Add(cellRecord);

						var cellStop = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellStop.Frame = new CGRect(0, 0, 350, 30);
						cellStop.ImageView.Image = UIImage.FromFile("images/Stop-20.png");
						cellStop.ContentView.Add(StopRecordingButton);
						cellStop.ContentView.Add(LengthOfRecordingLabel);
						sec.Add(cellStop);

						var cellPlay = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellPlay.Frame = new CGRect(0, 0, 350, 30);
						cellPlay.ImageView.Image = UIImage.FromFile("images/Play-20.png");
						cellPlay.ContentView.Add(PlayRecordedSoundButton);
						sec.Add(cellPlay);

						var cellSave = new UITableViewCell(UITableViewCellStyle.Default, null);
						cellSave.Frame = new CGRect(0, 0, 350, 30);
						cellSave.ImageView.Image = UIImage.FromFile("images/Plus-20.png");
						cellSave.ContentView.Add(SaveRecordedSound);
						sec.Add(cellSave);

						//sec.Add(RecordingStatusLabel);
						//sec.Add(LengthOfRecordingLabel);
						//sec.Add(StartRecordingButton);
						//sec.Add(StopRecordingButton);
						//sec.Add(PlayRecordedSoundButton);
						//sec.Add(SaveRecordedSound);
						//sec.Add(CancelRecording);
						sec.FooterView = CancelRecording;
						var headl = new UILabel(new CGRect(0, 0, 160, 30));
						headl.Text = "Dictation";
						sec.HeaderView = headl;

						DynaPadService.DynaPadService dps = new DynaPadService.DynaPadService();
						var dictations = dps.GetFormDictations(SelectedAppointment.selectedQForm.FormId, sectionId, "123", true, SelectedAppointment.selectedQForm.DoctorLocationId);
						//var cellDict = new UITableViewCell(UITableViewCellStyle.Value2, null);
						//cellDict.Frame = new CGRect(0, 0, 300, 0);
						foreach (string[] dictation in dictations)
						{
							UIButton PlaySavedDictationButton = new UIButton();
							PlaySavedDictationButton.Frame = new CGRect(20, 0, 160, 20);
							PlaySavedDictationButton.SetTitle(dictation[1], UIControlState.Normal);
							PlaySavedDictationButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
							PlaySavedDictationButton.TouchUpInside += delegate {
								OnPlaySavedDictation(dictation[1], dictation[2]);	
							};
							var cellDict = new UITableViewCell(UITableViewCellStyle.Default, null);
							cellDict.Frame = new CGRect(0, 0, 350, 20);
							cellDict.BackgroundColor = UIColor.LightGray;
							cellDict.ImageView.Image = UIImage.FromFile("images/Circled Play-20.png");
							cellDict.ContentView.Add(PlaySavedDictationButton);
							sec.Add(cellDict);
						}
						//sec.Add(cellDict);

						RootElement roo = new RootElement("dick");
						UIView vie = new UIView();
						roo.Add(sec);

						DialogViewController dia = new DialogViewController(roo);
						UIViewController con = new UIViewController();

						con.Add(vie);

						UIPopoverController pop = new UIPopoverController(dia);

						CancelRecording.TouchUpInside += delegate
						{
							//observer.Dispose();
							//recorder.Dispose();
							//recorder = null;
							pop.Dismiss(true);
						};
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

						pop.PresentFromBarButtonItem(this.NavigationItem.RightBarButtonItem, UIPopoverArrowDirection.Any, true);
					}), true);


					DynaSection presetSection = new DynaSection("Preset Answers");
					presetSection.Enabled = true;


					/*
					 * TODO: make presets password protected! (maybe component: Passcode)
					 * TODO: add 'clear' selection option! ??? DONE ???
					*/


					var presetPaddedView = new PaddedUIView<UILabel>();
					presetPaddedView.Enabled = true;
					presetPaddedView.Type = "Preset";
					presetPaddedView.Frame = new CGRect(0, 0, 0, 30);
					presetPaddedView.Padding = 5f;
					presetPaddedView.NestedView.Text = "Preset Answers";
					presetPaddedView.setStyle();

					presetSection.HeaderView = presetPaddedView;

					presetSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
					presetSection.FooterView.Hidden = true;

					int fs = SelectedAppointment.selectedQForm.FormSections.IndexOf(sectionQuestions);

					DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
					//string[,] FormPresetNames = pdds.GetSectionPresets("35", "5", origS, sectionId);
					var FormPresetNames = dds.GetAnswerPresets(SelectedAppointment.selectedQForm.FormId, sectionId, SelectedAppointment.selectedQForm.DoctorId, true, SelectedAppointment.selectedQForm.DoctorLocationId);


					RadioGroup presetGroup = new RadioGroup("PresetAnswers", sectionQuestions.SectionSelectedTemplateID);
					DynaRootElement presetsRoot = new DynaRootElement("Preset Answers", presetGroup);
					presetsRoot.IsPreset = true;
					//presetsRoot.GetActiveCell().BackgroundColor = UIColor.Red;
					//179,213,219
					//int fpnCount = FormPresetNames.GetLength(0) - 1;
					//for (int i = 0; i <= fpnCount; i++)
					foreach (string[] arrPreset in FormPresetNames)
					{
						var mre = new MyRadioElement(arrPreset[1], "PresetAnswers");
						mre.OnSelected += delegate (object sender, EventArgs e)
						{
							string presetJson = arrPreset[2];
							SelectedAppointment.selectedQForm.FormSections[fs] = JsonConvert.DeserializeObject<FormSection>(presetJson);
							SelectedAppointment.selectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId).SectionSelectedTemplateID = presetGroup.Selected;

							SetDetailItem(new Section(sectionQuestions.SectionName), sectionId, origS, IsDoctorForm);
						};

						presetSection.Add(mre);
					}

					GlassButton btnNewSectionPreset = new GlassButton(new RectangleF(0, 0, (float)View.Frame.Width, 50));
					btnNewSectionPreset.Font = UIFont.BoldSystemFontOfSize(17);
					btnNewSectionPreset.SetTitleColor(UIColor.Black, UIControlState.Normal);
					btnNewSectionPreset.NormalColor = UIColor.FromRGB(224, 238, 240);
					btnNewSectionPreset.SetTitle("Save New Section Preset", UIControlState.Normal);
					btnNewSectionPreset.TouchUpInside += (sender, e) =>
					{


					/*
					 * TODO: popup to enter preset name
					*/


					//Create Alert
					var okCancelAlertController = UIAlertController.Create("New Section Preset", "Necesito name", UIAlertControllerStyle.Alert);

					//Add Actions
					okCancelAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));//alert => Console.WriteLine("Okay was clicked")));
					okCancelAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));//alert => Console.WriteLine("Cancel was clicked")));

					//Present Alert
					PresentViewController(okCancelAlertController, true, null);
					};

					presetSection.Add(btnNewSectionPreset);


					presetsRoot.Add(presetSection);
					presetsRoot.Enabled = true;

					headSection.Add(presetsRoot);
				}


				QuestionsView = new DynaMultiRootElement(SelectedAppointment.selectedQForm.FormName);
				QuestionsView.Add(headSection);
				//QuestionsView.UnevenRows = true;

				foreach (SectionQuestion question in sectionQuestions.SectionQuestions)
				{
					bool enabled = !question.IsConditional || (question.IsConditional && question.IsEnabled);
					DynaSection qSection = new DynaSection(question.QuestionText);
					qSection.QuestionId = question.QuestionId;
					qSection.Enabled = enabled;
					//ConditionalEvent conEvent = new ConditionalEvent();

					switch (question.QuestionType)
					{
						case "BodyParts":
						case "Check":

							var checkPaddedView = new PaddedUIView<UILabel>();
							checkPaddedView.Enabled = enabled;
							checkPaddedView.Frame = new CGRect(0, 0, 0, 30);
							checkPaddedView.Padding = 5f;
							checkPaddedView.NestedView.Text = question.QuestionText.ToUpper();
							checkPaddedView.setStyle();

							qSection.HeaderView = checkPaddedView;

							qSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
							qSection.FooterView.Hidden = true;

							foreach (QuestionOption opt in question.QuestionOptions)
							{
								//if (rootSection.Count >= 7) break;
								DynaCheckBoxElement chk = new DynaCheckBoxElement(opt.OptionText, false, opt.OptionId);
								chk.Enabled = enabled;
								chk.ConditionTriggerId = question.ParentConditionTriggerId;
								chk.Value = opt.Chosen;
								//if (opt.Chosen && !string.IsNullOrEmpty(opt.ConditionTriggerIds))
								if (opt.Chosen && opt.ConditionTriggerIds != null && opt.ConditionTriggerIds.Count > 0)
								{
									ConditionalCheck(null, opt.ConditionTriggerIds, sectionId);
								}
								chk.Tapped += delegate
								{
									Chk_Tapped(question, opt, chk.Value, sectionId);
								};
								qSection.Add(chk);
							}

							QuestionsView.Add(qSection);
							break;
						case "Radio":
						case "Bool":
						case "YesNo":
							var radioPaddedView = new PaddedUIView<UILabel>();
							radioPaddedView.Enabled = enabled;
							radioPaddedView.Frame = new CGRect(0, 0, 0, 30);
							radioPaddedView.Padding = 5f;
							radioPaddedView.NestedView.Text = question.QuestionText.ToUpper();
							radioPaddedView.setStyle();

							qSection.HeaderView = radioPaddedView;
							qSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
							qSection.FooterView.Hidden = true;

							foreach (QuestionOption opt in question.QuestionOptions)
							{
								//DynaRadioElement radio = new DynaRadioElement(opt.OptionText, question.QuestionId);
								DynaMultiRadioElement radio = new DynaMultiRadioElement(opt.OptionText, question.QuestionId);
								radio.Enabled = enabled;
								radio.Chosen = opt.Chosen;
								radio.ConditionTriggerId = question.ParentConditionTriggerId;
								radio.ElementSelected += delegate
								{
									Radio_Tapped(question, opt);
									this.NavigationController.PopViewController(true);
								};
								qSection.Add(radio);
								if (opt.Chosen)
								{
									QuestionsView.Select(question.QuestionId, question.QuestionOptions.IndexOf(opt));

									//if (!string.IsNullOrEmpty(opt.ConditionTriggerIds))
									if (opt.ConditionTriggerIds != null && opt.ConditionTriggerIds.Count > 0)
									{
										ConditionalCheck(null, opt.ConditionTriggerIds, sectionId);
									}
								}
							}
							QuestionsView.Add(qSection);
							break;
						case "TextInput":

							DynaEntryElement entryElement = new DynaEntryElement("", "Enter your answer here", question.AnswerText);
							//FloatLabeledEntryElement entryElement = new FloatLabeledEntryElement(question.QuestionText, question.AnswerText);

							/* 
							 * TODO: add keyboard type...
							 * This includes Number Pad, Phone Pad, Email, URL along with other options.
							 * entryElement.KeyboardType = UIKeyboardType.
							 * also add in question maker if number min/max numbers
							 */

							//entryElement.AlignEntryWithAllSections = true;
							//entryElement.TextAlignment = UITextAlignment.Left;
							entryElement.Enabled = enabled;
							entryElement.QuestionId = question.QuestionId;
							entryElement.ConditionTriggerId = question.ParentConditionTriggerId;
							entryElement.EntryEnded += (sender, e) => { question.AnswerText = entryElement.Value; };

							var textPaddedView = new PaddedUIView<UILabel>();
							textPaddedView.Enabled = enabled;
							textPaddedView.Frame = new CGRect(0,0,0,30);
							textPaddedView.Padding = 5f;
							textPaddedView.NestedView.Text = question.QuestionText.ToUpper();
							textPaddedView.setStyle();

							qSection.HeaderView = textPaddedView;
							//textSection.HeaderView.BackgroundColor = UIColor.Red;

							qSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
							qSection.FooterView.Hidden = true;
							qSection.Add(entryElement);
							QuestionsView.Add(qSection);
							//QuestionsView[0].Add(rooty);
							break;
						case "Date":
							DateTime dt = new DateTime();
							if (!string.IsNullOrEmpty(question.AnswerText))
							{
								dt = Convert.ToDateTime(question.AnswerText);
							}
							else
							{
								dt = DateTime.Today;
							}

							NullableDateElementInline dateElement = new NullableDateElementInline("", dt);

							dateElement.Enabled = enabled;
							dateElement.Alignment = UITextAlignment.Left;
							dateElement.QuestionId = question.QuestionId;
							dateElement.ConditionTriggerId = question.ParentConditionTriggerId;
							//dateElement.DateSelected += (obj) => { question.AnswerText = dateElement.DateValue.ToString(); };

							var datePaddedView = new PaddedUIView<UILabel>();
							datePaddedView.Enabled = enabled;
							datePaddedView.Frame = new CGRect(0, 0, 0, 30);
							datePaddedView.Padding = 5f;
							datePaddedView.NestedView.Text = question.QuestionText.ToUpper();
							datePaddedView.setStyle();

							qSection.HeaderView = datePaddedView;

							qSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
							qSection.FooterView.Hidden = true;
							qSection.Add(dateElement);
							QuestionsView.UnevenRows = true;
							QuestionsView.Add(qSection);
							break;
						case "Height":
						case "Weight":
						case "Amount":
						case "Numeric":
							/*
							 *  TODO: options: UIStepper, Slider, Segmented Controls
							 * custom: migueldeicaza CounterElement
							 * components: BetterPickers, 
							 * have types: percent, currency, decimal, etc....
							*/

							//MDSlider slider = new MDSlider();

							//var stepperElement = new UIStepper();
							//var stepperLabel = new UILabel(new CGRect(0, 0, 100, 30));
							//stepperElement.MinimumValue = 0;
							//stepperElement.MaximumValue = 10;
							//stepperElement.ValueChanged += (object sender, EventArgs e) => stepperLabel.Text = ((UIStepper)sender).Value.ToString();

							float questionmin = 0;
							float questionmax = 100;
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
								questionmin = (float)Convert.ToInt32(question.MinValue);
							}
							if (!string.IsNullOrEmpty(question.MaxValue))
							{
								questionmax = (float)Convert.ToInt32(question.MaxValue);
							}
							if (!string.IsNullOrEmpty(question.AnswerText))
							{
								qanswer = (float)Convert.ToInt32(question.AnswerText);
							}


							//var sliderLabel = new UILabel(new CGRect(0, 0, 100, 20));
							//sliderLabel.Text = question.AnswerText;

							//var sliderElement = new UISlider(new CGRect(0, 0, 350, 30));
							var sliderElement = new FloatElementD(qanswer);
                            sliderElement.MinValue = questionmin;
							sliderElement.MaxValue = questionmax;
							//sliderElement.Value = 0.5f;//qanswer; // the current value
							//sliderElement.ValueChanged += (sender, e) => { sliderLabel.Text = ((UISlider)sender).Value.ToString(); question.AnswerText = ((UISlider)sender).Value.ToString();};
							//sliderElement.EnabledValueLabel = true;
							//sliderElement.MinimumValue = questionmin;
							//sliderElement.MaximumValue = questionmax;
							//sliderElement.Value = 0.5f;
							//sliderElement.SetValue(0.5f, true);
							sliderElement.ShowCaption = true;
							sliderElement.Caption = qanswer.ToString();
							sliderElement.Enabled = enabled;
							sliderElement.QuestionId = question.QuestionId;
							sliderElement.ConditionTriggerId = question.ParentConditionTriggerId;

							var amountPaddedView = new PaddedUIView<UILabel>();
							amountPaddedView.Enabled = enabled;
							amountPaddedView.Frame = new CGRect(0, 0, 0, 30);
							amountPaddedView.Padding = 5f;
							amountPaddedView.NestedView.Text = question.QuestionText.ToUpper();
							amountPaddedView.setStyle();

							qSection.HeaderView = amountPaddedView;

							qSection.FooterView = new UIView(new CGRect(0, 0, 0, 1));
							qSection.FooterView.Hidden = true;
							//qSection.Add(sliderLabel);
							qSection.Add(sliderElement); 
							//qSection.Add(slider);
							QuestionsView.UnevenRows = true;
							QuestionsView.Add(qSection);

							break;
						default:
							break;
					}
				}

							//ViewWillAppear(true);
				Root = QuestionsView;
				Root.TableView.ScrollEnabled = true;
				//Root.TableView.SeparatorInset = new UIEdgeInsets(0, 0, 0, 0);




			}
		}

		void Chk_Tapped(SectionQuestion cQuestion, QuestionOption cOption, bool selected, string sectionId)
		{
			//string newTriggerId = cOption.ConditionTriggerIds;
			List<string> newTriggerIds = cOption.ConditionTriggerIds;
			cOption.Chosen = selected;

			MultiConditionalCheck(cQuestion, sectionId);

		}


		void Radio_Tapped(SectionQuestion rQuestion, QuestionOption rOption)
		{
			//string newTriggerId = rOption.ConditionTriggerIds;
			List<string> newTriggerIds = rOption.ConditionTriggerIds;
			rQuestion.QuestionOptions.ForEach((obj) => obj.Chosen = false);
			rOption.Chosen = true;
			ConditionalCheck(rQuestion.ActiveTriggerIds, newTriggerIds, rQuestion.SectionId);
			rQuestion.ActiveTriggerIds = newTriggerIds;
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
				var sectionQuestions = SelectedAppointment.selectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);

				//if (!string.IsNullOrEmpty(activeTriggerIds))
				if (activeTriggerIds != null && activeTriggerIds.Count > 0)
				{
					var untriggeredQuestions = sectionQuestions.SectionQuestions.FindAll(((obj) => activeTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId)));

					TriggerCheck(untriggeredQuestions, false, sectionId);
				}

				//if (!string.IsNullOrEmpty(newTriggerIds))
				if (newTriggerIds != null && newTriggerIds.Count > 0)
				{
					//var triggeredQuestions = sectionQuestions.SectionQuestions.FindAll(((obj) => ((dynamic)obj).ParentConditionTriggerId == newTriggerIds));
					var triggeredQuestions = sectionQuestions.SectionQuestions.FindAll(((obj) => newTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId)));

					TriggerCheck(triggeredQuestions, true, sectionId);
				}
			}
		}


		private void MultiConditionalCheck(SectionQuestion activeQuestion, string sectionId)
		{
			var sectionQuestions = SelectedAppointment.selectedQForm.FormSections.Find((FormSection obj) => obj.SectionId == sectionId);
			List<SectionQuestion> untriggeredQuestions = new List<SectionQuestion>();
			List<SectionQuestion> triggeredQuestions = new List<SectionQuestion>();
			foreach (QuestionOption qOption in activeQuestion.QuestionOptions)
			{
				if (qOption.Chosen)
				{
					//if (!string.IsNullOrEmpty(qOption.ConditionTriggerIds))
					if(qOption.ConditionTriggerIds != null && qOption.ConditionTriggerIds.Count > 0)
					{
						triggeredQuestions.AddRange(sectionQuestions.SectionQuestions.FindAll(((obj) => qOption.ConditionTriggerIds.Contains(((dynamic)obj).ParentConditionTriggerId))));
					}
				}
				else
				{
					//if (!string.IsNullOrEmpty(qOption.ConditionTriggerIds))
					if (qOption.ConditionTriggerIds != null && qOption.ConditionTriggerIds.Count > 0)
					{
						//untriggeredQuestions.AddRange(sectionQuestions.SectionQuestions.FindAll(((obj) => ((dynamic)obj).ParentConditionTriggerId == qOption.ConditionTriggerIds)));
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
						//if (!string.IsNullOrEmpty(tOption.ConditionTriggerIds) && tOption.Chosen)
						if (tOption.ConditionTriggerIds != null && tOption.ConditionTriggerIds.Count > 0 && !string.IsNullOrEmpty(tOption.ConditionTriggerIds[0]) && tOption.Chosen)
						{
							//string optionTriggerId = tQuestion.ActiveTriggerIds;
							List<string> optionTriggerIds = tQuestion.ActiveTriggerIds;
							//if (!string.IsNullOrEmpty(optionTriggerIds))
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
						//((sec.HeaderView).Subviews[0] as UILabel).TextColor = triggered ? UIColor.Yellow : UIColor.Green;
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


		AVAudioSession session;
		AVAudioRecorder recorder;
		AVAudioPlayer player;
		Stopwatch stopwatch;
		NSUrl audioFilePath;
		NSObject observer;

		UILabel RecordingStatusLabel = new UILabel();
		UILabel LengthOfRecordingLabel = new UILabel();
		UIButton StartRecordingButton = new UIButton();
		UIButton StopRecordingButton = new UIButton();
		UIButton PlayRecordedSoundButton = new UIButton();
		UIButton SaveRecordedSound = new UIButton();
		UIButton CancelRecording = new UIButton();

		void OnStopRecording(object sender, EventArgs e)
		{
			if (recorder == null)
				return;

			recorder.Stop();
			stopwatch.Stop();

			LengthOfRecordingLabel.Text = string.Format("{0:hh\\:mm\\:ss}", stopwatch.Elapsed);
			RecordingStatusLabel.Text = "";
			StartRecordingButton.Enabled = true;
			StopRecordingButton.Enabled = false;
			PlayRecordedSoundButton.Enabled = true;
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
			StopRecordingButton.Enabled = true;
			PlayRecordedSoundButton.Enabled = false;
		}

		NSUrl CreateOutputUrl()
		{
			string fileName = string.Format("Myfile{0}.aac", DateTime.Now.ToString("yyyyMMddHHmmss"));
			string tempRecording = Path.Combine(Path.GetTempPath(), fileName);

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
				player.Play();
			}
			catch (Exception ex)
			{
				System.Console.WriteLine("There was a problem playing back audio: ");
				System.Console.WriteLine(ex.Message);
			}
		}

		void OnPlaySavedDictation(string title, string dictationBytes)
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
				byte[] bytes = Convert.FromBase64String(dictationBytes);
				NSData dataDictation = NSData.FromArray(bytes);
				NSError audioError;
				player = new AVAudioPlayer(dataDictation, "aac", out audioError);
				player.Play();
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
			if (!recorder.PrepareToRecord())
			{
				recorder.Dispose();
				recorder = null;
				return false;
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

		void OnSaveRecordedSound(string sectionId)
		{
			var dictationData = NSData.FromUrl(audioFilePath); //the path here can be a path to a video on the camera roll
			var array = dictationData.ToArray();
			try
			{
				DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();
				//DynaPadService.DynaPadService dds = new DynaPadService.DynaPadService();


				string dictationPath = dds.SaveDictation(SelectedAppointment.selectedQForm.FormId, sectionId, "123", true, SelectedAppointment.selectedQForm.DoctorLocationId, "Roy_" + DateTime.Now.ToShortTimeString(), array);
				System.Console.WriteLine("Saving Recording {0}", audioFilePath);
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
	}
}
