using Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class MMethod : ISimplexTableTransform {
		private bool multNegRows;

		public MMethod (bool multNegativeRowsByMinusOne=true) {
			multNegRows = multNegativeRowsByMinusOne;
		}

		public string MethodName => "M-Method";

		public string MakeTransform (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
			outputTable = new SimplexTable(inputTable);
			string result = "";

			if (multNegRows) {
				var invertedLines = false;
				for (int i = 0; i < outputTable.NumOfConstrains; i++) {
					if (outputTable.bVector[i].value >= 0) continue;

					invertedLines = true;
					outputTable.bVector[i].value *= BigRational.MinusOne;
					for (int j = 0; j < outputTable.NumOfVariables; j++) {
						outputTable.aMatrix[i][j].value *= BigRational.MinusOne;
					}
				}

				
				if (invertedLines) {
					result += "Some of constrains equations were multiplied by -1.<br>";
				}
			}
			

			success = true;
			return result + AddBasisVariables(outputTable);
		}

		private string AddBasisVariables (SimplexTable table) {
			if (table.TryFindBasis(out int[] basisIndex)) {
				var basisVariables = "";
				foreach (var ind in basisIndex) {
					basisVariables += $"{table.cLables[ind].Value} ";
				}
				return "Table has basis: " + basisVariables + "<br>";
			}

			string result = "Let's use M method to add some new variables.<br>";
			result += "So ..... our new sintetic variables will be: ";

			for (int i = 0; i < basisIndex.Length; i++) {
				if (basisIndex[i] >= 0) continue;

				table.NumOfVariables += 1;
				table.sinteticVariables.Add(table.cVector.Count - 1);
				var l_coef = table.cVector[table.cVector.Count - 1];
				
				l_coef.isM = true;
				l_coef.value = (table.MinimisationTask) ? BigRational.One : BigRational.MinusOne;
				l_coef.UpdateStringValue();

				table.cLables[table.cLables.Count - 1].Value = $"y{table.cLables.Count}";
				result += $"{table.cLables[table.cLables.Count - 1].Value} ";

				table.aMatrix[i][table.aMatrix[i].Count - 1].value = BigRational.One;
				table.aMatrix[i][table.aMatrix[i].Count - 1].UpdateStringValue();
			}

			result += "<br>";
			return result;
		}
	}
}
