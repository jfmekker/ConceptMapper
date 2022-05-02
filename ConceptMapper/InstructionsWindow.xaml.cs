using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ConceptMapper
{
	/// <summary>
	/// Interaction logic for InstructionsWindow.xaml
	/// </summary>
	public partial class InstructionsWindow : Window
	{
		private static readonly Dictionary<int , string> ImageIdToPathDictionary = new( )
		{
			{ 1 , "pack://application:,,,/Images/01_file_menu.png" } ,
			{ 2 , "pack://application:,,,/Images/02_view_menu.png" } ,
			{ 3 , "pack://application:,,,/Images/03_empty_map.png" } ,
			{ 4 , "pack://application:,,,/Images/04_root_node.png" } ,
			{ 5 , "pack://application:,,,/Images/05_first_main_idea.png" } ,
			{ 6 , "pack://application:,,,/Images/06_first_details.png" } ,
			{ 7 , "pack://application:,,,/Images/07_first_details_unselect.png" } ,
			{ 8 , "pack://application:,,,/Images/08_reselect.png" } ,
			{ 9 , "pack://application:,,,/Images/09_oops.png" } ,
			{ 10 , "pack://application:,,,/Images/10_precrosslink.png" } ,
			{ 11 , "pack://application:,,,/Images/11_postcrosslink.png" } ,
			{ 12 , "pack://application:,,,/Images/12_misc_info.png" } ,
			{ 13 , "pack://application:,,,/Images/13_calc_depth_width_hss.png" } ,
			{ 14 , "pack://application:,,,/Images/14_calc_main_ideas.png" } ,
			{ 15 , "pack://application:,,,/Images/15_calc_crosslinks.png" } ,
			{ 16 , "pack://application:,,,/Images/16_calc_custom.png" } ,
			{ 17 , "pack://application:,,,/Images/17_calc_done.png" } ,
		};

		private int currentImageId = 1;

		private const int MinImageId = 1;
		private const int MaxImageId = 17;

		public InstructionsWindow( )
		{
			this.InitializeComponent( );

			this.SetUiComponentSettings( );
		}

		private void SetUiComponentSettings( )
		{
			this.ImageLabel.Content = $"{this.currentImageId} / {MaxImageId}";
			this.ImagePanel.Source = new BitmapImage( new Uri( ImageIdToPathDictionary[this.currentImageId] ) );

			this.NextButton.IsEnabled = this.currentImageId < MaxImageId;
			this.PrevButton.IsEnabled = this.currentImageId > MinImageId;
		}

		private void PrevButton_Click( object sender , RoutedEventArgs e )
		{
			this.currentImageId = Math.Max( this.currentImageId - 1 , MinImageId );
			this.SetUiComponentSettings( );
		}

		private void NextButton_Click( object sender , RoutedEventArgs e )
		{
			this.currentImageId = Math.Min( this.currentImageId + 1 , MaxImageId );
			this.SetUiComponentSettings( );
		}
	}
}
