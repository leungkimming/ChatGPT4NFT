using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Azure.Functions.Worker.Http;

namespace Metadata1
{
    public class Meet
    {
        private readonly ILogger<Meet> _logger;

        public Meet(ILogger<Meet> logger) {
            _logger = logger;
        }

        [Function("metadata")]
        public IActionResult metadata(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req) {
            _logger.LogInformation("C# HTTP trigger function processed a Metadata request.");

            string date = req.Query["date"];
            string ID = req.Query["ID"];
            string responseMessage = $@"{{
""description"": ""{ date}"",
""image"": ""https://{req.Host.Value}/api/Image?ID={ID}&date={date}"",
""name"": ""MEET""
}}";
            return new OkObjectResult(responseMessage);
        }

        [Function("image")]
        public IActionResult Image(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req) {
            _logger.LogInformation("C# HTTP trigger function processed a image request.");

            string date = req.Query["date"];
            string ID = req.Query["ID"];

            var bitmap = new Bitmap(500, 500);
            var graphics = Graphics.FromImage(bitmap);

            graphics.FillRectangle(Brushes.LightBlue, 0, 0, 500, 500);
            graphics.DrawString($"Booking NFT #{ID}", new Font("Arial", 40), Brushes.Red, new PointF(50, 160));
            graphics.DrawString($"{date}", new Font("Arial", 35), Brushes.Black, new PointF(60, 250));

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            return new OkObjectResult(memoryStream);
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
