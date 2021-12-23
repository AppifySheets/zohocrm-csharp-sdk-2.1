using Com.Zoho.Crm.API.Util;
using System.Collections.Generic;

namespace Com.Zoho.Crm.API.BulkRead
{

	public class ActionWrapper : Model, ActionHandler
	{
		List<ActionResponse> data;
		Dictionary<string, object> info;
		Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public List<ActionResponse> Data
		{
			/// <summary>The method to get the data</summary>
			/// <returns>Instance of List<ActionResponse></returns>
			get
			{
				return  data;

			}
			/// <summary>The method to set the value to data</summary>
			/// <param name="data">Instance of List<ActionResponse></param>
			set
			{
				 data=value;

				 keyModified["data"] = 1;

			}
		}

		public Dictionary<string, object> Info
		{
			/// <summary>The method to get the info</summary>
			/// <returns>Dictionary representing the info<String,Object></returns>
			get
			{
				return  info;

			}
			/// <summary>The method to set the value to info</summary>
			/// <param name="info">Dictionary<string,object></param>
			set
			{
				 info=value;

				 keyModified["info"] = 1;

			}
		}

		/// <summary>The method to check if the user has modified the given key</summary>
		/// <param name="key">string</param>
		/// <returns>int? representing the modification</returns>
		public int? IsKeyModified(string key)
		{
			if((( keyModified.ContainsKey(key))))
			{
				return  keyModified[key];

			}
			return null;


		}

		/// <summary>The method to mark the given key as modified</summary>
		/// <param name="key">string</param>
		/// <param name="modification">int?</param>
		public void SetKeyModified(string key, int? modification)
		{
			 keyModified[key] = modification;


		}


	}
}