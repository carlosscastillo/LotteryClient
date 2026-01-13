using Lottery.Helpers;
using Lottery.Properties.Langs;
using Lottery.ViewModel.Base;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

public class SettingsViewModel : BaseViewModel
{
    private readonly Window _window;

    public List<string> AvailableLanguages { get; }
    public string SelectedLanguage { get; set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public SettingsViewModel(Window window)
    {
        _window = window;

        AvailableLanguages = new List<string>
        {
            "es",
            "en"
        };

        SelectedLanguage =
            LocalizationManager.CurrentCulture.TwoLetterISOLanguageName;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => _window.Close());
    }

    private void Save()
    {
        LocalizationManager.ChangeCulture(SelectedLanguage);

        var lang = (LangProxy)Application.Current.Resources["Lang"];
        lang.Refresh();

        _window.Close();
    }
}