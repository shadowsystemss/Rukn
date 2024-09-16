using Fuck.ViewModels;

namespace Fuck.Views;

public partial class HubPage : ContentPage
{
    public HubPage(HubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}