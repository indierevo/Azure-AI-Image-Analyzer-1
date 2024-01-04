// Import namespaces we will use
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Tutorial_ImageAnalyser_1
{
    internal class Program
    {
        // Computer vision client
        private static ComputerVisionClient cvClient;
        // Folder path containing the source images
        static string folderPath = "C:\\temp\\images\\analysis";
        // Folder path where we want the analysed images saved
        static string destinationDirectory = $"C:\\temp\\images\\analysis\\OUTPUTS";

        static async Task Main(string[] args)
        {
            try
            {
                // Get Azure account settings from appsettings
                IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = builder.Build();
                string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
                string cogSvcKey = configuration["CognitiveServiceKey"];

                // Authenticate client
                ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
                cvClient = new ComputerVisionClient(credentials)
                {
                    Endpoint = cogSvcEndpoint
                };

                // Get all image files (jpg only) we want to analyse and use a counter to keep track of the image name
                string[] imageFiles = Directory.EnumerateFiles(folderPath, "*.jpg", SearchOption.TopDirectoryOnly).ToArray();
                int counter = 1;

                foreach (string imageFile in imageFiles)
                {
                    // Send image for analysis
                    await AnalyzeImage(imageFile, counter);
                    counter++;
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static async Task AnalyzeImage(string imageFile, int counter)
        {
            try
            {
                Console.WriteLine($"Analyzing {imageFile}");

                // Specify features to be retrieved (we will use only description for now)
                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                     VisualFeatureTypes.Description
                };

                // Get image analysis
                using (var imageData = File.OpenRead(imageFile))
                {
                    string newFileName = string.Empty;
                    var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);
                    var caption = analysis.Description.Captions;

                    // If there is a description for the image...
                    if (caption != null)
                    {
                        Console.WriteLine($"Description: {caption[0].Text}");
                        // Create a new file name based on the description
                        newFileName = $"{counter}-{caption[0].Text}.jpg";
                        // Combine the new file name with the destination directory
                        string destinationFile = Path.Combine(destinationDirectory, newFileName);
                        // Copy the file to the new location and rename it
                        File.Copy(imageFile, destinationFile, true);
                        Console.WriteLine($"File copied and renamed to {destinationFile}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}