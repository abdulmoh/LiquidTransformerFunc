using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GMF.Transform
{
    public static class GMFTransformReceiver
    {
        [FunctionName("GmfTransformReceiver")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            string requestBody = await req.Content.ReadAsStringAsync();
            string source = LiquidConverter.GivenALiquidTemplateText();
			string output = LiquidConverter.ConvertFromXml(requestBody, "Inbound", source);
            return new ContentResult { Content = output, ContentType = "application/xml", StatusCode = 200 };
        }

    }
}
