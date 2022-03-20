using System.Windows.Media;
using System.Windows.Shapes;

namespace ConceptMapper
{
	public static class DrawingExtensions
	{
		public static Shape AsNormal( this Shape shape )
		{
			shape.Opacity = 0.5;

			shape.Fill = Brushes.Green;

			shape.StrokeThickness = 2;
			shape.Stroke = Brushes.Red;

			return shape;
		}

		public static Shape AsRoot( this Shape shape , bool yes )
		{
			if ( yes )
			{
				shape.Fill = Brushes.Yellow;
			}
			return shape;
		}

		public static Shape AsMainIdea( this Shape shape , bool yes )
		{
			if ( yes )
			{
				shape.Fill = Brushes.Orange;
			}
			return shape;
		}

		public static Shape AsCurrent( this Shape shape , bool yes )
		{
			if ( yes )
			{
				shape.Stroke = Brushes.Black;
				shape.StrokeThickness = 4;
			}
			return shape;
		}

		public static Line AsNormal( this Line line )
		{
			line.Opacity = 0.5;

			line.StrokeThickness = 2;
			line.Stroke = Brushes.Red;

			return line;
		}

		public static Line AsCrosslink( this Line line )
		{
			line.Opacity = 0.5;

			line.StrokeThickness = 2;
			line.Stroke = Brushes.Red;

			line.StrokeDashArray = new( ) { 2 , 4 };

			return line;
		}
	}
}
