using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
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

		public ICommand SimplexCommand {
			get => new ViewModelCommand(
				param => {
					var simplexTransform = new SimplexMethod();
					var output = simplexTransform.MakeTransform(InputTable, out SimplexTable table, out bool success);


					using (var nwFile = File.OpenWrite("result.html")) {
						using (var writer = new StreamWriter(nwFile)) {
							writer.WriteLine(output);
						}
					}
					
				}	
			);
		}

	}
}
