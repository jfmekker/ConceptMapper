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
		public Point Position { get; set; }

		public List<MapNode> Neighbors { get; set; } = new( );

		public List<MapNode> GetWholeGraph()
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
						if ( !all.Contains( next ) )
						{
							newLevel.Add( next );
						}
					}
				}

				level = newLevel;
			}

			return all;
		}
	}
}
