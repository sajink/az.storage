namespace Az.Storage
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    public partial class AzureStorageContext
    {
        public async Task<bool> UploadBlob(string container, string name, byte[] content)
        {
            var blob = Container(container).GetBlobClient(name);
            using (var stream = new MemoryStream(content))
                await blob.UploadAsync(stream);

            return true;
        }

        public async Task<Dictionary<string, bool>> UploadBlob(string container, Dictionary<string, byte[]> files)
        {
            var result = new Dictionary<string, bool>();
            foreach (var file in files)
            {
                result.Add(file.Key, await UploadBlob(container, file.Key, file.Value));
            }

            return result;
        }

        public async Task<byte[]> DownloadBlob(string path)
        {
            var blob = Blob(path);
            var prop = (await blob.GetPropertiesAsync()).Value;
            var content = new byte[prop.ContentLength];
            using (var stream = new MemoryStream(content))
                await blob.DownloadToAsync(stream);
            return content;
        }

        public async Task<byte[]> DownloadBlob(string container, string blob) =>
            await DownloadBlob($"{container}/{blob}");

        public async Task<bool> DeleteBlob(string path) =>
            (await Blob(path).DeleteIfExistsAsync()).Value;

        public async Task<bool> DeleteBlob(string container, string blob) =>
            await DeleteBlob($"{container}/{blob}");

    }
}
