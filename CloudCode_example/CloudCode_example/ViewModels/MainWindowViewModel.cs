using Avalonia.Controls;
using CloudCode_example.Classes;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.VisualBasic;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;

namespace CloudCode_example.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public string folder = "codes";

        private string? _data_input; //Input information data
        public string? DataInput {
            get => _data_input;
            set => this.RaiseAndSetIfChanged(ref _data_input, value);
        }

        private string _cloudCode_generated = "codes/default.PNG"; //Generated CloudCode
        public string CloudCodeGenerated
        {
            get => _cloudCode_generated;
            set => this.RaiseAndSetIfChanged(ref _cloudCode_generated, value);
        }

        private string? _cloudCode_to_decode; //CloudCode to decode
        public string? CloudCodeToDecode
        {
            get => _cloudCode_to_decode;
            set => this.RaiseAndSetIfChanged(ref _cloudCode_to_decode, value);
        }

        private string? _data_output; //Output information data
        public string? DataOutput
        {
            get => _data_output;
            set => this.RaiseAndSetIfChanged(ref _data_output, value);
        }


        public void Generate()
        {
            if (DataInput is not null) {
                CloudCode.Generator generator = new();
                string time = DateAndTime.Now.ToString("d.MM.yyyy H.mm.ss");
                string sourcefile = folder + "/" + time + ".png";
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                Bitmap file = generator.Generate(DataInput);
                file.Save(sourcefile, ImageFormat.Png);
                CloudCodeGenerated = sourcefile;
            }
        }

        public void Decode() {
            if (CloudCodeToDecode is not null)
            {
                //Image<Bgr, byte> emguImage = Classes.ImageConverter.ConvertFromNetImage(CloudCodeToDecode);

                //List<Point> points = new List<Point>(ImagePreprocessor.DetectCloudCodeCorners(emguImage));

                //Image<Bgr, byte> preprocessedImage = ImagePreprocessor.PreprocessImage(CloudCodeToDecode, points);

                //Bitmap bitmap = new Bitmap(preprocessedImage.ToBitmap());

                var decoder = new CloudCode_example.Classes.CloudCode.Decoder();

                System.Drawing.Image image = System.Drawing.Image.FromFile(CloudCodeToDecode);
                DataOutput = decoder.Decode((Bitmap)image);
            }
        }

        public async Task OpenFile()
        {
            var dialog = new OpenFileDialog();
            dialog.AllowMultiple = false; // Можно выбрать только один файл

            // Открываем диалоговое окно и ждем пока пользователь выберет файл
            string[] result = await dialog.ShowAsync(new Window());

            // Если файл был выбран, обновляем CloudCodeToDecode
            if (result != null && result.Length > 0)
            {
                CloudCodeToDecode = result[0];
                isSelected = true;
            }
        }

        private bool? _is_selected = false;
        public bool? isSelected {
            get {
                return _is_selected;
            }
            set {
                this.RaiseAndSetIfChanged(ref _is_selected, value);
            }
        }

    }
}