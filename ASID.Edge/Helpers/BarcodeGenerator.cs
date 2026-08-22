using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASID.Edge.Helpers
{
    public static class BarcodeGenerator
    {
        // Code 128 patterns (0..106). Each string is 6 digits representing widths of (bar, space, bar, space, bar, space).
        private static readonly string[] Code128Patterns = new string[]
        {
            "212222","222122","222221","121223","121322","131222","122213","122312","132212","221213",
            "221312","231212","112232","122132","122231","113222","123122","123221","223211","221132",
            "221231","213212","223112","312131","311222","321122","321221","312212","322112","322211",
            "212123","212321","232121","111323","131123","131321","112313","132113","132311","211313",
            "231113","231311","112133","112331","132131","113123","113321","133121","313121","211331",
            "231131","213113","213311","213131","311123","311321","331121","312113","312311","332111",
            "314111","221411","431111","111224","111422","121124","121421","141122","141221","112214",
            "112412","122114","122411","142112","142411","241211","221114","411112","421111","214112",
            "214211","411212","421112","421211","212141","214121","412121","111143","111341","131141",
            "114113","114311","411113","411311","113141","114131","311141","411131","211412","211214",
            "211232","2331112" // 103=StartA, 104=StartB, 105=StartC, 106=Stop (7 widths: 2331112)
        };

        public static ImageSource GenerateCode128(string text, int width = 300, int height = 100)
        {
            if (string.IsNullOrEmpty(text))
                text = "LANE A40";

            List<int> patternIndices = new List<int>();
            // Start B = index 104
            patternIndices.Add(104);
            int checksum = 104;

            for (int i = 0; i < text.Length; i++)
            {
                int val = text[i] - 32;
                if (val < 0 || val > 95) val = 0;
                patternIndices.Add(val);
                checksum += val * (i + 1);
            }

            checksum %= 103;
            patternIndices.Add(checksum);
            // Stop = index 106
            patternIndices.Add(106);

            // Convert pattern indices to module bar/space sequence
            List<bool> modules = new List<bool>();
            // Quiet zone
            for (int q = 0; q < 10; q++) modules.Add(false);

            foreach (int idx in patternIndices)
            {
                string p = Code128Patterns[idx];
                bool isBar = true;
                foreach (char c in p)
                {
                    int count = c - '0';
                    for (int k = 0; k < count; k++)
                    {
                        modules.Add(isBar);
                    }
                    isBar = !isBar;
                }
            }

            // Quiet zone
            for (int q = 0; q < 10; q++) modules.Add(false);

            DrawingGroup drawingGroup = new DrawingGroup();
            using (DrawingContext dc = drawingGroup.Open())
            {
                // Background
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));

                double moduleWidth = (double)width / modules.Count;
                double barHeight = height * 0.85;

                for (int i = 0; i < modules.Count; i++)
                {
                    if (modules[i])
                    {
                        dc.DrawRectangle(Brushes.Black, null, new Rect(i * moduleWidth, 0, moduleWidth + 0.5, barHeight));
                    }
                }
            }

            DrawingImage img = new DrawingImage(drawingGroup);
            if (img.CanFreeze)
            {
                img.Freeze();
            }
            return img;
        }
    }
}
