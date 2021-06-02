using Xamarin.Forms;

namespace DocScanOpenCV.CameraRenderer
{
    public class CameraXPreview
    {
        public static readonly BindableProperty CameraProperty = BindableProperty.Create(
            propertyName: "Camera",
            returnType: typeof(CameraOptions),
            declaringType: typeof(CameraPreview),
            defaultValue: CameraOptions.Rear);

    }
}
