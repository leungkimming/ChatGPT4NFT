using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Drawing;

namespace Metadata
{
    public static class Meet
    {
        [FunctionName("metadata")]
        public static async Task<IActionResult> Metadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log) {
            log.LogInformation("C# HTTP trigger function processed a Metadata request.");

            string date = req.Query["date"];
            string ID = req.Query["ID"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            date = date ?? data?.date;
            ID = ID ?? data?.id;

            string responseMessage = $@"{{
""description"": ""{ date}"",
""image"": ""https://{req.Host.Value}/api/Image?ID={ID}&date={date}"",
""name"": ""MEET""
}}";
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("image")]
        public static async Task<HttpResponseMessage> Image(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log) {
            log.LogInformation("C# HTTP trigger function processed a image request.");

            string date = req.Query["date"];
            string ID = req.Query["ID"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            date = date ?? data?.date;
            ID = ID ?? data?.id;

            var bitmap = new Bitmap(500, 500);
            var graphics = Graphics.FromImage(bitmap);

            graphics.FillRectangle(Brushes.LightBlue, 0, 0, 500, 500);
            graphics.DrawString($"Booking NFT #{ID}", new Font("Arial", 40), Brushes.Red, new PointF(50, 160));
            graphics.DrawString($"{date}", new Font("Arial", 35), Brushes.Black, new PointF(60, 250));

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            var result = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StreamContent(memoryStream)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            return result;
        }

		//[FunctionName("image1")]
		//public static async Task<HttpResponseMessage> Image1(
		//    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
		//    ILogger log) {
		//    log.LogInformation("C# HTTP trigger function processed a image request.");

		//    string date = req.Query["date"];
		//    string ID = req.Query["ID"];

		//    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
		//    dynamic data = JsonConvert.DeserializeObject(requestBody);
		//    date = date ?? data?.date;
		//    ID = ID ?? data?.id;

		//    string responseMessage = $@"<svg width=""500"" height=""500"" style=""background-color:lightblue"" xmlns=""http://www.w3.org/2000/svg"">
		//      <style>
		//        .small {{
		//          font: italic 50px sans-serif;
		//        }}
		//        .heavy {{
		//          font: bold 50px sans-serif;
		//          fill: red;
		//        }}
		//      </style>
		//      <text x=""60"" y=""210"" class=""heavy"">Booking NFT #{ID}</text>
		//      <text x=""100"" y=""300"" class=""small"">{date}</text>
		//    </svg>";

		//    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK) {
		//        Content = new StringContent(responseMessage)
		//    };
		//    result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/svg+xml");

		//    return result;
		//}
	}
}
