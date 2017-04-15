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

    public class APIGatewayOwinProxyFunctionThrowsTests
    {
        private readonly ThrowsOwinFunction _sut;

        public APIGatewayOwinProxyFunctionThrowsTests()
        {
            _sut = new ThrowsOwinFunction();
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

            response.StatusCode.ShouldBe(500);
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

            response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        }
    }
}