using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConceptMapper
{
	public class MainViewModel
	{
		private readonly MainModel model;
		private readonly MainView view;

		public MainViewModel( MainView mainView )
		{
			this.view = mainView;
			this.model = new( );
		}

		public int Depth { get => model.Depth; }

		public int Width { get => model.Width; }

		public int Hss { get => this.Depth + this.Width; }

		public int NumMainIdeas { get => this.model.Root?.Neighbors.Count ?? 0; }

		public int MaxNumDetails { get => -1; }

		public bool IsCompletable { get => this.model.Root is not null; }

		public void AddNode( Point point )
		{
			Debug.WriteLine( $"ViewModel: Adding Node at ({point.X}, {point.Y})..." );

			this.model.AddNode( new MapNode( ) { Position = point } );

			Debug.WriteLine( $"ViewModel: New depth = {this.Depth}, New width = {this.Width}" );

			// TODO notify property changed

			int radius = 50;
			var nodeCircle = new Ellipse( )
			{
				Width = radius ,
				Height = radius ,
				StrokeThickness = 2 ,
				Stroke = Brushes.Red
			};

			_ = this.view.Canvas.Children.Add( nodeCircle );

			nodeCircle.SetValue( Canvas.LeftProperty , point.X - ( radius / 2.0 ) );
			nodeCircle.SetValue( Canvas.TopProperty , point.Y - ( radius / 2.0 ) );
		}

		public void ResetCurrent( )
		{
			this.model.Current = this.model.Root;
		}
	}
}
