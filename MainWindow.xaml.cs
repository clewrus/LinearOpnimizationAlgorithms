
using System.Windows;
using YakimovTheSimplex.ViewModels;

namespace YakimovTheSimplex {
	public partial class MainWindow : Window {
		public MainWindow () {
			InitializeComponent();
			this.DataContext = new MainWindowViewModel();
		}
	}
}
