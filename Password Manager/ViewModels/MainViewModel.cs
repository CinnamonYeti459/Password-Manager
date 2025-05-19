using Password_Manager.Models;
using System.Collections.ObjectModel;

namespace Password_Manager.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<PasswordEntry> PasswordEntries { get; } = new();

    public MainViewModel()
    {
    }
}
