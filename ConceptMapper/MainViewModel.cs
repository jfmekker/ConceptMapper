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
			// Add node to model
			Debug.WriteLine( $"ViewModel: Adding Node at ({point.X}, {point.Y})" );
			MapNode? old = this.model.Current;
			MapNode node = new MapNode( ) { Position = point };
			this.model.AddNode( node );

			// Add circle centered on the click position
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

			// Add a connecting line between current and node
			if ( old is not null )
			{
				Debug.WriteLine( $"ViewModel: Adding Edge at ({point.X}, {point.Y}) to ({old.Position.X}, {old.Position.Y})" );
				var edgeLine = new Line( )
				{
					X1 = point.X ,
					Y1 = point.Y ,
					X2 = old.Position.X ,
					Y2 = old.Position.Y ,
					StrokeThickness = 2 ,
					Stroke = Brushes.OrangeRed
				};
				_ = this.view.Canvas.Children.Add( edgeLine );
				edgeLine.SetValue( Canvas.LeftProperty , 0.0 );
				edgeLine.SetValue( Canvas.TopProperty , 0.0 );
			}
		}

		public void ResetCurrent( )
		{
			this.model.Current = this.model.Root;
		}
	}
}
