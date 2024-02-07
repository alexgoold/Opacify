using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace Opacify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Image ChosenImage { get; set; }
        public string ChosenFolder { get; set; }
        public string ImagePath { get; set; }
        public int Amount { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    ChosenImage = new Image();
                    ChosenImage.Source = new BitmapImage(new Uri(dialog.FileName));
                    ImagePath = dialog.FileName;
                    ChosenImage.Width = 200;
                    ChosenImage.Height = 200;
                    ChosenImage.Margin = new Thickness(5);
                    ImageDisplay.Source = ChosenImage.Source;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading the image: " + ex.Message, "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private void FolderButton_OnClick(object sender, RoutedEventArgs e)
        {
            var openFolder = new OpenFolderDialog();
            if (openFolder.ShowDialog() == true)
            {
                ChosenFolder = openFolder.FolderName;
                FolderPathLabel.Text = ChosenFolder;
            }

        }

        private void AmountSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Amount = (int)AmountSlider.Value;
            Amountlabel.Content = Amount;
        }


        private void OpacifyButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ChosenFolder == null)
            {
                MessageBox.Show("Please select a folder before proceeding.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ImagePath == null)
            {
                MessageBox.Show("Please select an image before proceeding.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Amount == 0)
            {
                MessageBox.Show("Please select an amount of images to create.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BitmapImage baseImage = new BitmapImage(new Uri(ImagePath));

            for (int i = 0; i < Amount; i++)
            {
                double opacity = (i + 1) / (double)Amount;
                var visual = new DrawingVisual();

                using (var context = visual.RenderOpen())
                {
                    context.PushOpacity(opacity);
                    context.DrawImage(baseImage, new Rect(0, 0, baseImage.Width, baseImage.Height));
                }

                var bitmap = new RenderTargetBitmap(baseImage.PixelWidth, baseImage.PixelHeight, baseImage.DpiX, baseImage.DpiY, PixelFormats.Pbgra32);
                bitmap.Render(visual);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                string outputPath = Path.Combine(ChosenFolder, $"opacified{i}.png");

               try
                {
                    using (var stream = File.Create(outputPath))
                    {
                        encoder.Save(stream);
                    }
                }

                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the image '{outputPath}': {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                ProgressBar.Value = (i + 1) / (double)Amount * 100;
            }
        }
    }
}
    