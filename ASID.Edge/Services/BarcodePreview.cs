using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ASID.Edge.Services
{
    public class BarcodePreview
    {
        public async Task<BitmapImage?> GetImage(string zplCode)
        {
            using var client = new HttpClient();

            var content = new StringContent(
                zplCode,
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            var response = await client.PostAsync(
                "http://api.labelary.com/v1/printers/8dpmm/labels/4x6/0/",
                content);

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = await response.Content.ReadAsStreamAsync();

            var memory = new MemoryStream();

            await stream.CopyToAsync(memory);

            memory.Position = 0;

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = memory;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
    }
}