using Moq;
using mssqlMCP.Middleware;
using mssqlMCP.Models;
using mssqlMCP.Services;
using System.Text;
using Xunit;

namespace mssqlMCP.Tests.Middleware
{
    public class ApiKeyAuthMiddlewareTests
    {
        private readonly Mock<RequestDelegate> _next;
        private readonly Mock<ILogger<ApiKeyAuthMiddleware>> _logger;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IApiKeyManager> _apiKeyManager;
        private readonly Mock<IApiKeyRepository> _apiKeyRepository;

        public ApiKeyAuthMiddlewareTests()
        {
            _next = new Mock<RequestDelegate>();
            _logger = new Mock<ILogger<ApiKeyAuthMiddleware>>();
            _configuration = new Mock<IConfiguration>();
            _apiKeyManager = new Mock<IApiKeyManager>();
            _apiKeyRepository = new Mock<IApiKeyRepository>();
        }

        private ApiKeyAuthMiddleware CreateMiddleware()
        {
            return new ApiKeyAuthMiddleware(
                _next.Object,
                _logger.Object,
                _configuration.Object,
                _apiKeyManager.Object,
                _apiKeyRepository.Object
            );
        }

        private HttpContext CreateHttpContext(string apiKey, string? mcpMethod = null, string @params = "{}")
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["X-API-Key"] = apiKey;
            if (mcpMethod != null)
            {
                context.Request.Path = "/mcp";
                context.Request.Method = "POST";
                var requestBody = $"{{\"method\": \"{mcpMethod}\", \"params\": {@params}}}";
                context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
                context.Request.ContentType = "application/json";
            }

            return context;
        }

        [Theory]
        [InlineData("GetTableMetadata", "{}")]
        [InlineData("tools/call",
            "{\"name\":\"GetTableMetadata\",\"arguments\":{},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_AccessingAllowedEndpoint_ShouldSucceed(string method, string @params)
        {
            // Arrange
            var userApiKey = new ApiKey { Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user" };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.NotEqual(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("CreateApiKey", "{}")]
        [InlineData("tools/call",
            "{\"name\":\"CreateApiKey\",\"arguments\":{},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_AccessingManagementEndpoint_ShouldBeForbidden(string method, string @params)
        {
            // Arrange
            var userApiKey = new ApiKey { Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user" };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
            _next.Verify(next => next(context), Times.Never);
        }

        [Theory]
        [InlineData("CreateApiKey", "{}")]
        [InlineData("tools/call",
            "{\"name\":\"CreateApiKey\",\"arguments\":{},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task AdminKey_AccessingManagementEndpoint_ShouldSucceed(string method, string @params)
        {
            // Arrange
            var adminApiKey = new ApiKey
                { Id = "admin1", Name = "Test Admin Key", Key = "admin-key", KeyType = "admin" };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("admin-key")).ReturnsAsync(adminApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("admin-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("admin-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.NotEqual(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("CreateApiKey", "{}")]
        [InlineData("tools/call",
            "{\"name\":\"CreateApiKey\",\"arguments\":{},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task MasterKey_AccessingAnyEndpoint_ShouldSucceed(string method, string @params)
        {
            // Arrange
            var masterKey = GetMasterKey();
            _configuration.Setup(c => c["ApiSecurity:ApiKey"]).Returns(masterKey);
            var middleware = new ApiKeyAuthMiddleware(_next.Object, _logger.Object, _configuration.Object,
                _apiKeyManager.Object, _apiKeyRepository.Object);
            var context = CreateHttpContext(masterKey, method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.True(context.Items.ContainsKey("IsMaster"));
        }

        private string GetMasterKey() => Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY") ?? "master-key";

        [Fact]
        public async Task NoKey_AccessingAnyEndpoint_ShouldBeUnauthorized()
        {
            // Arrange
            var middleware = CreateMiddleware();
            var context = new DefaultHttpContext(); // No API key

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(401, context.Response.StatusCode);
            _next.Verify(next => next(context), Times.Never);
        }

        [Theory]
        [InlineData("GetTableMetadata", "{\"connectionName\":\"conn1\"}")]
        [InlineData("tools/call",
            "{\"name\":\"GetTableMetadata\",\"arguments\":{\"connectionName\":\"conn1\"},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_WithAllowedConnection_ShouldSucceed(string method, string @params)
        {
            // Arrange
            var userApiKey = new ApiKey
            {
                Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user",
                AllowedConnectionNames = "[\"conn1\",\"conn2\"]"
            };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.NotEqual(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("GetTableMetadata", "{\"connectionName\":\"conn3\"}")]
        [InlineData("tools/call",
            "{\"name\":\"GetTableMetadata\",\"arguments\":{\"connectionName\":\"conn3\"},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_WithDisallowedConnection_ShouldBeForbidden(string method, string @params)
        {
            // Arrange
            var userApiKey = new ApiKey
            {
                Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user",
                AllowedConnectionNames = "[\"conn1\",\"conn2\"]"
            };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(403, context.Response.StatusCode);
            _next.Verify(next => next(context), Times.Never);
        }

        [Theory]
        [InlineData("GetTableMetadata", "{\"connectionName\":\"conn1\"}")]
        [InlineData("tools/call",
            "{\"name\":\"GetTableMetadata\",\"arguments\":{\"connectionName\":\"conn1\"},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_WithConnectionRestriction_AndNoConnectionInRequest_ShouldSucceed(string method,
            string @params)
        {
            // Arrange
            var userApiKey = new ApiKey
            {
                Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user",
                AllowedConnectionNames = "[\"conn1\",\"conn2\"]"
            };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.NotEqual(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("GetTableMetadata", "{\"connectionName\":\"conn1\"}")]
        [InlineData("tools/call",
            "{\"name\":\"GetTableMetadata\",\"arguments\":{\"connectionName\":\"conn1\"},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_WithNoConnectionRestriction_ShouldSucceed(string method, string @params)
        {
            // Arrange
            var userApiKey = new ApiKey
            {
                Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user", AllowedConnectionNames = "[]"
            };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.NotEqual(403, context.Response.StatusCode);
        }

        [Theory]
        [InlineData("GetTableMetadata", "{\"connectionName\":\"conn1\"}")]
        [InlineData("tools/call",
            "{\"name\":\"GetTableMetadata\",\"arguments\":{\"connectionName\":\"conn1\"},\"_meta\":{\"progressToken\":\"1b4863a3-316a-4796-9a04-77282098a038\"}}")]
        public async Task UserKey_WithNullAllowedConnectionNames(string method, string @params)
        {
            // Arrange
            var userApiKey = new ApiKey
            {
                Id = "user1", Name = "Test User Key", Key = "user-key", KeyType = "user", AllowedConnectionNames = null
            };
            _apiKeyRepository.Setup(r => r.GetApiKeyByValueAsync("user-key")).ReturnsAsync(userApiKey);
            _apiKeyManager.Setup(m => m.ValidateApiKeyAsync("user-key")).ReturnsAsync(true);

            var middleware = CreateMiddleware();
            var context = CreateHttpContext("user-key", method, @params);

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
            Assert.NotEqual(403, context.Response.StatusCode);
        }
    }
}