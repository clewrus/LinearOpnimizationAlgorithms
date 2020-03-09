using BigRationalExtentions.Model;
using Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class GomoriII : GomoryCommon, ISimplexTableTransform {
		protected override string AddConstrain (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
			outputTable = new SimplexTable(inputTable);
			success = true;

			ReevaluateBasisAndDeltas(outputTable);
			var bFracts = FormBFractVector(outputTable);
			int selectedI = FindIndexOfMin(bFracts);

			int nwI = outputTable.NumOfConstrains;
			outputTable.NumOfConstrains += 1;

			var result = $"Let's use {outputTable.cLables[curBasis[selectedI]].Value} row to form new constrain.<br>";

			var bFract = outputTable.bVector[selectedI].value.Fract();
			for (int j = 0; j < outputTable.NumOfVariables; j++) {
				BigRational tarA = outputTable.aMatrix[selectedI][j].value;

				if (!outputTable.cLables[j].IsSelected) {
					if (tarA.Sign >= 0) {
						outputTable.aMatrix[nwI][j].value = -tarA;
					} else {
						outputTable.aMatrix[nwI][j].value = -((bFract / (BigRational.One - bFract)) * (-tarA));
					}
				} else {
					if (tarA.Fract() <= bFract) {
						outputTable.aMatrix[nwI][j].value = -tarA.Fract();
					} else {
						outputTable.aMatrix[nwI][j].value = -((bFract / (BigRational.One - bFract)) * (BigRational.One - tarA));
					}
				}
			}

			outputTable.bVector[nwI].value = -bFract;
			outputTable.NumOfVariables += 1;
			outputTable.aMatrix[nwI][outputTable.NumOfVariables - 1].value = BigRational.One;

			return result;

		}

		private List<SimplexCoef> FormBFractVector (SimplexTable table) {
			var bFracts = new List<SimplexCoef>();

			for (int i = 0; i < table.NumOfConstrains; i++) {
				var curBFract = table.bVector[i].value.Fract();

				if (curBFract == 0 || !table.cLables[curBasis[i]].IsSelected) {
					bFracts.Add(null);
				} else {
					var nwCoef = new SimplexCoef(table.bVector[i]);
					nwCoef.value = -curBFract;
					bFracts.Add(nwCoef);
				}
			}
			return bFracts;
		}

		protected override bool IsDone (SimplexTable table) {
			ReevaluateBasisAndDeltas(table);
			for (int i = 0; i < table.NumOfConstrains; i++) {
				int ind = curBasis[i];
				if (table.cLables[ind].IsSelected && table.bVector[i].value.Fract() != 0) {
					return false;
				}
			}
			return true;
		}
	}
}
