using System;
using System.Collections.Generic;
using System.Json;
using MonoTouch.Dialog;

namespace DynaPad
{
	public static class JsonHandler
	{
		public static string OriginalFormJsonString { get; set; }
		public static JsonElement OriginalFormJson
		{
			get;
			set;
		}
		public static List<Section> FormSections
		{
			get;
			set;
		}

		public static List<Section> FormQuestions
		{
			get;
			set;
		}
	}
}

