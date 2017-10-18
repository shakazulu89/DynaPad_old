using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace DynaPad
{
public class BaseTableViewController : UITableViewController
{
	protected const string cellIdentifier = "cellID";

	public BaseTableViewController()
	{
	}

	public BaseTableViewController(IntPtr handle) : base(handle)
	{
	}

		protected void ConfigureCell(UITableViewCell cell, Appointment product)
	{
		cell.TextLabel.Text = product.Title;
		string detailedStr = string.Format("{0:C} | {1}", product.IntroPrice, product.YearIntroduced);
		cell.DetailTextLabel.Text = detailedStr;
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();
		TableView.RegisterNibForCellReuse(UINib.FromName("TableCell", null), cellIdentifier);
	}	}
	public class ResultsTableController : BaseTableViewController
	{
		public List<Appointment> FilteredProducts { get; set; }

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return FilteredProducts.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			Appointment product = FilteredProducts[indexPath.Row];
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
			ConfigureCell(cell, product);
			return cell;
		}
	}
}
