using Com.Zoho.Crm.API.Util;
using System.Collections.Generic;

namespace Com.Zoho.Crm.API.Users
{

	public class ActionWrapper : Model, ActionHandler
	{
		List<ActionResponse> users;
		Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public List<ActionResponse> Users
		{
			/// <summary>The method to get the users</summary>
			/// <returns>Instance of List<ActionResponse></returns>
			get
			{
				return  users;

			}
			/// <summary>The method to set the value to users</summary>
			/// <param name="users">Instance of List<ActionResponse></param>
			set
			{
				 users=value;

				 keyModified["users"] = 1;

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