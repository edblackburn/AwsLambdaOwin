namespace AwsLambdaOwin
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Owin;

    public class TestOwinFunction : APIGatewayOwinProxyFunction
    {
        public OwinContext LastRequest;

        public TestOwinFunction()
        {
            EnableRequestLogging = GetEnvironmentVariableBool("EnableRequestLogging");
            EnableResponseLogging = GetEnvironmentVariableBool("EnableResponseLogging");
        }

        protected override Func<IDictionary<string, object>, Task> Init()
        {
            return async env =>
            {
                var context = new OwinContext(env);

                if(context.Request.Path.StartsWithSegments(PathString.FromUriComponent("/img")))
                {
                    var stream = typeof(TestOwinFunction).GetTypeInfo().Assembly.GetManifestResourceStream("AwsLambdaOwin.doge.jpg");
                    context.Response.ContentType = "image/jpeg";
                    context.Response.ContentLength = stream.Length;
                    context.Response.Body = stream;
                    return;
                }

                context.Response.StatusCode = 202;
                context.Response.ReasonPhrase = "OK";
                await context.Response.WriteAsync("Hello");
                LastRequest = context;
            };
        }

        private static bool GetEnvironmentVariableBool(string name)
        {
            var variable = Environment.GetEnvironmentVariable("EnableRequestLogging");
            bool.TryParse(variable, out bool result);
            return result;
        }
    }
}