using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FreezerTracker_List
{
    public class ResponseModel
    {
        public HttpStatusCode StatusCode { get; set; }
        public List<Item> Items { get; set; }
        public string Message { get; set; }

        public ResponseModel()
        {
            StatusCode = HttpStatusCode.OK;
            Message = string.Empty;
            Items = new List<Item>();
        }
    }

    public class Item
    {
        public string UserId { get; set; }
        public string ItemName { get; set; }
        public string Drawer { get; set; }
        public string Quantity { get; set; }

        public Item()
        {
            UserId = string.Empty;
            ItemName = string.Empty;
            Drawer = string.Empty;
            Quantity = string.Empty;
        }
    }

    public class EventData
    {
        public string UserId { get; set; }
        public EventData()  
        {
            UserId = string.Empty;
        }
    }


    public class Function
    {
        public async Task<ResponseModel> FunctionHandler(EventData input)
        {
            var tableName = Environment.GetEnvironmentVariable("FREEZER_TRACKER_TABLE_NAME");
            var client = new AmazonDynamoDBClient();

            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserId = :UserId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":UserId", new AttributeValue { S =  input.UserId }}}
            };



            QueryResponse dbResponse = new QueryResponse();
            ResponseModel response = new ResponseModel();
            try
            {
                dbResponse = await client.QueryAsync(request);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.Message = ex.Message;
                return response;
            }

            if (dbResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                foreach (var dbItem in dbResponse.Items)
                {
                    Item item = new Item()
                    {
                        ItemName = dbItem.GetValueOrDefault("ItemName").S ?? string.Empty,
                        Drawer = dbItem.GetValueOrDefault("Drawer").S ?? string.Empty,
                        Quantity = dbItem.GetValueOrDefault("Quantity").N ?? string.Empty
                };

                    response.Items.Add(item);
                }

                response.Message = "Items returned successfully";
                return response;
            }

            response.StatusCode = dbResponse.HttpStatusCode;
            response.Message = "Something went wrong";
            return response;
        }
    }
}