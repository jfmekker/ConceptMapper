﻿using Microsoft.Win32;
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

		private void Menu_SelectImage( object sender , RoutedEventArgs e )
		{
			OpenFileDialog dialog = new( );
			_ = dialog.ShowDialog( );
		}

		private void Menu_About( object sender , RoutedEventArgs e ) => _ = MessageBox.Show( "Title: Concept Mapper\nAuthor: Jacob Mekker\n\nDescription:\nTODO\nLicense:\nTODO" );
	}
}