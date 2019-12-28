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

			public Vector2 GetAvaragePosition()
			{
				var avX = Children.Average(x => x.Position.X);
				var avY = Children.Average(x => x.Position.Y);

				return new Vector2(avX, avY);
			}
		}

		private readonly Random _random = new Random();
		private CanvasControl _canvas;

		private int _canvasWidth,
					_canvasHeight;

		private Dot[] _samplesPoints;
		private Cluster[] _clustersPoints;

		public MainPage()
		{
			this.InitializeComponent();
		}

		private void OnGenerateClick(object sender, RoutedEventArgs e)
		{
			DestroyWin2DContainers();
			InitializeContainerSize();

			_canvas = new CanvasControl();
			_canvas.Draw += OnDraw;

			Container.Children.Add(_canvas);
		}

		private void OnCalculateClick(object sender, RoutedEventArgs e)
		{
			RecalculateClusterCenter();

			DestroyWin2DContainers();
			InitializeContainerSize();

			_canvas = new CanvasControl();
			_canvas.Draw += OnRedraw;

			Container.Children.Add(_canvas);
		}

		private void RecalculateClusterCenter()
		{
			foreach (var item in _clustersPoints)
			{
				item.Position = item.GetAvaragePosition();
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
			DrawDots(args.DrawingSession);

			Calculate(args.DrawingSession);
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

				samplePoint.Color = closestClass.Color;
				closestClass.Children.Add(samplePoint);
				drawingSession.FillCircle(samplePoint.Position, 2, closestClass.Color);
			}
		}

		private void DrawDots(CanvasDrawingSession drawingSession)
		{
			var samplesQuantity = GetSamplesQuantity();
			var clustersQuantity = GetClustersQuantity();

			_samplesPoints = new Dot[samplesQuantity];
			_clustersPoints = new Cluster[clustersQuantity];

			for (int i = 0; i < samplesQuantity; i++)
			{
				_samplesPoints[i] = new Dot
				{
					Position = GenerateRandomPosition(),
				};

				drawingSession.FillCircle(_samplesPoints[i].Position, 2, Colors.Black);
			}

			for (int i = 0; i < clustersQuantity; i++)
			{
				var b = new byte[3];

				_random.NextBytes(b);

				_clustersPoints[i] = new Cluster
				{
					Position = GenerateRandomPosition(),
					Color = Color.FromArgb(byte.MaxValue, b[0], b[1], b[2])
				};

				drawingSession.FillCircle(_clustersPoints[i].Position, 6, _clustersPoints[i].Color);
				drawingSession.DrawCircle(_clustersPoints[i].Position.X, _clustersPoints[i].Position.Y, 8, Colors.Black, 4);
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
			return int.TryParse(textBox.Text, out var result)
				? result
				: default;
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
			return Math.Sqrt(
				Math.Pow(p1.Position.X - p2.Position.X, 2) +
				Math.Pow(p1.Position.Y - p2.Position.Y, 2));
		}

		private void OnRedraw(CanvasControl sender, CanvasDrawEventArgs args)
		{
			RedrawDots(args.DrawingSession);

			Calculate(args.DrawingSession);
		}

		private void RedrawDots(CanvasDrawingSession drawingSession)
		{
			foreach (var sample in _samplesPoints)
			{
				drawingSession.FillCircle(sample.Position, 2, sample.Color);
			}

			foreach (var cluster in _clustersPoints)
			{
				drawingSession.FillCircle(cluster.Position, 6, cluster.Color);
				drawingSession.DrawCircle(cluster.Position.X, cluster.Position.Y, 8, Colors.Black, 4);
			}
		}
	}
}
