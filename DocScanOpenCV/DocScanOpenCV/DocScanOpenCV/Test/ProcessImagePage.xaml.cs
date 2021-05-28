using DocScanOpenCV.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DocScanOpenCV.Test
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProcessImagePage : ContentPage
    {
        public ProcessImagePage(BaseViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}