using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace La3bni.Adminpanel
{
    public class ImageManager
    {
        private Random random = new Random();
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly string wwwRootPath;

        public ImageManager(IWebHostEnvironment _webHostEnvironment)
        {
            webHostEnvironment = _webHostEnvironment;
            wwwRootPath = webHostEnvironment.WebRootPath;
        }

        public string UploadFile(IFormFile file, string folderName)
        {
            string image = "";
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                var fileBytes = ms.ToArray();
                image = Convert.ToBase64String(fileBytes);
            }

            string path = wwwRootPath + "/" + folderName + "/";
            string base64 = image.Substring(image.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] chartData = Convert.FromBase64String(base64);
            string imageName = DateTime.Now.ToString("yymmssfff") + random.Next(255522, 99999999) + ".png";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(path, imageName));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(filePath, chartData);
            return imageName;
        }

        public string UploadFile(string file, string folderName)
        {
            string image = file;

            string path = wwwRootPath + "/" + folderName + "/";
            string base64 = image.Substring(image.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] chartData = Convert.FromBase64String(base64);
            string imageName = DateTime.Now.ToString("yymmssfff") + random.Next(255522, 99999999) + ".png";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(path, imageName));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(filePath, chartData);
            return imageName;
        }

        public string UploadVideo(string image, string path, string name)
        {
            string base64 = image.Substring(image.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] chartData = Convert.FromBase64String(base64);
            string imageName = name.Replace(" ", "") + DateTime.Now.ToString("yymmssfff") + random.Next(255522, 99999999) + ".mp4";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine(path, imageName));
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), path)))
            {
                Directory.CreateDirectory(path);
            }
            File.WriteAllBytes(filePath, chartData);
            return path + "/" + imageName;
        }

        public void DeleteFile(string folderName, string imageName)
        {
            string path = wwwRootPath + "/" + folderName + "/";
            FileInfo file = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), path , imageName));
            if (file.Exists)
            {
                file.Delete();
            }
        }
    }
}