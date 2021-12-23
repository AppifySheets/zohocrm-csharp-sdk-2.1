using Com.Zoho.Crm.API.Fields;
using Com.Zoho.Crm.API.Util;
using System.Collections.Generic;

namespace Com.Zoho.Crm.API.BulkRead
{

	public class Criteria : Model
	{
		string apiName;
		object value;
		Choice<string> groupOperator;
		List<Criteria> group;
		Field field;
		Choice<string> comparator;
		Dictionary<string, int?> keyModified=new Dictionary<string, int?>();

		public string APIName
		{
			/// <summary>The method to get the aPIName</summary>
			/// <returns>string representing the apiName</returns>
			get
			{
				return  apiName;

			}
			/// <summary>The method to set the value to aPIName</summary>
			/// <param name="apiName">string</param>
			set
			{
				 apiName=value;

				 keyModified["api_name"] = 1;

			}
		}

		public object Value
		{
			/// <summary>The method to get the value</summary>
			/// <returns>object representing the value</returns>
			get
			{
				return  value;

			}
			/// <summary>The method to set the value to value</summary>
			/// <param name="value">object</param>
			set
			{
				 this.value=value;

				 keyModified["value"] = 1;

			}
		}

		public Choice<string> GroupOperator
		{
			/// <summary>The method to get the groupOperator</summary>
			/// <returns>Instance of Choice<String></returns>
			get
			{
				return  groupOperator;

			}
			/// <summary>The method to set the value to groupOperator</summary>
			/// <param name="groupOperator">Instance of Choice<string></param>
			set
			{
				 groupOperator=value;

				 keyModified["group_operator"] = 1;

			}
		}

		public List<Criteria> Group
		{
			/// <summary>The method to get the group</summary>
			/// <returns>Instance of List<Criteria></returns>
			get
			{
				return  group;

			}
			/// <summary>The method to set the value to group</summary>
			/// <param name="group">Instance of List<Criteria></param>
			set
			{
				 group=value;

				 keyModified["group"] = 1;

			}
		}

		public Field Field
		{
			/// <summary>The method to get the field</summary>
			/// <returns>Instance of Field</returns>
			get
			{
				return  field;

			}
			/// <summary>The method to set the value to field</summary>
			/// <param name="field">Instance of Field</param>
			set
			{
				 field=value;

				 keyModified["field"] = 1;

			}
		}

		public Choice<string> Comparator
		{
			/// <summary>The method to get the comparator</summary>
			/// <returns>Instance of Choice<String></returns>
			get
			{
				return  comparator;

			}
			/// <summary>The method to set the value to comparator</summary>
			/// <param name="comparator">Instance of Choice<string></param>
			set
			{
				 comparator=value;

				 keyModified["comparator"] = 1;

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