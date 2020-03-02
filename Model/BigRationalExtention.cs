using Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BigRationalExtentions.Model {
	public static class BigRationalExtention {
		public static BigRational Fract (this BigRational num) {
			var fractionPart = num.GetFractionPart();
			return (num.Sign > 0 || fractionPart == BigRational.Zero) ? fractionPart : 1 + fractionPart;
		}

		public static BigRational Floor (this BigRational num) {
			return (num.Sign >= 0 || num.GetFractionPart() == BigRational.Zero) ? num.GetWholePart() : num.GetWholePart() - 1;
		}

		public static BigRational ToBigRational (this double num, int precision=6) {
			int den = (int)Math.Pow(10, precision);
			return new BigRational((BigInteger)(num * den), (BigInteger)den);
		}
	}
}
