namespace Fuck.Views;

public class ProfileView : ContentView
{
    private readonly Profile _profile;
    private readonly KeysManager _manager;

    private readonly Picker _branch = new();
    private readonly Picker _employee = new();
    private readonly Picker _year = new();
    private readonly Picker _group = new();
    private readonly Switch _mode = new();

    public ProfileView(Profile updater, KeysManager manager)
    {
        _profile = updater;
        _manager = manager;

        Content = new VerticalStackLayout
        {
            new VerticalStackLayout
            {
                Children = {
                    new Label { Text = "Филиал" },
                    _branch,
                    new HorizontalStackLayout
                    {
                        Children = {
                            new Label { Text = "Режим преподавателя" },
                            _mode
                        }
                    },
                    new Label { Text = "Дата зачисления" },
                    _year,
                    new Label { Text = "Группа" },
                    _group,
                    new Label { Text = "Имя работника" },
                    _employee
                }
            }
        };
        _ = InitAsync();
    }

    private async Task InitAsync()
    {
        _branch.ItemsSource = (await _manager.GetKeys(_profile, "branch", default))?.Keys.ToList();
        _employee.ItemsSource = (await _manager.GetKeys(_profile, "employee", default))?.Keys.ToList();
        _year.ItemsSource = (await _manager.GetKeys(_profile, "year", default))?.Keys.ToList();
        _group.ItemsSource = (await _manager.GetKeys(_profile, "group", default))?.Keys.ToList();

        _branch.SelectedItem = _profile.Branch;
        _mode.IsToggled = _profile.IsEmployee;
        _employee.SelectedItem = _profile.Employee;
        _year.SelectedItem = _profile.Year;
        _group.SelectedItem = _profile.Group;

        Bind();
    }

    private void Bind()
    {
        _branch.SelectedIndexChanged += UpdateEventAdapter;
        _employee.SelectedIndexChanged += UpdateEventAdapter;
        _year.SelectedIndexChanged += UpdateEventAdapter;
        _group.SelectedIndexChanged += UpdateEventAdapter;
        _mode.Toggled += (sender, e) =>
        {
            _profile.IsEmployee = e.Value;
            Preferences.Set("employeeMode", _profile.IsEmployee);
            _ = UpdateAsync(_branch, default);
        };
    }

    private void UpdateEventAdapter(object? sender, EventArgs e)
    {
        if (sender is not Picker picker || picker.SelectedItem is null)
            return;

        _ = UpdateAsync(sender, default);
    }

    private async Task UpdateAsync(object? sender, CancellationToken cancel)
    {
        _ = SetData("branch", (string)_branch.SelectedItem, cancel);
        _ = SetData("employee", (string)_employee.SelectedItem, cancel);
        _ = SetData("year", (string)_year.SelectedItem, cancel);
        _ = SetData("group", (string)_group.SelectedItem, cancel);

        if (sender == _branch)
        {
            if (_profile.IsEmployee)
            {
                _employee.ItemsSource = (await _manager.GetKeys(_profile, "employee", default))?.Keys.ToList();
                _employee.SelectedItem = _profile.Employee;
            }
            else
            {
                _ = SetData("year", (string)_year.SelectedItem, cancel);
                _year.ItemsSource = (await _manager.GetKeys(_profile, "year", default))?.Keys.ToList();
                _year.SelectedItem = _profile.Year;
            }
        }

        if (sender == _year)
        {
            _group.ItemsSource = (await _manager.GetKeys(_profile, "group", cancel))?.Keys.ToList();
            _group.SelectedItem = _profile.Group;
            return;
        }
    }

    private async Task SetData(string key, string value, CancellationToken cancel)
    {
        await _profile.SetData(key, value, cancel);
        Preferences.Set(key, value);
    }
}