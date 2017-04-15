namespace AwsLambdaOwin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.TestUtilities;
    using Shouldly;
    using Xunit;

    public class APIGatewayOwinProxyFunctionTests
    {
        private readonly TestOwinFunction _sut;

        public APIGatewayOwinProxyFunctionTests()
        {
            _sut = new TestOwinFunction
            {
                EnableRequestLogging = true,
                EnableResponseLogging = true
            };
        }

        [Fact]
        public async Task ProxyRequestTest()
        {
            var context = new TestLambdaContext
            {
                FunctionName = "Owin"
            };
            var request = new APIGatewayProxyRequest
            {
                HttpMethod = "GET",
                Body = "Hi",
                Headers = new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "Accept-Encoding", "gzip,deflate" },
                    { "Host", "example.com" }
                },
                Path = "/path",
                QueryStringParameters = new Dictionary<string, string>
                {
                    { "a" , "1" },
                    { "b" , "2" }
                }
            };
            var response = await _sut.FunctionHandler(request, context);

            response.StatusCode.ShouldBe(202);

            AssertLastRequest();
        }

        [Fact]
        public async Task HttpClientTest()
        {
            var handler = new OwinHttpMessageHandler(_sut.AppFunc)
            {
                UseCookies = true
            };
            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://example.com")
            };
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");

            var response = await client.GetAsync("/path?a=1&b=2");

            response.StatusCode.ShouldBe(HttpStatusCode.Accepted);

            AssertLastRequest();
        }

        private void AssertLastRequest()
        {
            _sut.LastRequest.Request.Host.Value.ShouldBe("example.com");
            _sut.LastRequest.Request.Path.Value.ShouldBe("/path");
            _sut.LastRequest.Request.Accept.ShouldBe("application/json");
            _sut.LastRequest.Request.Query["a"].ShouldBe("1");
            _sut.LastRequest.Request.Query["b"].ShouldBe("2");
        }
    }
}
