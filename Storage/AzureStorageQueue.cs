namespace Az.Storage;

public partial class AzureStorageContext
{
    public async Task Send(string queueName, string message)
    {
        var queue = await Queue(queueName);
        await queue.SendMessageAsync(Base64Encode(message));
    }

    public static string Base64Encode(string plainText)
        => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));

    public static string Base64Decode(string base64Text)
        => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64Text));
}
