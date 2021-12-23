using Com.Zoho.Crm.API.Util;
using System.Collections.Generic;

namespace Com.Zoho.Crm.API.File
{

	public class APIException : Model, ActionResponse, ActionHandler, ResponseHandler
	{
		Choice<string> status;
		Choice<string> code;
		Choice<string> message;
		Dictionary<string, object> details;
		Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public Choice<string> Status
		{
			/// <summary>The method to get the status</summary>
			/// <returns>Instance of Choice<String></returns>
			get
			{
				return  status;

			}
			/// <summary>The method to set the value to status</summary>
			/// <param name="status">Instance of Choice<string></param>
			set
			{
				 status=value;

				 keyModified["status"] = 1;

			}
		}

		public Choice<string> Code
		{
			/// <summary>The method to get the code</summary>
			/// <returns>Instance of Choice<String></returns>
			get
			{
				return  code;

			}
			/// <summary>The method to set the value to code</summary>
			/// <param name="code">Instance of Choice<string></param>
			set
			{
				 code=value;

				 keyModified["code"] = 1;

			}
		}

		public Choice<string> Message
		{
			/// <summary>The method to get the message</summary>
			/// <returns>Instance of Choice<String></returns>
			get
			{
				return  message;

			}
			/// <summary>The method to set the value to message</summary>
			/// <param name="message">Instance of Choice<string></param>
			set
			{
				 message=value;

				 keyModified["message"] = 1;

			}
		}

		public Dictionary<string, object> Details
		{
			/// <summary>The method to get the details</summary>
			/// <returns>Dictionary representing the details<String,Object></returns>
			get
			{
				return  details;

			}
			/// <summary>The method to set the value to details</summary>
			/// <param name="details">Dictionary<string,object></param>
			set
			{
				 details=value;

				 keyModified["details"] = 1;

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