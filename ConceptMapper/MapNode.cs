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
	}
}
