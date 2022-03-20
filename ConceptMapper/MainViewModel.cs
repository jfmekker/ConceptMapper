using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// There's a Path class in Windows.Shapes for some reason
using Path = System.IO.Path;

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

			int nodeSizeIncrement = 15;
			this.NodeIncreaseSizeCommand = new RelayCommand(
				( ) => MapNode.Radius <= MapNode.MaxRadius - nodeSizeIncrement ,
				( ) => {
					MapNode.Radius += nodeSizeIncrement;
					this.Update( );
				}
			);
			this.NodeDecreaseSizeCommand = new RelayCommand(
				( ) => MapNode.Radius >= MapNode.MinRadius + nodeSizeIncrement ,
				( ) => {
					MapNode.Radius -= nodeSizeIncrement;
					this.Update( );
				}
			);
			this.NodeResetSizeCommand = new RelayCommand(
				( ) => MapNode.Radius != MapNode.DefaultRadius ,
				( ) => {
					MapNode.Radius = MapNode.DefaultRadius;
					this.Update( );
				}
			);
		}

		public string? ImageFolder { get; set; }
		public bool AutoNextImage { get; set; } = true;

		public RelayCommand NodeIncreaseSizeCommand { get; }
		public RelayCommand NodeDecreaseSizeCommand { get; }
		public RelayCommand NodeResetSizeCommand { get; }

		public string ImageFile
		{
			get => this.model.ImageFilePath?.LocalPath ?? "Concept Mapper";
			set
			{
				var uri = new Uri( value );
				this.model.ImageFilePath = uri;
				this.ImageFolder = Path.GetDirectoryName( uri.LocalPath );
				this.Update( );
			}
		}
		public string OutputFile
		{
			get => this.model.OutputFilePath?.LocalPath ?? "not selected";
			set
			{
				this.model.OutputFilePath = new Uri( value );
				this.Update( );
			}
		}

		public int NumNodes => this.model.NumNodes;
		public int NumEdges => this.model.NumEdges;

		public int Depth => this.model.Depth;
		public int Width => this.model.Width;
		public int Hss => this.model.Hss;
		public int NumMainIdeas => this.model.MainIdeas.Count;
		public int MaxNumDetails => this.model.MaxNumDetails;

		public int? PriorKnowledge { get => this.model.PriorKnowledge; set => this.model.PriorKnowledge = value; }
		public int? Questions { get => this.model.Questions; set => this.model.Questions = value; }
		public int? NumCrosslinks { get => this.model.NumCrosslinks; set => this.model.NumCrosslinks = value; }
		public int? MaxCrosslinkDist { get => this.model.MaxCrosslinkDist; set => this.model.MaxCrosslinkDist = value; }

		public bool ShowCurrent { get; set; } = true;
		public bool ShowRoot { get; set; } = true;
		public bool ShowMainIdeas { get; set; } = true;

		public bool IsCompletable => this.model.IsCompletable;
		public string CompletableTooltip =>
			this.IsCompletable ? "Good to go! :)" : ("Can not complete because:" +
			(this.model.Root is null ? "\n - No nodes have been placed." : "") +
			(this.model.ImageFilePath is null ? "\n - No image file has been selected." : "") +
			(this.model.OutputFilePath is null ? "\n - No output file has been selected." : "") +
			(this.model.PriorKnowledge is null ? "\n - Field 'Prior Knowledge' is empty." : "") +
			(this.model.Questions is null ? "\n - Field 'Questions' is empty." : "") +
			(this.model.NumCrosslinks is null ? "\n - Field '# Crosslinks' is empty." : "") +
			(this.model.MaxCrosslinkDist is null ? "\n - Field 'Max Crosslink dist.' is empty." : ""));

		public event PropertyChangedEventHandler? PropertyChanged;

		public void Click( Point point )
		{
			// Check if the click was in an existing node
			List<MapNode> graph = this.model.AllNodes;
			MapNode? current = this.model.Current;
			MapNode? selected = null;
			foreach ( MapNode node in graph )
			{
				if ( node.DistanceTo( point ) <= MapNode.Radius )
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
				Debug.WriteLine( $"ViewModel: Setting current node to {(selected == current ? "null" : selected)}." );
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

		public void ResetGraph( )
		{
			this.model.ResetGraph( );
			this.Update( );
		}

		public void Done( )
		{
			this.model.Export( );

			Uri? nextImage = null;
			if ( this.AutoNextImage && this.ImageFolder is not null )
			{
				int i = 0;
				string[] images = Directory.GetFiles( this.ImageFolder );

				while ( i < images.Length && this.ImageFile != images[i] )
					i += 1;

				if ( i + 1 < images.Length )
					nextImage = new Uri( images[i + 1] );
			}
			this.model.ImageFilePath = nextImage;

			this.PriorKnowledge = null;
			this.Questions = null;
			this.NumCrosslinks = null;
			this.MaxCrosslinkDist = null;
			this.ResetGraph( );
		}

		public void DeleteCurrent( )
		{
			this.model.DeleteCurrentNode( );
			this.Update( );
		}

		private void Update( )
		{
			this.DrawNodesAndEdges( );

			if ( this.PropertyChanged is not null )
			{
				// I know it's not the best practice to update everything everytime... but who cares
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumNodes ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumEdges ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Width ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Depth ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Hss ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumMainIdeas ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.MaxNumDetails ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.PriorKnowledge ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.Questions ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.NumCrosslinks ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.MaxCrosslinkDist ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.ImageFile ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.ImageFolder ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.OutputFile ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.IsCompletable ) ) );
				this.PropertyChanged( this , new PropertyChangedEventArgs( nameof( this.CompletableTooltip ) ) );
			}

			this.NodeIncreaseSizeCommand.OnCanExecuteChanged( );
			this.NodeDecreaseSizeCommand.OnCanExecuteChanged( );
			this.NodeResetSizeCommand.OnCanExecuteChanged( );
		}

		private void DrawNodesAndEdges( )
		{
			this.canvas.Children.Clear( );
			this.canvas.Background = this.model.ImageFilePath is not null ? new ImageBrush( new BitmapImage( this.model.ImageFilePath ) ) : Brushes.LightGray;

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
				else if ( this.model.MainIdeas.Contains( node ) && this.ShowMainIdeas )
				{
					nodeShape.Fill = Brushes.Orange.Clone( );
					nodeShape.Fill.Opacity = 0.25;
				}
				else
				{
					nodeShape.Fill = Brushes.Green.Clone( );
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
