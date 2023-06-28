using CloudCode_example.Classes;
using Microsoft.VisualBasic;
using ReactiveUI;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace CloudCode_example.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string folder = "codes";

        private string? _data_input; //Input information data
        public string? DataInput {
            get => _data_input;
            set => this.RaiseAndSetIfChanged(ref _data_input, value);
        }

        private string? _cloudCode_generated; //Generated CloudCode
        public string? CloudCodeGenerated
        {
            get => _cloudCode_generated;
            set => this.RaiseAndSetIfChanged(ref _cloudCode_generated, value);
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
                CloudCodeGenerated = "/" + sourcefile;
            }
        }

    }
}