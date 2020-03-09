using System;
using System.Collections.Generic;
using System.Text;

namespace YakimovTheSimplex.Model {
	public interface ISimplexTableTransform {
		string MakeTransform (SimplexTable inputTable, out SimplexTable outputTable, out bool success);

		string MethodName { get; }
	}
}
