using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MessageToImage
{
    internal class Program
    {
        private static string file, fileName;

        private static void Main(string[] args)
        {
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                Console.WriteLine(@"Drag and drop either a .txt or .png file onto this exe to have it translated");
                Console.ReadKey();
                return;
            }

            file = args[0];
            fileName = Path.GetFileNameWithoutExtension(file);

            string extension = Path.GetExtension(file);

            if (extension == ".png")
                DecodeToText(new Bitmap(file));

            if (extension == ".txt")
            {
                string text;

                using (var sr = new StreamReader(file))
                {
                    text = sr.ReadToEnd();
                }

                EncodeToImage(text);
            }
        }

        private static void DecodeToText(Bitmap image)
        {
            string binary = "";

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                        binary += "1";
                    else if (image.GetPixel(x, y).ToArgb() == Color.White.ToArgb())
                        binary += "0";
                }
            }

            string base64 = "";

            for (int i = 0; i < binary.Length; i += 8)
                base64 += (char)Convert.ToInt32(binary.Substring(i, 8), 2);

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), fileName + ".txt"),
                string.Join("", Convert.FromBase64String(base64).Select(x => (char)x).ToArray()));
        }

        private static void EncodeToImage(string text)
        {
            string base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(text));
            string binary = string.Join("", Encoding.ASCII.GetBytes(base64).Select(x => Convert.ToString(x, 2).PadLeft(8, '0')).ToArray());

            Bitmap image = CreateImage(binary);

            image.Save(Path.Combine(Path.GetDirectoryName(file), fileName + ".png"));
        }

        private static Bitmap CreateImage(string binary)
        {
            Bitmap image;

            int square = (int)Math.Floor(Math.Sqrt(binary.Length));
            while (binary.Length % square != 0) { square++; }

            if (square % 8 != 0)
                image = new Bitmap(square, binary.Length / square);
            else
                image = new Bitmap(binary.Length / square, square);

            int index = 0;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (!(index > binary.Length - 1))
                    {
                        if (binary[index] == '1')
                            image.SetPixel(x, y, Color.Black);
                        else
                            image.SetPixel(x, y, Color.White);
                    }
                    else
                        image.SetPixel(x, y, Color.Red);
                    index++;
                }
            }

            return image;
        }
    }
}