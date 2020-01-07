using System.ComponentModel;

namespace M11.Resources
{
    public class GlobalResources : INotifyPropertyChanged
    {
        public static GlobalResources Current = new GlobalResources();

        public event PropertyChangedEventHandler PropertyChanged;

        private string _settingsBadge;

        public string SettingsBadge
        {
            get => _settingsBadge;
            set
            {
                _settingsBadge = value;
                OnPropertyChanged("SettingsBadge");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
