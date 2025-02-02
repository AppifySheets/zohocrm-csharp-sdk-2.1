﻿using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Com.Zoho.API.Exception
{
    /// <summary>
    /// This class is the common SDKException object. This stands as a POJO for the SDKException thrown.
    /// </summary>
    public class SDKException : System.Exception
    {
        string code;

        string message;

        System.Exception cause;

        JObject details;

        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="code">A string containing the Exception error code.</param>
        /// <param name="message">A string containing the Exception error message.</param>
        /// <param name="cause">An Exception class instance.</param>
        public SDKException(string code, string message, System.Exception cause) : this(code: code, message: message, details: null, cause: cause) { }


        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="code">A string containing the Exception error code.</param>
        /// <param name="message">A string containing the Exception error message.</param>
        /// <param name="details">A JSONObject containing the error response.</param>
        public SDKException(string code, string message, JObject details) : this(code, message: message, details: details, cause: null) { }

        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="code">A string containing the Exception error code.</param>
        /// <param name="details">A JSONObject containing the error response.</param>
        public SDKException(string code, JObject details) : this(code, message: null, details: details, cause: null) { }

        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="code">A string containing the Exception error code.</param>
        /// <param name="message">A string containing the Exception error message.</param>
        public SDKException(string code, string message) : this(code: code, message: message, details: null, cause: null) { }

        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="cause">An Exception class instance.</param>
        public SDKException(System.Exception cause) : this(null, cause.Message, cause) { }

        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="message">A string containing the Exception error message.</param>
        /// <param name="cause">An Exception class instance.</param>
        public SDKException(string message, System.Exception cause) : this(code: null, message: message, details: null, cause: cause) { }


        /// <summary>
        /// Creates an SDKException class instance with the specified parameters.
        /// </summary>
        /// <param name="code">A string containing the Exception error code.</param>
        /// <param name="message">A string containing the Exception error message.</param>
        /// <param name="details">A JObject containing the error response.</param>
        /// <param name="cause">An Exception class instance.</param>
        public SDKException(string code, string message, JObject details,  System.Exception cause) : base(message, cause)
        {
            this.code = code;

            this.message = message;

            this.details = details;

            this.cause = cause;
        }

        /// <summary>
        /// This is a getter method to get Exception error code.
        /// </summary>
        /// <returns>A String representing the Exception error code.</returns>
        public string Code
        {
            get
            {
                return code;
            }
        }

        /// <summary>
        /// This is a getter method to get Exception error message.
        /// </summary>
        /// <returns>A String representing the Exception error message.</returns>
        public override string Message
        {
            get
            {
                return message;
            }
        }

        /// <summary>
        /// This is a getter method to get Exception class instance.
        /// </summary>
        /// <returns>A Exception class instance.</returns>
        public System.Exception Cause
        {
            get
            {
                return cause;
            }
        }

        /// <summary>
        /// This is a getter method to get error response JSONObject.
        /// </summary>
        /// <returns>A JSONObject representing the error response.</returns>
        public JObject Details
        {
            get
            {
                return details;
            }
        }

        public override string ToString()
        {
            var returnMsg = typeof(SDKException).FullName + ". Caused by : ";

            if (details != null)
            {
                message = message != null ? message + JsonConvert.SerializeObject(details) : JsonConvert.SerializeObject(details);
            }

            if (Code != null)
            {
                returnMsg += Code + " - " + Message;
            }
            else
            {
                returnMsg += Message;
            }

            return returnMsg;
        }
    }
}
