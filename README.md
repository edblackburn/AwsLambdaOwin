# Amazon.Lambda.Owin

[![NuGet](https://img.shields.io/nuget/v/AwsLambdaOwin.svg)](https://www.nuget.org/packages/AwsLambdaOwin/)
[![AppVeyor](https://img.shields.io/appveyor/ci/damianh/awslambdaowin.svg)](https://ci.appveyor.com/project/damianh/awslambdaowin)

This package makes it easy to run OWIN applications as a Lambda function with
API Gateway. This allows .NET core developers to create "serverless" applications
using OWIN and any framework that supports OWIN.

The function takes a request from an [API Gateway Proxy](0) and converts that
request into an OWIN enviroment dictionary and then converts the response from
your `AppFunc` into a response body that API Gateway Proxy understands.

**Note:** Lambda API Gateway Proxy to is a message orientated design. That is, it
expects request and response bodies to be strings (typically `application/json`)
and are buffered. It is not suitable for stream based APIs nor binary requests /
response.

## Example Lambda Function

In your OWIN application / library, add a class that extends
`APIGatewayOwinProxyFunction` and implement the Init method.

Here is an example implementation that uses [KatanaNetStandard](1) for the
`OwinContext` type

```
namespace TestOwinApp
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AwsLambdaOwin;
    using Microsoft.Owin;

    public class HelloFunction : APIGatewayOwinProxyFunction
    {
        protected override Func<IDictionary<string, object>, Task> Init()
        {
            return async env =>
            {
                var ctx = new OwinContext(env);
                await ctx.Response.WriteAsync("Hello OWIN on Lambda");
            };
        }
    }
}
```

The function handler for the Lambda function will be
`TestOwinApp::TestOwinApp.HelloFunction::FunctionHandler`.

Once the function is deployed configure API Gateway with a HTTP Proxy to call
the Lambda Function. Refer to the API Gateway [developer guide][2] for more
information.

Reach me at [randompunter][3] on twitter.

[0]: http://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-create-api-as-simple-proxy.html
[1]: https://www.nuget.org/packages/KatanaNetStandard/
[2]: http://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-create-api-as-simple-proxy.html
[3]: https:twitter.com/randompunter