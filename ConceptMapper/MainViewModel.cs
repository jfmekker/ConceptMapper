using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	public class MainViewModel : INotifyPropertyChanged
	{
		private readonly MainModel model;
		private readonly Canvas canvas;

		public MainViewModel( Canvas canvas )
		{
			this.canvas = canvas;
			this.model = new( );
		}

		public int Depth => this.model.Depth;

		public int Width => this.model.Width;

		public int Hss => this.Depth + this.Width;

		public int NumMainIdeas => this.model.Root?.Neighbors.Count ?? 0;

		public int MaxNumDetails => -1;

		public bool IsCompletable => this.model.Root is not null;

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Click(Point point)
		{
			this.AddNode( point );
		}

		public void ResetCurrent( )
		{
			this.model.Current = this.model.Root;
			this.DrawNodesAndEdges( );
		}

		private void AddNode( Point point )
		{
			Debug.WriteLine( $"ViewModel: Adding Node at ({point.X}, {point.Y})." );
			MapNode node = new( ) { Position = point };
			this.model.AddNode( node );
			this.DrawNodesAndEdges( );

			if ( this.PropertyChanged is not null )
			{
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Width ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Depth ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Hss ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumMainIdeas ) ) );
			}
		}

		private void DrawNodesAndEdges( )
		{
			this.canvas.Children.Clear( );
			if ( this.model.Root is not null )
			{
				List<MapNode> graph = this.model.Root.GetWholeGraph( );
				List<MapNode> drawn = new( graph.Count );
				Debug.WriteLine( $"ViewModel: Redrawing {graph.Count} nodes and edges." );

				foreach ( MapNode node in graph )
				{
					// Draw node
					int radius = 50;
					var nodeCircle = new Ellipse( )
					{
						Width = radius ,
						Height = radius ,
						StrokeThickness = 2 ,
						Stroke = node == this.model.Current ? Brushes.Green : Brushes.Red
					};
					_ = this.canvas.Children.Add( nodeCircle );
					nodeCircle.SetValue( Canvas.LeftProperty , node.Position.X - ( radius / 2.0 ) );
					nodeCircle.SetValue( Canvas.TopProperty , node.Position.Y - ( radius / 2.0 ) );
					drawn.Add( node );

					// Draw edges
					foreach ( MapNode next in node.Neighbors )
					{
						if ( !drawn.Contains( next ) )
						{
							var edgeLine = new Line( )
							{
								X1 = node.Position.X ,
								Y1 = node.Position.Y ,
								X2 = next.Position.X ,
								Y2 = next.Position.Y ,
								StrokeThickness = 2 ,
								Stroke = Brushes.OrangeRed
							};
							_ = this.canvas.Children.Add( edgeLine );
						}
					}
				}
			}
		}
	}
}
