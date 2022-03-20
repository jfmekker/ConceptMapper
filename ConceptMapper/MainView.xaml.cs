using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

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
			this.viewModel.ResetGraph( );
		}

		private void Canvas_MouseLeftButtonUp( object sender , MouseButtonEventArgs e )
		{
			Point spot = e.GetPosition( this.Canvas );
			this.viewModel.Click( new( (int)spot.X , (int)spot.Y ) );
		}

		private void Canvas_MouseRightButtonUp( object sender , MouseButtonEventArgs e ) => this.viewModel.ResetCurrent( );

		private void Menu_ResetGraph( object sender , RoutedEventArgs e ) => this.viewModel.ResetGraph( );

		private void Menu_SelectImageFile( object sender , RoutedEventArgs e )
		{
			OpenFileDialog dialog = new( );
			if ( dialog.ShowDialog( ) is true )
			{
				this.viewModel.ImageFile = dialog.FileName;
			}
		}

		private void Menu_SelectOutputFile( object sender , RoutedEventArgs e )
		{
			SaveFileDialog dialog = new( );
			dialog.Filter = "Comma-Separated Value files (*csv)|*.csv";
			dialog.FileName = "output.csv";
			dialog.AddExtension = true;
			dialog.OverwritePrompt = false;
			if ( dialog.ShowDialog( ) is true )
			{
				this.viewModel.OutputFile = dialog.FileName;
			}
		}

		private void Menu_About( object sender , RoutedEventArgs e ) => new AboutWindow( ).ShowDialog( );

		private void Button_DoneClick( object sender , RoutedEventArgs e ) => this.viewModel.Done( this.GetImage( ) );

		private void Window_KeyDown( object sender , KeyEventArgs e )
		{
			Debug.WriteLine( $"View: Key pressed - '{e.Key}'" );
			if ( e.Key is Key.Delete or Key.Back )
			{
				this.viewModel.DeleteCurrent( );
			}
		}

		private RenderTargetBitmap? GetImage( )
		{
			var size = new Size( this.ActualWidth , this.ActualHeight );
			if ( size.IsEmpty )
				return null;

			var result = new RenderTargetBitmap( (int)size.Width , (int)size.Height , 96 , 96 , PixelFormats.Pbgra32 );
			var drawingvisual = new DrawingVisual( );

			using ( DrawingContext context = drawingvisual.RenderOpen( ) )
			{
				context.DrawRectangle( new VisualBrush( this ) , null , new Rect( new Point( ) , size ) );
				context.Close( );
			}

			result.Render( drawingvisual );
			return result;
		}
	}
}