using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using YakimovTheSimplex.Model;

namespace YakimovTheSimplex.ViewModels {
	class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged {

		public MainWindowViewModel () {
			InputTable = new SimplexTable();
		}

		private SimplexTable _inputTable;
		public	SimplexTable InputTable {
			get => _inputTable;
			set => SetProperty(ref _inputTable, value);
		}

	}
}
