using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConceptMapper
{
	/// <summary>
	/// Object to represent a node in a concept map graph.
	/// </summary>
	public class MapNode
	{
		private static int nextId;

		/// <summary>
		/// The default radius of nodes when displayed.
		/// </summary>
		/// <remarks>
		/// Not sure if this is pixels or dpi.
		/// </remarks>
		public const int DefaultRadius = 25;

		/// <summary>
		/// The minimum radius of nodes when displayed.
		/// </summary>
		/// <remarks>
		/// Not sure if this is pixels or dpi.
		/// </remarks>
		public const int MinRadius = 10;

		/// <summary>
		/// The maximum radius of nodes when displayed.
		/// </summary>
		/// <remarks>
		/// Not sure if this is pixels or dpi.
		/// </remarks>
		public const int MaxRadius = 55;

		/// <summary>
		/// The current radius used to display nodes.
		/// </summary>
		public static int Radius { get; set; } = DefaultRadius;

		/// <summary>
		/// Create a new instance of the <see cref="MapNode"/> class.
		/// </summary>
		/// <param name="point">Center point of the node.</param>
		public MapNode( Point point )
		{
			this.ID = nextId++;
			this.position = point;
		}

		/// <summary>
		/// Unique (per-run) ID for this node. Only used for debugging purposes.
		/// </summary>
		public int ID { get; init; }

		private readonly Point position;

		/// <summary>
		/// Cartesian x coordinate of the node on the canvas.
		/// </summary>
		public int X => this.position.X;

		/// <summary>
		/// Cartesian y coordinate of the node on the canvas.
		/// </summary>
		public int Y => this.position.Y;

		/// <summary>
		/// Collection of nodes directly connected to this node.
		/// </summary>
		public List<MapNode> Neighbors { get; set; } = new( );

		/// <summary>
		/// Flatten the entire graph connected to this node into a list.
		/// </summary>
		/// <returns>All connected (directly and indirectly) nodes.</returns>
		public List<MapNode> GetWholeGraph( )
		{
			List<MapNode> level = new( ) { this };
			List<MapNode> all = new( ) { };
			while ( level.Count > 0 )
			{
				all.AddRange( level );

				List<MapNode> newLevel = new( );
				foreach ( MapNode node in level )
				{
					foreach ( MapNode next in node.Neighbors )
					{
						if ( !all.Contains( next ) && !newLevel.Contains( next ) )
						{
							newLevel.Add( next );
						}
					}
				}

				level = newLevel;
			}

			return all;
		}

		/// <inheritdoc/>
		public override string? ToString( )
		{
			string str = $"Node #{this.ID}: ";

			foreach ( MapNode neighbor in this.Neighbors )
			{
				str += neighbor.ID + " ";
			}

			return str;
		}

		/// <summary>
		/// Compute the minimum graph distance to another node.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if the other node is not connected.</exception>
		/// <param name="other">Other node to reach.</param>
		/// <returns>Number of "hops" to get to the other node.</returns>
		public int DistanceTo( MapNode other )
		{
			int dist = 0;
			List<MapNode> level = new( ) { this };
			List<MapNode> all = new( ) { };
			while ( level.Count > 0 )
			{
				all.AddRange( level );

				if ( level.Contains( other ) )
					break;

				List<MapNode> newLevel = new( );
				foreach ( MapNode node in level )
				{
					foreach ( MapNode next in node.Neighbors )
					{
						if ( !all.Contains( next ) && !newLevel.Contains( next ) )
						{
							newLevel.Add( next );
						}
					}
				}

				level = newLevel;
				dist += 1;
			}

			return all.Contains( other ) ? dist : throw new InvalidOperationException( "Nodes were not connected when DistanceTo was called." );
		}

		/// <summary>
		/// Compute the Cartesian distance to a point relative to the node's center.
		/// </summary>
		/// <param name="other">Point to measure to.</param>
		/// <returns>Distance to the other point.</returns>
		public double DistanceTo( Point other ) => Math.Sqrt( Math.Pow( this.X - other.X , 2 ) + Math.Pow( this.Y - other.Y , 2 ) );

		/// <summary>
		/// Construct this node as a <see cref="Shape"/> object.
		/// </summary>
		/// <returns>An <see cref="Ellipse"/> representing the node.</returns>
		public Shape AsShape( )
		{
			if ( Radius is < 10 or > 100 )
				throw new InvalidOperationException( $"Node radius out of bounds: {Radius}" );

			var e = new Ellipse( ) {
				Width = Radius * 2 ,
				Height = Radius * 2 ,
				RenderTransform = new TranslateTransform( this.X - Radius , this.Y - Radius ) ,
			};

			return e;
		}

		/// <summary>
		/// Construct a <see cref="Line"/> object to another node.
		/// </summary>
		/// <param name="that">Destination node.</param>
		/// <returns>A <see cref="Line"/> to the other node.</returns>
		public Line MakeLineTo( MapNode that )
		{
			if ( Radius is < 10 or > 100 )
				throw new InvalidOperationException( $"Node radius out of bounds: {Radius}" );

			double theta = Math.Atan2( that.Y - this.Y , that.X - this.X );
			double dx = Radius * Math.Cos( theta );
			double dy = Radius * Math.Sin( theta );

			var l = new Line( ) {
				X1 = this.X + dx ,
				Y1 = this.Y + dy ,
				X2 = that.X - dx ,
				Y2 = that.Y - dy ,
			};

			return l;
		}
	}
}
