using System.Windows.Media;
using System.Windows.Shapes;

namespace ConceptMapper
{
	/// <summary>
	/// Defines extension methods to make a small fluent interface.
	/// </summary>
	public static class DrawingExtensions
	{
		/// <summary>
		/// Adjust this shape to be a "normal" node.
		/// </summary>
		/// <param name="shape">The shape to adjust.</param>
		/// <returns>Adjusted shape.</returns>
		public static Shape AsNormal( this Shape shape )
		{
			shape.Opacity = 0.5;

			shape.Fill = Brushes.Green;

			shape.StrokeThickness = 2;
			shape.Stroke = Brushes.Red;

			return shape;
		}

		/// <summary>
		/// Adjust this shape to be a "root" node.
		/// </summary>
		/// <param name="shape">The shape to adjust.</param>
		/// <param name="yes">Whether to adjust the shape.</param>
		/// <returns>Adjusted shape.</returns>
		public static Shape AsRoot( this Shape shape , bool yes )
		{
			if ( yes )
			{
				shape.Fill = Brushes.Yellow;
			}
			return shape;
		}

		/// <summary>
		/// Adjust this shape to be a "main idea" node.
		/// </summary>
		/// <param name="shape">The shape to adjust.</param>
		/// <param name="yes">Whether to adjust the shape.</param>
		/// <returns>Adjusted shape.</returns>
		public static Shape AsMainIdea( this Shape shape , bool yes )
		{
			if ( yes )
			{
				shape.Fill = Brushes.Orange;
			}
			return shape;
		}

		/// <summary>
		/// Adjust this shape to be the "current selected" node.
		/// </summary>
		/// <param name="shape">The shape to adjust.</param>
		/// <param name="yes">Whether to adjust the shape.</param>
		/// <returns>Adjusted shape.</returns>
		public static Shape AsCurrent( this Shape shape , bool yes )
		{
			if ( yes )
			{
				shape.Stroke = Brushes.Black;
				shape.StrokeThickness = 4;
			}
			return shape;
		}

		/// <summary>
		/// Adjust this line to be a normal connection.
		/// </summary>
		/// <param name="line">The line to adjust.</param>
		/// <returns>Adjusted line.</returns>
		public static Line AsNormal( this Line line )
		{
			line.Opacity = 0.5;

			line.StrokeThickness = 2;
			line.Stroke = Brushes.Red;

			return line;
		}

		/// <summary>
		/// Adjust this line to be a crosslink connection.
		/// </summary>
		/// <param name="line">The line to adjust.</param>
		/// <returns>Adjusted line.</returns>
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
