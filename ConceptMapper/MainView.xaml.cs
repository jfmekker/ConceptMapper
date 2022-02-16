using System.Windows;

namespace ConceptMapper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainView : Window
	{
		public MainView( )
		{
			this.DataContext = new MainViewModel( );
			this.InitializeComponent( );
		}
	}
}
