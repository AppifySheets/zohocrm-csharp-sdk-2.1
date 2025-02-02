using Com.Zoho.Crm.API.Util;

namespace Com.Zoho.Crm.API.Territories
{

	public class TerritoriesOperations
	{
		/// <summary>The method to get territories</summary>
		/// <returns>Instance of APIResponse<ResponseHandler></returns>
		public APIResponse<ResponseHandler> GetTerritories()
		{
			var handlerInstance=new CommonAPIHandler();

			var apiPath="";

			apiPath=string.Concat(apiPath, "/crm/v2.1/settings/territories");

			handlerInstance.APIPath=apiPath;

			handlerInstance.HttpMethod=Constants.REQUEST_METHOD_GET;

			handlerInstance.CategoryMethod=Constants.REQUEST_CATEGORY_READ;

			return handlerInstance.APICall<ResponseHandler>(typeof(ResponseHandler), "application/json");


		}

		/// <summary>The method to get territory</summary>
		/// <param name="id">long?</param>
		/// <returns>Instance of APIResponse<ResponseHandler></returns>
		public APIResponse<ResponseHandler> GetTerritory(long? id)
		{
			var handlerInstance=new CommonAPIHandler();

			var apiPath="";

			apiPath=string.Concat(apiPath, "/crm/v2.1/settings/territories/");

			apiPath=string.Concat(apiPath, id.ToString());

			handlerInstance.APIPath=apiPath;

			handlerInstance.HttpMethod=Constants.REQUEST_METHOD_GET;

			handlerInstance.CategoryMethod=Constants.REQUEST_CATEGORY_READ;

			return handlerInstance.APICall<ResponseHandler>(typeof(ResponseHandler), "application/json");


		}


	}
}