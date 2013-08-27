using System;

namespace TemplateEngine
{
	class TemplateFormatException : Exception
	{
		public TemplateFormatException(string message)
		{
		}
	}

	class RemoteServiceException : Exception
	{
		public RemoteServiceException(string message)
		{
		}
	}

	class BadCodeException : Exception
	{
		public BadCodeException(string message)
		{
		}
	}
}

