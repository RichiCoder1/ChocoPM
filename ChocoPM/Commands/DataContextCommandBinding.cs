﻿using System.Windows;
using System.Windows.Input;

namespace ChocoPM.Commands
{
    /// <summary>
    ///     A <see cref="RoutedCommandBinding"/> implementation that handles a
    ///     <see cref="RoutedCommand"/> by executing methods of the DataContext of the
    ///     <see cref="UIElement"/> whose <see cref="UIElement.CommandBindings"/> collection
    ///     contains the <see cref="DataContextCommandBinding"/>.
    /// </summary>
    public class DataContextCommandBinding : RoutedCommandBinding
    {
        /// <summary>
        ///     Name of the method of the DataContext that is executed when the command associated
        ///     with this <see cref="DataContextCommandBinding"/> initiates a check to determine
        ///     whether the command can be executed on the current command target.
        /// </summary>
        /// <remarks>
        ///     The corresponding method must have one of two signatures below, with the first
        ///     taking precedence over the other:
        ///     <code>void MyCanExecuteMethod(object parameter);</code>
        ///     <code>void MyCanExecuteMethod();</code>
        /// </remarks>
        public new string CanExecute { get; set; }

        /// <summary>
        ///     Name of the method of the DataContext that is executed when the command associated
        ///     with this <see cref="DataContextCommandBinding"/> executes.
        /// </summary>
        /// <remarks>
        ///     The corresponding method must have one of two signatures below, with the first
        ///     taking precedence over the other:
        ///     <code>void MyExecutedMethod(object parameter);</code>
        ///     <code>void MyExecutedMethod();</code>
        /// </remarks>
        public new string Executed { get; set; }

        /// <summary>
        ///     Name of the method of the DataContext that is executed when the command associated
        ///     with this <see cref="DataContextCommandBinding"/> initiates a check to determine
        ///     whether the command can be executed on the current command target.
        /// </summary>
        /// <remarks>
        ///     The corresponding method must have one of two signatures below, with the first
        ///     taking precedence over the other:
        ///     <code>void MyCanExecuteMethod(object parameter);</code>
        ///     <code>void MyCanExecuteMethod();</code>
        /// </remarks>
        public new string PreviewCanExecute { get; set; }

        /// <summary>
        ///     Name of the method of the DataContext that is executed when the command associated
        ///     with this <see cref="DataContextCommandBinding"/> executes.
        /// </summary>
        /// <remarks>
        ///     The corresponding method must have one of two signatures below, with the first
        ///     taking precedence over the other:
        ///     <code>void MyExecutedMethod(object parameter);</code>
        ///     <code>void MyExecutedMethod();</code>
        /// </remarks>
        public new string PreviewExecuted { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContextCommandBinding"/> class.
        /// </summary>
        public DataContextCommandBinding() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContextCommandBinding"/> class by
        ///     using the specified <see cref="ICommand"/>.
        /// </summary>
        public DataContextCommandBinding(ICommand command)
            : base(command)
        { }

        /// <summary>
        ///     The method that is called when the PreviewCanExecute <see cref="RoutedEvent"/> for the
        ///     <see cref="ICommand"/> associated with this <see cref="DataContextCommandBinding"/>
        ///     should be handled.
        /// </summary>
        /// <param name="sender">The command target on which the command is executing.</param>
        /// <param name="e">The event data.</param>
        protected internal override void OnPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var target = GetDataContext(sender);
            bool canExecute;
            if (!CommandExecutionManager.TryExecuteCommand(target, e.Parameter, false,
                    PreviewExecuted, PreviewCanExecute, out canExecute))
            {
                return;
            }

            e.CanExecute = canExecute;
            e.Handled = true;
        }
        
        /// <summary>
        ///     The method that is called when the CanExecute <see cref="RoutedEvent"/> for the
        ///     <see cref="ICommand"/> associated with this <see cref="DataContextCommandBinding"/>
        ///     should be handled.
        /// </summary>
        /// <param name="sender">The command target on which the command is executing.</param>
        /// <param name="e">The event data.</param>
        protected internal override void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var target = GetDataContext(sender);
            bool canExecute;
            if (!CommandExecutionManager.TryExecuteCommand(target, e.Parameter, false, Executed,
                    CanExecute, out canExecute))
            {
                return;
            }

            e.CanExecute = canExecute;
            e.Handled = true;
        }

        /// <summary>
        ///     The method that is called when the PreviewExecuted <see cref="RoutedEvent"/> for
        ///     the <see cref="ICommand"/> associated with this
        ///     <see cref="DataContextCommandBinding"/> should be handled.
        /// </summary>
        /// <param name="sender">The command target on which the command is executing.</param>
        /// <param name="e">The event data.</param>
        protected internal override void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var target = GetDataContext(sender);
            bool canExecute;
            if (CommandExecutionManager.TryExecuteCommand(target, e.Parameter, true, PreviewExecuted, PreviewCanExecute, out canExecute))
                e.Handled = true;
        }

        /// <summary>
        ///     The method that is called when the Executed <see cref="RoutedEvent"/> for the
        ///     <see cref="ICommand"/> associated with this <see cref="DataContextCommandBinding"/>
        ///     should be handled.
        /// </summary>
        /// <param name="sender">The command target on which the command is executing.</param>
        /// <param name="e">The event data.</param>
        protected internal override void OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var target = GetDataContext(sender);
            bool canExecute;
            if (CommandExecutionManager.TryExecuteCommand(target, e.Parameter, true, Executed, CanExecute, out canExecute))
                e.Handled = true;
        }
        
        private static object GetDataContext(object element)
        {
            var fe = element as FrameworkElement;
            if (fe != null)
                return fe.DataContext;

            var fce = element as FrameworkContentElement;
            return fce == null ? null : fce.DataContext;
        }
    }
}
