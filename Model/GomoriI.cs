﻿using BigRationalExtentions.Model;
using Numerics;
using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class GomoriI : GomoryCommon, ISimplexTableTransform {
		protected override string AddConstrain (SimplexTable inputTable, out SimplexTable outputTable) {
			outputTable = new SimplexTable(inputTable);
			var bFracts = FormBFractVector(outputTable);

			int selectedI = FindIndexOfMin(bFracts);
			int nwI = outputTable.NumOfConstrains;

			ReevaluateBasisAndDeltas(outputTable);
			var result = $"Let's use {outputTable.cLables[curBasis[selectedI]].Value} row to form new constrain.<br>";

			outputTable.NumOfConstrains += 1;
			for (int j = 0; j < outputTable.NumOfVariables; j++) {
				outputTable.aMatrix[nwI][j].value = -outputTable.aMatrix[selectedI][j].value.Fract();
			}
			outputTable.bVector[nwI].value = bFracts[selectedI].value;

			outputTable.NumOfVariables += 1;
			outputTable.aMatrix[nwI][outputTable.NumOfVariables - 1].value = BigRational.One;

			return result;
		}

		private List<SimplexCoef> FormBFractVector (SimplexTable table) {
			var bFracts = new List<SimplexCoef>();
			foreach (var b in table.bVector) {
				var curBFract = b.value.Fract();
				if (curBFract == 0) {
					bFracts.Add(null);
				} else {
					var nwCoef = new SimplexCoef(b);
					nwCoef.value = -curBFract;
					bFracts.Add(nwCoef);
				}
			}
			return bFracts;
		}
	}
}