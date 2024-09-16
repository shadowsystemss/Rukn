using Fuck.ViewModels;

namespace Fuck.Views;

public partial class WeekPage : ContentPage
{
    public WeekPage(WeekViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}