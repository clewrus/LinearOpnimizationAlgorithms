using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace YakimovTheSimplex.Model {
	public class SimplexTable : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

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

		public SimplexCoef constantValue;

		public List<CostLable> cLables;
		public List<SimplexCoef> cVector;

		public List<List<SimplexCoef>> aMatrix;
		public List<SimplexCoef> bVector;

		public List<List<SimplexCoef>> discreteSet;
		public List<int> sinteticVariables;

		public SimplexTable () {
			constantValue = new SimplexCoef();
			cLables = new List<CostLable>();
			cVector = new List<SimplexCoef>();
			aMatrix = new List<List<SimplexCoef>>();
			bVector = new List<SimplexCoef>();
			discreteSet = new List<List<SimplexCoef>>();
			sinteticVariables = new List<int>();
		}

		public SimplexTable (SimplexTable other) : this() {
			this._numOfVariables = other._numOfVariables;
			this._numOfConstrains = other._numOfConstrains;
			this._minimisationTask = other._minimisationTask;

			this.constantValue = new SimplexCoef(other.constantValue);
			other.cLables.ForEach(otherLable => this.cLables.Add(new CostLable(otherLable)));
			other.cVector.ForEach(otherCoef => this.cVector.Add(new SimplexCoef(otherCoef)));

			other.aMatrix.ForEach(otherRow => {
				var nwRow = new List<SimplexCoef>(otherRow.Count);
				otherRow.ForEach(otherCoef => nwRow.Add(new SimplexCoef(otherCoef)));
				this.aMatrix.Add(nwRow);
			});

			other.bVector.ForEach(otherCoef => this.bVector.Add(new SimplexCoef(otherCoef)));
			other.discreteSet.ForEach(otherSet => {
				var nwSet = new List<SimplexCoef>();
				otherSet.ForEach(otherCoef => nwSet.Add(new SimplexCoef(otherCoef)));
				discreteSet.Add(nwSet);
			});
			this.sinteticVariables = new List<int>(other.sinteticVariables);
		}

		public bool TryFindBasis (out int[] basisVariablesIndexes) {
			var result = new int[aMatrix.Count];
			for (int i = 0; i < result.Length; i++) result[i] = -1;

			for (int i = 0; i < aMatrix.Count; i++) {
				for (int j = 0; j < aMatrix[i].Count; j++) {
					if (!aMatrix[i][j].IsOne()) continue;

					bool isBasis = true;
					for (int i0 = 0; i0 < aMatrix.Count; i0++) {
						isBasis = isBasis && (i0 == i || aMatrix[i0][j].IsZero()); 
					}

					if (isBasis) {
						result[i] = j;
						break;
					}
				}
			}

			basisVariablesIndexes = result;
			foreach (var ind in result) {
				if (ind == -1) return false;
			}

			return true;
		}

		#region Risizing

		private void SetNumOfVariables (int nwNum) {
			int delta = nwNum - cLables.Count;
			for (int i = 0; i < delta; i++) {
				cLables.Add(new CostLable { Value = $"x{cLables.Count + 1}" });
				cVector.Add(new SimplexCoef());
				discreteSet.Add(new List<SimplexCoef>());

				for (int j = 0; j < aMatrix.Count; j++) {
					aMatrix[j].Add(new SimplexCoef());
				}
			}

			for (int i = delta; i < 0; i++) {
				cLables.RemoveAt(cLables.Count - 1);
				cVector.RemoveAt(cVector.Count - 1);
				discreteSet.RemoveAt(discreteSet.Count - 1);

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

		#endregion

		public class CostLable : INotifyPropertyChanged {
			public event PropertyChangedEventHandler PropertyChanged;

			public CostLable () { }

			public CostLable (CostLable other) {
				this._hasError = other._hasError;
				this._value = other._value;
				this._isSelected = other._isSelected;
			}

			private bool _isSelected;
			public bool IsSelected {
				get => _isSelected;
				set {
					_isSelected = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Selected"));
				}
			}

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
