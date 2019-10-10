using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Rewrite;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace dotnetfoundation.Helpers
{
	public class UrlStandardizationRule : IRule
	{
		public virtual void ApplyRule(RewriteContext context)
		{
			if (!context.HttpContext.Request.Path.HasValue || context.HttpContext.Request.Path.Value == "/")
			{
				return;
			}

			var path = context.HttpContext.Request.Path.Value;
			var containsUpper = !string.IsNullOrWhiteSpace(path) && path.Any(c => Char.IsUpper(c));
			var endWithSlash = path.EndsWith('/');

			if (containsUpper || endWithSlash)
			{
				if (endWithSlash)
				{
					path = path.Remove(path.Length - 1);
				}

				if (containsUpper)
				{
					path = path.ToLowerInvariant();
				}

				var response = context.HttpContext.Response;
				response.StatusCode = StatusCodes.Status301MovedPermanently;
				context.Result = RuleResult.EndResponse;
				response.Headers[HeaderNames.Location] = context.HttpContext.Request.PathBase + path + context.HttpContext.Request.QueryString;

				context.Logger?.LogInformation($"Request was redirected to {path}");
			}
		}
	}
}