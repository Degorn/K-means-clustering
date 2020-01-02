using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Numerics;
using Windows.UI;
using Windows.UI.Popups;
using System.Threading.Tasks;

namespace MiAPR_Lab4
{
	public sealed partial class MainPage : Page
	{
		private class Dot
		{
			public Color Color { get; set; }
			public Vector2 Position { get; set; }
		}

		private const int SAMPLE_RADIUS = 6;
		private const int ZOOM_LEVEL = 20;

		private const int ITERATIONS_NUMBER = 1000;
		private const double LEARNING_RATE = 0.01;

		private readonly Random _random = new Random();
		private CanvasControl _canvas;

		public double CanvasWidth => Container.ActualWidth;
		public double CanvasHeight => Container.ActualHeight;

		private Vector2[] _line = new Vector2[2];
		private double[] _lineFormula = new double[3];

		private Dot[] _samples = new Dot[0];

		public MainPage()
		{
			this.InitializeComponent();

			InitializeCanvas();
		}

		private void InitializeCanvas()
		{
			DestroyWin2DContainers();

			_canvas = new CanvasControl();
			_canvas.Draw += OnDraw;

			Container.Children.Add(_canvas);
		}

		private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			DrawAxis(args.DrawingSession);
			DrawSamples(args.DrawingSession);
			DrawLine(args.DrawingSession);
		}

		private void DrawSamples(CanvasDrawingSession drawingSession)
		{
			if (_samples.Length == 0)
			{
				return;
			}

			foreach (var sample in _samples)
			{
				drawingSession.FillCircle(sample.Position, SAMPLE_RADIUS, sample.Color);
			}
		}

		private void DrawAxis(CanvasDrawingSession drawingSession)
		{
			var axisCenterX = (float)Container.ActualWidth / 2;
			var axisCenterY = (float)Container.ActualHeight / 2;

			drawingSession.DrawLine(axisCenterX, (float)Container.ActualHeight, axisCenterX, 0, Colors.Black, 2);
			drawingSession.DrawLine((float)Container.ActualWidth, axisCenterY, 0, axisCenterY, Colors.Black, 2);

			//for (int i = 0; i < 10; i++)
			//{
			//	drawingSession.DrawLine((float)CanvasWidth / 2 + i * ZOOM_LEVEL, 0, (float)CanvasWidth / 2 + i * ZOOM_LEVEL, (float)CanvasHeight, Colors.Black, 2);
			//}
			//for (int i = 0; i < 10; i++)
			//{
			//	drawingSession.DrawLine(0, (float)CanvasHeight / 2 + i * ZOOM_LEVEL, (float)CanvasWidth, (float)CanvasHeight / 2 + i * ZOOM_LEVEL, Colors.Black, 2);
			//}
		}

		private void DrawLine(CanvasDrawingSession drawingSession)
		{
			//drawingSession.DrawLine(
			//	(float)CanvasWidth / 2 + 0,
			//	(float)CanvasHeight / 2 + 0,
			//	(float)CanvasWidth / 2 + 100,
			//	(float)CanvasHeight / 2 + 100,
			//	Colors.Gray, 2);


			drawingSession.DrawLine(
				(float)CanvasWidth / 2 + (_line[0].X * ZOOM_LEVEL),
				(float)CanvasHeight / 2 - (_line[0].Y * ZOOM_LEVEL),
				(float)CanvasWidth / 2 + (_line[1].X * ZOOM_LEVEL),
				(float)CanvasHeight / 2 - (_line[1].Y * ZOOM_LEVEL),
				Colors.Gray, 2);

			//drawingSession.DrawLine(
			//	(float)CanvasWidth / 2 + _line[0].X * ZOOM_LEVEL - 8,
			//	-_line[0].Y * ZOOM_LEVEL + (float)CanvasHeight / 2,
			//	(float)CanvasWidth / 2 + _line[1].X * ZOOM_LEVEL - 8,
			//	-_line[1].Y * ZOOM_LEVEL + (float)CanvasHeight / 2,
			//	Colors.Blue, 2);

			//drawingSession.DrawLine(
			//	(float)CanvasWidth / 2 + _line[0].X * ZOOM_LEVEL + 8,
			//	-_line[0].Y * ZOOM_LEVEL + (float)CanvasHeight / 2,
			//	(float)CanvasWidth / 2 + _line[1].X * ZOOM_LEVEL + 8,
			//	-_line[1].Y * ZOOM_LEVEL + (float)CanvasHeight / 2,
			//	Colors.Red, 2);
		}

		private void DestroyWin2DContainers()
		{
			_canvas?.RemoveFromVisualTree();

			Container.Children.Remove(_canvas);

			_canvas = null;
		}

		private void OnGenerateClick(object sender, RoutedEventArgs e)
		{
			GenerateDots();
			InitializeLine();
			InitializeCanvas();
		}

		private void GenerateDots()
		{
			// TODO: Custom number of classes.

			_samples = new Dot[GetTextBoxNumberValue(SamplesQuantityTextBox) * 2];
			for (int i = 0; i < _samples.Length / 2; i++)
			{
				_samples[i] = new Dot
				{
					Position = GenerateRandomPosition(),
					Color = Colors.Red
				};
			}

			for (int i = _samples.Length / 2; i < _samples.Length; i++)
			{
				_samples[i] = new Dot
				{
					Position = GenerateRandomPosition(),
					Color = Colors.Blue
				};
			}

			PointsPositionsStackPanel.Children.Clear();
			for (int i = 0; i < _samples.Length; i++)
			{
				PointsPositionsStackPanel.Children.Add(new TextBlock
				{
					Text = $"{i}: {GetDotPositionX(_samples[i]).ToString("0.0")}, {GetDotPositionY(_samples[i]).ToString("0.0")}"
				});
			}
		}

		private Vector2 GenerateRandomPosition()
		{
			return new Vector2(
				(float)(CanvasWidth * _random.NextDouble()),
				(float)(CanvasHeight * _random.NextDouble()));
		}

		private int GetTextBoxNumberValue(TextBox textBox)
		{
			var isSucceed = int.TryParse(textBox.Text, out var result);

			if (isSucceed && result > 0)
			{
				return result;
			}

			ShowMessage($"Неверное значение текстового поля {textBox.Name}. Значение должно быть > 0.\nВзято стандартное");

			return 1;
		}

		private async void ShowMessage(string message)
		{
			var messageDialog = new MessageDialog(message);
			await messageDialog.ShowAsync();
		}

		private async void OnCalculateClick(object sender, RoutedEventArgs e)
		{
			// TODO: Get a random line.

			for (int i = 0; i < ITERATIONS_NUMBER; i++)
			{
				await Task.Delay(2);

				var s2 = _lineFormula[1] > 0 ? "+" : "";
				var s3 = _lineFormula[2] > 0 ? "+" : "";
				FormulaTextBlock.Text = $"{_lineFormula[0].ToString("0.0")}X {s2} {_lineFormula[1].ToString("0.0")}Y {s3} {_lineFormula[2].ToString("0.0")} = 0";

				if (IsPointsClassifiedCorrectly())
				{
					ShowMessage("Успех!");
					
					return;
				}

				var dot = PickRandomPoint();

				if (IsPointCorrectlyClassified(dot))
				{
					continue;
				}

				if (dot.Color == Colors.Blue &&
					GetDotPositionX(dot) * _lineFormula[0] +
					GetDotPositionY(dot) * _lineFormula[1] +
					_lineFormula[2] > 0)
				{
					_lineFormula[0] -= LEARNING_RATE * GetDotPositionX(dot) / 50;
					_lineFormula[1] -= LEARNING_RATE * GetDotPositionY(dot) / 50;
					_lineFormula[2] -= LEARNING_RATE;
				}

				if (dot.Color == Colors.Red &&
					GetDotPositionX(dot) * _lineFormula[0] +
					GetDotPositionY(dot) * _lineFormula[1] +
					_lineFormula[2] < 0)
				{
					_lineFormula[0] += LEARNING_RATE * GetDotPositionX(dot) / 50;
					_lineFormula[1] += LEARNING_RATE * GetDotPositionY(dot) / 50;
					_lineFormula[2] += LEARNING_RATE;
				}

				RecalculateLine();

				InitializeCanvas();
			}

			ShowMessage("Решение найти не удалось...");
		}

		private bool IsPointsClassifiedCorrectly()
		{
			foreach (var sample in _samples)
			{
				if (!IsPointCorrectlyClassified(sample))
				{
					return false;
				}
			}

			return true;
		}

		private bool IsPointCorrectlyClassified(Dot dot)
		{
			if (dot.Color == Colors.Blue &&
				_lineFormula[0] * GetDotPositionX(dot) +
				_lineFormula[1] * GetDotPositionY(dot) +
				_lineFormula[2] < 0)
			{
				return true;
			}

			if (dot.Color == Colors.Red &&
				_lineFormula[0] * GetDotPositionX(dot) +
				_lineFormula[1] * GetDotPositionY(dot) +
				_lineFormula[2] > 0)
			{
				return true;
			}

			return false;
		}

		private float GetDotPositionX(Dot dot)
		{
			return -(float)CanvasWidth / 2 + dot.Position.X;
		}

		private float GetDotPositionY(Dot dot)
		{
			return -dot.Position.Y + (float)CanvasHeight / 2;
		}

		private Dot PickRandomPoint()
		{
			return _samples[_random.Next(_samples.Length)];
		}

		private void InitializeLine()
		{
			// 2x  + 3y - 8 = 0
			// x = -3y / 2 + 4
			// y = -2x / 3 + 8/3

			_lineFormula[0] = 2;
			_lineFormula[1] = 2;
			_lineFormula[2] = 4;

			RecalculateLine();
		}

		private void RecalculateLine()
		{
			_line[0].X = (float)((_lineFormula[1] * -100 - _lineFormula[2]) / _lineFormula[0]);
			_line[1].X = (float)((_lineFormula[1] * 100 - _lineFormula[2]) / _lineFormula[0]);
			_line[0].Y = (float)((-_lineFormula[0] * -100 - _lineFormula[2]) / _lineFormula[1]);
			_line[1].Y = (float)((-_lineFormula[0] * 100 - _lineFormula[2]) / _lineFormula[1]);
		}
	}
}
