using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace MiAPR_Lab7
{
	public enum VarTypeEnum
	{
		HORIZONTAL_LINE,
		VERTICAL_LINE,
		DIAGONAL_LINE_DOWN,
		DIAGONAL_LINE_UP,
		TWO_HORIZONTAL_LINES
	}

	public sealed partial class MainPage : Page
	{
		public ObservableCollection<VarType> FromTransformList { get; set; }
		public ObservableCollection<VarType> TransformList { get; set; }

		private readonly RulesFactory _rulesFactory = new RulesFactory();

		private IDictionary<VarTypeEnum, ICollection<Line>> _typedLineDictionary;
		private readonly ICollection<Line>
			_horizontalLines = new Collection<Line>(),
			_verticalLines = new Collection<Line>(),
			_diagonalLinesDown = new Collection<Line>(),
			_diagonalLinesUp = new Collection<Line>();

		private readonly Random _random = new Random();

		public MainPage()
		{
			this.InitializeComponent();

			InitializeVarTypes();
		}

		private void InitializeVarTypes()
		{
			TransformList = new ObservableCollection<VarType>
			{
				new VarType(VarTypeEnum.HORIZONTAL_LINE, "Горизонтальная линия"),
				new VarType(VarTypeEnum.VERTICAL_LINE, "Вертикальная линия"),
				new VarType(VarTypeEnum.DIAGONAL_LINE_DOWN, "Диагональная линия (вниз)"),
				new VarType(VarTypeEnum.DIAGONAL_LINE_UP, "Диагональная линия (вверх)"),
				new VarType(VarTypeEnum.TWO_HORIZONTAL_LINES, "Две горизонтальные линии"),
			};

			FromTransformList = new ObservableCollection<VarType>(TransformList.Take(TransformList.Count - 1));
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			GenerateImage();
		}

		private void GenerateImage()
		{
			ClearContainer();

			var val = _random.Next(2);
			switch (val)
			{
				case 0:
					GenerateSquare();
					break;
				case 1:
					GenerateRhombus();
					break;
				default:
					break;
			}

			InitializeTypeDictionary();
		}

		private void ClearContainer()
		{
			Container.Children.Clear();
			_horizontalLines.Clear();
			_verticalLines.Clear();
			_diagonalLinesDown.Clear();
			_diagonalLinesUp.Clear();
		}

		private void GenerateSquare()
		{
			var line1 = new Line
			{
				X1 = 200,
				Y1 = 200,
				X2 = 300,
				Y2 = 200,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_horizontalLines.Add(line1);
			Container.Children.Add(line1);

			var line2 = new Line
			{
				X1 = 300,
				Y1 = 200,
				X2 = 300,
				Y2 = 300,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_verticalLines.Add(line2);
			Container.Children.Add(line2);

			var line3 = new Line
			{
				X1 = 200,
				Y1 = 300,
				X2 = 300,
				Y2 = 300,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_horizontalLines.Add(line3);
			Container.Children.Add(line3);

			var line4 = new Line
			{
				X1 = 200,
				Y1 = 300,
				X2 = 200,
				Y2 = 200,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_verticalLines.Add(line4);
			Container.Children.Add(line4);
		}

		private void GenerateRhombus()
		{
			var line1 = new Line
			{
				X1 = 200,
				Y1 = 200,
				X2 = 200 + 50,
				Y2 = 200 + 50,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_diagonalLinesDown.Add(line1);
			Container.Children.Add(line1);

			var line2 = new Line
			{
				X1 = 200 + 50,
				Y1 = 200 + 50,
				X2 = 200,
				Y2 = 300,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_diagonalLinesUp.Add(line2);
			Container.Children.Add(line2);

			var line3 = new Line
			{
				X1 = 200,
				Y1 = 300,
				X2 = 200 - 50,
				Y2 = 300 - 50,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_diagonalLinesDown.Add(line3);
			Container.Children.Add(line3);

			var line4 = new Line
			{
				X1 = 200 - 50,
				Y1 = 300 - 50,
				X2 = 200,
				Y2 = 200,
				Stroke = new SolidColorBrush(Colors.Black)
			};
			_diagonalLinesUp.Add(line4);
			Container.Children.Add(line4);
		}

		private void InitializeTypeDictionary()
		{
			_typedLineDictionary = new Dictionary<VarTypeEnum, ICollection<Line>>
			{
				{ VarTypeEnum.HORIZONTAL_LINE, _horizontalLines },
				{ VarTypeEnum.VERTICAL_LINE, _verticalLines },
				{ VarTypeEnum.DIAGONAL_LINE_DOWN, _diagonalLinesDown },
				{ VarTypeEnum.DIAGONAL_LINE_UP, _diagonalLinesUp },
				{ VarTypeEnum.TWO_HORIZONTAL_LINES, _horizontalLines },
			};
		}

		private void OnChangeClick(object sender, RoutedEventArgs e)
		{
			var selectedFromType = (ComboBoxFrom.SelectedItem as VarType)?.Type;
			if (!selectedFromType.HasValue)
			{
				return;
			}

			var selectedToType = (ComboBoxTo.SelectedItem as VarType)?.Type;
			if (!selectedToType.HasValue)
			{
				return;
			}

			var rule = _rulesFactory.Create(selectedFromType.Value, selectedToType.Value);
			if (rule == null)
			{
				return;
			}

			var fromLines = _typedLineDictionary[selectedFromType.Value];
			var toLines = _typedLineDictionary[selectedToType.Value];

			foreach (var fromLine in fromLines.ToList())
			{
				var newLines = rule.GetLines(fromLine);
				if (newLines == null || newLines.Length == 0)
				{
					return;
				}

				Container.Children.Remove(fromLine);

				foreach (var newLine in newLines)
				{
					toLines.Add(newLine);
					Container.Children.Add(newLine);
				}
			}

			if (fromLines != toLines)
			{
				fromLines.Clear();
			}
		}
	}

	public class VarType
	{
		public VarTypeEnum Type { get; set; }
		public string Text { get; set; }

		public VarType(VarTypeEnum type, string text)
		{
			Type = type;
			Text = text;
		}
	}

	public class RulesFactory
	{
		public Rule Create(VarTypeEnum fromType, VarTypeEnum toType)
		{
			if (fromType == toType)
			{
				return null;
			}

			return new Rule((fromType, toType));
		}
	}

	public class Rule
	{
		public (VarTypeEnum, VarTypeEnum) Ruleset { get; set; }

		private const double TWO_LINES_GAP = 10;

		public Rule((VarTypeEnum, VarTypeEnum) ruleset)
		{
			Ruleset = ruleset;
		}

		public Line[] GetLines(Line fromLine)
		{
			var newLine = new Line
			{
				X1 = fromLine.X1,
				Y1 = fromLine.Y1,
			};

			switch (Ruleset.Item2)
			{
				case VarTypeEnum.HORIZONTAL_LINE:
					SetHorizontalLineProps(fromLine, newLine);
					return new Line[] { newLine };
				case VarTypeEnum.VERTICAL_LINE:
					SetVerticalLineProps(fromLine, newLine);
					return new Line[] { newLine };
				case VarTypeEnum.DIAGONAL_LINE_DOWN:
					SetDiagonalLineDownProps(fromLine, newLine);
					return new Line[] { newLine };
				case VarTypeEnum.DIAGONAL_LINE_UP:
					SetDiagonalLineUpProps(fromLine, newLine);
					return new Line[] { newLine };
				case VarTypeEnum.TWO_HORIZONTAL_LINES:
					return SetTwoHorizontalLines(fromLine, newLine);
				default:
					return null;
			}
		}

		private double GetLineWidth(Line line)
		{
			return Math.Sqrt(Math.Pow(line.X2 - line.X1, 2) + Math.Pow(line.Y2 - line.Y1, 2));
		}

		private void SetHorizontalLineProps(Line fromLine, Line newLine)
		{
			var lineWidth = GetLineWidth(fromLine);

			newLine.X2 = fromLine.X1 + lineWidth;
			newLine.Y2 = fromLine.Y1;

			SetSameProperies(fromLine, newLine);
		}

		private void SetVerticalLineProps(Line fromLine, Line newLine)
		{
			var lineWidth = GetLineWidth(fromLine);

			newLine.X2 = fromLine.X1;
			newLine.Y2 = fromLine.Y1 + lineWidth;

			SetSameProperies(fromLine, newLine);
		}

		private void SetDiagonalLineDownProps(Line fromLine, Line newLine)
		{
			var hypLineWidth = GetLineWidth(fromLine);
			var catLineWidth = Math.Sqrt(Math.Pow(hypLineWidth, 2) / 2);

			newLine.X2 = fromLine.X1 + catLineWidth;
			newLine.Y2 = fromLine.Y1 + catLineWidth;

			SetSameProperies(fromLine, newLine);
		}

		private void SetDiagonalLineUpProps(Line fromLine, Line newLine)
		{
			var hypLineWidth = GetLineWidth(fromLine);
			var catLineWidth = Math.Sqrt(Math.Pow(hypLineWidth, 2) / 2);

			newLine.X2 = fromLine.X1 + catLineWidth;
			newLine.Y2 = fromLine.Y1 - catLineWidth;

			SetSameProperies(fromLine, newLine);
		}

		private Line[] SetTwoHorizontalLines(Line fromLine, Line newLine)
		{
			var lineWidth = GetLineWidth(fromLine);

			newLine.Y1 = fromLine.Y1 - TWO_LINES_GAP;
			newLine.X2 = fromLine.X1 + lineWidth;
			newLine.Y2 = fromLine.Y1 - TWO_LINES_GAP;
			SetSameProperies(fromLine, newLine);

			var newLine2 = new Line
			{
				X1 = fromLine.X1,
				Y1 = fromLine.Y1 + TWO_LINES_GAP,
				X2 = fromLine.X1 + lineWidth,
				Y2 = fromLine.Y1 + TWO_LINES_GAP
			};
			SetSameProperies(fromLine, newLine2);

			return new Line[]
			{
				newLine,
				newLine2
			};
		}

		private void SetSameProperies(Line fromLine, Line toLine)
		{
			if (fromLine == null || toLine == null)
			{
				return;
			}

			toLine.Stroke = fromLine.Stroke;
		}
	}
}
