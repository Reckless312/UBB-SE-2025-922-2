using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using DrinkDb_Auth.ViewModel.Authentication.Interfaces;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;

namespace DrinkDb_Auth.ViewModel.Authentication
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

        public AuthenticationQRCodeAndTextBoxDigits(string keyCode = QRCodeDefaultKeyCode)
        {
            firstDigit = string.Empty;
            secondDigit = string.Empty;
            thirdDigit = string.Empty;
            fourthDigit = string.Empty;
            fifthDigit = string.Empty;
            sixthDigit = string.Empty;

            if (keyCode.CompareTo("none") != 0)
            {
                CreateQRCode(keyCode);
            }
        }

        public string FirstDigit
        {
            get => firstDigit;
            set
            {
                firstDigit = value;
                OnPropertyChanged();
            }
        }

        public string SecondDigit
        {
            get => secondDigit;
            set
            {
                secondDigit = value;
                OnPropertyChanged();
            }
        }

        public string ThirdDigit
        {
            get => thirdDigit;
            set
            {
                thirdDigit = value;
                OnPropertyChanged();
            }
        }

        public string FourthDigit
        {
            get => fourthDigit;
            set
            {
                fourthDigit = value;
                OnPropertyChanged();
            }
        }

        public string FifthDigit
        {
            get => fifthDigit;
            set
            {
                fifthDigit = value;
                OnPropertyChanged();
            }
        }

        public string SixthDigit
        {
            get => sixthDigit;
            set
            {
                sixthDigit = value;
                OnPropertyChanged();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public BitmapImage? QrCodeImage
        {
            get => qrCodeImage;
            set
            {
                qrCodeImage = value;
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
            QrCodeImage = qrCodeBitmapImage;
        }
    }
}
