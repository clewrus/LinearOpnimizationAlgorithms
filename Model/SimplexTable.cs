using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class SimplexTable : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		public SimplexCoef constantValue;

		public List<CostLable> cLables;
		public List<SimplexCoef> cVector;

		public List<List<SimplexCoef>> aMatrix;
		public List<SimplexCoef> bVector;

		public SimplexTable () {
			constantValue = new SimplexCoef();
			cLables = new List<CostLable>();
			cVector = new List<SimplexCoef>();
			aMatrix = new List<List<SimplexCoef>>();
			bVector = new List<SimplexCoef>();
		}

		private void SetNumOfVariables (int nwNum) {
			int delta = nwNum - cLables.Count;
			for (int i = 0; i < delta; i++) {
				cLables.Add(new CostLable { Value = $"x{cLables.Count + 1}" });
				cVector.Add(new SimplexCoef());

				for (int j = 0; j < aMatrix.Count; j++) {
					aMatrix[j].Add(new SimplexCoef());
				}
			}

			for (int i = delta; i < 0; i++) {
				cLables.RemoveAt(cLables.Count - 1);
				cVector.RemoveAt(cVector.Count - 1);

				for (int j = 0; j < aMatrix.Count; j++) {
					aMatrix[j].RemoveAt(aMatrix[j].Count - 1);
				}
			}
		}

		private void SetNumOfConstrains (int nwNum) {
			int delta = nwNum - aMatrix.Count;
			for (int i = 0; i < delta; i++) {
				bVector.Add(new SimplexCoef());

				var nwConstrain = new List<SimplexCoef>();
				for (int j = 0; j < cVector.Count; j++) {
					nwConstrain.Add(new SimplexCoef());
				}
				aMatrix.Add(nwConstrain);
			}

			for (int i = delta; i < 0; i++) {
				aMatrix.RemoveAt(aMatrix.Count - 1);
				bVector.RemoveAt(bVector.Count - 1);
			}

		}

		private int _numOfVariables;
		public int NumOfVariables {
			get => _numOfVariables;
			set {
				_numOfVariables = value;
				SetNumOfVariables(_numOfVariables);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumOfVariables"));
			}
		}

		private int _numOfConstrains;
		public int NumOfConstrains {
			get => _numOfConstrains;
			set {
				_numOfConstrains = value;
				SetNumOfConstrains(_numOfConstrains);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NumOfConstrains"));
			}
		}

		private bool _minimisationTask;
		public bool MinimisationTask {
			get => _minimisationTask;
			set {
				_minimisationTask = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MinimisationTask"));
			}
		}

		public class CostLable : INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;

			private bool _hasError;
			public bool HasError {
				get => _hasError;
				set {
					_hasError = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HasError"));
				}
			}

			private string _value;
			public string Value {
				get => _value;
				set {
					_value = value;
					HasError = _value.Trim().Length == 0;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
				}
			}
		}
	}
}
