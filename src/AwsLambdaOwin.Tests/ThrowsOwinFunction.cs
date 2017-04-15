namespace AwsLambdaOwin.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class ThrowsOwinFunction : APIGatewayOwinProxyFunction
    {
        protected override Func<IDictionary<string, object>, Task> Init()
        {
            return env => throw new Exception("boom");
        }
    }
}