using OpenCvSharp;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;

namespace DocScanOpenCV.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {
            BackCommand = new Command(async () => await PerformCommand(NavigationPage.PopAsync));
            TakePhotoToProcess = new Command(async () => await PerformCommand(ProcessImageAsync));
        }
        #region OpenCvAbstractions

        public List<Point2f> point2Fs = new List<Point2f>();

        // //(bl, tl, tr, br) 
        public Point2f[] srcPoints = new Point2f[]
        {
                new Point2f(0, 0),
                new Point2f(0, 0),
                new Point2f(0, 0),
                new Point2f(0, 0),
        };
        public abstract Mat Excec(Mat oryginalImage);


        #endregion OpenCvAbstractions


        #region BaseViewModelFundamentals
        public ICommand BackCommand { get; set; }
        public ICommand TakePhotoToProcess { get; set; }
        private bool processingImage;

        public bool ProcessingImage
        {
            get { return processingImage; }
            set => ChangeValue(ref processingImage, value);
        }


        protected static volatile bool CommandExecuting = false;
        public NavigationPage NavigationPage => Application.Current.MainPage as NavigationPage;

        protected void ChangeValue<T>(ref T changingProp, T newValue, [CallerMemberName] string propertyName = null)
        {
            changingProp = newValue;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool CanExecuteCommand() => !CommandExecuting;
        protected async Task PerformCommand(Func<Task> action)
        {
            if (CommandExecuting) return;
            CommandExecuting = true;
            await action.Invoke();
            CommandExecuting = false;
        }
        protected async Task PerformCommand(Func<object, Task> action, object objectToPass)
        {
            if (CommandExecuting) return;
            CommandExecuting = true;
            await action.Invoke(objectToPass);
            CommandExecuting = false;
        }
        protected async Task PerformCommand(Func<object, object, Task> action, object objectToPass1, object objectToPass2)
        {
            if (CommandExecuting) return;
            CommandExecuting = true;
            await action.Invoke(objectToPass1, objectToPass2);
            CommandExecuting = false;
        }
        #endregion BaseViewModelFundamentals
        #region MediaPicker
        public async Task ProcessImageAsync()
        {
            ProcessingImage = true;
            var file = await TakePhotoAsync();
            if (file != null)
            {
                Mat OriginalImage = new Mat(file.Path, ImreadModes.AnyColor);
                Excec(OriginalImage);
            }
            ProcessingImage = false;
        }
        public async Task<MediaFile> TakePhotoAsync()
        {
            ProcessingImage = true;
            var status = await CheckAndRequestPermissionAsync(new Permissions.StorageRead());
            if (status != PermissionStatus.Granted)
            {
                // Notify user permission was denied
                return null;
            }
            var status1 = await CheckAndRequestPermissionAsync(new Permissions.StorageWrite());
            if (status1 != PermissionStatus.Granted)
            {
                // Notify user permission was denied
                return null;
            }
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                CompressionQuality = 100,
                PhotoSize = PhotoSize.Full
            });
            return file;
        }
        public async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
          where T : BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }
        #endregion MediaPicker
    }
}
