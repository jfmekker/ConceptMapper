using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConceptMapper
{
	public class MainModel
	{
		public MapNode? Root { get; set; }

		public MapNode? Current { get; set; }

		public int Width
		{
			get; private set;
		}

		public int Depth
		{
			get; private set;
		}

		public List<MapNode> AllNodes => this.Root?.GetWholeGraph( ) ?? new( );

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

			this.CalculateWidthAndDepth( );
		}

		public void AddEdge( MapNode node1 , MapNode node2 )
		{
			if ( !node1.Neighbors.Contains( node2 ) )
			{
				node1.Neighbors.Add( node2 );
				node2.Neighbors.Add( node2 );
			}
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
	}
}
