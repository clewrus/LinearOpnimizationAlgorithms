using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		private List<(CheckBox isSelected, TextBox label, TextBox value)> costElements;

		private List<List<TextBox>> constrains;
		private List<UIElement> constrainsHeaders;

		private List<TextBox> bValues;

		public SimplexTableControl () {
			costElements = new List<(CheckBox isSelected, TextBox label, TextBox value)>();
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

		private void RebindFields (SimplexTable tarTable) {
			if (tarTable == null) return;

			var nwValue = (TaskTypeComboBox.SelectedItem as ComboBoxItem).Content.ToString();
			tarTable.MinimisationTask = nwValue == null || nwValue.ToLower() == "min"; ;

			UnbindFields();

			tarTable.NumOfConstrains = constrains.Count;
			tarTable.NumOfVariables = costElements.Count;

			var highlightErrorStyle = this.FindResource("HighlightErrorStyle") as Style;

			for (int i = 0; i < costElements.Count; i++) {
				costElements[i].isSelected.DataContext = tarTable.cLables[i];
				costElements[i].isSelected.SetBinding(CheckBox.IsCheckedProperty, new Binding {
					Path = new PropertyPath("IsSelected"),
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				});

				costElements[i].label.DataContext = tarTable.cLables[i];
				costElements[i].label.SetBinding(TextBox.TextProperty, new Binding {
					Path = new PropertyPath("Value"),
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				});
				costElements[i].label.Style = highlightErrorStyle;

				costElements[i].value.DataContext = tarTable.cVector[i];
				costElements[i].value.SetBinding(TextBox.TextProperty, new Binding {
					Path = new PropertyPath("StringValue"),
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				});
				costElements[i].value.Style = highlightErrorStyle;

			}
			for (int i = 0; i < bValues.Count; i++) {
				bValues[i].DataContext = tarTable.bVector[i];
				bValues[i].SetBinding(TextBox.TextProperty, new Binding {
					Path = new PropertyPath("StringValue"),
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				});
				bValues[i].Style = highlightErrorStyle;
			}
			for (int i = 0; i < constrains.Count; i++) {
				for (int j = 0; j < constrains[i].Count; j++) {
					constrains[i][j].DataContext = tarTable.aMatrix[i][j];
					constrains[i][j].SetBinding(TextBox.TextProperty, new Binding {
						Path = new PropertyPath("StringValue"),
						Mode = BindingMode.TwoWay,
						UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
					});
					constrains[i][j].Style = highlightErrorStyle;
				}
			}
			for (int i = 0; i < tarTable.NumOfVariables; i++) {
				var curRow = ((StackPanel)xSet.Children[i]).Children;
				if (tarTable.discreteSet[i].Count != curRow.Count - 1) {
					tarTable.discreteSet[i] = new List<SimplexCoef>();
					for (int k = 0; k < curRow.Count - 1; k++) {
						tarTable.discreteSet[i].Add(new SimplexCoef());
					}
				}
				for (int k = 0; k < curRow.Count - 1; k++) {
					var curText = ((TextBox)curRow[k]).Text;
					((TextBox)curRow[k]).DataContext = tarTable.discreteSet[i][k];
					((TextBox)curRow[k]).SetBinding(TextBox.TextProperty, new Binding {
						Path = new PropertyPath("StringValue"),
						UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
					});

					((TextBox)curRow[k]).TextChanged -= XSetCellTextChanged;
					((TextBox)curRow[k]).Text = curText;
					((TextBox)curRow[k]).TextChanged += XSetCellTextChanged;
				}
			}

			Constant.DataContext = tarTable.constantValue;
			Constant.SetBinding(TextBox.TextProperty, new Binding {
				Path = new PropertyPath("StringValue"),
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
			});
			Constant.Style = highlightErrorStyle;
		}

		private void UnbindFields () {
			costElements.ForEach(c => {
				c.label.DataContext = null;
				BindingOperations.ClearBinding(c.isSelected, CheckBox.IsCheckedProperty);

				c.label.Style = null;
				c.label.DataContext = null;
				BindingOperations.ClearBinding(c.label, TextBox.TextProperty);

				c.value.Style = null;
				c.value.DataContext = null;
				BindingOperations.ClearBinding(c.value, TextBox.TextProperty);
			});
			bValues.ForEach(v => {
				v.Style = null;
				v.DataContext = null;
				BindingOperations.ClearBinding(v, TextBox.TextProperty);
			});
			constrains.ForEach(c => c.ForEach(coef => {
				coef.Style = null;
				coef.DataContext = null;
				BindingOperations.ClearBinding(coef, TextBox.TextProperty);
			}));

			foreach (StackPanel rows in xSet.Children) {
				for (int i = 0; i < rows.Children.Count; i++) {
					TextBox cell = rows.Children[i] as TextBox;
					var t = cell.Text;
					BindingOperations.ClearBinding(cell, TextBox.TextProperty);
					cell.DataContext = null;

					cell.TextChanged -= XSetCellTextChanged;
					cell.Text = t;
					cell.TextChanged += XSetCellTextChanged;
				}
			}

			Constant.Style = null;
			Constant.DataContext = null;
			BindingOperations.ClearBinding(Constant, TextBox.TextProperty);
		}

		private void AddCostFunctionVariable (object sender, RoutedEventArgs e) {
			AddVariableToCostFunction();
			AddXSetRow();

			RebindFields(SimplexTableProperty);
		}

		private void RemoveCostFunctionVariable (object sender, RoutedEventArgs e) {
			if (costElements.Count <= 1) return;

			RemoveVariableFromCostFunction();
			RemoveVariableFromConstrains();

			RebindFields(SimplexTableProperty);
			RemoveXSetRow();
		}

		private void TaskTypeChanged (object sender, SelectionChangedEventArgs e) {
			var nwValue = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string;
			var isMax = nwValue != null && nwValue.ToLower() == "max";

			if (SimplexTableProperty != null) {
				SimplexTableProperty.MinimisationTask = !isMax;
			}
		}

#region CostFunction

		private void AddVariableToCostFunction () {
			var nwElement = CreateCostElement();
			costElements.Add(nwElement);

			LGrid.ColumnDefinitions.Add(new ColumnDefinition() {
				Width = new GridLength(50)
			});

			LGrid.Children.Add(nwElement.isSelected);
			LGrid.Children.Add(nwElement.label);
			LGrid.Children.Add(nwElement.value);

			Grid.SetRow(nwElement.isSelected, 0);
			Grid.SetRow(nwElement.label, 1);
			Grid.SetRow(nwElement.value, 2);

			Grid.SetColumn(nwElement.isSelected, LGrid.ColumnDefinitions.Count - 1);
			Grid.SetColumn(nwElement.label, LGrid.ColumnDefinitions.Count - 1);
			Grid.SetColumn(nwElement.value, LGrid.ColumnDefinitions.Count - 1);

			AddConstraintVariable(nwElement.label);
		}

		private (CheckBox isSelected, TextBox label, TextBox value) CreateCostElement () {
			bool theFirst = LGrid.ColumnDefinitions.Count == 0;

			var nwIsSelected = new CheckBox {
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Bottom,

				BorderBrush = new SolidColorBrush(Colors.DarkGray),
				BorderThickness = new Thickness(1, 1, 1, 1),
				Margin = new Thickness(0, -20, 0, 0),
			};

			var nwLabel = new TextBox {
				Text = $"x{LGrid.ColumnDefinitions.Count + 1}",
				Padding = new Thickness(4),
				FontSize = 15,
				TextAlignment = TextAlignment.Center,

				BorderBrush = new SolidColorBrush(Colors.LightGray),
				BorderThickness = new Thickness((theFirst) ? 0 : 1, 0, 0, 1),
			};

			var nwValue = new TextBox {
				Text = "0",
				Padding = new Thickness(4),
				FontSize = 15,
				TextAlignment = TextAlignment.Center,

				BorderBrush = new SolidColorBrush(Colors.LightGray),
				BorderThickness = new Thickness((theFirst)? 0: 1, 1, 0, 0),
			};

			return (nwIsSelected, nwLabel, nwValue);
		}

		private void RemoveVariableFromCostFunction () {
			var tarElement = costElements[costElements.Count - 1];
			costElements.RemoveAt(costElements.Count - 1);

			LGrid.Children.Remove(tarElement.isSelected);
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

			RebindFields(SimplexTableProperty);
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

			RebindFields(SimplexTableProperty);
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
			header.SetBinding(TextBox.TextProperty, new Binding {
				Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				Source = label,
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
			var c = new TextBox {
				TextAlignment = TextAlignment.Center,
				BorderBrush = new SolidColorBrush(Colors.LightGray),
				FontSize = 15,
				Text = "0",
			};

			return c;
		}

		#endregion

		#region XSet

		private void AddXSetRow () {
			var nwRow = new StackPanel {
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Left,
			};

			var c = XSetCoef();	
			c.Text = "0";
			c.Background = new SolidColorBrush(Colors.LightGray);
			c.IsReadOnly = true;
			nwRow.Children.Add(c);

			AddFreeXSetCell(nwRow);

			xSet.Children.Add(nwRow);
			if (SimplexTableProperty != null) {
				var nwCoef = new SimplexCoef();
				SimplexTableProperty.discreteSet[SimplexTableProperty.discreteSet.Count - 1].Add(nwCoef);

				c.DataContext = nwCoef;
				c.SetBinding(TextBox.TextProperty, new Binding {
					Path = new PropertyPath("StringValue"),
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				});
			}
		}

		private void RemoveXSetRow () {
			var lastRow = xSet.Children[xSet.Children.Count - 1] as StackPanel;
			foreach (TextBox elem in lastRow.Children) {
				BindingOperations.ClearBinding(elem, TextBox.TextProperty);
				elem.TextChanged -= XSetCellTextChanged;
				elem.LostKeyboardFocus -= XSetCellInputComplete;
			}
			xSet.Children.RemoveAt(xSet.Children.Count - 1);
		}

		private TextBox XSetCoef () {
			var c = new TextBox {
				TextAlignment = TextAlignment.Center,
				BorderBrush = new SolidColorBrush(Colors.DarkGray),
				FontSize = 15,
				Width = 50,
			};

			return c;
		}

		private void AddFreeXSetCell (StackPanel row) {
			var c = XSetCoef();

			c.TextChanged += XSetCellTextChanged;
			c.LostKeyboardFocus += XSetCellInputComplete;

			row.Children.Add(c);
		}

		private void XSetCellTextChanged (object sender, TextChangedEventArgs e) {
			if (!(sender is TextBox tarCell)) return;
			if (tarCell.DataContext is SimplexCoef) return;
			if (tarCell.Text.Trim().Length == 0) return;

			int variableIndex = FindVariableIndex(tarCell);

			if (SimplexTableProperty != null) {
				var nwCoef = new SimplexCoef();
				SimplexTableProperty.discreteSet[variableIndex].Add(nwCoef);
				tarCell.DataContext = nwCoef;
				tarCell.Style = this.FindResource("HighlightErrorStyle") as Style;

				var curText = tarCell.Text;
				tarCell.SetBinding(TextBox.TextProperty, new Binding {
					Path = new PropertyPath("StringValue"),
					Mode = BindingMode.TwoWay,
					UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
				});
				tarCell.Text = curText;
				tarCell.CaretIndex = tarCell.Text.Length;
			}

			AddFreeXSetCell(xSet.Children[variableIndex] as StackPanel);
		}

		private void XSetCellInputComplete (object sender, KeyboardFocusChangedEventArgs e) {
			if (!(sender is TextBox tarCell)) return;
			if (!(tarCell.DataContext is SimplexCoef)) return;
			if (tarCell.Text.Trim().Length != 0) return;

			int variableIndex = FindVariableIndex(tarCell);
			int elemIndex = ((StackPanel)xSet.Children[variableIndex]).Children.IndexOf(tarCell);

			BindingOperations.ClearBinding(tarCell, TextBox.TextProperty);
			tarCell.TextChanged -= XSetCellTextChanged;
			tarCell.LostKeyboardFocus -= XSetCellInputComplete;

			((StackPanel)xSet.Children[variableIndex]).Children.RemoveAt(elemIndex);
			if (SimplexTableProperty != null) {
				SimplexTableProperty.discreteSet[variableIndex].RemoveAt(elemIndex);
			}
		}

		private int FindVariableIndex (TextBox tarCell) {
			int variableIndex = -1;
			for (int j = 0; j < xSet.Children.Count; j++) {
				if (((StackPanel)xSet.Children[j]).Children.Contains(tarCell)) {
					variableIndex = j;
				}
			}

			Debug.Assert(variableIndex >= 0, "Can't find such cell");
			return variableIndex;
		}

		#endregion

		public SimplexTable SimplexTableProperty {
			get { return (SimplexTable)GetValue(SimplexTablePropertyProperty); }
			set { SetValue(SimplexTablePropertyProperty, value); }
		}

		public static void SimplexTablePropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e) {
			(d as SimplexTableControl)?.RebindFields(e.NewValue as SimplexTable);
		}

		public static readonly DependencyProperty SimplexTablePropertyProperty =
			DependencyProperty.Register("SimplexTableProperty", typeof(SimplexTable),
				typeof(SimplexTableControl), new PropertyMetadata(SimplexTablePropertyChanged));
	}
}
