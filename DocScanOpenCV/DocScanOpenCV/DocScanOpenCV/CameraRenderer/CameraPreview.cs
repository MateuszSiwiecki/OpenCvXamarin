using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DocScanOpenCV.CameraRenderer
{
    public class CameraPreview : View
    {
        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
            propertyName: "Camera",
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);
        public OpenCvSharp.Mat ScannedDocument { get; set; }
        public CameraOptions Camera
        {
            get { return (CameraOptions)GetValue(CameraProperty); }
            set { SetValue(CameraProperty, value); }
        }

        public event Action ScanDocumentCalled;
        public async Task<OpenCvSharp.Mat> ScanDocument()
        {
            await Task.Delay(1000);
            ScanDocumentCalled?.Invoke();
            return ScannedDocument;
        }
    }
}
