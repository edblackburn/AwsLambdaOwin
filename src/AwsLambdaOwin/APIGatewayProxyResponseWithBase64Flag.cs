namespace AwsLambdaOwin
{
    using System.Runtime.Serialization;
    using Amazon.Lambda.APIGatewayEvents;

    /// <summary>
    ///     Temporary type until https://github.com/aws/aws-lambda-dotnet/pull/75 is shipped.
    /// </summary>
    public class APIGatewayProxyResponseWithBase64Flag : APIGatewayProxyResponse
    {
        /// <summary>
        /// Flag indicating whether the body should be treated as a base64-encoded string
        /// </summary>
        [DataMember(Name = "isBase64Encoded")]
        public bool IsBase64Encoded { get; set; }
    }
}