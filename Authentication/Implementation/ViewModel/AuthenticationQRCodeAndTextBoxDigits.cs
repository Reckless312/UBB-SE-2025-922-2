using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;

namespace DrinkDb_Auth.ViewModel
{
    public class AuthenticationQRCodeAndTextBoxDigits : INotifyPropertyChanged, IAuthenticationWindowSetup
    {
        private string firstDigit;
        private string secondDigit;
        private string thirdDigit;
        private string fourthDigit;
        private string fifthDigit;
        private string sixthDigit;

        private BitmapImage? qrCodeImage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public const string QRCodeDefaultKeyCode = "none";

        public AuthenticationQRCodeAndTextBoxDigits(string keyCode = AuthenticationQRCodeAndTextBoxDigits.QRCodeDefaultKeyCode)
        {
            this.firstDigit = string.Empty;
            this.secondDigit = string.Empty;
            this.thirdDigit = string.Empty;
            this.fourthDigit = string.Empty;
            this.fifthDigit = string.Empty;
            this.sixthDigit = string.Empty;

            if (keyCode.CompareTo("none") != 0)
            {
                this.CreateQRCode(keyCode);
            }
        }

        public string FirstDigit
        {
            get => this.firstDigit;
            set
            {
                this.firstDigit = value;
                OnPropertyChanged();
            }
        }

        public string SecondDigit
        {
            get => this.secondDigit;
            set
            {
                this.secondDigit = value;
                OnPropertyChanged();
            }
        }

        public string ThirdDigit
        {
            get => this.thirdDigit;
            set
            {
                this.thirdDigit = value;
                OnPropertyChanged();
            }
        }

        public string FourthDigit
        {
            get => this.fourthDigit;
            set
            {
                this.fourthDigit = value;
                OnPropertyChanged();
            }
        }

        public string FifthDigit
        {
            get => this.fifthDigit;
            set
            {
                this.fifthDigit = value;
                OnPropertyChanged();
            }
        }

        public string SixthDigit
        {
            get => this.sixthDigit;
            set
            {
                this.sixthDigit = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BitmapImage? QrCodeImage
        {
            get => this.qrCodeImage;
            set
            {
                this.qrCodeImage = value;
                OnPropertyChanged();
            }
        }

        public void CreateQRCode(string keyCode)
        {
            QRCodeData qrCodeData = new QRCodeGenerator().CreateQrCode(keyCode, QRCodeGenerator.ECCLevel.Q);
            BitmapByteQRCode qrCode = new (qrCodeData);
            byte[] qrCodeImageBytes = qrCode.GetGraphic(20);
            Bitmap qrCodeBitmap = new Bitmap(new MemoryStream(qrCodeImageBytes));
            BitmapImage qrCodeBitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                qrCodeBitmapImage.SetSource(stream.AsRandomAccessStream());
            }
            this.QrCodeImage = qrCodeBitmapImage;
        }
    }
}
