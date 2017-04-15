namespace AwsLambdaOwin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.TestUtilities;
    using Shouldly;
    using Xunit;
    using Xunit.Abstractions;

    public class APIGatewayOwinProxyFunctionThrowsTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ThrowsOwinFunction _sut;

        public APIGatewayOwinProxyFunctionThrowsTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
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

            Func<Task> act = async () => await _sut.FunctionHandler(request, context);

            await act.ShouldThrowAsync<Exception>();

            _testOutputHelper.WriteLine(((TestLambdaLogger)context.Logger).Buffer.ToString());
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

            Func<Task> act = async () => await client.GetAsync("/path");

            await act.ShouldThrowAsync<Exception>();
        }
    }
}