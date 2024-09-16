using Fuck.ViewModels;

namespace Fuck.Views
{
    public partial class DayPage : ContentPage
    {
        public DayPage(DayViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }

}
