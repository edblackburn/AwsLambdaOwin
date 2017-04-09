namespace AwsLambdaOwin
{
    using System.IO;
    using System.Text;
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.Serialization.Json;
    using Microsoft.Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public abstract class APIGatewayOwinProxyFunction
    {
        // Manage the serialization so the raw requests and responses can be logged.
        private readonly ILambdaSerializer _serializer = new JsonSerializer();

        /// <summary>
        ///     If true the request JSON coming from API Gateway will be logged. This is used to help debugging and not meant to be
        ///     enabled for production.
        /// </summary>
        public bool EnableRequestLogging { get; set; }

        /// <summary>
        ///     If true the response JSON coming sent to API Gateway will be logged. This is used to help debugging and not meant
        ///     to be enabled for production.
        /// </summary>
        public bool EnableResponseLogging { get; set; }

        public AppFunc AppFunc { get; }

        protected APIGatewayOwinProxyFunction()
        {
            AppFunc = Init();
        }

        protected abstract AppFunc Init();

        public virtual async Task<Stream> FunctionHandler(Stream requestStream, ILambdaContext lambdaContext)
        {
            if (EnableRequestLogging)
            {
                var reader = new StreamReader(requestStream);
                var json = reader.ReadToEnd();
                lambdaContext.Logger.LogLine(json);
                requestStream.Position = 0;
            }

            var proxyRequest = _serializer.Deserialize<APIGatewayProxyRequest>(requestStream);
            lambdaContext.Logger.Log($"Incoming {proxyRequest.HttpMethod} requests to {proxyRequest.Path}");

            var owinContext = new OwinContext();
            MarshalRequest(owinContext, proxyRequest);

            await AppFunc(owinContext.Environment);

            var response = MarshalResponse(owinContext.Response);

            var responseStream = new MemoryStream();
            _serializer.Serialize(response, responseStream);
            responseStream.Position = 0;

            if (EnableResponseLogging)
            {
                var reader = new StreamReader(responseStream);
                var json = reader.ReadToEnd();
                lambdaContext.Logger.LogLine(json);
                responseStream.Position = 0;
            }

            return responseStream;
        }

        private void MarshalRequest(OwinContext owinContext, APIGatewayProxyRequest proxyRequest)
        {
            owinContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(proxyRequest.Body ?? string.Empty));
            if (proxyRequest.Headers != null)
            {
                foreach (var header in proxyRequest.Headers)
                {
                    owinContext.Request.Headers.AppendCommaSeparatedValues(header.Key, header.Value.Split(','));
                }
            }

            owinContext.Request.Path = new PathString(proxyRequest.Path);

            var sb = new StringBuilder();
            var encoder = UrlEncoder.Default;
            foreach (var kvp in proxyRequest.QueryStringParameters)
            {
                if (sb.Length > 1)
                {
                    sb.Append("&");
                }
                sb.Append($"{encoder.Encode(kvp.Key)}={encoder.Encode(kvp.Value)}");
            }
            owinContext.Request.QueryString = new QueryString(sb.ToString());

            owinContext.Response.Body = new MemoryStream();
        }

        private APIGatewayProxyResponse MarshalResponse(IOwinResponse owinResponse)
        {
            var response = new APIGatewayProxyResponse
            {
                StatusCode = owinResponse.StatusCode,
            };
            using (var reader = new StreamReader(owinResponse.Body, Encoding.UTF8))
            {
                response.Body = reader.ReadToEnd();
            }

            foreach (var owinResponseHeader in owinResponse.Headers)
            {
                if (owinResponseHeader.Value.Length == 1)
                {
                    response.Headers[owinResponseHeader.Key] = owinResponseHeader.Value[0];
                }
                else
                {
                    response.Headers[owinResponseHeader.Key] = string.Join(",", owinResponseHeader.Value);
                }
            }
            return response;
        }
    }
}