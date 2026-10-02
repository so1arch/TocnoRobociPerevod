using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace ScreenTranslator.Services
{
    public class OcrRegion
    {
        public string Text { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class OcrService
    {
        private OcrEngine? _ocrEngine;

        public OcrService()
        {
            var language = new Windows.Globalization.Language("en");
            if (OcrEngine.IsLanguageSupported(language))
            {
                _ocrEngine = OcrEngine.TryCreateFromLanguage(language);
            }
            else
            {
                _ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
            }
        }

        public async Task<List<OcrRegion>> ExtractTextRegionsAsync(Bitmap bitmap)
        {
            var list = new List<OcrRegion>();
            if (_ocrEngine == null) return list;

            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            var decoder = await BitmapDecoder.CreateAsync(memoryStream.AsRandomAccessStream());
            using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            var ocrResult = await _ocrEngine.RecognizeAsync(softwareBitmap);

            foreach (var line in ocrResult.Lines)
            {
                if (string.IsNullOrWhiteSpace(line.Text)) continue;

                double minX = double.MaxValue, minY = double.MaxValue;
                double maxX = double.MinValue, maxY = double.MinValue;

                foreach (var word in line.Words)
                {
                    var rect = word.BoundingRect;
                    if (rect.Left < minX) minX = rect.Left;
                    if (rect.Top < minY) minY = rect.Top;
                    if (rect.Right > maxX) maxX = rect.Right;
                    if (rect.Bottom > maxY) maxY = rect.Bottom;
                }

                list.Add(new OcrRegion
                {
                    Text = line.Text,
                    X = minX,
                    Y = minY,
                    Width = Math.Max(10, maxX - minX),
                    Height = Math.Max(10, maxY - minY)
                });
            }

            return list;
        }
    }
}