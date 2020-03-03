using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class JordanTransform : ISimplexTableTransform {
		int tarI;
		int tarJ;

		public JordanTransform (int i, int j) {
			this.tarI = i;
			this.tarJ = j;
		}

		public string MakeTransform (SimplexTable inputTable, out SimplexTable outputTable, out bool success) {
			outputTable = new SimplexTable(inputTable);

			var a = inputTable.aMatrix;
			for (int i = 0; i < outputTable.NumOfConstrains; i++) {
				for (int j = 0; j < outputTable.NumOfVariables; j++) {
					if (i == tarI) {
						outputTable.aMatrix[i][j] /= a[tarI][tarJ];
					} else {
						outputTable.aMatrix[i][j] -= (a[i][tarJ] * a[tarI][j]) / a[tarI][tarJ];
					}
				}

				if (i == tarI) {
					outputTable.bVector[i] /= a[tarI][tarJ];
				} else {
					outputTable.bVector[i] -= (a[i][tarJ] * inputTable.bVector[tarI]) / a[tarI][tarJ];
				}
				
			}

			success = true;
			return "";
		}
	}
}
