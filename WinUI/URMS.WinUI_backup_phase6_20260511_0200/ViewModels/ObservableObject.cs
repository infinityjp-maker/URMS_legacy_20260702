using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace URMS.WinUI.ViewModels
{
    /// <summary>軽量 INotifyPropertyChanged 基底クラス（CommunityToolkit.Mvvm 非依存）</summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }
    }
}
