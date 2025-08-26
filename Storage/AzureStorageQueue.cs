namespace Az.Storage;

public partial class AzureStorageContext
{
    public async Task Send(string queueName, string message)
    {
        var queue = Queue(queueName);
        if (_createMissing) queue.CreateIfNotExists();
        await queue.SendMessageAsync(Base64Encode(message));
    }

    private static string Base64Encode(string plainText)
        => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
}
