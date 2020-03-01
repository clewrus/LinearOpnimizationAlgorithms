using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class SimplexCoef : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;


		private bool IsValid (string strValue) {
			if (strValue.Length == 0) return true;
			if (strValue[0] == '+' || strValue[0] == '-') {
				strValue = strValue.Substring(1).Trim();
				if (strValue.Length == 0) return true;
			}
			if (!Char.IsDigit(strValue[0]) && strValue[0] != 'M') return false;

			if (strValue[strValue.Length - 1] == 'M') {
				strValue = strValue.Substring(0, strValue.Length - 1).Trim();
				if (strValue.Length == 0) return true;
			}

			if (double.TryParse(strValue, out double n)) return true;

			var num_den = strValue.Split("/");
			if (num_den.Length != 2) return false;

			if (int.TryParse(num_den[0], out int a) && int.TryParse(num_den[1], out int b)) return true;
			return false;
		}

		private bool _hasError;
		public bool HasError {
			get => _hasError;
			set {
				_hasError = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasError"));
			}
		}

		private string _stringValue;
		public string StringValue {
			get => _stringValue;
			set {
				_stringValue = value;
				HasError = !IsValid(_stringValue.Trim());
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StringValue"));
			}
		}
	}
}
