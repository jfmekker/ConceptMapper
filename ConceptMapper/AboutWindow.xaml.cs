using System.IO;
using System.Reflection;
using System.Windows;

namespace ConceptMapper
{
	/// <summary>
	/// Interaction logic for AboutWindow.xaml
	/// </summary>
	public partial class AboutWindow : Window
	{
		public AboutWindow( )
		{
			this.InitializeComponent( );

			this.BuildVersionLabel.Content = Assembly.GetExecutingAssembly( ).GetName( ).Version?.ToString( ) ?? "unknown";
			this.BuildDateLabel.Content = File.GetLastWriteTime( Assembly.GetExecutingAssembly( ).Location );
		}
	}
}
