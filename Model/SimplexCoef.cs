using BigRationalExtentions.Model;
using Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class SimplexCoef : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public bool isM;
		public BigRational value;

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
				ParseValue(_stringValue.Trim());
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StringValue"));
			}
		}


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

			return BigInteger.TryParse(num_den[0], out BigInteger a) &&
				BigInteger.TryParse(num_den[1], out BigInteger b);
		}

		private void ParseValue (string strValue) {
			if (HasError) {
				isM = false;
				value = BigRational.One;
				return;
			}

			bool isPositive = true;
			if (strValue[0] == '+' || strValue[0] == '-') {
				isPositive = strValue[0] == '+';
				strValue = strValue.Substring(1).Trim();
			}

			isM = (strValue[strValue.Length - 1] == 'M');
			if (isM) {
				strValue = strValue.Substring(0, strValue.Length - 1).Trim();
				if (strValue.Length == 0) {
					value = (isPositive) ? BigRational.One : BigRational.MinusOne;
					return;
				}
			}

			if (double.TryParse(strValue, out double dResult)) {
				value = (isPositive) ? dResult.ToBigRational() : -dResult.ToBigRational();
				return;
			}

			var num_den = strValue.Split('/');
			var uValue = new BigRational(BigInteger.Parse(num_den[0].Trim()), BigInteger.Parse(num_den[1].Trim()));
			value = (isPositive) ? uValue : -uValue;
		}

		public override string ToString () {
			if (HasError) return "Can't parse";
			string result = "";

			result += (value.Fract() == 0) ? value.Numerator.ToString() : $"{value.Numerator}/{value.Denominator}";
			result += (isM) ? " M" : "";

			return result;
		}
	}
}
