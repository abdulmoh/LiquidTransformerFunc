using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GMF.Transform
{
    public static class GMFTransformTimerFunc
    {
        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("");
        static CloudBlobClient client = storageAccount.CreateCloudBlobClient();
        static string liquidtransformerUrl = Environment.GetEnvironmentVariable("LiquidTransformerURL");
        [FunctionName("GMFTransformTimerFunc")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            string output = string.Empty;
            var inputcontainer = client.GetContainerReference("liquid-transforms-input");
            var blobs = GetBlobs(inputcontainer).Result;
            foreach (var item in blobs)
            {
                CloudBlockBlob inputblob = inputcontainer.GetBlockBlobReference(item.Name);
                string inputxml = await inputblob.DownloadTextAsync();
                output = await GetTransformedxml(inputxml);
                await UploadBlob(output, item.Name);
                await inputblob.DeleteAsync();
            }
        }


        private static async Task<IEnumerable<CloudBlob>> GetBlobs(CloudBlobContainer container)
        {

            BlobContinuationToken continuationToken = null;
            List<CloudBlob> blobs = new List<CloudBlob>();
            do
            {
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(string.Empty,
                true, BlobListingDetails.Metadata, null, continuationToken, null, null);

                foreach (var blobItem in resultSegment.Results)
                {
                    blobs.Add((CloudBlob)blobItem);
                }

                // Get the continuation token and loop until it is null.
                continuationToken = resultSegment.ContinuationToken;

            } while (continuationToken != null);

            return blobs;
        }


        private static async Task<string> GetTransformedxml(string input)
        {
            string output = string.Empty;
            try
            {
                using var req = new HttpClient();
                req.DefaultRequestHeaders.Add("User-Agent", "Timer Function");
                req.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("text/plain"));

                HttpResponseMessage response = await req.PostAsync(liquidtransformerUrl, new StringContent(input));
                response.EnsureSuccessStatusCode();
                output = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }
            return output;
        }

        private static async Task UploadBlob(string data, string name)
            {
            CloudBlobContainer outputcontainer = client.GetContainerReference("liquid-transforms-output");
            await outputcontainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, new BlobRequestOptions(), new OperationContext());
            CloudBlockBlob blob = outputcontainer.GetBlockBlobReference(name);
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);
            using (Stream stream = new MemoryStream(dataBytes, 0, dataBytes.Length))
            {
                await blob.UploadFromStreamAsync(stream).ConfigureAwait(false);
            }
        }
    }
}
