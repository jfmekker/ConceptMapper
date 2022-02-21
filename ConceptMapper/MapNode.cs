﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ConceptMapper
{
	public class MapNode
	{
		private static int nextId;

		public const int RADIUS = 40;

		public MapNode( Point point )
		{
			this.ID = nextId++;
			this.Position = point;
		}

		public int ID { get; init; }

		public Point Position { get; set; }

		public int X => this.Position.X;

		public int Y => this.Position.Y;

		public List<MapNode> Neighbors { get; set; } = new( );

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

		public override string? ToString( )
		{
			string str = $"Node #{this.ID}: ";

			foreach ( MapNode neighbor in this.Neighbors )
			{
				str += neighbor.ID + " ";
			}

			return str;
		}

		public double DistanceTo( MapNode other ) => Math.Sqrt( Math.Pow( this.X - other.X , 2 ) + Math.Pow( this.Y - other.Y , 2 ) );

		public double DistanceTo( Point other ) => Math.Sqrt( Math.Pow( this.X - other.X , 2 ) + Math.Pow( this.Y - other.Y , 2 ) );

		public Shape AsShape( )
		{
			return new Ellipse( )
			{
				Width = RADIUS * 2 ,
				Height = RADIUS * 2 ,
				Opacity = 0.5 ,
				StrokeThickness = 2 ,
				Stroke = Brushes.Black ,
				RenderTransform = new TranslateTransform( this.X - RADIUS , this.Y - RADIUS ) ,
			};
		}

		public Line MakeLineTo( MapNode other )
		{
			return new Line( )
			{
				X1 = this.X ,
				Y1 = this.Y ,
				X2 = other.X ,
				Y2 = other.Y ,
				Opacity = 0.5 ,
				StrokeThickness = 2 ,
				Stroke = Brushes.Gray ,
			};
		}
	}
}
