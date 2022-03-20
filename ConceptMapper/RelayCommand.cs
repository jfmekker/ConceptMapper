using System;
using System.Windows.Input;

namespace ConceptMapper
{
	/// <summary>
	/// An implementation of <see cref="ICommand"/> for WPF command binding.
	/// </summary>
	public class RelayCommand : ICommand
	{
		/// <summary>
		/// Event to raise if the value of <see cref="CanExecute"/> might have changed.
		/// </summary>
		public event EventHandler? CanExecuteChanged;

		private readonly Func<bool> canExecute;
		private readonly Action execute;

		/// <summary>
		/// Create a new instance of the <see cref="RelayCommand"/> class.
		/// </summary>
		/// <param name="canExecute">Function to determine if the command can execute.</param>
		/// <param name="execute">Function to call to execute the command.</param>
		public RelayCommand( Func<bool> canExecute , Action execute )
		{
			this.canExecute = canExecute;
			this.execute = execute;
		}

		/// <summary>
		/// Check if the command can execute.
		/// </summary>
		/// <param name="parameter">N/A</param>
		/// <returns><see langword="true"/> if the command can execute.</returns>
		public bool CanExecute( object? parameter ) => this.canExecute( );

		/// <summary>
		/// Execute the command.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="CanExecute"/> is <see langword="false"/>.</exception>
		/// <param name="parameter">N/A</param>
		public void Execute( object? parameter )
		{
			this.execute( );
			this.OnCanExecuteChanged( );
		}

		/// <summary>
		/// Raise the <see cref="CanExecuteChanged"/> event.
		/// </summary>
		public void OnCanExecuteChanged( ) => this.CanExecuteChanged?.Invoke( this , EventArgs.Empty );
	}
}
