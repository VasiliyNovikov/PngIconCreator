using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PngIconCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<BitmapImage> _images = new ObservableCollection<BitmapImage>();

        public MainWindow()
        {
            InitializeComponent();
            Images.ItemsSource = _images;
        }

        private void AddImage(string source)
        {
            try
            {
                var img = new BitmapImage(new Uri(source));
                img.CacheOption = BitmapCacheOption.OnLoad;
                _images.Add(img);
            }
            catch
            {
                MessageBox.Show("Couldn't load image");
            }
        }

        private void OnAddFileClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog { Filter = "*.png|*.png", Multiselect = true };
            if (fileDialog.ShowDialog() != true)
                return;

            var files = fileDialog.FileNames;
            foreach (var file in files)
                this.AddImage(file);
        }

        private void OnAddUrlClick(object sender, RoutedEventArgs e)
        {
            var url = UrlWindow.GetUrl();
            if (url != null)
                this.AddImage(url);
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog { Filter = "*.ico|*.ico" };
            if (fileDialog.ShowDialog() != true)
                return;

            var imageData = new List<(int with, int height, byte[] data)>();
            foreach (var image in _images)
            {
                BitmapSource img = image;
                if (img.PixelWidth > 256 || img.PixelHeight > 256)
                {
                    var scale = Math.Max(256.0 / img.PixelWidth, 256.0 / img.PixelHeight);
                    img = new TransformedBitmap(img, new ScaleTransform(scale, scale));
                }

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(img));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    imageData.Add((img.PixelWidth, img.PixelHeight, stream.ToArray()));
                }
            }

            using (var stream = File.OpenWrite(fileDialog.FileName))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((Int16)0);             // Reserved. Must always be 0.
                writer.Write((Int16)1);             // Specifies image type: 1 for icon (.ICO) image, 2 for cursor (.CUR) image. Other values are invalid.
                writer.Write((Int16)_images.Count); // Specifies number of images in the file.

                var dataOffset = 24;
                foreach(var (width, height, data) in imageData)
                {
                    writer.Write((byte)width);  // Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels
                    writer.Write((byte)height); // Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels
                    writer.Write((byte)0);      // Specifies number of colors in the color palette. Should be 0 if the image does not use a color palette
                    writer.Write((byte)0);      // Reserved. Should be 0
                    writer.Write((Int16)0);     // In ICO format: Specifies color planes. Should be 0 or 1.
                    writer.Write((Int16)0);     // In ICO format: Specifies bits per pixel.
                    writer.Write(data.Length);  // Specifies the size of the image's data in bytes
                    writer.Write(dataOffset);   // Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                    dataOffset += data.Length;
                }

                writer.Write((Int16)0);
                foreach (var (width, height, data) in imageData)
                    writer.Write(data);
            }
        }
    }
}
