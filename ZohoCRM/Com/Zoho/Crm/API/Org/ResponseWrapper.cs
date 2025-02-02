using Com.Zoho.Crm.API.Util;
using System.Collections.Generic;

namespace Com.Zoho.Crm.API.Org
{

	public class ResponseWrapper : Model, ResponseHandler
	{
		List<Org> org;
		Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public List<Org> Org
		{
			/// <summary>The method to get the org</summary>
			/// <returns>Instance of List<Org></returns>
			get
			{
				return  org;

			}
			/// <summary>The method to set the value to org</summary>
			/// <param name="org">Instance of List<Org></param>
			set
			{
				 org=value;

				 keyModified["org"] = 1;

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