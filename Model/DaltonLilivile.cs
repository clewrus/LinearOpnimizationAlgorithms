using BigRationalExtentions.Model;
using Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class DaltonLilivile : GomoryCommon, ISimplexTableTransform {
		protected override string AddConstrain (SimplexTable inputTable, out SimplexTable outputTable) {
			outputTable = new SimplexTable(inputTable);
			foreach (var set in outputTable.discreteSet) {
				set.Sort();
				for (int i = 0; i < set.Count - 1; i++) {
					if (set[i] == set[i + 1]) {
						set.RemoveAt(i--);
					}
				}
			}
			ReevaluateBasisAndDeltas(outputTable);

			var nuVector = FormNuVector(outputTable, curBasis);
			var gamas = FormGamasVector(outputTable, nuVector, curBasis);

			var selectedI = FindIndexOfMin(gamas);
			var nwI = outputTable.NumOfConstrains;
			var result = $"Let's use {outputTable.cLables[curBasis[selectedI]].Value} row to form new constrain.<br>";

			outputTable.NumOfConstrains += 1;
			for (int j = 0; j < outputTable.NumOfVariables; j++) {
				if (curBasis.Contains(j)) continue;
				if (outputTable.aMatrix[selectedI][j].value.Sign >= 0) {
					outputTable.aMatrix[nwI][j].value = (-outputTable.aMatrix[selectedI][j]).value;
				} else {
					var b = outputTable.bVector[selectedI];
					var nu = outputTable.discreteSet[curBasis[selectedI]][nuVector[selectedI]];
					var next_nu = outputTable.discreteSet[curBasis[selectedI]][nuVector[selectedI] + 1];

					outputTable.aMatrix[nwI][j].value = (-(((b - nu) / (next_nu - b)) * (-outputTable.aMatrix[selectedI][j]))).value;
				}
			}

			outputTable.bVector[nwI].value = gamas[selectedI].value;

			outputTable.NumOfVariables += 1;
			outputTable.aMatrix[nwI][outputTable.NumOfVariables - 1].value = BigRational.One;

			return result;
		}

		private List<SimplexCoef> FormGamasVector (SimplexTable table, List<int> nuVec, List<int> basis) {
			var gamas = new List<SimplexCoef>();
			for (int i = 0; i < table.NumOfConstrains; i++) {
				if (nuVec[i] < 0) {
					gamas.Add(null);
				} else {
					gamas.Add(-(table.bVector[i] - table.discreteSet[basis[i]][nuVec[i]]));
				}
			}
			return gamas;
		}

		private List<int> FormNuVector (SimplexTable table, List<int> basis) {
			var nuVec = new List<int>();

			for (int i = 0; i < table.NumOfConstrains; i++) {
				if (!table.cLables[basis[i]].IsSelected || table.discreteSet[basis[i]].Contains(table.bVector[i])) {
					nuVec.Add(-1);
					continue;
				} else {
					int nu = -1;
					for (; nu < table.discreteSet[basis[i]].Count - 1; nu++) {
						if (table.bVector[i] < table.discreteSet[basis[i]][nu + 1]) break;
					}
					Debug.Assert(nu >= 0, "Variable is greater then any number from discrete set.");
					nuVec.Add(nu);
				}
			}
			return nuVec;
		}

		protected override bool IsDone (SimplexTable table) {
			ReevaluateBasisAndDeltas(table);
			for (int i = 0; i < table.NumOfConstrains; i++) {
				int ind = curBasis[i];
				if (table.cLables[ind].IsSelected) {
					bool contains = false;
					foreach (var c in table.discreteSet[ind]) {
						contains = contains || (table.bVector[i].value == c.value);
					}
					if (!contains) {
						return false;
					}
				}
			}
			return true;
		}
	}
}
