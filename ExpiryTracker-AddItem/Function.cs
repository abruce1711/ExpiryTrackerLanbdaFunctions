using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ExpiryTracker_CreateUpdate
{
    public class EventData
    {
        public string UserId { get; set; }
        public string ItemName { get; set; }
        public string ExpiryDate { get; set; }
        public string BestBeforeOrUseBy { get; set; }
        public string Quantity { get; set; }

        public EventData()
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
        public async Task<EventData> FunctionHandler(EventData input, ILambdaContext context)
        {
            var tableName = Environment.GetEnvironmentVariable("EXPIRY_TRACKER_TABLE_NAME");
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();

            var itemAttributes = new Dictionary<string, AttributeValue>
            {
                { "UserId", new AttributeValue { S = input.UserId } },
                { "ItemName", new AttributeValue { S = input.ItemName } },
                { "ExpiryDate", new AttributeValue { S = input.ExpiryDate } },
                { "BestBeforeOrUseBy", new AttributeValue { S = input.BestBeforeOrUseBy } },
                { "Quantity", new AttributeValue { N = input.Quantity } },
            };

            await client.PutItemAsync(tableName, itemAttributes);

            return input;
        }
    }
}
