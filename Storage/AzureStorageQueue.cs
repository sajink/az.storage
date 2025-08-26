namespace Az.Storage;

public partial class AzureStorageContext
{
    public void WriteToQueue(string queueName, string message)
    {
        var queue = Queue(queueName);
        if (_createMissing) queue.CreateIfNotExists();
        queue.SendMessage(Base64Encode(message));
    }

    private static string Base64Encode(string plainText)
        => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
}

