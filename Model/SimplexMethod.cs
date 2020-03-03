using Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class SimplexMethod : ISimplexTableTransform {
		private List<int> curBasis;
		private List<SimplexCoef> curDelta;

		public string MakeTransform (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
			outputTable = new SimplexTable(inputTable);
			string result = "Let's use simplex method.<br>";
			PrepareForMethod(outputTable, out outputTable, ref result);

			result += "Step 0:<br>";
			result += PrintTableToHTML(outputTable, curBasis, curDelta);

			int iterNum = 0;
			string checkResult = "";
			while (CanContinue(outputTable, curDelta, out checkResult, out success)) {
				if (iterNum++ > 0) result += $"Step {iterNum}:<br>";
				int minDeltaIndex = FindIndexOfMin(curDelta);

				var tetas = new List<SimplexCoef>();
				for (int i = 0; i < outputTable.NumOfConstrains; i++) {
					if (outputTable.aMatrix[i][minDeltaIndex].value.Sign <= 0) {
						tetas.Add(null);
						continue;
					}
					tetas.Add(outputTable.bVector[i] / outputTable.aMatrix[i][minDeltaIndex]);
				}

				int minTetaIndex = FindIndexOfMin(tetas);
				result += $"{outputTable.cLables[minDeltaIndex].Value} going inside the basis.<br>";
				result += $"{outputTable.cLables[curBasis[minTetaIndex]].Value} going outside the basis.<br>";

				var jordanTransform = new JordanTransform(minTetaIndex, minDeltaIndex);
				result += jordanTransform.MakeTransform(outputTable, out outputTable, out bool s);

				ReevaluateBasisAndDaltas(outputTable);
				result += PrintTableToHTML(outputTable, curBasis, curDelta);
				result += "<br><br><br>";
			}

			result += checkResult;

			return result;
		}

		private bool CanContinue (SimplexTable outputTable, List<SimplexCoef> delta, out string result, out bool success) {
			bool hasNegative = false;
			delta.ForEach(c => hasNegative = hasNegative || c.value.Sign < 0);

			if (!hasNegative) {
				result = "All simplex deltas aren't negative.<br>";
				success = true;
				return false;
			}

			for (int j = 0; j < outputTable.NumOfVariables; j++) {
				bool allNegative = delta[j].value.Sign < 0;
				for (int i = 0; i < outputTable.NumOfConstrains; i++) {
					allNegative = allNegative && outputTable.aMatrix[i][j].value.Sign < 0;
				}

				if (allNegative) {
					result = "Problem has Infinite solution. Simplex method was ended.<br>";
					success = false;
					return false;
				}
			}

			result = "";
			success = false;
			return true;
		}

		private string PrintTableToHTML (SimplexTable table, List<int> basis, List<SimplexCoef> delta) {
			var resBuilder = new StringBuilder();
			resBuilder.Append("<table><tr>");

			resBuilder.Append("<th>C(i)</th><th>X(basis)</th>");
			table.cLables.ForEach(lable => resBuilder.Append($"<th>{lable.Value}</th>"));
			resBuilder.Append("<th>B</th></tr>");

			for (int i = 0; i < table.NumOfConstrains; i++) {
				resBuilder.Append("<tr>");
				resBuilder.Append($"<td class='cValue'>{table.cVector[basis[i]].ToString()}</td>");
				resBuilder.Append($"<td class='cLabel'>{table.cLables[basis[i]].Value}</td>");

				for (int j = 0; j < table.NumOfVariables; j++) {
					resBuilder.Append($"<td class='aMatrixElement'>{table.aMatrix[i][j].ToString()}</td>");
				}

				resBuilder.Append($"<td class='bVector'>{table.bVector[i].ToString()}</td>");
				resBuilder.Append("</tr>");
			}

			resBuilder.Append("<tr><td></td><td></td>");
			for (int j = 0; j < table.NumOfVariables; j++) {
				resBuilder.Append($"<td class='deltaElement'>{delta[j].ToString()}</td>");
			}
			resBuilder.Append("<td></td></tr></table>");

			return resBuilder.ToString();
		}

		private void PrepareForMethod (SimplexTable inputTable, out SimplexTable outputTable, ref string result) {
			outputTable = new SimplexTable(inputTable);
			if (!outputTable.MinimisationTask) {
				result += "Let's Invert Cost function so that we can solve minimization problem<br>";

				outputTable.MinimisationTask = true;
				outputTable.cVector.ForEach(coef => coef.value *= BigRational.MinusOne);
				outputTable.constantValue.value *= BigRational.MinusOne;
			}

			var basisFormer = new MMethod();
			result += basisFormer.MakeTransform(outputTable, out outputTable, out bool s);

			ReevaluateBasisAndDaltas(outputTable);
		}

		private void ReevaluateBasisAndDaltas (SimplexTable table) {
			if (!table.TryFindBasis(out int[] basis)) {
				Debug.Fail("Can't find basis after MMethod.");
			}

			curBasis = new List<int>(basis);

			var basisC = new List<SimplexCoef>();
			foreach (int ind in basis) {
				basisC.Add(table.cVector[ind]);
			}

			curDelta = EvaluateDeltas(basisC, table);
		}

		private int FindIndexOfMin (List<SimplexCoef> delta) {
			int resIndex = 0;
			SimplexCoef minCoef = delta[0];

			for (int i = 0; i < delta.Count; i++) {
				if (delta[i] == null) continue;
				if (minCoef == null || delta[i] < minCoef) {
					resIndex = i;
					minCoef = delta[i];
				}
			}

			return resIndex;
		}

		private List<SimplexCoef> EvaluateDeltas (List<SimplexCoef> basisC, SimplexTable table) {
			var simplexDeltes = new List<SimplexCoef>();

			for (int j = 0; j < table.NumOfVariables; j++) {
				var dotProd = new SimplexCoef();
				for (int i = 0; i < table.NumOfConstrains; i++) {
					dotProd += basisC[i] * table.aMatrix[i][j];
				}
				simplexDeltes.Add(table.cVector[j] - dotProd);
			}

			return simplexDeltes;
		}
	}
}
