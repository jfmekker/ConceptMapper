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

		/// <summary>
		/// Create a new instance of the <see cref="MainView"/> class.
		/// Starts a new <see cref="MainViewModel"/> (and by extension,
		/// a new <see cref="MainModel"/>).
		/// </summary>
		public MainView( )
		{
			this.InitializeComponent( );
			this.viewModel = new MainViewModel( this.Canvas );
			this.DataContext = this.viewModel;
			this.viewModel.ResetGraph( );
		}

		/// <summary>
		/// Pass a left mouse click on the canvas to <see cref="MainViewModel"/>.
		/// </summary>
		private void Canvas_MouseLeftButtonUp( object sender , MouseButtonEventArgs e )
		{
			Point spot = e.GetPosition( this.Canvas );
			this.viewModel.Click( new( (int)spot.X , (int)spot.Y ) );
		}

		/// <summary>
		/// Pass a right mouse click on the canvas to <see cref="MainViewModel"/>.
		/// </summary>
		private void Canvas_MouseRightButtonUp( object sender , MouseButtonEventArgs e )
		{
			Point spot = e.GetPosition( this.Canvas );
			this.viewModel.Click( new( (int)spot.X , (int)spot.Y ) , true );
		}

		/// <inheritdoc cref="MainViewModel.ResetGraph"/>
		private void Menu_ResetGraph( object sender , RoutedEventArgs e ) => this.viewModel.ResetGraph( );

		/// <summary>
		/// Create a dialog box to select an existing image file.
		/// </summary>
		private void Menu_SelectImageFile( object sender , RoutedEventArgs e )
		{
			OpenFileDialog dialog = new( );
			if ( dialog.ShowDialog( ) is true )
			{
				this.viewModel.ImageFile = dialog.FileName;
			}
		}

		/// <summary>
		/// Create a dialog box to select an existing image file.
		/// </summary>
		private void Menu_SelectImageFolder( object sender , RoutedEventArgs e )
		{
			OpenFileDialog dialog = new( );
			dialog.CheckFileExists = false;
			dialog.CheckPathExists = false;
			dialog.Filter = "folder|*...";
			dialog.FileName = "this";
			dialog.AddExtension = false;
			if ( dialog.ShowDialog( ) is true )
			{
				Debug.WriteLine( $"View: Folder selected - '{dialog.FileName}'" );
				this.viewModel.ImageFolder = System.IO.Path.GetDirectoryName( dialog.FileName );

				if ( this.viewModel.AutoNextImage )
				{
					this.viewModel.NextImage( );
				}
				else
				{
					this.viewModel.UnsetImage( );
				}
			}
		}

		/// <summary>
		/// Automatically select the next unprocessed image file in the folder.
		/// </summary>
		private void Menu_SelectNextImageFile( object sender , RoutedEventArgs e )
		{
			this.viewModel.NextImage( );
		}

		/// <summary>
		/// Create a dialog box to select or create a output CSV file.
		/// </summary>
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

		/// <summary>
		/// Create and show the <see cref="AboutWindow"/>.
		/// </summary>
		private void Menu_About( object sender , RoutedEventArgs e ) => new AboutWindow( ).ShowDialog( );

		/// <inheritdoc cref="MainViewModel.Done"/>
		private void Button_DoneClick( object sender , RoutedEventArgs e ) => this.viewModel.Done( this.GetImage( ) );

		/// <summary>
		/// Handle a key press when the window has focus.
		/// </summary>
		/// <remarks>
		/// <list type="table">
		///		<item>
		///			<term>Delete</term>
		///			<description><see cref="MainViewModel.DeleteCurrent"/></description>
		///		</item>
		///		<item>
		///			<term>Escape</term>
		///			<description><see cref="MainViewModel.DeleteCurrent"/></description>
		///		</item>
		/// </list>
		/// </remarks>
		/// <param name="sender">N/A</param>
		/// <param name="e">Event args that contain the key pressed.</param>
		private void Window_KeyDown( object sender , KeyEventArgs e )
		{
			Debug.WriteLine( $"View: Key pressed - '{e.Key}'" );
			if ( e.Key is Key.Delete or Key.Back )
			{
				this.viewModel.DeleteCurrent( );
				e.Handled = true;
			}
		}

		/// <summary>
		/// Get a PNG screenshot of the window.
		/// </summary>
		/// <returns></returns>
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