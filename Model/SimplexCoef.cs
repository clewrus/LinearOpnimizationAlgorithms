using BigRationalExtentions.Model;
using Numerics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class SimplexCoef : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public SimplexCoef () {
			this.isM = false;
			this.value = BigRational.Zero;

			this._hasError = false;
			this._stringValue = "0";
		}

		public SimplexCoef (SimplexCoef other) {
			this.isM = other.isM;
			this.value = other.value;

			this._hasError = other._hasError;
			this._stringValue = other._stringValue;
		}

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

		public override string ToString () {
			if (HasError) return "Can't parse";
			string result = "";

			result += (value.Fract() == 0) ? value.Numerator.ToString() : $"{value.Numerator}/{value.Denominator}";
			result += (isM) ? " M" : "";

			return result;
		}

		#region Parsing

		private bool IsValid (string strValue) {
			if (strValue.Length == 0) return false;
			if (strValue[0] == '+' || strValue[0] == '-') {
				strValue = strValue.Substring(1).Trim();
				if (strValue.Length == 0) return false;
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
				value = BigRational.Zero;
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

		#endregion

		#region Operations

		public bool IsZero () {
			return this.value == BigRational.Zero;
		}

		public bool IsOne () {
			return this.value == BigRational.One;
		}

		public SimplexCoef GetInverted () {
			if (this.HasError || this.IsZero()) return null;
			if (this.isM) return new SimplexCoef();

			var result = new SimplexCoef(this);
			result.value = BigRational.One / result.value;
			result._stringValue = result.ToString();

			return result;
		}

		public static SimplexCoef operator + (SimplexCoef l, SimplexCoef r) {
			if (l.HasError || r.HasError) return null;

			SimplexCoef res = null;
			if (l.isM && r.isM || !l.isM && !r.isM) {
				res = new SimplexCoef(l);
				res.value = l.value + r.value;

			} else if (l.isM && !r.isM) {
				res = new SimplexCoef(l);

			} else if (!l.isM && r.isM) {
				res = new SimplexCoef(r);
			}

			if (res.value == BigRational.Zero) {
				res.isM = false;
			}

			Debug.Assert(res != null, "So strange ... ");
			res._stringValue = res.ToString();

			return res;
		}

		public static SimplexCoef operator - (SimplexCoef l, SimplexCoef r) {
			if (l.HasError || r.HasError) return null;

			SimplexCoef res = null;
			if (l.isM && r.isM || !l.isM && !r.isM) {
				res = new SimplexCoef(l);
				res.value = l.value - r.value;

			} else if (l.isM && !r.isM) {
				res = new SimplexCoef(l);

			} else if (!l.isM && r.isM) {
				res = new SimplexCoef(r);
			}

			if (res.value == BigRational.Zero) {
				res.isM = false;
			}

			Debug.Assert(res != null, "So strange ... ");
			res._stringValue = res.ToString();

			return res;
		}

		public static SimplexCoef operator - (SimplexCoef l) {
			if (l.HasError) return null;

			var res = new SimplexCoef(l);
			res.value = -res.value;

			return res;
		}

		public static SimplexCoef operator * (SimplexCoef l, SimplexCoef r) {
			if (l.HasError || r.HasError) return null;

			SimplexCoef res = null;
			if (l.isM && r.isM || !l.isM && !r.isM) {
				res = new SimplexCoef(l);

			} else if (l.isM && !r.isM) {
				res = new SimplexCoef(l);

			} else if (!l.isM && r.isM) {
				res = new SimplexCoef(r);
			}

			Debug.Assert(res != null, "So strange ... ");

			res.value = l.value * r.value;
			if (res.value == BigRational.Zero) {
				res.isM = false;
			}

			res._stringValue = res.ToString();
			return res;
		}

		public static SimplexCoef operator / (SimplexCoef l, SimplexCoef r) {
			if (l.HasError || r.HasError) return null;
			if (r.value == BigRational.Zero) return null;

			SimplexCoef res = null;
			if (l.isM && r.isM || !l.isM && !r.isM) {
				res = new SimplexCoef(l);
				res.isM = false;
				res.value = l.value / r.value;

			} else if (l.isM && !r.isM) {
				res = new SimplexCoef(l);
				res.value = l.value / r.value;

			} else if (!l.isM && r.isM) {
				res = new SimplexCoef(r);
				res.isM = false;
				res.value = BigRational.Zero;
			}

			Debug.Assert(res != null, "So strange ... ");

			if (res.value == BigRational.Zero) {
				res.isM = false;
			}

			res._stringValue = res.ToString();
			return res;
		}

		public static bool operator > (SimplexCoef l, SimplexCoef r) {
			if (l.HasError || r.HasError) throw new ArgumentException("Can't compare coefs with errors.");

			if (l.isM && r.isM || !l.isM && !r.isM) {
				return l.value > r.value;
			} else if (l.isM && !r.isM) {
				return l.value > BigRational.Zero;
			} else if (!l.isM && r.isM) {
				return BigRational.Zero > l.value;
			}

			throw new Exception("Logic fucked");
		}

		public static bool operator < (SimplexCoef l, SimplexCoef r) {
			return r > l;
		}

		public static bool operator >= (SimplexCoef l, SimplexCoef r) {
			if (l.HasError || r.HasError) throw new ArgumentException("Can't compare coefs with errors.");

			if (l.isM && r.isM || !l.isM && !r.isM) {
				return l.value >= r.value;
			} else if (l.isM && !r.isM) {
				return l.value > BigRational.Zero;
			} else if (!l.isM && r.isM) {
				return BigRational.Zero > l.value;
			}

			throw new Exception("Logic fucked");
		}

		public static bool operator <= (SimplexCoef l, SimplexCoef r) {
			return r >= l;
		}
		#endregion
	}
}
