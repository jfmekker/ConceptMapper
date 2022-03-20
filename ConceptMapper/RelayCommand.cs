using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ConceptMapper
{
	public class RelayCommand : ICommand
	{
		public event EventHandler? CanExecuteChanged;

		private readonly Func<bool> canExecute;
		private readonly Action execute;

		public RelayCommand( Func<bool> canExecute , Action execute )
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		public bool CanExecute( object? parameter ) => this.canExecute( );

		public void Execute( object? parameter )
		{
			this.execute( );
			this.OnCanExecuteChanged( );
		}

		public void OnCanExecuteChanged( ) => this.CanExecuteChanged?.Invoke( this , EventArgs.Empty );
	}
}
