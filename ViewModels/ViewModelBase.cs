using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace YakimovTheSimplex.ViewModels {
	public class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging {
		protected void SetProperty<T> (ref T storage, T value, [CallerMemberName] string caller = "") {
			if (!EqualityComparer<T>.Default.Equals(storage, value)) {
				if (PropertyChanging != null) {
					PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(caller));
				}

				storage = value;

				if (PropertyChanged != null) {
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
				}
			}
		}

		protected void OnPropertyChanged (string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		protected void InvokePropertyChanged ([CallerMemberName] string caller = "") {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public event PropertyChangingEventHandler PropertyChanging;
	}
}
