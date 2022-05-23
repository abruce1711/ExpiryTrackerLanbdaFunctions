using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpiryTracker_CreateUpdate
{
    public class ResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public Item? Item { get; set; }
        public string Message { get; set; }

        public ResponseModel()
        {
            StatusCode = HttpStatusCode.OK;
            Message = string.Empty;
        }
    }

    public class Item
    {
        public string UserId { get; set; }
        public string? PreviousItemName { get; set; }
        public string ItemName { get; set; }
        public string ExpiryDate { get; set; }
        public string BestBeforeOrUseBy { get; set; }
        public string Quantity { get; set; }

        public Item()
        {
            UserId = string.Empty;
            ItemName = string.Empty;
            ExpiryDate = string.Empty;
            BestBeforeOrUseBy = string.Empty;
            Quantity = string.Empty;
        }
    }

    public class Function
    {
        public async Task<ResponseModel> FunctionHandler(Item item)
        {
            var tableName = Environment.GetEnvironmentVariable("EXPIRY_TRACKER_TABLE_NAME");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            if(item.PreviousItemName != null)
            {
                DeleteItem(client, tableName == null ? string.Empty : tableName, item.UserId, item.PreviousItemName);
            }
            

            var itemAttributes = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = item.UserId } },
                { "ItemName", new AttributeValue { S = item.ItemName } },
                { "ExpiryDate", new AttributeValue { S = item.ExpiryDate } },
                { "BestBeforeOrUseBy", new AttributeValue { S = item.BestBeforeOrUseBy } },
                { "Quantity", new AttributeValue { N = item.Quantity } },
            };

            PutItemResponse dbResponse = new PutItemResponse();
            ResponseModel response = new ResponseModel();
            try
            {
                dbResponse = await client.PutItemAsync(tableName, itemAttributes);
            } catch(Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }

            if(dbResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                response.Item = item;
                response.Message = item.ItemName + " added successfully";
                return response;
            }

            response.StatusCode = dbResponse.HttpStatusCode;
            response.Message = "Something went wrong";
            return response;
        }

        private async  void DeleteItem(AmazonDynamoDBClient client, string tableName, string userId, string previousItemName)
        {
            var request = new DeleteItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string, AttributeValue>()
                {
                    {
                        "UserId",
                        new AttributeValue
                        {
                            S = userId
                        }
                    },
                    {
                        "ItemName",
                        new AttributeValue()
                        {
                            S = previousItemName
                        }
                    }
                }
            };

            await client.DeleteItemAsync(request);
        }
    }
}
