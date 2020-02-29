using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YakimovTheSimplex.Model;

namespace YakimovTheSimplex.UIElements {

	public partial class SimplexTableControl : UserControl {

		private List<(TextBox label, TextBox value)> costElements;

		private List<List<TextBox>> constrains;
		private List<UIElement> constrainsHeaders;

		private List<TextBox> bValues;

		public SimplexTableControl () {
			costElements = new List<(TextBox label, TextBox value)>();
			constrains = new List<List<TextBox>>();
			constrainsHeaders = new List<UIElement>();
			bValues = new List<TextBox>();

			InitializeComponent();
			InitializeConstraintHeader();

			AddCostFunctionVariable(null, null);
			AddCostFunctionVariable(null, null);
			AddCostFunctionVariable(null, null);

			AddConstraint(null, null);
			AddConstraint(null, null);
		}

		private void AddCostFunctionVariable (object sender, RoutedEventArgs e) {
			AddVariableToCostFunction();
		}

		private void RemoveCostFunctionVariable (object sender, RoutedEventArgs e) {
			if (costElements.Count <= 1) return;

			RemoveVariableFromCostFunction();
			RemoveVariableFromConstrains();
		}

#region CostFunction

		private void AddVariableToCostFunction () {
			var nwElement = CreateCostElement();
			costElements.Add(nwElement);

			LGrid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(50)
			});

			LGrid.Children.Add(nwElement.label);
			LGrid.Children.Add(nwElement.value);

			Grid.SetRow(nwElement.label, 0);
			Grid.SetRow(nwElement.value, 1);

			Grid.SetColumn(nwElement.label, LGrid.ColumnDefinitions.Count - 1);
			Grid.SetColumn(nwElement.value, LGrid.ColumnDefinitions.Count - 1);

			AddConstraintVariable(nwElement.label);
		}

		private (TextBox label, TextBox value) CreateCostElement () {
			bool theFirst = LGrid.ColumnDefinitions.Count == 0;

			var nwLabel = new TextBox {
				Text = $"x{LGrid.ColumnDefinitions.Count + 1}",
				Padding = new Thickness(4),
				FontSize = 15,
				TextAlignment = TextAlignment.Center,

				BorderBrush = new SolidColorBrush(Colors.AliceBlue),
				BorderThickness = new Thickness((theFirst) ? 0 : 1, 0, 0, 1),
			};

			var nwValue = new TextBox {
				Text = "0",
				Padding = new Thickness(4),
				FontSize = 15,
				TextAlignment = TextAlignment.Center,

				BorderBrush = new SolidColorBrush(Colors.AliceBlue),
				BorderThickness = new Thickness((theFirst)? 0: 1, 1, 0, 0),
			};

			return (nwLabel, nwValue);
		}

		private void RemoveVariableFromCostFunction () {
			var tarElement = costElements[costElements.Count - 1];
			costElements.RemoveAt(costElements.Count - 1);

			LGrid.Children.Remove(tarElement.label);
			LGrid.Children.Remove(tarElement.value);

			LGrid.ColumnDefinitions.RemoveAt(LGrid.ColumnDefinitions.Count - 1);
		}

		#endregion

#region ConstrainsFunction

		private void InitializeConstraintHeader () {
			CGrid.RowDefinitions.Add(new RowDefinition { });
		}

		private void AddConstraint (object sender, RoutedEventArgs e) {
			CGrid.RowDefinitions.Add(new RowDefinition { });
			constrains.Add(new List<TextBox>());

			for (int i = 0; i < CGrid.ColumnDefinitions.Count; i++) {
				var nwCoef = ConstrainsCoef();

				Grid.SetRow(nwCoef, CGrid.RowDefinitions.Count - 1);
				Grid.SetColumn(nwCoef, i);

				CGrid.Children.Add(nwCoef);
				constrains[constrains.Count - 1].Add(nwCoef);
			}

			var nwBValue = ConstrainsCoef();
			bValues.Add(nwBValue);

			BGrid.RowDefinitions.Add(new RowDefinition { });
			Grid.SetRow(nwBValue, BGrid.RowDefinitions.Count - 1);
			Grid.SetColumn(nwBValue, 0);
			BGrid.Children.Add(nwBValue);
		}

		private void RemoveConstraint (object sender, RoutedEventArgs e) {
			if (constrains.Count <= 1) return;

			foreach (var c in constrains[constrains.Count - 1]) {
				CGrid.Children.Remove(c);
			}

			constrains.RemoveAt(constrains.Count - 1);
			CGrid.RowDefinitions.RemoveAt(CGrid.RowDefinitions.Count - 1);

			BGrid.Children.Remove(bValues[bValues.Count - 1]);
			BGrid.RowDefinitions.RemoveAt(BGrid.RowDefinitions.Count - 1);
			bValues.RemoveAt(bValues.Count - 1);
		}

		private void AddConstraintVariable (TextBox label) {
			CGrid.ColumnDefinitions.Add(new ColumnDefinition {
				Width = new GridLength(50),
			});

			var header = new TextBox {
				Background = new SolidColorBrush(Colors.LightGray),
				TextAlignment = TextAlignment.Center,
				FontSize = 15,
			};

			Grid.SetRow(header, 0);
			Grid.SetColumn(header, CGrid.ColumnDefinitions.Count - 1);

			CGrid.Children.Add(header);
			constrainsHeaders.Add(header);

			var curText = label.Text;
			label.SetBinding(TextBox.TextProperty, new Binding {
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				Source = header,
				Path = new PropertyPath("Text"),
			});
			label.Text = curText;

			for (int i = 0; i < constrains.Count; i++) {
				var nwCoef = ConstrainsCoef();

				Grid.SetRow(nwCoef, i + 1);
				Grid.SetColumn(nwCoef, CGrid.ColumnDefinitions.Count - 1);

				CGrid.Children.Add(nwCoef);
				constrains[i].Add(nwCoef);
			}
		}

		private void RemoveVariableFromConstrains () {
			CGrid.Children.Remove(constrainsHeaders[constrainsHeaders.Count - 1]);
			constrainsHeaders.RemoveAt(constrainsHeaders.Count - 1);

			CGrid.ColumnDefinitions.RemoveAt(CGrid.ColumnDefinitions.Count - 1);
			for (int i = 0; i < constrains.Count; i++) {
				CGrid.Children.Remove(constrains[i][constrains[i].Count - 1]);
				constrains[i].RemoveAt(constrains[i].Count - 1);
			}
		}


		private TextBox ConstrainsCoef () {
			return new TextBox {
				TextAlignment = TextAlignment.Center,
				BorderBrush = new SolidColorBrush(Colors.LightGray),
				FontSize = 15,
				Text = "0",			
			};
		}

		#endregion

		public SimplexTable SimplexTableProperty {
			get { return (SimplexTable)GetValue(SimplexTablePropertyProperty); }
			set { SetValue(SimplexTablePropertyProperty, value); }
		}

		public static readonly DependencyProperty SimplexTablePropertyProperty =
			DependencyProperty.Register("SimplexTableProperty", typeof(SimplexTable),
				typeof(SimplexTableControl), new PropertyMetadata(null));
	}
}
