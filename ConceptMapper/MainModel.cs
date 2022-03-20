﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ConceptMapper
{
	public class MainModel
	{
		public MapNode? Root { get; set; }
		public MapNode? Current { get; set; }

		public List<MapNode> AllNodes => this.Root?.GetWholeGraph( ) ?? new( );
		public List<MapNode> MainIdeas => this.Root?.Neighbors ?? new( );

		public int Width { get; private set; }
		public int Depth { get; private set; }
		public int Hss => this.Width + this.Depth;
		public int MaxNumDetails { get; private set; }

		public int NumNodes => this.AllNodes.Count;
		public int NumEdges => this.AllNodes.Sum( x => x.Neighbors.Count ) / 2;

		public int? PriorKnowledge { get; set; }
		public int? Questions { get; set; }
		public int? NumCrosslinks { get; set; }
		public int? MaxCrosslinkDist { get; set; }

		public Uri? ImageFilePath { get; set; }
		public Uri? OutputFilePath { get; set; }

		public bool IsCompletable
		{
			get
			{
				return this.Root is not null
					&& this.ImageFilePath is not null
					&& this.OutputFilePath is not null
					&& this.PriorKnowledge is not null
					&& this.Questions is not null
					&& this.NumCrosslinks is not null
					&& this.MaxCrosslinkDist is not null;
			}
		}

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
			this.CalculateMaxNumDetails( );
		}

		public void AddEdge( MapNode node1 , MapNode node2 )
		{
			bool n1 = node1.Neighbors.Contains( node2 );
			bool n2 = node2.Neighbors.Contains( node1 );
			Debug.Assert( n1 == n2 );

			if ( !n1 && !n2 )
			{
				node1.Neighbors.Add( node2 );
				node2.Neighbors.Add( node1 );
			}
			else
			{
				Debug.WriteLine( "Model: Node already contains edge, not adding duplicate." );
			}

			this.CalculateWidthAndDepth( );
			this.CalculateMaxNumDetails( );
		}

		public void ResetGraph( )
		{
			this.Current = null;
			this.Root = null;

			this.Width = 0;
			this.Depth = 0;
			this.MaxNumDetails = 0;

			this.PriorKnowledge = null;
			this.Questions = null;
			this.NumCrosslinks = null;
			this.MaxCrosslinkDist = null;
		}

		public void DeleteCurrentNode( )
		{
			List<MapNode> all = this.AllNodes;

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

				this.Current = null;
				this.CalculateWidthAndDepth( );
				this.CalculateMaxNumDetails( );
			}
		}

		public void Export( )
		{
			if ( !this.IsCompletable )
			{
				throw new InvalidOperationException( "Done was executed when IsCompletable was false." );
			}

			bool writeHeader = !File.Exists( this.OutputFilePath!.LocalPath );
			using StreamWriter writer = new( this.OutputFilePath!.LocalPath , true );

			if ( writeHeader )
			{
				writer.WriteLine( "Image,NumNodes,Width,Depth,HSS,NumMainIdeas,MaxNumOfDetails,PriorKnowledge,Questions,NumCrosslinks,MaxCrosslinkDistance" );
			}

			string info = $"{Path.GetFileName( this.ImageFilePath!.LocalPath )},{this.NumNodes},{this.Width},{this.Depth},{this.Hss},{this.MainIdeas.Count},{this.MaxNumDetails},{this.PriorKnowledge},{this.Questions},{this.NumCrosslinks},{this.MaxCrosslinkDist}";
			Debug.WriteLine( $"Model: {info}" );
			writer.WriteLine( info );
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
	}
}
