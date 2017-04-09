namespace Amazon.Lambda.Owin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.TestUtilities;
    using AwsLambdaOwin;
    using Microsoft.Owin;
    using Shouldly;
    using Xunit;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class UnitTest1
    {
        private readonly TestOwinFunction _sut;

        public UnitTest1()
        {
            _sut = new TestOwinFunction();
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

            AssetLastRequest();
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
            AssetLastRequest();
        }

        private void AssetLastRequest()
        {
            _sut.LastRequest.Request.Host.Value.ShouldBe("example.com");
            _sut.LastRequest.Request.Path.Value.ShouldBe("/path");
            _sut.LastRequest.Request.Accept.ShouldBe("application/json");
            _sut.LastRequest.Request.Query["a"].ShouldBe("1");
            _sut.LastRequest.Request.Query["b"].ShouldBe("2");
            
        }
    }

    public class TestOwinFunction : APIGatewayOwinProxyFunction
    {
        public OwinContext LastRequest;

        protected override AppFunc Init()
        {
            return async env =>
            {
                var context = new OwinContext(env);
                context.Response.StatusCode = 202;
                context.Response.ReasonPhrase = "OK";
                await context.Response.WriteAsync("Hello");
                LastRequest = context;
            };
        }
    }
}