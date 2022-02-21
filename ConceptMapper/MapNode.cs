using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConceptMapper
{
	public class MapNode
	{
		private static int nextId;

		public MapNode( ) { this.ID = nextId++; }

		public int ID { get; init; }

		public Point Position { get; set; }

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
	}
}
