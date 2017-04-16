using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoreGraphics;
//using DynaClassLibrary;
using UIKit;

namespace DynaPad
{
	public static class CommonFunctions
	{
		//public static CommonFunctions()
		//{
		//}

		public static DynaPadService.ConfigurationObjects GetUserConfig()
		{
			
			try
			{
				var UserConfig = new DynaPadService.ConfigurationObjects()
				{
					EmailSupport = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailSupport,
					EmailPostmaster = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailPostmaster,
					EmailRoy = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailRoy,
					EmailSmtp = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailSmtp,
					EmailUser = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailUser,
					EmailPass = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailPass,
					EmailPort = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.EmailPort,
					ConnectionString = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.ConnectionString,
					ConnectionName = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.ConnectionName,
					DatabaseName = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DatabaseName,
					DomainRootPathVirtual = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainRootPathVirtual,
					DomainRootPathPhysical = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainRootPathPhysical,
					DomainClaimantsPathVirtual = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainClaimantsPathVirtual,
					DomainClaimantsPathPhysical = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainClaimantsPathPhysical,
					DomainHost = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainHost
					//DomainPaths = DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainPaths.Select(dPath => new DynaClassLibrary.DynaClasses.DomainPath[]
					//{
					//	DomainPathName = dPath.DomainPathName,
					//	DomainPathVirtual = dPath.DomainPathVirtual,
					//	DomainPathPhysical = dPath.DomainPathPhysical
					//}).ToArray()
				};

				var domList = new List<DynaPadService.DomainPath>();
                foreach (var dPath in DynaClassLibrary.DynaClasses.LoginContainer.User.DynaConfig.DomainPaths)
                {
                    var ServiceDomainPath = new DynaPadService.DomainPath
					{
						DomainPathName = dPath.DomainPathName,
						DomainPathVirtual = dPath.DomainPathVirtual,
						DomainPathPhysical = dPath.DomainPathPhysical
					};
				domList.Add(ServiceDomainPath);
                }
                UserConfig.DomainPaths = domList.ToArray();
				//foreach (var item in domList)
				//{
				//	UserConfig.DomainPaths.Add(item);
				//}

				return UserConfig;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message + Environment.NewLine + ex.StackTrace, ex.InnerException);
			}
		}

		public static UIAlertController AlertPrompt(string alertTitle, string alertMessage, bool OKButton, Action<UIAlertAction> OKAction, bool CancelButton, Action<UIAlertAction> CancelAction)
		{
			var prompt = UIAlertController.Create(alertTitle, alertMessage, UIAlertControllerStyle.Alert);
			if (OKButton)
			{
				prompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, OKAction));
			}
			if (CancelButton)
			{
				prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, CancelAction));
			}

			return prompt;
		}

		public static UIAlertController InternetAlertPrompt()
		{
			var prompt = UIAlertController.Create("No Internet Connection", "An active internet connection is required", UIAlertControllerStyle.Alert);
			prompt.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
			return prompt;
		}
	}
}
