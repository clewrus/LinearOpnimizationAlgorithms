using BigRationalExtentions.Model;
using Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class DaltonLilivile : GomoryCommon, ISimplexTableTransform {
		protected override string AddConstrain (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
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

			var nuVector = GetValidNuVector(outputTable, out outputTable, out string result);
			if (nuVector == null) {
				success = false;
				return "Can't solve task with such discrete set.<br>";
			}

			var gamas = FormGamasVector(outputTable, nuVector, curBasis);

			var selectedI = FindIndexOfMin(gamas);
			var nwI = outputTable.NumOfConstrains;
			result += $"Let's use {outputTable.cLables[curBasis[selectedI]].Value} row to form new constrain.<br>";

			outputTable.NumOfConstrains += 1;
			for (int j = 0; j < outputTable.NumOfVariables; j++) {
				if (curBasis.Contains(j)) continue;
				if (outputTable.aMatrix[selectedI][j].value.Sign >= 0) {
					outputTable.aMatrix[nwI][j].value = (-outputTable.aMatrix[selectedI][j]).value;
				} else {
					var b = outputTable.bVector[selectedI];
					var nu = outputTable.discreteSet[curBasis[selectedI]][nuVector[selectedI].Value];
					if (b != nu) {
						var next_nu = outputTable.discreteSet[curBasis[selectedI]][nuVector[selectedI].Value + 1];
						outputTable.aMatrix[nwI][j].value = (-(((b - nu) / (next_nu - b)) * (-outputTable.aMatrix[selectedI][j]))).value;
					} else {
						outputTable.aMatrix[nwI][j].value = BigRational.Zero;
					}
				}
			}

			outputTable.bVector[nwI].value = gamas[selectedI].value;

			outputTable.NumOfVariables += 1;
			outputTable.aMatrix[nwI][outputTable.NumOfVariables - 1].value = BigRational.One;

			success = true;
			return result;
		}

		private List<SimplexCoef> FormGamasVector (SimplexTable table, List<int?> nuVec, List<int> basis) {
			var gamas = new List<SimplexCoef>();
			for (int i = 0; i < table.NumOfConstrains; i++) {
				if (nuVec[i] == null || nuVec[i].Value < 0) {
					gamas.Add(null);
				} else {
					gamas.Add(-(table.bVector[i] - table.discreteSet[basis[i]][nuVec[i].Value]));
				}
			}
			return gamas;
		}


		private List<int?> GetValidNuVector (SimplexTable inputTable, out SimplexTable outputTable, out string result) {
			outputTable = inputTable;
			result = "";

			List<int?> nuVector = null;
			bool foundAllNus = false;
			while (!foundAllNus) {
				ReevaluateBasisAndDeltas(outputTable);
				nuVector = FormNuVector(outputTable, curBasis, out foundAllNus);
				if (!foundAllNus) {
					var tarJ = curBasis[nuVector.IndexOf(-1)];
					result += $"{outputTable.cLables[tarJ].Value} became bigger than any element of its set.<br>";
					var maxElem = outputTable.discreteSet[tarJ].Max();
					result += $"So, let's add constrain '{outputTable.cLables[tarJ].Value}' < {maxElem.ToString()} and run MMethod.<br>";

					outputTable = new SimplexTable(outputTable);
					outputTable.NumOfConstrains += 1;
					outputTable.NumOfVariables += 1;
					outputTable.aMatrix[outputTable.NumOfConstrains - 1][tarJ].value = BigRational.One;
					outputTable.aMatrix[outputTable.NumOfConstrains - 1][outputTable.NumOfVariables - 1].value = BigRational.One;
					outputTable.bVector[outputTable.NumOfConstrains - 1].value = maxElem.value;

					var simplexMethod = new SimplexMethod();
					result += simplexMethod.MakeTransform(outputTable, out outputTable, out bool success);
					if (!success) {
						return null;
					}
				}
			}
			
			return nuVector;
		}

		private List<int?> FormNuVector (SimplexTable table, List<int> basis, out bool success) {
			var nuVec = new List<int?>();
			success = true;

			for (int i = 0; i < table.NumOfConstrains; i++) {
				if (!table.cLables[basis[i]].IsSelected || table.discreteSet[basis[i]].Contains(table.bVector[i])) {
					nuVec.Add(null);
					continue;
				} else {
					int nu = -1;
					
					for (int k = 1; k < table.discreteSet[basis[i]].Count; k++) {
						if (table.bVector[i].value < table.discreteSet[basis[i]][k].value) {
							nu = k - 1;
						}
					}

					if (table.discreteSet[basis[i]][table.discreteSet[basis[i]].Count - 1].value == table.bVector[i].value) {
						nu = table.discreteSet[basis[i]].Count - 1;
					}

					success = success && (nu >= 0);
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
