using Fuck.ViewModels;

namespace Fuck.Views;

public partial class LessonPage : ContentPage
{
    public LessonPage(LessonViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}