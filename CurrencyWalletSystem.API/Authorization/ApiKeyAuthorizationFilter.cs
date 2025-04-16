using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyWalletSystem.API.Authorization
{
    /// <summary>
    /// Filter to authorize API requests using an API key.
    /// </summary>
    public class ApiKeyAuthorizationFilter : IActionFilter
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyAuthorizationFilter"/> class.
        /// </summary>
        /// <param name="configuration">The configuration instance to retrieve the API key from.</param>
        public ApiKeyAuthorizationFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Called before the action executes, to check the API key.
        /// </summary>
        /// <param name="context">The context for the action.</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var endpoint = context.ActionDescriptor.EndpointMetadata;
            if (endpoint.Any(meta => meta is AllowAnonymousAttribute))
            {
                return;
            }

            var apiKey = _configuration["ApiSettings:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                context.Result = new UnauthorizedObjectResult("API key is missing in configuration.");
                return;
            }

            if (!context.HttpContext.Request.Headers.TryGetValue("x-api-key", out var providedKey) || providedKey != apiKey)
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized: Invalid API Key");
            }
        }

        /// <summary>
        /// Called after the action executes.
        /// </summary>
        /// <param name="context">The context for the action.</param>
        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
