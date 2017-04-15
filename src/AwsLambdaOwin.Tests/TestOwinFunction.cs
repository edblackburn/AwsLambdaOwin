namespace AwsLambdaOwin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AwsLambdaOwin;
    using Microsoft.Owin;

    public class TestOwinFunction : APIGatewayOwinProxyFunction
    {
        public OwinContext LastRequest;

        protected override Func<IDictionary<string, object>, Task> Init()
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

    public class ThrowsOwinFunction : APIGatewayOwinProxyFunction
    {
        public OwinContext LastRequest;

        protected override Func<IDictionary<string, object>, Task> Init()
        {
            return env => throw new NotSupportedException();
        }
    }
}