using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Models;

public partial class ReadingOption : INotifyPropertyChanged
{
    private bool? _userSelected;

    // Property with PropertyChanged notifications
    public bool? UserSelected
    {
        get => _userSelected;
        set
        {
            if (_userSelected != value)
            {
                _userSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
