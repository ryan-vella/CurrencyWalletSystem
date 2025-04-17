using CurrencyWalletSystem.API.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Configuration;

namespace CurrencyWalletSystem.Tests.Api.Authorization
{
    public class ApiKeyAuthorizationFilterTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ApiKeyAuthorizationFilter _filter;

        public ApiKeyAuthorizationFilterTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _filter = new ApiKeyAuthorizationFilter(_mockConfiguration.Object);
        }

        [Fact]
        public void OnActionExecuting_ShouldProceed_WhenApiKeyIsValid()
        {
            // Arrange
            var validApiKey = "correct-key";
            _mockConfiguration.Setup(c => c["ApiSettings:ApiKey"]).Returns(validApiKey);

            var context = CreateActionExecutingContext(new Dictionary<string, string>
                {
                    { "x-api-key", validApiKey }
                });

            // Act
            _filter.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result); // No response means request proceeds
        }

        [Fact]
        public void OnActionExecuting_ShouldReturnUnauthorized_WhenApiKeyIsMissing()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["ApiSettings:ApiKey"]).Returns("correct-key");

            var context = CreateActionExecutingContext(new Dictionary<string, string>());

            // Act
            _filter.OnActionExecuting(context);

            // Assert
            var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
            Assert.Equal("Unauthorized: Invalid API Key", result.Value);
        }

        [Fact]
        public void OnActionExecuting_ShouldReturnUnauthorized_WhenApiKeyIsIncorrect()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["ApiSettings:ApiKey"]).Returns("correct-key");

            var context = CreateActionExecutingContext(new Dictionary<string, string>
            {
                { "x-api-key", "wrong-key" }
            });

            // Act
            _filter.OnActionExecuting(context);

            // Assert
            var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
            Assert.Equal("Unauthorized: Invalid API Key", result.Value);
        }

        [Fact]
        public void OnActionExecuting_ShouldReturnUnauthorized_WhenApiKeyIsNotConfigured()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["ApiSettings:ApiKey"]).Returns((string)null); // No API key set

            var context = CreateActionExecutingContext(new Dictionary<string, string>
            {
                { "x-api-key", "correct-key" }
            });

            // Act
            _filter.OnActionExecuting(context);

            // Assert
            var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
            Assert.Equal("API key is missing in configuration.", result.Value);
        }

        [Fact]
        public void OnActionExecuted_ShouldDoNothing()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            var context = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Mock<Controller>().Object
            );

            // Act
            _filter.OnActionExecuted(context);

            // Assert
            Assert.NotNull(context); // No changes expected
        }

        private ActionExecutingContext CreateActionExecutingContext(Dictionary<string, string> headers)
        {
            var httpContext = new DefaultHttpContext();
            foreach (var header in headers)
            {
                httpContext.Request.Headers[header.Key] = header.Value;
            }

            var actionContext = new ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            );

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new Mock<Controller>().Object
            );
        }
    }
}
