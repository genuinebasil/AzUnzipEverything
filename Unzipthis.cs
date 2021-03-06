using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzUnzipEverything
{
    public static class Unzipthis
    {
        [FunctionName("Unzipthis")]
        public static async Task Run([BlobTrigger("input-files/{name}", Connection = "cloud5mins_storage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            string destinationStorage = Environment.GetEnvironmentVariable("destinationStorage");
            string destinationContainer = Environment.GetEnvironmentVariable("destinationContainer");

            try{
                if(name.Split('.').Last().ToLower() == "zip"){

                    ZipArchive archive = new ZipArchive(myBlob);

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(destinationStorage);
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = blobClient.GetContainerReference(destinationContainer);

                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        log.LogInformation($"Now processing {entry.FullName}");

                        CloudBlockBlob blockBlob = container.GetBlockBlobReference(entry.Name);
                        using (var fileStream = entry.Open())
                        {
                            await blockBlob.UploadFromStreamAsync(fileStream);
                        }
                    }
                }
            }
            catch(Exception ex){
                log.LogInformation($"Error! Something went wrong: {ex.Message}");

            }            
        }
    }
}
