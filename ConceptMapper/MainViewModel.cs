using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Controls;
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

		/// <summary>
		/// Create a new instance of the <see cref="MainViewModel"/> class.
		/// </summary>
		/// <param name="canvas">Canvas to draw nodes on.</param>
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

		/// <summary>
		/// Folder containg the selected image file.
		/// </summary>
		public string? ImageFolder { get; set; }

		/// <summary>
		/// Option to automatically change to the next image in the folder when
		/// the "Done" button is clicked.
		/// </summary>
		public bool AutoNextImage { get; set; } = true;

		/// <summary>
		/// Command to increase the size of the nodes.
		/// </summary>
		public RelayCommand NodeIncreaseSizeCommand { get; }

		/// <summary>
		/// Command to decrease the size of the nodes.
		/// </summary>
		public RelayCommand NodeDecreaseSizeCommand { get; }

		/// <summary>
		/// Command to reset the size of the nodes to default.
		/// </summary>
		public RelayCommand NodeResetSizeCommand { get; }

		/// <inheritdoc cref="MainModel.ImageFilePath"/>
		/// <remarks>
		/// If <see cref="MainModel.ImageFilePath"/> is <see langword="null"/>
		/// then this equals <c>"Concept Mapper"</c>.
		/// </remarks>
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

		/// <inheritdoc cref="MainModel.OutputFilePath"/>
		/// <remarks>
		/// If <see cref="MainModel.OutputFilePath"/> is <see langword="null"/>
		/// then this equals <c>"not selected"</c>.
		/// </remarks>
		public string OutputFile
		{
			get => this.model.OutputFilePath?.LocalPath ?? "not selected";
			set
			{
				this.model.OutputFilePath = new Uri( value );
				this.Update( );
			}
		}

		/// <inheritdoc cref="MainModel.NumNodes"/>
		public int NumNodes => this.model.NumNodes;

		/// <inheritdoc cref="MainModel.NumEdges"/>
		public int NumEdges => this.model.NumEdges;

		/// <inheritdoc cref="MainModel.Depth"/>
		public int Depth => this.model.Depth;

		/// <inheritdoc cref="MainModel.Width"/>
		public int Width => this.model.Width;

		/// <inheritdoc cref="MainModel.HSS"/>
		public int Hss => this.model.Hss;

		/// <inheritdoc cref="MainModel.NumMainIdeas"/>
		public int NumMainIdeas => this.model.NumMainIdeas;

		/// <inheritdoc cref="MainModel.MaxNumDetails"/>
		public int MaxNumDetails => this.model.MaxNumDetails;

		/// <inheritdoc cref="MainModel.NumCrosslinks"/>
		public int NumCrosslinks => this.model.NumCrosslinks;

		/// <inheritdoc cref="MainModel.MaxCrosslinkDist"/>
		public int MaxCrosslinkDist => this.model.MaxCrosslinkDist;

		/// <inheritdoc cref="MainModel.PriorKnowledge"/>
		public int? PriorKnowledge { get => this.model.PriorKnowledge; set => this.model.PriorKnowledge = value; }

		/// <inheritdoc cref="MainModel.Questions"/>
		public int? Questions { get => this.model.Questions; set => this.model.Questions = value; }

		/// <summary>
		/// Option to display the current selected node differently.
		/// </summary>
		public bool ShowCurrent { get; set; } = true;

		/// <summary>
		/// Option to display the root node differently.
		/// </summary>
		public bool ShowRoot { get; set; } = true;

		/// <summary>
		/// Option to display the main idea nodes differently.
		/// </summary>
		public bool ShowMainIdeas { get; set; } = true;

		/// <inheritdoc cref="MainModel.IsCompletable"/>
		public bool IsCompletable => this.model.IsCompletable;

		/// <summary>
		/// Tooltip string to attach to the "Done" button. Tells
		/// the user what actions (if any) need to be taken.
		/// </summary>
		public string CompletableTooltip =>
			this.IsCompletable ? "Good to go! :)" : ("Can not complete because:" +
			(this.model.Root is null ? "\n - No nodes have been placed." : "") +
			(this.model.ImageFilePath is null ? "\n - No image file has been selected." : "") +
			(this.model.OutputFilePath is null ? "\n - No output file has been selected." : ""));

		/// <summary>
		/// Event to raise when a propety changes.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Handle a user clicking a point on the canvas.
		/// </summary>
		/// <param name="point">Relative point of the click.</param>
		/// <param name="rightClick">Whether this was a right click.</param>
		public void Click( Point point , bool rightClick = false )
		{
			// Check if the click was in an existing node
			IReadOnlyList<MapNode> graph = this.model.AllNodes;
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
				if ( !rightClick )
				{
					Debug.WriteLine( $"ViewModel: Adding edge from ({selected.X}, {selected.Y}) to ({current.X}, {current.Y})." );
					this.model.AddEdge( selected , current );
				}
				else
				{
					Debug.WriteLine( $"ViewModel: Adding crosslink from ({selected.X}, {selected.Y}) to ({current.X}, {current.Y})." );
					this.model.AddCrosslink( selected , current );
				}
				this.model.Current = selected;
			}
			// Reset current node
			else if ( rightClick )
			{
				Debug.WriteLine( $"ViewModel: Resetting current node." );
				this.model.Current = null;
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

		/// <inheritdoc cref="MainModel.ResetGraph"/>
		public void ResetGraph( )
		{
			this.model.ResetGraph( );
			this.Update( );
		}

		/// <inheritdoc cref="MainModel.Export(RenderTargetBitmap?)"/>
		/// <remarks>
		/// After exporting, the next image is selected if <see cref="AutoNextImage"/> is on,
		/// then <see cref="ResetGraph"/> is called.
		/// </remarks>
		public void Done( RenderTargetBitmap? bitmap = null )
		{
			this.model.Export( bitmap );

			Uri? nextImage = null;
			if ( this.AutoNextImage && this.ImageFolder is not null )
			{
				int i = 0;
				string[] images = Directory.GetFiles( this.ImageFolder );

				while ( i < images.Length && this.ImageFile != images[i] )
					i += 1;

				if ( i + 1 < images.Length )
					nextImage = new Uri( images[i + 1] );

				if ( nextImage is null )
					_ = System.Windows.MessageBox.Show( $"All images in '{this.ImageFolder}' have been processed." );
			}
			this.model.ImageFilePath = nextImage;

			this.ResetGraph( );
		}

		/// <inheritdoc cref="MainModel.DeleteCurrentNode"/>
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

			IReadOnlyList<MapNode> graph = this.model.AllNodes;
			List<MapNode> drawn = new( graph.Count );
			Debug.WriteLine( $"ViewModel: Redrawing {this.NumNodes} nodes and {this.NumEdges} edges." );

			foreach ( MapNode node in graph )
			{
				// Draw node with modifications if needed
				Shape nodeShape =
					node.AsShape( )
					.AsNormal( )
					.AsCurrent( this.ShowCurrent && node == this.model.Current )
					.AsRoot( this.ShowRoot && node == this.model.Root )
					.AsMainIdea( this.ShowMainIdeas && this.model.MainIdeas.Contains( node ) );

				_ = this.canvas.Children.Add( nodeShape );
				drawn.Add( node );

				// Draw edges that haven't been drawn
				foreach ( MapNode next in node.Neighbors )
				{
					if ( !drawn.Contains( next ) )
					{
						Line edgeLine = node.MakeLineTo( next ).AsNormal( );
						_ = this.canvas.Children.Add( edgeLine );
					}
				}
			}

			// Draw crosslink edges
			foreach ( (MapNode node1, MapNode node2) in this.model.Crosslinks )
			{
				Line linkLine = node1.MakeLineTo( node2 ).AsCrosslink( );
				_ = this.canvas.Children.Add( linkLine );
			}
		}
	}
}
