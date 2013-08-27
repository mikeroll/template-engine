using System;
using System.Collections.Generic;
using System.Xml;

namespace TemplateEngine
{
	public class IdeoneJob
	{
		public static string Username
		{ 
			get;
			private set;
		}

		public static string ApiPassword
		{
			get;
			private set;
		}

		public Ideone_Service_v1Service client;

		public string Code
		{
			get; 
			set; 
		}

		public string Output
		{ 
			get;
			private set; 
		}

		public int Lang
		{
			get; 
			set; 
		}

		/// <summary>
		/// Initializes a new Ideone job"/> class.
		/// </summary>
		/// <param name="code">Code.</param>
		/// <param name="lang">Lang.</param>
		public IdeoneJob(string code, int lang)
		{
			client = new Ideone_Service_v1Service();
			Code = code;
			Lang = lang;
		}

		/// <summary>
		/// Executes the job.
		/// </summary>
		/// <returns>Output of compiled program.</returns>
		public string Execute()
		{
			var returnedData = client.createSubmission(
				user: Username,
				pass: ApiPassword,
				sourceCode: Code,
				language: Lang,
				input: null,
				run: true,
				@private: false
			);
			var result = FilterResult(returnedData);
			if (result["error"] != "OK")
				throw new RemoteServiceException("createSubmission: " + result["error"]);
			var link = result["link"];

			do
			{
				System.Threading.Thread.Sleep(1000);
				returnedData = client.getSubmissionDetails(
					Username, 
					ApiPassword, 
					link,
					withSource: false,
					withInput: false,
					withOutput: true,
					withStderr: false,
					withCmpinfo: false
				);
				result = FilterResult(returnedData);
			} while (result["status"] != "0");
			if (result["result"] == "15")
				Output = result["output"];
			else
				throw new BadCodeException("Failed to execute properly");
			return Output;
		}

		/// <summary>
		/// Sets Ideone credentials class-wide.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="apiPassword">API password.</param>
		public static void Authorize(string username, string apiPassword)
		{
			Username = username;
			ApiPassword = apiPassword;
		}

		/// <summary>
		/// Tests the access to Ideone server.
		/// </summary>
		/// <returns>Error code.</returns>
		public string TestAccess()
		{
			object[] returnedData = client.testFunction(Username, ApiPassword);
			Dictionary<string, string> result = FilterResult(returnedData);
			return result["error"];
		}

		/// <summary>
		/// Extracts data fields from XML formatted response.
		/// </summary>
		/// <returns>Dictionary of key:value items.</returns>
		/// <param name="returnedData">Response from Ideone.</param>
		Dictionary<string, string> FilterResult(object[] returnedData)
		{
			var result = new Dictionary<string, string>();
			foreach (object o in returnedData)
			{
				if (o is XmlElement)
				{
					XmlNodeList x = ((XmlElement)o).ChildNodes;
					result.Add(x.Item(0).InnerText, x.Item(1).InnerText);
				}
			}
			return result;
		}
	}
}

