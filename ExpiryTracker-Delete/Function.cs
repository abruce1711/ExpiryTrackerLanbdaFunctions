using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpiryTracker_Delete
{
    public class ResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }

        public ResponseModel()
        {
            StatusCode = HttpStatusCode.OK;
            Message = string.Empty;
        }
    }

    public class EventData
    {
        public string UserId { get; set; }
        public string ItemName { get; set; }

        public EventData()
        {
            UserId = string.Empty;
            ItemName = string.Empty;
        }
    }
    public class Function
    {

        public async Task<ResponseModel> FunctionHandler(EventData input, ILambdaContext context)
        {
            var tableName = Environment.GetEnvironmentVariable("EXPIRY_TRACKER_TABLE_NAME");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    {
                        "UserId",
                        new AttributeValue
                        {
                            S = input.UserId
                        }
                    },
                    {
                        "ItemName",
                        new AttributeValue()
                        {
                            S = input.ItemName
                        }
                    }
                }
            };

            ResponseModel response = new ResponseModel();
            DeleteItemResponse dbResponse = new DeleteItemResponse();

            try
            {
                dbResponse = await client.DeleteItemAsync(request);
            } catch(Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }

            if(dbResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                response.Message = "Item delete successfully";
                return response;
            }

            response.StatusCode = dbResponse.HttpStatusCode;
            response.Message = "Something went wrong";
            return response;
        }
    }
}
