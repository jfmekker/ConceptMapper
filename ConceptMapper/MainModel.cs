using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Windows.Media.Imaging;

namespace ConceptMapper
{
	public class MainModel
	{
		/// <summary>
		/// The root node that is the base of the graph.
		/// </summary>
		public MapNode? Root { get; set; }

		/// <summary>
		/// The currently selected node.
		/// </summary>
		public MapNode? Current { get; set; }

		/// <summary>
		/// All nodes in the entire graph.
		/// </summary>
		public IReadOnlyList<MapNode> AllNodes => this.Root?.GetWholeGraph( ) ?? new( );

		/// <summary>
		/// Nodes directly connected to <see cref="Root"/>.
		/// </summary>
		public IReadOnlyList<MapNode> MainIdeas => this.Root?.Neighbors ?? new( );

		/// <summary>
		/// List of special edges called crosslinks. They do not count as a normal connection.
		/// </summary>
		public IReadOnlyList<(MapNode, MapNode)> Crosslinks => this.crosslinks;

		private readonly List<(MapNode, MapNode)> crosslinks = new( );

		/// <summary>
		/// The largest number of nodes in any level (measured from <see cref="Root"/>).
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		/// The farthest distance from <see cref="Root"/>.
		/// </summary>
		public int Depth { get; private set; }

		/// <summary>
		/// Sum of <see cref="Width"/> and <see cref="Depth"/>.
		/// </summary>
		public int Hss => this.Width + this.Depth;

		/// <summary>
		/// Number of nodes directly connected to <see cref="Root"/>.
		/// </summary>
		public int NumMainIdeas => this.MainIdeas.Count;

		/// <summary>
		/// The largest number of nodes that are connected to a main idea while not going through <see cref="Root"/>.
		/// </summary>
		public int MaxNumDetails { get; private set; }

		/// <summary>
		/// Number of crosslink connections.
		/// </summary>
		public int NumCrosslinks => this.crosslinks.Count;

		/// <summary>
		/// The max distance between crosslink nodes.
		/// Finds the minimum graph distance for all crosslinks and returns the maximum of those distances.
		/// </summary>
		public int MaxCrosslinkDist { get; private set; }

		/// <summary>
		/// Total number of nodes.
		/// </summary>
		public int NumNodes => this.AllNodes.Count;

		/// <summary>
		/// Total number of connections (not including crosslinks).
		/// </summary>
		public int NumEdges => this.AllNodes.Sum( x => x.Neighbors.Count ) / 2;

		/// <summary>
		/// Manual field set by the user.
		/// </summary>
		public int? PriorKnowledge { get; set; }

		/// <summary>
		/// Manual field set by the user.
		/// </summary>
		public int? Questions { get; set; }

		/// <summary>
		/// Path to the image for the background.
		/// </summary>
		public Uri? ImageFilePath { get; set; }

		/// <summary>
		/// Path to the output csv file.
		/// </summary>
		public Uri? OutputFilePath { get; set; }

		/// <summary>
		/// If the model is ready to be exported. Conditions:
		/// <list type="bullet">
		///		<item><see cref="Root"/> exists.</item>
		///		<item><see cref="ImageFilePath"/> is set.</item>
		///		<item><see cref="OutputFilePath"/> is set.</item>
		/// </list>
		/// </summary>
		public bool IsCompletable => this.Root is not null && this.ImageFilePath is not null && this.OutputFilePath is not null;

		/// <summary>
		/// Add a new node to the graph, connected to current.
		/// </summary>
		/// <param name="node">The new node to add.</param>
		public void AddNode( MapNode node )
		{
			if ( this.Root is null )
			{
				this.Root ??= node;
				this.Current = this.Root;
			}
			else if ( this.Current is not null )
			{
				this.Current.Neighbors.Add( node );
				node.Neighbors.Add( this.Current );
				this.Current = node;
			}
			else
			{
				throw new InvalidOperationException( "AddNode called while Root not null but Current is." );
			}

			this.CalculateProperties( );
		}

		/// <summary>
		/// Add a normal connection between two nodes. Does nothing if an edge or crosslink already exists.
		/// </summary>
		/// <param name="node1">First node to connect.</param>
		/// <param name="node2">First node to connect.</param>
		public void AddEdge( MapNode node1 , MapNode node2 )
		{
			bool n1 = node1.Neighbors.Contains( node2 );
			bool n2 = node2.Neighbors.Contains( node1 );

			if ( n1 != n2 )
				throw new InvalidOperationException( "Node has a one-way edge." );

			bool crosslink = false;
			foreach ( (MapNode, MapNode) link in this.crosslinks )
			{
				if ( link == (node1, node2) || link == (node2, node1) )
				{
					crosslink = true;
					break;
				}
			}

			if ( !n1 && !n2 && !crosslink )
			{
				node1.Neighbors.Add( node2 );
				node2.Neighbors.Add( node1 );
			}
			else
			{
				Debug.WriteLine( "Model: Node already contains edge or crosslink, not adding edge." );
			}

			this.CalculateProperties( );
		}

		/// <summary>
		/// Add a crosslink connection between two nodes. Does nothing if an edge or crosslink already exists.
		/// </summary>
		/// <param name="node1">First node to connect.</param>
		/// <param name="node2">First node to connect.</param>
		public void AddCrosslink( MapNode node1 , MapNode node2 )
		{
			bool n1 = node1.Neighbors.Contains( node2 );
			bool n2 = node2.Neighbors.Contains( node1 );

			if ( n1 != n2 )
				throw new InvalidOperationException( "Node has a one-way edge." );

			bool crosslink = false;
			foreach ( (MapNode, MapNode) link in this.crosslinks )
			{
				if ( link == (node1, node2) || link == (node2, node1) )
				{
					crosslink = true;
					break;
				}
			}

			if ( !n1 && !n2 && !crosslink )
			{
				this.crosslinks.Add( (node1, node2) );
			}

			this.CalculateProperties( );
		}

		/// <summary>
		/// Reset the graph by removing all nodes.
		/// </summary>
		public void ResetGraph( )
		{
			this.Current = null;
			this.Root = null;

			this.crosslinks.Clear( );

			this.Width = 0;
			this.Depth = 0;
			this.MaxNumDetails = 0;
			this.MaxCrosslinkDist = 0;
			this.PriorKnowledge = 0;
			this.Questions = 0;
		}

		/// <summary>
		/// Delete <see cref="Current"/> and remove all connections, then set to <see langword="null"/>.
		/// </summary>
		public void DeleteCurrentNode( )
		{
			IReadOnlyList<MapNode> all = this.AllNodes;

			if ( this.Current == this.Root )
			{
				this.ResetGraph( );
			}
			else if ( this.Current is not null )
			{
				Debug.WriteLine( $"Model: Removing node {this.Current}" );

				foreach ( MapNode node in all )
				{
					_ = node.Neighbors.Remove( this.Current );
				}

				foreach ( (MapNode, MapNode) crosslink in this.Crosslinks )
				{
					if ( crosslink.Item1 == this.Current || crosslink.Item2 == this.Current )
					{
						_ = this.crosslinks.Remove( crosslink );
						break;
					}
				}

				this.Current = null;
				this.CalculateProperties( );
			}
		}

		/// <summary>
		/// Export the the graph information to the output CSV file. If the output file does not exist,
		/// a header row is added first. Potentially saves a screenshot under a "ConceptMapper" folder.
		/// </summary>
		/// <param name="bitmap">Screenshot to save with "_nodes" as an added suffix to the filename.</param>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="IsCompletable"/> is <see langword="false"/> when called.</exception>
		public void Export( RenderTargetBitmap? bitmap = null )
{
Debug.WriteLine( $"Model: Exporting graph info..." );
if ( !this.IsCompletable )
{
throw new InvalidOperationException( "Done was executed when IsCompletable was false." );
}
string directory = Path.GetDirectoryName( this.ImageFilePath!.LocalPath ) ?? "";
			string filename = Path.GetFileNameWithoutExtension( this.ImageFilePath!.LocalPath );
			string extension = Path.GetExtension( this.ImageFilePath!.LocalPath );

			bool writeHeader = !File.Exists( this.OutputFilePath!.LocalPath );
			using StreamWriter writer = new( this.OutputFilePath!.LocalPath , true );

			if ( writeHeader )
			{
				writer.WriteLine( "Image,NumNodes,NumEdges,Width,Depth,HSS,NumMainIdeas,MaxNumDetails,PriorKnowledge,Questions,NumCrosslinks,MaxCrosslinkDistance" );
			}

			string info = $"{filename}.{extension},{this.NumNodes},{this.Width},{this.Depth},{this.Hss},{this.NumMainIdeas},{this.MaxNumDetails},{this.PriorKnowledge},{this.Questions},{this.NumCrosslinks},{this.MaxCrosslinkDist}";
			Debug.WriteLine( $"Model: {info}" );
			writer.WriteLine( info );

			if ( bitmap is not null )
			{
				string subdirName = "ConceptMapperScreenshots";
				_ = Directory.CreateDirectory( Path.Combine( directory , subdirName ) );
				using var fs = new FileStream( Path.Combine( directory , subdirName , filename + "_nodes" + extension ) , FileMode.Create , FileAccess.Write );

				var encoder = new PngBitmapEncoder( );
				encoder.Frames.Add( BitmapFrame.Create( bitmap ) );

				encoder.Save( fs );
			}
		}

		/// <summary>
		/// Calculate the public facing properties with the current state of the graph. Calls:
		/// <list type="bullet">
		/// <item><see cref="CalculateWidthAndDepth"/></item>
		/// <item><see cref="CalculateMaxNumDetails"/></item>
		/// <item><see cref="CalculateCrosslinkDist"/></item>
		/// </list>
		/// </summary>
		private void CalculateProperties( )
		{
			this.CalculateWidthAndDepth( );
			this.CalculateMaxNumDetails( );
			this.CalculateCrosslinkDist( );
		}

		private void CalculateWidthAndDepth( )
		{
			int maxWidth = 0;
			int maxDepth = -1;

			if ( this.Root is not null )
			{
				List<MapNode> level = new( ) { this.Root };
				List<MapNode> considered = new( ) { };
				while ( level.Count > 0 )
				{
					// Increment max depth to the current level
					maxDepth += 1;

					// Set max width if there are more nodes on this level
					maxWidth = Math.Max( maxWidth , level.Count );

					// Mark all nodes on this level
					considered.AddRange( level );

					// Construct next level from neighbors that are not marked
					List<MapNode> newLevel = new( );
					foreach ( MapNode node in level )
					{
						foreach ( MapNode next in node.Neighbors )
						{
							if ( !considered.Contains( next ) )
							{
								newLevel.Add( next );
							}
						}
					}
					level = newLevel;
				}
			}

			this.Width = maxWidth;
			this.Depth = maxDepth;
			Debug.WriteLine( $"Model: Calculated width={this.Width} and depth={this.Depth}." );
		}

		private void CalculateMaxNumDetails( )
		{
			int max = 0;
			foreach ( MapNode idea in this.MainIdeas )
			{
				// Get all nodes connected to idea not via Root
				List<MapNode> details = new( ) { idea };
				List<MapNode> considering = new( ) { idea };
				while ( considering.Count > 0 )
				{
					foreach ( MapNode next in considering[0].Neighbors )
					{
						if ( !details.Contains( next ) && next != this.Root )
						{
							details.Add( next );
							considering.Add( next );
						}
					}
					_ = considering.Remove( considering[0] );
				}

				max = Math.Max( max , details.Count );
			}

			// Subtract 1 to not count the main idea itself
			this.MaxNumDetails = max - 1;
		}

		private void CalculateCrosslinkDist( )
		{
			int maxDist = 0;

			foreach ( (MapNode n1, MapNode n2) in this.Crosslinks )
			{
				maxDist = Math.Max( maxDist , n1.DistanceTo( n2 ) );
			}

			this.MaxCrosslinkDist = maxDist;
		}
	}
}
