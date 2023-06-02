namespace Gpt.Labs.Models.Base
{
    #region

    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    #endregion

    public partial class ObservableObject : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Methods

        public void RaiseAllPropertiesChanged()
        {
            RaisePropertyChanged(string.Empty);
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Set<T>(ref T oldValue, T newValue, [CallerMemberName] string propertyName = null)
        {
            return Set(propertyName, ref oldValue, newValue);
        }

        public virtual bool Set<T>(string propertyName, ref T oldValue, T newValue)
        {
            if (Equals(oldValue, newValue))
            {
                return false;
            }

            oldValue = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
