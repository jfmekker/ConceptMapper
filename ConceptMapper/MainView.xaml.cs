using System.Windows;
using System.Windows.Input;

namespace ConceptMapper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainView : Window
	{
		private readonly MainViewModel viewModel;

		public MainView( )
		{
			this.InitializeComponent( );
			this.viewModel = new MainViewModel( this.Canvas );
			this.DataContext = this.viewModel;
		}

		private void Canvas_MouseLeftButtonUp( object sender , MouseButtonEventArgs e )
		{
			Point spot = e.GetPosition( this.Canvas );
			this.viewModel.Click( new( (int)spot.X , (int)spot.Y ) );
		}

		private void Canvas_MouseRightButtonUp( object sender , MouseButtonEventArgs e ) => this.viewModel.ResetCurrent( );

		private void Menu_ResetImage( object sender , RoutedEventArgs e ) => this.viewModel.ResetCanvas( );
	}
}