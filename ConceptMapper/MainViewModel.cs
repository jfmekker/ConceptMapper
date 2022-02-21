using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

		public int NumNodes => this.model.AllNodes.Count;

		public int NumEdges => this.model.AllNodes.Sum( x => x.Neighbors.Count ) / 2;

		public int Depth => this.model.Depth;

		public int Width => this.model.Width;

		public int Hss => this.Depth + this.Width;

		public int NumMainIdeas => this.model.MainIdeas.Count;

		public int MaxNumDetails => this.model.MaxNumDetails;

		public bool ShowRoot { get; set; } = true;

		public bool ShowCurrent { get; set; } = true;

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
				if ( node.DistanceTo( point ) <= MapNode.RADIUS )
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
				Debug.WriteLine( $"ViewModel: Setting current node to {( selected == current ? "null" : selected )}." );
				this.model.Current = selected == current ? null : selected;
			}
			// Add a new node
			else if ( current is not null || this.model.Root is null )
			{
				Debug.WriteLine( $"ViewModel: Adding Node at ({point.X}, {point.Y})." );
				this.model.AddNode( new( point ) );
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

			foreach ( MapNode node in this.model.AllNodes )
			{
				Debug.WriteLine( node );
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
				// Draw node with modifications if needed
				Shape nodeShape = node.AsShape( );

				if ( node == this.model.Root && this.ShowRoot )
				{
					nodeShape.Fill = Brushes.Yellow.Clone( );
					nodeShape.Fill.Opacity = 0.25;
				}

				if ( node == this.model.Current && this.ShowCurrent )
				{
					nodeShape.Stroke = Brushes.Green;
				}

				_ = this.canvas.Children.Add( nodeShape );
				drawn.Add( node );

				// Draw edges that haven't been drawn
				foreach ( MapNode next in node.Neighbors )
				{
					if ( !drawn.Contains( next ) )
					{
						Line edgeLine = node.MakeLineTo( next );
						_ = this.canvas.Children.Add( edgeLine );
					}
				}
			}
		}
	}
}
