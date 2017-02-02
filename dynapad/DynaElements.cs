﻿using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Foundation;
using MonoTouch.Dialog;
using UIKit;
using System.Diagnostics;
#if __UNIFIED__

using NSAction = System.Action;
#else
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
#endif


#if !__UNIFIED__
using nint = global::System.Int32;
using nuint = global::System.UInt32;
using nfloat = global::System.Single;

using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;
using CGRect = global::System.Drawing.RectangleF;
#endif

using LoginScreen;

namespace DynaPad
{



	public class CredentialsProvider : ICredentialsProvider
	{
		// Constructor without parameters is required

		public bool NeedLoginAfterRegistration
		{
			get
			{
				// If you want your user to login after he/she has been registered
				return true;

				// Otherwise you can:
				// return false;
			}
		}

		public void Login(string userName, string password, Action successCallback, Action<LoginScreenFaultDetails> failCallback)
		{
			// Do some operations to login user
			bool isValid = (userName == Constants.Username && password == Constants.Password);
			if (isValid)
			{
				// If login was successfully completed
				successCallback();
			}
			else
			{
				var loginDetails = new LoginScreenFaultDetails();
				if (userName != Constants.Username)
				{
					loginDetails.UserNameErrorMessage = "User name is wrong or does not exist";
				}
				else if (password != Constants.Password)
				{
					loginDetails.PasswordErrorMessage = "Password is wrong or does not match user name";
				}
				else
				{
					loginDetails.CommonErrorMessage = "Some error message relative to whole form";
				}
				// Otherwise
				failCallback(loginDetails);
			}
		}

		public void Register(string email, string userName, string password, Action successCallback, Action<LoginScreenFaultDetails> failCallback)
		{
			// Do some operations to register user

			// If registration was successfully completed
			successCallback();

			// Otherwise
			// failCallback(new LoginScreenFaultDetails {
			//  CommonErrorMessage = "Some error message relative to whole form",
			//  EmailErrorMessage = "Some error message relative to e-mail form field",
			//  UserNameErrorMessage = "Some error message relative to user name form field",
			//  PasswordErrorMessage = "Some error message relative to password form field"
			// });
		}

		public void ResetPassword(string email, Action successCallback, Action<LoginScreenFaultDetails> failCallback)
		{
			// Do some operations to reset user's password

			// If password was successfully reset
			successCallback();

			// Otherwise
			// failCallback(new LoginScreenFaultDetails {
			//  CommonErrorMessage = "Some error message relative to whole form",
			//  EmailErrorMessage = "Some error message relative to e-mail form field"
			// });
		}

		public bool ShowPasswordResetLink
		{
			get
			{
				// If you want your login screen to have a forgot password button
				//return true;

				// Otherwise you can:
				 return false;
			}
		}

		public bool ShowRegistration
		{
			get
			{
				// If you want your login screen to have a register new user button
				//return true;

				// Otherwise you can:
				 return false;
			}
		}
	}



	public class DynaDialogViewController : DialogViewController
	{

		public DynaDialogViewController(IntPtr handle) : base(handle)
		{
			Style = UITableViewStyle.Plain;
		}

		public DynaDialogViewController(RootElement root) : base(root)
		{
			Style = UITableViewStyle.Plain;
			Title = root.Caption;
		}

		public DynaDialogViewController(RootElement root, bool pushing) : base(root, pushing)
		{
			Style = UITableViewStyle.Plain;
			Title = root.Caption;
		}

		public override void LoadView()
		{
			base.LoadView();

			var myTitleLabel = new UILabel(new CGRect(0, 0, 1, 1)) { Text = Title };
			myTitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
			myTitleLabel.Lines = 0;
			myTitleLabel.TextAlignment = UITextAlignment.Center;
			NavigationItem.TitleView = myTitleLabel;
			NavigationItem.TitleView.SizeToFit();

			NavigationItem.BackBarButtonItem = new UIBarButtonItem(@"", UIBarButtonItemStyle.Plain, null, null);

			if (NavigationItem.LeftBarButtonItem == null && this != NavigationController.ViewControllers[0])
			{
				var btnBack = new UIBarButtonItem(UIImage.FromBundle("Back"), UIBarButtonItemStyle.Plain, (sender, args) =>
				{
					NavigationController.PopViewController(true);
				});
				NavigationItem.SetLeftBarButtonItem(btnBack, true);
			}
		}
	}



	public class SectionStringElement : Element
	{
		static NSString skey = new NSString("StringElement");
		static NSString skeyvalue = new NSString("StringElementValue");
		public UITextAlignment Alignment = UITextAlignment.Left;
		public string Value;
		//public bool selected = false;
		public bool selected;

		public SectionStringElement(string caption) : base(caption) { }

		public SectionStringElement(string caption, string value) : base(caption)
		{
			Value = value;
		}

		public SectionStringElement(string caption, NSAction tapped) : base(caption)
		{
			Tapped += tapped;
		}

		public event NSAction Tapped;

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(Value == null ? skey : skeyvalue);
			if (cell == null)
			{
				cell = new UITableViewCell(Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1, Value == null ? skey : skeyvalue);
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
			}
			cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			cell.BackgroundColor = UIColor.White;
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			cell.TextLabel.Lines = 0;

			if (selected)
			{
				cell.BackgroundColor = UIColor.FromRGB(220, 237, 185);
			}

			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value ?? "";

			return cell;
		}

		public override string Summary()
		{
			return Caption;
		}

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var cell = base.GetCell(tableView);
			cell.BackgroundColor = UIColor.Red;

			base.Selected(dvc, tableView, path);

			if (Tapped != null)
				Tapped();
			selected = !selected;
			tableView.SelectRow(path, true, UITableViewScrollPosition.None);
		}

		public override bool Matches(string text)
		{
			return (Value != null && Value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1) || base.Matches(text);
		}
	}




	class MyRadioElement : RadioElement
	{
		public MyRadioElement(string cCaption, string cGroup) : base(cCaption, cGroup) { Group = cGroup; }
		//public MyRadioElement(string s) : base(s) { }

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			base.Selected(dvc, tableView, indexPath);
			var selected = OnSelected;
			if (selected != null)
				selected(this, EventArgs.Empty);
			
			dvc.DeactivateController(true);
		}

		public event EventHandler<EventArgs> OnSelected;
	}



	public class DebugRadioElement : RadioElement
	{
		Action<DebugRadioElement, EventArgs> onCLick;

		public DebugRadioElement(string s, Action<DebugRadioElement, EventArgs> onCLick) : base(s)
		{
			this.onCLick = onCLick;
		}

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			base.Selected(dvc, tableView, indexPath);
			var selected = onCLick;
			if (selected != null)
				selected(this, EventArgs.Empty);
		}

		static public event EventHandler<EventArgs> OnSelected;
	}



	public class PaddedUIView<T> : UIView where T : UIView, new()
	{
		nfloat _padding;
		private T _nestedView;
		public bool Enabled;
		public string Type;

		public PaddedUIView()
		{
			Initialize();
		}

		public PaddedUIView(CGRect bounds)
			: base(bounds)
		{
			Initialize();
		}

		void Initialize()
		{
			if (_nestedView == null)
			{
				_nestedView = new T();
				AddSubview(_nestedView);
			}

			_nestedView.Frame = new CGRect(_padding + 5, _padding, Frame.Width - 2 * _padding, Frame.Height - 2 * _padding);
		}

		public void setStyle()
		{
			switch (Type)
			{
				case "Question":
					//_nestedView.Frame = new CGRect(_padding + 5, _padding, Frame.Width - 2 * _padding, Frame.Height - 2 * _padding);

					(_nestedView as UILabel).TextColor = UIColor.DarkGray;
					(_nestedView as UILabel).Font = UIFont.SystemFontOfSize(13);
					if (Enabled)
					{
						BackgroundColor = UIColor.FromRGB(230, 230, 250);
					}
					else
					{
						BackgroundColor = UIColor.GroupTableViewBackgroundColor;
					}
					break;
				case "Preset":
					(_nestedView as UILabel).TextColor = UIColor.DarkGray;
					(_nestedView as UILabel).Font = UIFont.SystemFontOfSize(13);
					BackgroundColor = UIColor.FromRGB(216,219,226);
					break;
				case "Section":
					//_nestedView.Frame = new CGRect(_padding + 5, _padding, Frame.Width - 2 * _padding, Frame.Height - 2 * _padding);

					(_nestedView as UILabel).TextColor = UIColor.Black;
					(_nestedView as UILabel).Font = UIFont.SystemFontOfSize(17);
					BackgroundColor = UIColor.FromRGB(169, 188, 208);
					break;
				default:
					(_nestedView as UILabel).TextColor = UIColor.DarkGray;
					(_nestedView as UILabel).Font = UIFont.SystemFontOfSize(13);
					BackgroundColor = UIColor.GroupTableViewBackgroundColor;
					break;
			}
		}

		public T NestedView
		{
			
			get { return _nestedView; }
		}

		public nfloat Padding
		{
			get { return _padding; }
			set { if (value != _padding) { _padding = value; Initialize(); } }
		}

		public override CGRect Frame
		{
			get { return base.Frame; }
			set { base.Frame = value; Initialize(); }
		}
	}



	public class DynaSectionLabel : UILabel
	{
		public bool IsEnabled;
		public string Type;

		public DynaSectionLabel() 		{
			Font = UIFont.BoldSystemFontOfSize(17);

			Frame = new CGRect(10, 0, 100f, 40);

			//Insets.Left = 20;

			TextColor = UIColor.Black;
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			BackgroundColor = UIColor.FromRGB(55,63,81);
			//UserInteractionEnabled = IsEnabled;
			if (!IsEnabled)
			{
				TextColor = UIColor.LightGray;
				BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}
		}

		public DynaSectionLabel(CGRect frame) : base( frame )
    	{
		}

		public UIEdgeInsets Insets { get; set; }

		public override void DrawText(CGRect rect)
		{
			base.DrawText(Insets.InsetRect(rect));
		}
	}



	public class DynaSection : Section
	{
		public bool Enabled;
		public string ConditionTriggerId;
		public string ActiveTriggerId = "";
		public List<QuestionOption> QuestionOptions;
		public List<QuestionOption> QuestionAnswers;
		public string QuestionId { get; set; }
		public string QuestionParentId { get; set; }
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string QuestionKeyboardType { get; set; }
		public bool Answered { get; set; }
		public bool Disabled { get; set; }
		public string AnswerId { get; set; }
		public string AnswerText { get; set; }
		public string ParentConditionTriggerId { get; set; }
		public bool IsConditional { get; set; }
		public Group thisGroup { get; set; }

		public DynaSection(string caption) : base(caption)
		{
		}

		public override string Summary()
		{
			//UITableView viewty = base.GetContainerTableView();
			return base.Summary();
		}
		private static NSString cellKey = new NSString("Identifier");
		protected override NSString CellKey
		{
			get
			{
				return (NSString)"Identifier";
			}
		}
		public override UITableViewCell GetCell(UITableView tv)
		{
			//var cell = base.GetCell(tv);
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			//return cell;
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = base.GetCell(tv);
				//RemoveTag(cell, 1);
				//RemoveTag(cell, 2);
				//cell = new UITableViewCell(UITableViewCellStyle.Value1, CellKey);
				//cell.TextLabel.Text = QuestionText;
				//cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				//if (RadioSelected == -1)
				//{
				//	cell.DetailTextLabel.Text = "";
				//}
			}

			//cell.PrepareForReuse();
			cell.ContentView.AutosizesSubviews = false;
			cell.UserInteractionEnabled = Enabled;
			return cell;
		}
	}



	public class DynaRootElement : RootElement
	{
		public bool Enabled;
		public string ConditionTriggerId;
		public string ActiveTriggerId = "";
		public List<QuestionOption> QuestionOptions;
		public List<QuestionOption> QuestionAnswers;
		public string QuestionId { get; set; }
		public string QuestionParentId { get; set; }
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string QuestionKeyboardType { get; set; }
		public bool Answered { get; set; }
		public bool Disabled { get; set; }
		public string AnswerId { get; set; }
		public string AnswerText { get; set; }
		public string ParentConditionTriggerId { get; set; }
		public bool IsConditional { get; set; }
		public Group thisGroup { get; set; }
		public bool IsPreset {get; set; }

		public DynaRootElement(string caption) : base(caption)
		{
			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public DynaRootElement(string caption, Func<RootElement, UIViewController> createOnSelected) : base(caption, createOnSelected)
		{
			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public DynaRootElement(string caption, int section, int element) : base(caption, section, element)
		{
			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public DynaRootElement(string caption, Group group) : base(caption, group)
		{
			thisGroup = group;

			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public override string Summary()
		{
			//UITableView viewty = base.GetContainerTableView();
			return base.Summary();
		}
		private static NSString cellKey = new NSString("Identifier");
		protected override NSString CellKey
		{
			get
			{
				return (NSString) "Identifier";
			}
		}
		public override UITableViewCell GetCell(UITableView tv)
		{
			//var cell = base.GetCell(tv);
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			//return cell;
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = base.GetCell(tv);
				//RemoveTag(cell, 1);
				//RemoveTag(cell, 2);
				//cell = new UITableViewCell(UITableViewCellStyle.Value1, CellKey);
				//cell.TextLabel.Text = QuestionText;
				//cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				if (RadioSelected == -1)
				{
					cell.DetailTextLabel.Text = "";
				}
			}
			//cell.PrepareForReuse();

			cell.UserInteractionEnabled = Enabled;

			cell.TextLabel.TextColor = UIColor.Black;
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			cell.BackgroundColor = UIColor.White;

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}

			if (IsPreset)
			{
				cell.BackgroundColor = UIColor.FromRGB(224, 238, 240);
			}

			return cell;

		}

		protected override void PrepareDialogViewController(UIViewController dvc)
		{
			//dvc.View.BackgroundColor = Settings.RootBackgroundColour;
			base.PrepareDialogViewController(dvc);
		}
	}



	public class DynaFormRootElement : RootElement
	{
		public bool Enabled;
		public string FormID { get; set; }
		public string FormName { get; set; }
		public bool IsDoctorForm { get; set; }
		public string MenuValue { get; set; }
		public string MenuAction { get; set; }
		public string PatientID { get; set; }
		public string DoctorID { get; set; }
		public string LocationID { get; set; }
		public string ApptID { get; set; }
		public List<Report> ApptReports { get; set; }
		public Group thisGroup { get; set; }

		public DynaFormRootElement(string caption) : base(caption)
		{
			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public DynaFormRootElement(string caption, Func<RootElement, UIViewController> createOnSelected) : base(caption, createOnSelected)
		{
			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public DynaFormRootElement(string caption, int section, int element) : base(caption, section, element)
		{
			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public DynaFormRootElement(string caption, Group group) : base(caption, group)
		{
			thisGroup = group;

			createOnSelected = (RootElement arg) =>
			{
				return new DynaDialogViewController(arg);
			};
		}

		public override string Summary()
		{
			//UITableView viewty = base.GetContainerTableView();
			return base.Summary();
		}
		static NSString cellKey = new NSString("Identifier");
		protected override NSString CellKey
		{
			get
			{
				return (NSString)"Identifier";
			}
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			//var cell = base.GetCell(tv);
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			//return cell;
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = base.GetCell(tv);
			}
			//cell.PrepareForReuse();

			cell.UserInteractionEnabled = Enabled;

			cell.TextLabel.TextColor = UIColor.Black;

			cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
			cell.TextLabel.Lines = 0;

			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			cell.BackgroundColor = UIColor.White;

			//cell.SelectionStyle = UITableViewCellSelectionStyle.Gray;
			//cell.Selected = true;

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}

			//UIView bgColorView = new UIView();
			//bgColorView.BackgroundColor = UIColor.LightGray;
			//cell.SelectedBackgroundView = bgColorView;

			return cell;

		}

		//public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		//{
		//	// dostuff
		//	tableView.SelectRow(indexPath, true, UITableViewScrollPosition.Top);
		//}


		protected override void PrepareDialogViewController(UIViewController dvc)
		{
			//dvc.View.BackgroundColor = Settings.RootBackgroundColour;
			base.PrepareDialogViewController(dvc);
		}
	}



	public class DynaEntryElement : EntryElement
	{
		public DynaEntryElement(string cCaption, string cPlaceHolder, string cValue) : base(cCaption, cPlaceHolder, cValue) { }

		public bool Enabled;
		public string ConditionTriggerId;
		public string ActiveTriggerId = "";
		public List<QuestionOption> QuestionOptions;
		public List<QuestionOption> QuestionAnswers;
		public string QuestionId { get; set; }
		public string QuestionParentId { get; set; }
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string QuestionKeyboardType { get; set; }
		public bool Answered { get; set; }
		public bool Disabled { get; set; }
		public string AnswerId { get; set; }
		public string AnswerText { get; set; }
		public string ParentConditionTriggerId { get; set; }
		public bool IsConditional { get; set; }
		//static NSString MyCellId = new NSString("MyCellId");
		private UITextField EntryTextField { get; set; }
 		protected override UITextField CreateTextField(CGRect frame) 		{ 			var tf = base.CreateTextField(frame); 			//tf.HorizontalAlignment = UIControlContentHorizontalAlignment.Left; 			//tf.TextAlignment = UITextAlignment.Left; 			EntryTextField = tf; 			return tf; 		}  		public override UITableViewCell GetCell(UITableView tv) 		{ 			var cell = base.GetCell(tv);

			cell.ContentView.AutosizesSubviews = false;
			//cell.ContentView.Frame = new CGRect(48, 0, 310, 44);
			cell.UserInteractionEnabled = Enabled;
			cell.TextLabel.TextColor = UIColor.Black;
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			cell.BackgroundColor = UIColor.White;
			EntryTextField.TextColor = UIColor.Black;
			//EntryTextField.Placeholder = "Enter your answer here";

			var offset = (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone) ? 20 : 90;
			cell.Frame = new RectangleF((float)cell.Frame.X, (float)cell.Frame.Y, (float)(tv.Frame.Width - offset), (float)cell.Frame.Height);
			//  SizeF size = s.EntryAlignment;
			SizeF size = GetEntryPosition(UIFont.BoldSystemFontOfSize(17));
			var yOffset = (float)((cell.ContentView.Bounds.Height - size.Height) / 2 - 1);
			var width = (float)(cell.ContentView.Bounds.Width - size.Width);
			if (TextAlignment == UITextAlignment.Right)
			{
				// Add padding if right aligned
				width -= 10;
			}

			var entryFrame = new RectangleF(size.Width, yOffset, width, size.Height);

			EntryTextField.Frame = entryFrame;

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
				EntryTextField.TextColor = UIColor.LightGray;
				EntryTextField.Placeholder = "Not applicable";
			}
 			return cell; 		}  		SizeF GetEntryPosition(UIFont font) 		{ 			var s = Parent as Section;  			var max = new SizeF(-15, 17);  			foreach (var e in s.Elements) 			{ 				var ee = e as DynaEntryElement; 				if (ee == null) 					continue;  				if (ee.Caption != null) 				{ 					// var size = tv.StringSize (ee.Caption, font); 					var size = new NSString(ee.Caption).StringSize(font); 					if (size.Width > max.Width) 						max = (SizeF)size; 				} 			}
 			s.EntryAlignment = new SizeF(20 + Math.Min(max.Width, 160), max.Height);  			return (SizeF)s.EntryAlignment; 		}
	}



	public class DynaBooleanElement : BooleanElement
	{
		public DynaBooleanElement(string cCaption, bool cValue) : base(cCaption, cValue) { }

		public bool Enabled;
		public string CheckedValue;
		public string UncheckedValue;
		public string ConditionTriggerId;
		public string ActiveTriggerId = "";
		public List<QuestionOption> QuestionOptions;
		public List<QuestionOption> QuestionAnswers;
		public string QuestionId { get; set; }
		public string QuestionParentId { get; set; }
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string QuestionKeyboardType { get; set; }
		public bool Answered { get; set; }
		public bool Disabled { get; set; }
		public string AnswerId { get; set; }
		public string AnswerText { get; set; }
		public string ParentConditionTriggerId { get; set; }
		public bool IsConditional { get; set; }

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			//var cell = base.GetCell (tableView);
			//cell.BackgroundColor = UIColor.Blue;

			base.Selected(dvc, tableView, path);
			var selected = OnSelected;
			if (selected != null)
				//base.GetActiveCell().Highlighted = true;
				//Value = true;
				selected(this, EventArgs.Empty);
		}

		public override void Deselected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			base.Deselected(dvc, tableView, path);
			var deselected = OnDeselected;
			if (deselected != null)
				//base.GetActiveCell().Highlighted = false;	
				//Value = false;
				deselected(this, EventArgs.Empty);
		}
		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);
			cell.ContentView.AutosizesSubviews = false;
			var switchy = cell.AccessoryView as UISwitch;
			switchy.OnTintColor = UIColor.Green;
			cell.TextLabel.TextColor = UIColor.Black;
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			cell.BackgroundColor = UIColor.White;
			cell.UserInteractionEnabled = Enabled;

			if (!Enabled)
			{
				switchy.OnTintColor = UIColor.LightTextColor;
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}

			//cell.ReloadInputViews();
			return cell;
		}

		public event EventHandler<EventArgs> OnSelected;
		public event EventHandler<EventArgs> OnDeselected;
	}



	public class DynaRadioElement : RadioElement
	{
		public DynaRadioElement(string cCaption, string cGroup = null) : base(cCaption, cGroup) { Group = cGroup; }

		public bool Enabled;
		public string ParentQuestionId { get; set; }
		public string OptionText { get; set; }
		public string OptionId { get; set; }
		public string ConditionTriggerId { get; set; }
		public bool Chosen { get; set; }

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			//var cell = base.GetCell (tableView);
			//cell.BackgroundColor = UIColor.Blue;


			base.Selected(dvc, tableView, indexPath);
			var selected = OnSelected;
			if (selected != null)
				//base.GetActiveCell().Highlighted = true;
				//Value = true;
				selected(this, EventArgs.Empty);
		}

		public override void Deselected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			base.Deselected(dvc, tableView, path);
			var deselected = OnDeselected;
			if (deselected != null)
				//base.GetActiveCell().Highlighted = false;	
				//Value = false;
				deselected(this, EventArgs.Empty);
		}
		public UITableViewCellAccessory Accessory { get; set; }
		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);

			return cell;
		}

		public event EventHandler<EventArgs> OnSelected;
		public event EventHandler<EventArgs> OnDeselected;
	}



	public class DynaCheckBoxElement : CheckboxElement
	{

		public DynaCheckBoxElement(string cCaption, bool cValue, string cGroup) : base(cCaption, cValue, cGroup) { }

		public bool Enabled;
		public string ParentQuestionId { get; set; }
		public string OptionText { get; set; }
		public string OptionId { get; set; }
		public string ConditionTriggerId { get; set; }
		public bool Chosen { get; set; }
		public string QuestionId { get; set; }

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			base.Selected(dvc, tableView, path);
			var selected = OnSelected;
			if (selected != null)
				//base.GetActiveCell().Highlighted = true;
				//Value = true;
				selected(this, EventArgs.Empty);
			
			tableView.SelectRow(path, true, UITableViewScrollPosition.None);

		}

		public override void Deselected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			base.Deselected(dvc, tableView, path);
			var deselected = OnDeselected;
			if (deselected != null)
				//base.GetActiveCell().Highlighted = false;	
				//Value = false;
				deselected(this, EventArgs.Empty);
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);
			//cell.TextLabel.Font = UIFont.SystemFontOfSize(20);
			//cell.IndentationWidth = 15;

			cell.ContentView.AutosizesSubviews = false;

			cell.UserInteractionEnabled = Enabled;

			cell.BackgroundColor = UIColor.White;
			cell.TextLabel.TextColor = UIColor.Black;

			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			if (Value)
			{
				cell.BackgroundColor = UIColor.FromRGB(239, 246, 223);
			}

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

				//if (cell.Selected)
				//{
					cell.Accessory = UITableViewCellAccessory.None;
				//}
			}

			return cell;
		}

		public event EventHandler<EventArgs> OnSelected;
		public event EventHandler<EventArgs> OnDeselected;
	}



	public class DynaMultiRootElement : RootElement
	{
		public bool Enabled;
		public string QuestionId { get; set; }
		private RadioGroup _defaultGroup = new RadioGroup(0);
		private Dictionary<string, RadioGroup> _groups = new Dictionary<string, RadioGroup>();

		public DynaMultiRootElement(string caption = "") : base(caption, new RadioGroup("default", -1))
		{
		}

		public DynaMultiRootElement(string caption, Group group, Func<RootElement, UIViewController> createOnSelected) : base(caption, group)
		{

			var radioGroup = group as RadioGroup;

			if (radioGroup != null)
			{
				_groups.Add(radioGroup.Key.ToLower(), radioGroup);
			}

			this.createOnSelected = createOnSelected;
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);

			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			return cell;
		}

		public int Selected(string group)
		{
			if (string.IsNullOrEmpty(group))
			{
				throw new ArgumentNullException(nameof(group));
			}

			group = group.ToLower();
			if (_groups.ContainsKey(group))
			{
				return _groups[group].Selected;
			}

			return -1;
		}

		public void Select(string group, int selected)
		{
			if (string.IsNullOrEmpty(group))
			{
				throw new ArgumentNullException(nameof(group));
			}

			var radioGroup = GetGroup(group);
			radioGroup.Selected = selected;
		}

		internal RadioGroup GetGroup(string group)
		{
			if (string.IsNullOrEmpty(group))
			{
				throw new ArgumentNullException(nameof(group));
			}

			group = group.ToLower();
			if (!_groups.ContainsKey(group))
			{
				_groups[group] = new RadioGroup(group, -1);
			}

			return _groups[group];
		}

		internal NSIndexPath PathForRadioElement(string group, int index)
		{

			foreach (var section in this)
			{
				foreach (var e in section.Elements)
				{
					var re = e as DynaMultiRadioElement;
					if (re != null
						&& string.Equals(re.Group, group, StringComparison.InvariantCultureIgnoreCase)
						&& re.Index == index)
					{
						return e.IndexPath;
					}
				}
			}

			return null;
		}
	}



	public class DynaMultiRadioElement : RadioElement
	{

		public bool Enabled;
		public string ParentQuestionId { get; set; }
		public string OptionText { get; set; }
		public string OptionId { get; set; }
		public string ConditionTriggerId { get; set; }
		public bool Chosen { get; set; }
		public string QuestionId { get; set; }

		public event Action<DynaMultiRadioElement> OnDeselected;

		public event Action<DynaMultiRadioElement> ElementSelected;

		private readonly static NSString ReuseId = new NSString("CustomRadioElement");
		private string _subtitle;
		public int? Index { get; protected set; }

		public DynaMultiRadioElement(string caption, string group = null, string subtitle = null) : base(caption, group)
		{
			_subtitle = subtitle;

		}

		protected override NSString CellKey
		{
			get
			{
				return ReuseId;
			}
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			EnsureIndex();
			//tv.CellLayoutMarginsFollowReadableWidth = false;
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellKey);
			}
			//cell.ContentView.Frame = new CGRect(0, 0, 310, 50);
			cell.ContentView.AutosizesSubviews = false;
			//cell.ApplyStyle(this);
			//this.GetContainerTableView().CellLayoutMarginsFollowReadableWidth = false;
			cell.TextLabel.Text = Caption;

			if (!string.IsNullOrEmpty(_subtitle))
			{
				cell.DetailTextLabel.Text = _subtitle;
			}

			var selected = false;
			var slRoot = Parent.Parent as DynaMultiRootElement;

			if (slRoot != null)
			{
				selected = Index == slRoot.Selected(Group);

			}
			else
			{
				var root = (RootElement)Parent.Parent;
				selected = Index == root.RadioSelected;
			}

			cell.Accessory = selected ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;

			cell.Selected = selected;

			cell.UserInteractionEnabled = Enabled;

			cell.TextLabel.TextColor = UIColor.Black;

			cell.BackgroundColor = UIColor.White;

			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			if (selected)
			{
				cell.BackgroundColor = UIColor.FromRGB(239, 246, 223);
			}

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
				if (selected)
				{
					cell.Accessory = UITableViewCellAccessory.None;
				}
			}

			return cell;
		}

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			var slRoot = Parent.Parent as DynaMultiRootElement;

			if (slRoot != null)
			{
				RadioGroup radioGroup = slRoot.GetGroup(Group);

				UITableViewCell cell;

				if (radioGroup.Selected == Index)
				{
					var deSelectedIndex = slRoot.PathForRadioElement(Group, radioGroup.Selected);
					if (deSelectedIndex != null)
					{
						cell = tableView.CellAt(deSelectedIndex);
						if (cell != null)
						{
							cell.Selected = false;
							cell.BackgroundColor = UIColor.White;
							cell.Accessory = UITableViewCellAccessory.None;
						}
					}

					var unhandler = OnDeselected;
					if (unhandler != null)
					{
						unhandler(this);
					}

					radioGroup.Selected = -1;
					slRoot.Deselected(dvc, tableView, indexPath);

					return;
				}


				var selectedIndex = slRoot.PathForRadioElement(Group, radioGroup.Selected);
				if (selectedIndex != null)
				{
					cell = tableView.CellAt(selectedIndex);
					if (cell != null)
					{
						cell.Accessory = UITableViewCellAccessory.None;
					}
				}


				cell = tableView.CellAt(indexPath);
				if (cell != null)
				{
					cell.Accessory = UITableViewCellAccessory.Checkmark;
				}

				radioGroup.Selected = Index.Value;

				var handler = ElementSelected;
				if (handler != null)
				{
					handler(this);
				}

			}
			else
			{
				base.Selected(dvc, tableView, indexPath);
			}
		}

		public override void Deselected(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			base.Deselected(dvc, tableView, path);
			var deselected = OnDeselected;
			if (deselected != null)
				//base.GetActiveCell().Highlighted = false;	
				//Value = false;
				deselected(this);
		}

		private void EnsureIndex()
		{
			if (!Index.HasValue)
			{
				var parent = Parent as Section;

				Index = parent.Elements.IndexOf(this);
			}
		}
	}
   	public class DynaDateElement : DateElement 	{ 		public bool Enabled; 		public string ConditionTriggerId; 		public string ActiveTriggerId = ""; 		public List<QuestionOption> QuestionOptions; 		public List<QuestionOption> QuestionAnswers; 		public string QuestionId { get; set; } 		public string QuestionParentId { get; set; } 		public string QuestionText { get; set; } 		public string QuestionType { get; set; } 		public string QuestionKeyboardType { get; set; } 		public bool Answered { get; set; } 		public bool Disabled { get; set; } 		public string AnswerId { get; set; } 		public string AnswerText { get; set; } 		public string ParentConditionTriggerId { get; set; } 		public bool IsConditional { get; set; }  		public DynaDateElement(string caption, DateTime date) : base(caption, date) 		{ 			fmt.DateStyle = NSDateFormatterStyle.Medium; 		} 		public override UIDatePicker CreatePicker() 		{ 			UIDatePicker futureDatePicker = base.CreatePicker(); 			futureDatePicker.BackgroundColor = UIColor.White; 			futureDatePicker.MinimumDate = (NSDate)DateTime.Today; 			return futureDatePicker; 		}  		public override string FormatDate(DateTime dt) 		{ 			return fmt.ToString((NSDate)GetDateWithKind(dt)); 		}  		public override UITableViewCell GetCell(UITableView tv) 		{ 			var cell = base.GetCell(tv);  			cell.UserInteractionEnabled = Enabled;   			cell.TextLabel.TextColor = UIColor.Black; 			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17); 			cell.BackgroundColor = UIColor.White;  			if (!Enabled) 			{ 				cell.TextLabel.TextColor = UIColor.LightGray; 				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor; 			}  			return cell; 		} 	}



	public class NullableDateElementInline : StringElement
	{
		public bool Enabled;
		public string ConditionTriggerId;
		public string ActiveTriggerId = "";
		public List<QuestionOption> QuestionOptions;
		public List<QuestionOption> QuestionAnswers;
		public string QuestionId { get; set; }
		public string QuestionParentId { get; set; }
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string QuestionKeyboardType { get; set; }
		public bool Answered { get; set; }
		public bool Disabled { get; set; }
		public string AnswerId { get; set; }
		public string AnswerText { get; set; }
		public string ParentConditionTriggerId { get; set; }
		public bool IsConditional { get; set; }

		static NSString skey = new NSString("NullableDateTimeElementInline");
		public DateTime? DateValue;
		public event NSAction DateSelected;
		public event NSAction PickerClosed;
		public event NSAction PickerOpened;
		//InlineDateElement _inline_date_element = null;
		InlineDateElement _inline_date_element;
		//private bool _picker_present = false;
		bool _picker_present;

		public NullableDateElementInline(string caption, DateTime? date)
			: base(caption)
		{
			DateValue = date;
			Value = FormatDate(date);
		}

		/// <summary>
		/// Returns true iff the picker is currently open
		/// </summary>
		/// <returns></returns>
		public bool IsPickerOpen()
		{
			return _picker_present;
		}

		protected internal NSDateFormatter fmt = new NSDateFormatter
		{
			DateStyle = NSDateFormatterStyle.Medium
		};

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (fmt != null)
				{
					fmt.Dispose();
					fmt = null;
				}
			}
		}

		public virtual string FormatDate(DateTime? dt)
		{
			if (!dt.HasValue)
				return " ";

			dt = GetDateWithKind(dt);
			return fmt.ToString((NSDate)dt);
		}

		protected DateTime? GetDateWithKind(DateTime? dt)
		{
			if (!dt.HasValue)
				return dt;

			if (dt.Value.Kind == DateTimeKind.Unspecified)
				return DateTime.SpecifyKind(dt.Value, DateTimeKind.Local);

			return dt;
		}

		public void ClosePickerIfOpen(DialogViewController dvc)
		{
			if (_picker_present)
			{
				var index_path = IndexPath;
				var table_view = GetContainerTableView();

				Selected(dvc, table_view, index_path);
			}
		}

		public void SetDate(DateTime? date)
		{
			DateValue = date;
			AnswerText = date.ToString();
			Value = FormatDate(date);
			var r = GetImmediateRootElement();
			r.Reload(this, UITableViewRowAnimation.None);
		}

		public override void Selected(DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			TogglePicker(dvc, tableView, indexPath);

			// Deselect the row so the row highlint tint fades away.
			tableView.DeselectRow(indexPath, true);
		}

		/// <summary>
		/// Shows or hides the nullable picker
		/// </summary>
		/// <param name="dvc"></param>
		/// <param name="tableView"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		private void TogglePicker(DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var sectionAndIndex = GetMySectionAndIndex(dvc);
			if (sectionAndIndex.Key != null)
			{
				Section section = sectionAndIndex.Key;
				int index = sectionAndIndex.Value;

				var cell = tableView.CellAt(path);

				if (_picker_present)
				{
					// Remove the picker.
					cell.DetailTextLabel.TextColor = UIColor.Black;
					section.Remove(_inline_date_element);
					_picker_present = false;
					if (PickerClosed != null)
						PickerClosed();
				}
				else
				{
					// Show the picker.
					cell.DetailTextLabel.TextColor = UIColor.Red;
					_inline_date_element = new InlineDateElement(DateValue);

					_inline_date_element.DateSelected += (DateTime? date) =>
					{
						DateValue = date;
						cell.DetailTextLabel.Text = FormatDate(date);
						Value = cell.DetailTextLabel.Text;
						if (DateSelected != null)       // Fire our changed event.
						DateSelected();
					};

					_inline_date_element.ClearPressed += () =>
					{
						DateTime? null_date = null;
						DateValue = null_date;
						cell.DetailTextLabel.Text = " ";
						Value = cell.DetailTextLabel.Text;
						cell.DetailTextLabel.TextColor = UIColor.Black;
						section.Remove(_inline_date_element);
						_picker_present = false;
						if (PickerClosed != null)
							PickerClosed();
					};

					section.Insert(index + 1, UITableViewRowAnimation.Bottom, _inline_date_element);
					_picker_present = true;
					tableView.ScrollToRow(_inline_date_element.IndexPath, UITableViewScrollPosition.None, true);

					if (PickerOpened != null)
						PickerOpened();
				}
			}
		}

		/// <summary>
		/// Locates this instance of this Element within a given DialogViewController.
		/// </summary>
		/// <returns>The Section instance and the index within that Section of this instance.</returns>
		/// <param name="dvc">Dvc.</param>
		private KeyValuePair<Section, int> GetMySectionAndIndex(DialogViewController dvc)
		{
			foreach (var section in dvc.Root)
			{
				for (int i = 0; i < section.Count; i++)
				{
					if (section[i] == this)
					{
						return new KeyValuePair<Section, int>(section, i);
					}
				}
			}
			return new KeyValuePair<Section, int>();
		}

		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = base.GetCell(tv);
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			cell.DetailTextLabel.Font = UIFont.SystemFontOfSize(14);

			cell.DetailTextLabel.TextAlignment = UITextAlignment.Left;

			cell.UserInteractionEnabled = Enabled;

			cell.DetailTextLabel.TextColor = UIColor.Black;

			cell.TextLabel.TextColor = UIColor.FromRGB(200, 200, 205);
			cell.BackgroundColor = UIColor.White;

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}

				return cell;
		}

		/// <summary>
		/// Class that has the UIDatePicker and a button for clearing/cancelling
		/// </summary>
		public class InlineDateElement : Element, IElementSizing
		{
			public UIDatePicker _date_picker;
			private UIButton _clear_cancel_button;

			static NSString skey = new NSString("InlineDateElement");

			public event Action<DateTime?> DateSelected;
			public event Action ClearPressed;

			private DateTime? _current_date;
			private SizeF _picker_size;
			private readonly SizeF _cell_size;

			public InlineDateElement(DateTime? current_date)
				: base("")
			{
				_current_date = current_date;
				_date_picker = new UIDatePicker();
				_date_picker.Mode = UIDatePickerMode.Date;
				_picker_size = (SizeF)_date_picker.SizeThatFits(SizeF.Empty);
				_cell_size = _picker_size;
				_cell_size.Height += 30f; // Add a little bit for the clear button
			}

			/// <summary>
			/// Returns the cell, with some additions
			/// </summary>
			/// <param name="tv"></param>
			/// <returns></returns>
			public override UITableViewCell GetCell(UITableView tv)
			{
				Debug.Assert(_date_picker != null);

				var cell = base.GetCell(tv);

				if (!_current_date.HasValue && DateSelected != null)
					DateSelected(DateTime.Now);
				else if (_current_date.HasValue)
					_date_picker.Date = NSDateExtensions.ToNSDate((DateTime)_current_date);

				_date_picker.ValueChanged += (object sender, EventArgs e) =>
				{
					if (DateSelected != null)
						DateSelected((DateTime?)_date_picker.Date);
				};

				if (_clear_cancel_button == null)
				{
					_clear_cancel_button = UIButton.FromType(UIButtonType.RoundedRect);
					_clear_cancel_button.SetTitle("Clear", UIControlState.Normal);
				}
				_clear_cancel_button.Frame = new RectangleF((float)(tv.Frame.Width / 2 - 20f), _cell_size.Height - 40f, 40f, 40f);
				_date_picker.Frame = new RectangleF((float)(tv.Frame.Width / 2 - _picker_size.Width / 2), _cell_size.Height / 2 - _picker_size.Height / 2, _picker_size.Width, _picker_size.Height);
				_clear_cancel_button.TouchUpInside += (object sender, EventArgs e) =>
				{
				// Clear button pressed. 
				if (ClearPressed != null)
						ClearPressed();
				};

				cell.AddSubview(_date_picker);

				cell.AddSubview(_clear_cancel_button);

				return cell;
			}

			/// <summary>
			/// Returns the height of the cell
			/// </summary>
			/// <param name="tableView"></param>
			/// <param name="indexPath"></param>
			/// <returns></returns>
			public nfloat GetHeight(UITableView tableView, NSIndexPath indexPath)
			{
				return _cell_size.Height;
			}
		}
	}



	public static class NSDateExtensions
	{
		static readonly DateTime reference = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

		public static DateTime ToDateTime(this NSDate date)
		{
			var utcDateTime = reference.AddSeconds(date.SecondsSinceReferenceDate);
			var dateTime = utcDateTime.ToLocalTime();
			return dateTime;
		}

		public static NSDate ToNSDate(this DateTime datetime)
		{
			var utcDateTime = datetime.ToUniversalTime();
			var date = NSDate.FromTimeIntervalSinceReferenceDate((utcDateTime - reference).TotalSeconds);
			return date;
		}
	}



	public class DynaSlider : Element
	{
		public bool Enabled;
		public string ConditionTriggerId;
		public string ActiveTriggerId = "";
		public List<QuestionOption> QuestionOptions;
		public List<QuestionOption> QuestionAnswers;
		public string QuestionId { get; set; }
		public string QuestionParentId { get; set; }
		public string QuestionText { get; set; }
		public string QuestionType { get; set; }
		public string QuestionKeyboardType { get; set; }
		public bool Answered { get; set; }
		public bool Disabled { get; set; }
		public string AnswerId { get; set; }
		public string AnswerText { get; set; }
		public string ParentConditionTriggerId { get; set; }
		public bool IsConditional { get; set; }

		public SectionQuestion SQuestion;
		public bool ShowCaption;
		public float Value;
		public float MinValue, MaxValue;
		static NSString skey = new NSString("FloatElement");
		//UIImage Left, Right;
		UISlider slider;

		public DynaSlider(float value, SectionQuestion sQuestion) : this(null, null, value, sQuestion)
		{
			Value = value;
			SQuestion = sQuestion;
		}

		public DynaSlider(UIImage left, UIImage right, float value, SectionQuestion sQuestion) : base(null)
		{
			//Left = left;
			//Right = right;
			MinValue = 0;
			MaxValue = 1;
			Value = value;
			SQuestion = sQuestion;
				
		}

		protected override NSString CellKey
		{
			get
			{
				return skey;
			}
		}
		public override UITableViewCell GetCell(UITableView tv)
		{
			var cell = tv.DequeueReusableCell(CellKey);
			if (cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Default, CellKey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			}
			else
				RemoveTag(cell, 1);

			var captionSize = new CGSize(0, 0);
			if (Caption != null && ShowCaption)
			{
				cell.TextLabel.Text = Caption;
				captionSize = Caption.StringSize(UIFont.FromName(cell.TextLabel.Font.Name, UIFont.LabelFontSize));
				captionSize.Width += 10; // Spacing
			}
			if (slider == null)
			{
				slider = new UISlider(new CGRect(10f + captionSize.Width, UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? 18f : 12f, cell.ContentView.Bounds.Width - 20 - captionSize.Width, 7f))
				{
					 
					BackgroundColor = UIColor.Clear,
					MinValue = MinValue,
					MaxValue = MaxValue,
					Continuous = true,
					Value = Value,
					Tag = 1,
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth
				};
				slider.ValueChanged += delegate
				{
					Value = (int)slider.Value;
					SQuestion.AnswerText = Value.ToString();
					AnswerText = Value.ToString();
					Caption = Value.ToString();
					captionSize = Caption.StringSize(UIFont.FromName(cell.TextLabel.Font.Name, UIFont.LabelFontSize));
					captionSize.Width += 10; // Spacing
					cell.TextLabel.Text = Caption;
					slider.Frame = new CGRect(10f + captionSize.Width, UIDevice.CurrentDevice.CheckSystemVersion(7, 0) ? 18f : 12f, cell.ContentView.Bounds.Width - 20 - captionSize.Width, 7f);
				};
			}
			else {
				slider.Value = Value;
			}

			cell.ContentView.AddSubview(slider);

			cell.UserInteractionEnabled = Enabled;
			cell.TextLabel.TextColor = UIColor.Black;
			//cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(17);
			cell.BackgroundColor = UIColor.White;

			if (!Enabled)
			{
				cell.TextLabel.TextColor = UIColor.LightGray;
				cell.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
			}

			return cell;
		}

		public override string Summary()
		{
			return Value.ToString();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (slider != null)
				{
					slider.Dispose();
					slider = null;
				}
			}
		}
	}



	public class LoadingOverlay : UIView
	{
		// control declarations
		UIActivityIndicatorView activitySpinner;
		UILabel loadingLabel;

		public LoadingOverlay(CGRect frame) : base(frame)
		{
			// configurable bits
			BackgroundColor = UIColor.Black;
			Alpha = 0.75f;
			AutoresizingMask = UIViewAutoresizing.All;

			nfloat labelHeight = 22;
			nfloat labelWidth = Frame.Width - 20;

			// derive the center x and y
			nfloat centerX = Frame.Width / 2;
			nfloat centerY = Frame.Height / 2;

			// create the activity spinner, center it horizontall and put it 5 points above center x
			activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
			activitySpinner.Frame = new CGRect(
				centerX - (activitySpinner.Frame.Width / 2),
				centerY - activitySpinner.Frame.Height - 20,
				activitySpinner.Frame.Width,
				activitySpinner.Frame.Height);
			activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
			AddSubview(activitySpinner);
			activitySpinner.StartAnimating();

			// create and configure the "Loading Data" label
			loadingLabel = new UILabel(new CGRect(
				centerX - (labelWidth / 2),
				centerY + 20,
				labelWidth,
				labelHeight
				));
			loadingLabel.BackgroundColor = UIColor.Clear;
			loadingLabel.TextColor = UIColor.White;
			loadingLabel.Text = "Loading Data...";
			loadingLabel.TextAlignment = UITextAlignment.Center;
			loadingLabel.AutoresizingMask = UIViewAutoresizing.All;
			AddSubview(loadingLabel);

		}

		/// <summary>
		/// Fades out the control and then removes it from the super view
		/// </summary>
		public void Hide()
		{
			Animate(
				0.5, // duration
				() => { Alpha = 0; },
				() => { RemoveFromSuperview(); }
			);
		}
	}



}

