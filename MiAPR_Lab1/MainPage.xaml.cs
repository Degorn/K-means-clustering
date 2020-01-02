using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Numerics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MiAPR_Lab1
{
	public sealed partial class MainPage : Page
	{
		private class Dot
		{
			public Color Color { get; set; }
			public Vector2 Position { get; set; }
		}

		class Cluster : Dot
		{
			public ICollection<Dot> Children { get; set; } = new Collection<Dot>();

			public double LastMovedDistance { get; set; }

			public Vector2 GetAvaragePosition()
			{
				var avX = Children.Average(x => x.Position.X);
				var avY = Children.Average(x => x.Position.Y);

				return new Vector2(avX, avY);
			}
		}

		private const double CLUSTERS_MOVE_DIFFERENCE = 0.1;
		private const int MAX_ITERATIONS = 20;

		private readonly Random _random = new Random();
		private CanvasControl _canvas;

		private int _canvasWidth,
					_canvasHeight;

		private Dot[] _samplesPoints;
		private Cluster[] _clustersPoints;

		private bool _generated;

		public MainPage()
		{
			this.InitializeComponent();
		}

		private void OnGenerateClick(object sender, RoutedEventArgs e)
		{
			DestroyWin2DContainers();
			InitializeContainerSize();

			InitializeDots();
			InitializeClusters();

			InitializeCanvas();

			_generated = true;
		}

		private async void OnCalculateClick(object sender, RoutedEventArgs e)
		{
			if (!_generated)
			{
				ShowMessage("Данные не были сгенерированы");
				return;
			}

			var iteration = 0;

			while (true)
			{
				await Task.Delay(20);

				RecalculateClusterCenter();

				DestroyWin2DContainers();
				InitializeContainerSize();

				InitializeCanvas();

				if (++iteration >= MAX_ITERATIONS ||
					!CheckIfCentroidsMovedTooLittle())
				{
					break;
				}
			}
		}

		private bool CheckIfCentroidsMovedTooLittle()
		{
			return _clustersPoints.Any(x => x.LastMovedDistance > CLUSTERS_MOVE_DIFFERENCE);
		}

		private void InitializeCanvas()
		{
			_canvas = new CanvasControl();
			_canvas.Draw += OnDraw;

			Container.Children.Add(_canvas);
		}

		private void InitializeDots()
		{
			var samplesQuantity = GetSamplesQuantity();

			_samplesPoints = new Dot[samplesQuantity];

			for (int i = 0; i < samplesQuantity; i++)
			{
				_samplesPoints[i] = new Dot
				{
					Position = GenerateRandomPosition(),
					Color = Colors.Black
				};
			}
		}

		private void InitializeClusters()
		{
			var clustersQuantity = GetClustersQuantity();

			_clustersPoints = new Cluster[clustersQuantity];

			for (int i = 0; i < clustersQuantity; i++)
			{
				var b = new byte[3];
				_random.NextBytes(b);

				_clustersPoints[i] = new Cluster
				{
					Position = GenerateRandomPosition(),
					Color = Color.FromArgb(byte.MaxValue, b[0], b[1], b[2])
				};
			}
		}

		private void RecalculateClusterCenter()
		{
			foreach (var item in _clustersPoints)
			{
				var oldPosition = item.Position;

				item.Position = item.GetAvaragePosition();

				item.LastMovedDistance = GetDitance(item.Position, oldPosition);
			}
		}

		private void OnPageUnloaded(object sender, RoutedEventArgs e)
		{
			DestroyWin2DContainers();
		}

		private void DestroyWin2DContainers()
		{
			_canvas?.RemoveFromVisualTree();

			Container.Children.Remove(_canvas);

			_canvas = null;
		}

		private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			DrawDotsAndClusters(args.DrawingSession);

			Calculate(args.DrawingSession);

			// Redraw.
			DrawDots(args.DrawingSession);
		}

		private void Calculate(CanvasDrawingSession drawingSession)
		{
			foreach (var item in _clustersPoints)
			{
				item.Children.Clear();
			}

			foreach (var samplePoint in _samplesPoints)
			{
				double minDist = double.MaxValue;
				Cluster closestClass = null;

				foreach (var classPoint in _clustersPoints)
				{
					if (minDist > GetDitance(classPoint, samplePoint))
					{
						minDist = GetDitance(classPoint, samplePoint);
						closestClass = classPoint;
					}
				}

				if (closestClass == null)
				{
					return;
				}

				samplePoint.Color = closestClass.Color;
				closestClass.Children.Add(samplePoint);
			}
		}

		private int GetClustersQuantity()
		{
			return GetTextBoxNumberValue(ClassesQuantityTextBox);
		}

		private int GetSamplesQuantity()
		{
			return GetTextBoxNumberValue(SamplesQuantityTextBox);
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

		private void InitializeContainerSize()
		{
			_canvasWidth = (int)Container.ActualWidth;
			_canvasHeight = (int)Container.ActualHeight;
		}

		private Vector2 GenerateRandomPosition()
		{
			return new Vector2(_random.Next(_canvasWidth), _random.Next(_canvasHeight));
		}

		private double GetDitance(Dot p1, Dot p2)
		{
			return GetDitance(p1.Position, p2.Position);
		}

		private double GetDitance(Vector2 p1, Vector2 p2)
		{
			return Math.Sqrt(
				Math.Pow(p1.X - p2.X, 2) +
				Math.Pow(p1.Y - p2.Y, 2));
		}

		private void DrawDotsAndClusters(CanvasDrawingSession drawingSession)
		{
			DrawDots(drawingSession);
			DrawClusters(drawingSession);
		}

		private void DrawDots(CanvasDrawingSession drawingSession)
		{
			foreach (var sample in _samplesPoints)
			{
				drawingSession.FillCircle(sample.Position, 2, sample.Color);
			}
		}

		private void DrawClusters(CanvasDrawingSession drawingSession)
		{
			foreach (var cluster in _clustersPoints)
			{
				drawingSession.FillCircle(cluster.Position, 6, cluster.Color);
				drawingSession.DrawCircle(cluster.Position.X, cluster.Position.Y, 8, Colors.Black, 4);
			}
		}

		private async void ShowMessage(string message)
		{
			var messageDialog = new MessageDialog(message);
			await messageDialog.ShowAsync();
		}
	}
}
