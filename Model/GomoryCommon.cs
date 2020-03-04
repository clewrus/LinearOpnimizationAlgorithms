
using BigRationalExtentions.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public abstract class GomoryCommon : SimplexMethod, ISimplexTableTransform {
		public override string MakeTransform (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
			var result = "<h3>Let's ignore integer constrain:</h3><br>";
			var simplex = new SimplexMethod();
			result += simplex.MakeTransform(inputTable, out outputTable, out success);

			int counter = 1;
			while (success && !IsIntegerVector(outputTable.bVector)) {
				result += $"<br><br><h4>Step {counter++}:</h4><br>";
				result += "BVector has fractional components. Let's add another constrain.<br>";
				result += AddConstrain(outputTable, out outputTable);
				result += "So new Table looks like this:<br>";

				ReevaluateBasisAndDeltas(outputTable);
				result += PrintTableToHTML(outputTable, curBasis, curDelta);

				result += "Using dual Simplex method:<br>";
				var dualSimplex = new DualSimplexMethod();
				result += dualSimplex.MakeTransform(outputTable, out outputTable, out success);
			}

			if (success) {
				result += "That's it. Now we have integer solution.<br>";
			}

			return result;
		}

		protected abstract string AddConstrain (SimplexTable inputTable, out SimplexTable outputTable);

		protected bool IsIntegerVector (List<SimplexCoef> bVector) {
			bool integerVector = true;
			bVector.ForEach(b => integerVector = integerVector && b.value.Fract() == 0);
			return integerVector;
		}
	}
}
