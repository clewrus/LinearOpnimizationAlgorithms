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
				param => RunMethod(new SimplexMethod(), "SimplexResult.html")	
			);
		}

		public ICommand DualSimplexCommand {
			get => new ViewModelCommand(
				param => RunMethod(new DualSimplexMethod(), "DualSimplexResult.html"),
				param => InputTable.TryFindBasis(out int[] basis)
			);
		}

		public ICommand GomoriICommand {
			get => new ViewModelCommand(
				param => RunMethod(new GomoriI(), "GomoriIResult.html")
			);
		}

		public ICommand GomoriIICommand {
			get => new ViewModelCommand(
				param => RunMethod(new GomoriII(), "GomoriIIResult.html")
			);
		}

		public ICommand DaltonLilivileCommand {
			get => new ViewModelCommand(
				param => RunMethod(new DaltonLilivile(), "DaltonLilivileResult.html")
			);
		}

		private void RunMethod (ISimplexTableTransform method, string fileName) {
			string output = "";
			try {
				output = method.MakeTransform(InputTable, out SimplexTable table, out bool success);
			} catch {

			}
			

			string path = null;
			using (var nwFile = File.Open(fileName, FileMode.Create, FileAccess.Write)) {
				using (var writer = new StreamWriter(nwFile)) {
					writer.WriteLine(output);
				}
				path = nwFile.Name;
			}

			try {
				System.Diagnostics.Process.Start(@"cmd.exe ", @"/c " + path);
			} catch { }
			
		}

	}
}
