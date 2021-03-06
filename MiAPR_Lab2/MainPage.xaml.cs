﻿using Microsoft.Graphics.Canvas;
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

namespace MiAPR_Lab2
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

		private const string RESULT_TEXT = "Кол-во классов: {0}\n{1}";
		private const int MAX_ITERATIONS = 20;

		private readonly Random _random = new Random();
		private CanvasControl _canvas;

		private int _canvasWidth,
					_canvasHeight;

		private Dot[] _dotsPoints;
		private ICollection<Cluster> _clustersPoints;

		private bool _isGenerated;

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

			_isGenerated = true;
		}

		private async void OnCalculateClick(object sender, RoutedEventArgs e)
		{
			if (!_isGenerated)
			{
				ShowMessage("Данные не были сгенерированы");
				return;
			}

			var iteration = 0;

			while (true)
			{
				await Task.Delay(10);

				DestroyWin2DContainers();
				InitializeContainerSize();

				var newCluster = CalculateNewCluster();

				if (newCluster == null)
				{
					InitializeCanvas();

					break;
				}
				else
				{
					CreateCluster(newCluster.Position);

					ResultTextBlock.Text = string.Format(RESULT_TEXT, _clustersPoints.Count, "Не завершено");
				}

				InitializeCanvas();

				if (++iteration >= MAX_ITERATIONS)
				{
					break;
				}
			}

			ResultTextBlock.Text = string.Format(RESULT_TEXT, _clustersPoints.Count, "Завершено");
		}

		private Cluster CreateCluster(Vector2 position)
		{
			var b = new byte[3];
			_random.NextBytes(b);

			var cluster = new Cluster
			{
				Position = position,
				Color = Color.FromArgb(byte.MaxValue, b[0], b[1], b[2]),
			};

			_clustersPoints.Add(cluster);

			return cluster;
		}

		private Dot CalculateNewCluster()
		{
			var maxDistance = double.MinValue;
			Dot farthestDot = null;

			var clusters = _clustersPoints
				.SelectMany(cluster => cluster.Children
					.Select(clusterChild => (cluster, clusterChild)));

			foreach (var (cluster, clusterChild) in clusters)
			{
				var dist = GetDitance(cluster, clusterChild);
				if (dist > maxDistance)
				{
					farthestDot = clusterChild;
					maxDistance = dist;
				}
			}

			var halfOfAvarageClustersDistance = GetHalfOfAvarageClustersDistance();
			if (maxDistance > halfOfAvarageClustersDistance)
			{
				return farthestDot;
			}

			return null;
		}

		private double GetHalfOfAvarageClustersDistance()
		{
			var connectionsCount = 0;
			var distanceSum = 0d;

			var clusters = _clustersPoints
				.SelectMany(cluster =>_clustersPoints
					.Except(new Cluster[] { cluster })
					.Select(secondCluster => (cluster, secondCluster)));

			foreach (var (cluster, secondCluster) in clusters)
			{
				connectionsCount++;
				distanceSum += GetDitance(cluster, secondCluster);
			}

			return distanceSum / connectionsCount / 2;
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

			_dotsPoints = new Dot[samplesQuantity];

			for (int i = 0; i < samplesQuantity; i++)
			{
				_dotsPoints[i] = new Dot
				{
					Position = GenerateRandomPosition(),
					Color = Colors.Black
				};
			}
		}

		private void InitializeClusters()
		{
			_clustersPoints = new Collection<Cluster>();

			var randomDot = _dotsPoints[_random.Next(_dotsPoints.Count())];
			var cluster = CreateCluster(randomDot.Position);

			var furtherestDot = GetFurtherestDot(cluster);
			CreateCluster(furtherestDot.Position);
		}

		private Dot GetFurtherestDot(Dot dot)
		{
			var maxDistance = double.MinValue;
			Dot resultDot = null;

			foreach (var dotPoint in _dotsPoints)
			{
				var distance = GetDitance(dot, dotPoint);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					resultDot = dotPoint;
				}
			}

			return resultDot;
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

			foreach (var samplePoint in _dotsPoints)
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
			}
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
			foreach (var sample in _dotsPoints)
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
