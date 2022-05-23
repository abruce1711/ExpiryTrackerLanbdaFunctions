using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpiryTracker_List
{
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
        public async Task<List<object>> FunctionHandler(EventData input)
        {
            var tableName = Environment.GetEnvironmentVariable("EXPIRY_TRACKER_TABLE_NAME");
            var client = new AmazonDynamoDBClient();

            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserId = :UserId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":UserId", new AttributeValue { S =  input.UserId }}}
            };

            var response = await client.QueryAsync(request);

            var output = new List<object>();
            foreach(var item in response.Items)
            {
                output.Add(new
                {
                    itemName = item.GetValueOrDefault("ItemName").S ?? string.Empty,
                    expiryDate = item.GetValueOrDefault("ExpiryDate").S ?? string.Empty,
                    bestBeforeOrUseBy = item.GetValueOrDefault("BestBeforeOrUseBy").S ?? string.Empty,
                    quantity = item.GetValueOrDefault("Quantity").N ?? "",
                });
            }

            return output;
        }
    }
}