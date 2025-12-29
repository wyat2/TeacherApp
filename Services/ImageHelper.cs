using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace TeacherApp.Services
{
    public static class ImageHelper
    {
        public static byte[] ResizeImage(byte[] imageData, int targetWidth = 200, int targetHeight = 200)
        {
            using var image = Image.Load(imageData);
            
            // Calculate dimensions maintaining aspect ratio
            var ratioX = (double)targetWidth / image.Width;
            var ratioY = (double)targetHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);
            
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            
            // Create a new image with target dimensions and white background
            using var outputImage = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(targetWidth, targetHeight);
            
            outputImage.Mutate(ctx =>
            {
                // Fill with white background
                ctx.BackgroundColor(Color.White);
                
                // Resize the original image
                var resizedImage = image.Clone(x => x.Resize(newWidth, newHeight));
                
                // Calculate position to center the image
                var x = (targetWidth - newWidth) / 2;
                var y = (targetHeight - newHeight) / 2;
                
                // Draw the resized image centered
                ctx.DrawImage(resizedImage, new Point(x, y), 1f);
            });
            
            // Save to byte array as JPEG
            using var outputStream = new MemoryStream();
            outputImage.Save(outputStream, new JpegEncoder { Quality = 90 });
            return outputStream.ToArray();
        }
    }
}
