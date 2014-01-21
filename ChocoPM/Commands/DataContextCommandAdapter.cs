using System;
using System.Reflection;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Input;

namespace ChocoPM.Commands
{
    /// <summary>
    ///     A markup extension that returns an <see cref="ICommand"/> that is capable of executing
    ///     methods of the DataContext of a target FrameworkElement.
    /// </summary>
    /// <remarks>
    ///     When the <see cref="ICommand.Executed"/> and <see cref="ICommand.CanExecute"/> methods
    ///     of the returned <see cref="ICommand"/> object are invoked, methods on the DataContext
    ///     whose names correspond to the values of the <see cref="Executed"/> and
    ///     <see cref="CanExecute"/> properties are invoked. See the <see cref="Executed"/> and
    ///     <see cref="CanExecute"/> properties for specifics on the allowable method signatures.
    /// </remarks>
    public class DataContextCommandAdapter : MarkupExtension, ICommand
    {
        private object _target; 

        /// <summary>
        ///     Name of the method of the target object's DataContext that determines whether the
        ///     command can execute in its current state.
        /// </summary>
        /// <remarks>
        ///     The corresponding method must have one of two signatures below, with the first
        ///     taking precedence over the other:
        ///     <code>void MyCanExecuteMethod(object parameter);</code>
        ///     <code>void MyCanExecuteMethod();</code>
        /// </remarks>
        public string CanExecute { get; set; }


        /// <summary>
        ///     Name of the method of the target object's DataContext to be called when the command
        ///     is invoked.
        /// </summary>
        /// <remarks>
        ///     The corresponding method must have one of two signatures below, with the first
        ///     taking precedence over the other:
        ///     <code>void MyExecutedMethod(object parameter);</code>
        ///     <code>void MyExecutedMethod();</code>
        /// </remarks>
        public string Executed { get; set; }

        /// <summary>
        ///     Initializes a new instance of the DataContextCommandAdapter class.
        /// </summary>
        public DataContextCommandAdapter() { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContextCommandAdapter"/> class by
        ///     using the specified method name for the <see cref="Executed"/> property.
        /// </summary>
        /// <param name="executed">
        ///     The name of the <see cref="Executed"/> method.
        /// </param>
        public DataContextCommandAdapter(string executed)
        {
            this.Executed = executed;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContextCommandAdapter"/> class by
        ///     using the specified method names for the <see cref="Executed"/> and
        ///     <see cref="CanExecute"/> properties.
        /// </summary>
        /// <param name="executed">
        ///     The name of the <see cref="Executed"/> method.
        /// </param>
        /// <param name="canExecute">
        ///     The name of the <see cref="CanExecute"/> method.
        /// </param>
        public DataContextCommandAdapter(string executed, string canExecute)
        {
            this.Executed = executed;
            this.CanExecute = canExecute;
        }

        /// <summary>
        ///     Returns an <see cref="ICommand"/> that is capable of executing methods of the
        ///     DataContext of the target.
        /// </summary>
        /// <param name="serviceProvider">
        ///     Object that can provide services for the markup extension.
        /// </param>
        /// <returns>
        ///     The <see cref="ICommand"/> object.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (target == null)
                throw new Exception("IProvideValueTarget could not be resolved.");

            
            this._target = 
                target.TargetObject is InputBinding
                ? this.GetInputBindingsCollectionOwner(target)
                : target.TargetObject;

            return this;
        }

        //
        // This method only works with the C# 4.0 XamlParser.
        // If there was another way to do this without reflection... I would do it that way
        // Regardless, this method will only be called once when the xaml is initially parsed, so its
        // not really a performance issue.
        private object GetInputBindingsCollectionOwner(IProvideValueTarget targetService)
        {
            var xamlContextField = targetService.GetType().GetField("_xamlContext", BindingFlags.Instance | BindingFlags.NonPublic);
            if (xamlContextField != null)
            {
                var xamlContext = xamlContextField.GetValue(targetService);
                var grandParentInstanceProperty = xamlContext.GetType().GetProperty("GrandParentInstance");
                if (grandParentInstanceProperty != null)
                {
                    var inputBindingsCollection = grandParentInstanceProperty.GetGetMethod().Invoke(xamlContext, null);
                    var ownerField = inputBindingsCollection.GetType().GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (ownerField != null)
                    {
                        var owner = ownerField.GetValue(inputBindingsCollection);
                        return owner;
                    }
                }
            }
            return null;
        }

        #region ICommand Implementation

        event EventHandler ICommand.CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        bool ICommand.CanExecute(object parameter)
        {
            var target = GetDataContext(this._target);
            if (this._target != null)
            {
                bool canExecute;
                if (CommandExecutionManager.TryExecuteCommand(target, parameter, false, this.Executed, this.CanExecute, out canExecute))
                    return canExecute;
            }
            return false;
        }

        void ICommand.Execute(object parameter)
        {
            var target = GetDataContext(this._target);
            if (this._target != null)
            {
                bool canExecute;
                CommandExecutionManager.TryExecuteCommand(target, parameter, true, this.Executed, this.CanExecute, out canExecute);
            }
        }

        private static object GetDataContext(object element)
        {
            var fe = element as FrameworkElement;
            if (fe != null)
                return fe.DataContext;
            else
            {
                var fce = element as FrameworkContentElement;
                return fce == null ? null : fce.DataContext;
            }
        }

        #endregion
    }
}
