using Com.Zoho.Crm.API;
using Com.Zoho.Crm.API.Util;

namespace Com.Zoho.Crm.API.AssignmentRules
{

	public class AssignmentRulesOperations
	{
		/// <summary>The method to get assignment rules</summary>
		/// <returns>Instance of APIResponse<ResponseHandler></returns>
		public APIResponse<ResponseHandler> GetAssignmentRules()
		{
			var handlerInstance=new CommonAPIHandler();

			var apiPath="";

			apiPath=string.Concat(apiPath, "/crm/v2.1/settings/automation/assignment_rules");

			handlerInstance.APIPath=apiPath;

			handlerInstance.HttpMethod=Constants.REQUEST_METHOD_GET;

			handlerInstance.CategoryMethod=Constants.REQUEST_CATEGORY_READ;

			return handlerInstance.APICall<ResponseHandler>(typeof(ResponseHandler), "application/json");


		}

		/// <summary>The method to get assignment rule</summary>
		/// <param name="ruleId">long?</param>
		/// <param name="paramInstance">Instance of ParameterMap</param>
		/// <returns>Instance of APIResponse<ResponseHandler></returns>
		public APIResponse<ResponseHandler> GetAssignmentRule(long? ruleId, ParameterMap paramInstance)
		{
			var handlerInstance=new CommonAPIHandler();

			var apiPath="";

			apiPath=string.Concat(apiPath, "/crm/v2.1/settings/automation/assignment_rules/");

			apiPath=string.Concat(apiPath, ruleId.ToString());

			handlerInstance.APIPath=apiPath;

			handlerInstance.HttpMethod=Constants.REQUEST_METHOD_GET;

			handlerInstance.CategoryMethod=Constants.REQUEST_CATEGORY_READ;

			handlerInstance.Param=paramInstance;

			return handlerInstance.APICall<ResponseHandler>(typeof(ResponseHandler), "application/json");


		}


		public static class GetAssignmentRuleParam
		{
			public static readonly Param<string> MODULE=new Param<string>("module", "com.zoho.crm.api.AssignmentRules.GetAssignmentRuleParam");
		}

	}
}