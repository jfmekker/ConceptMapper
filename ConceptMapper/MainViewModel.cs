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

		private static readonly int nodeRadius = 25;

		public MainViewModel( Canvas canvas )
		{
			this.canvas = canvas;
			this.model = new( );
		}

		public int NumNodes => this.model.AllNodes.Count;

		public int NumEdges => this.model.AllNodes.Sum( x => x.Neighbors.Count ) / 2;

		public int Depth => this.model.Depth;

		public int Width => this.model.Width;

		public int Hss => this.Depth + this.Width;

		public int NumMainIdeas => this.model.MainIdeas.Count;

		public int MaxNumDetails => this.model.MaxNumDetails;

		public bool IsCompletable => this.model.Root is not null;

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Click( Point point )
		{
			// Check if the click was in an existing node
			List<MapNode> graph = this.model.AllNodes;
			MapNode? current = this.model.Current;
			MapNode? selected = null;
			foreach ( MapNode node in graph )
			{
				Point nodePos = node.Position;
				double dist = Math.Sqrt( Math.Pow( nodePos.X - point.X , 2 ) + Math.Pow( nodePos.Y - point.Y , 2 ) );
				if ( dist <= nodeRadius )
				{
					selected = node;
					break;
				}
			}

			// Add edge between two existing nodes
			if ( selected is not null && current is not null && selected != current )
			{
				Debug.WriteLine( $"ViewModel: Adding edge from ({selected.Position.X}, {selected.Position.Y}) to ({current.Position.X}, {current.Position.Y})." );
				this.model.AddEdge( selected , current );
				this.model.Current = selected;
			}
			// Set or reset current node
			else if ( selected is not null )
			{
				Debug.WriteLine( $"ViewModel: Setting current node to {( selected == current ? null : selected )}." );
				this.model.Current = selected == current ? null : selected;
			}
			// Add a new node
			else if ( current is not null || this.model.Root is null )
			{
				Debug.WriteLine( $"ViewModel: Adding Node at ({point.X}, {point.Y})." );
				this.model.AddNode( new( ) { Position = point } );
			}
			else
			{
				Debug.WriteLine( $"ViewModel: Doing nothing." );
			}

			this.Update( );
		}

		public void ResetCurrent( )
		{
			this.model.Current = null;
			this.Update( );
		}

		private void Update( )
		{
			this.DrawNodesAndEdges( );

			if ( this.PropertyChanged is not null )
			{
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumNodes ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumEdges ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Width ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Depth ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Hss ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumMainIdeas ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.MaxNumDetails ) ) );
			}
		}

		private void DrawNodesAndEdges( )
		{
			this.canvas.Children.Clear( );

			List<MapNode> graph = this.model.AllNodes;
			List<MapNode> drawn = new( graph.Count );
			Debug.WriteLine( $"ViewModel: Redrawing {this.NumNodes} nodes and {this.NumEdges} edges." );

			foreach ( MapNode node in graph )
			{
				// Draw node
				var nodeCircle = new Ellipse( )
				{
					Width = nodeRadius * 2 ,
					Height = nodeRadius * 2 ,
					StrokeThickness = 2 ,
					Stroke = node == this.model.Current ? Brushes.Green : Brushes.Red
				};
				_ = this.canvas.Children.Add( nodeCircle );
				nodeCircle.SetValue( Canvas.LeftProperty , (double)node.Position.X - nodeRadius );
				nodeCircle.SetValue( Canvas.TopProperty , (double)node.Position.Y - nodeRadius );
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
