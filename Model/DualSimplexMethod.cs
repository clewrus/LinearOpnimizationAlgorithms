using Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class DualSimplexMethod : SimplexMethod, ISimplexTableTransform {
		public override string MakeTransform (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
			outputTable = new SimplexTable(inputTable);
			string result = "Let's use dual simplex method.<br>";
			PrepareForMethod(outputTable, out outputTable, ref result);

			result += "Step 0:<br>";
			result += PrintTableToHTML(outputTable, curBasis, curDelta);
			result += "<br><br>";

			int iterNum = 0;
			string checkResult = "";
			while (CanContinue(outputTable, out checkResult, out success)) {
				if (iterNum++ > 0) result += $"Step {iterNum}:<br>";
				int minBIndex = FindIndexOfMin(outputTable.bVector);

				var gamas = new List<SimplexCoef>();
				for (int j = 0; j < outputTable.NumOfVariables; j++) {
					if (outputTable.aMatrix[minBIndex][j].value.Sign >= 0) {
						gamas.Add(null);
						continue;
					}

					gamas.Add(curDelta[j] / (-outputTable.aMatrix[minBIndex][j]));
				}

				int minGamaIndex = FindIndexOfMin(gamas);
				result += $"{outputTable.cLables[minGamaIndex].Value} going inside the basis.<br>";
				result += $"{outputTable.cLables[curBasis[minBIndex]].Value} going outside the basis.<br>";

				var jordanTransform = new JordanTransform(minBIndex, minGamaIndex);
				result += jordanTransform.MakeTransform(outputTable, out outputTable, out bool s);

				ReevaluateBasisAndDeltas(outputTable);

				int numOfVariablesBeforeRemovingUnused = outputTable.NumOfVariables;
				result += RemoveUnusedSinteticVariables(outputTable, out outputTable, curBasis);
				if (outputTable.NumOfVariables != numOfVariablesBeforeRemovingUnused) {
					ReevaluateBasisAndDeltas(outputTable);
				}

				result += PrintTableToHTML(outputTable, curBasis, curDelta);
				result += "<br><br>";
			}

			result += checkResult;

			if (success) {
				result += FormAnswer(outputTable, curBasis);
			}
			return result;
		}

		private bool CanContinue (SimplexTable outputTable, out string result, out bool success) {
			bool hasNegative = false;
			var b = outputTable.bVector;
			b.ForEach(c => hasNegative = hasNegative || c.value.Sign < 0);

			if (!hasNegative) {
				result = "All dual simplex b aren't negative.<br>";
				success = true;
				return false;
			}

			for (int i = 0; i < outputTable.NumOfConstrains; i++) {
				if (b[i].value.Sign >= 0) continue;

				hasNegative = false;
				for (int j = 0; j < outputTable.NumOfVariables; j++) {
					hasNegative = hasNegative || outputTable.aMatrix[i][j].value.Sign < 0;
				}

				if (!hasNegative) {
					result = "Solution doesn't exist.";
					success = false;
					return false;
				}
			}

			result = "";
			success = false;
			return true;
		}

		private void PrepareForMethod (SimplexTable inputTable, out SimplexTable outputTable, ref string result) {
			outputTable = new SimplexTable(inputTable);
			if (!outputTable.MinimisationTask) {
				result += "Let's Invert Cost function so that we can solve minimization problem<br>";

				outputTable.MinimisationTask = true;
				outputTable.cVector.ForEach(coef => coef.value *= BigRational.MinusOne);
				outputTable.constantValue.value *= BigRational.MinusOne;
			}

			if (!outputTable.TryFindBasis(out int[] ind)) {
				throw new ArgumentException("Dual simplex method can't find basis.");
			}

			ReevaluateBasisAndDeltas(outputTable);
		}
	}
}
