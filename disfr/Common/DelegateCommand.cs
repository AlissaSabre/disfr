using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace disfr.UI
{
    /// <summary>
    /// The common base class for <see cref="DelegateCommand"/> and its variations.
    /// </summary>
    /// <remarks>
    /// <see cref="DelegateCommandHelper"/> looks for this class to make a magic.
    /// </remarks>
    /// <seealso cref="DelegateCommandHelper"/>
    /// <seealso cref="DelegateCommand"/>
    public abstract class DelegateCommandBase
    {
        /// <summary>
        /// Initialize the common parts of <see cref="DelegateCommand"/> and its variations.
        /// </summary>
        /// <param name="has_can_execute"></param>
        protected DelegateCommandBase(bool has_can_execute)
        {
            HasCanExecute = has_can_execute;
        }

        /// <summary>
        /// A common implementation of <see cref="ICommand.CanExecuteChanged"/>.
        /// </summary>
        /// <remarks>
        /// The attached event handler is simply delegated to <see cref="CommandManager.RequerySuggested"/> if <see cref="HasCanExecute"/> is true.
        /// Otherwise, the event handler is never invoked (since <see cref="ICommand.CanExecute(object)"/> will always return true.)
        /// </remarks>
        public event EventHandler CanExecuteChanged
        {
            add { if (HasCanExecute) CommandManager.RequerySuggested += value; }
            remove { if (HasCanExecute) CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Whether this instance has a CanExecute delegate.
        /// </summary>
        protected readonly bool HasCanExecute;

        #region StaleException event

        /// <summary>
        /// Provides Exception data for <see cref="StaleException"/> events.
        /// </summary>
        public class StaleExceptionEventArgs : EventArgs
        {
            /// <summary>
            /// The captured exception.
            /// </summary>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Creates an instance.
            /// </summary>
            /// <param name="exception">An exception.</param>
            public StaleExceptionEventArgs(Exception exception)
            {
                Exception = exception;
            }
        }

        /// <summary>
        /// Occurs when the asynchronous execution of _execute_ delegate invoked through an Execute method throws an exception. 
        /// </summary>
        public event EventHandler<StaleExceptionEventArgs> StaleException;

        /// <summary>
        /// Raises an <see cref="StaleException"/> event.
        /// </summary>
        /// <param name="e">An StaleExceptionEventArgs instance containing a captured exception.</param>
        protected void RaiseStaleException(StaleExceptionEventArgs e)
        {
            StaleException?.Invoke(this, e);
        }

        /// <summary>
        /// Raises an <see cref="StaleException"/> event using a captured exception.
        /// </summary>
        /// <param name="e">A captured exception.</param>
        protected void OnStaleException(Exception e)
        {
            RaiseStaleException(new StaleExceptionEventArgs(e));
        }

        #endregion

        /// <summary>
        /// Invokes an asynchronous delegate, preparing to capture any exception.
        /// </summary>
        /// <param name="execute">An asynchronous delegate.</param>
        /// <remarks>
        /// If the delegate threw an exception, a <see cref="StaleException"/> event would be raised.
        /// The event handler would be executed by the same synchronization context
        /// as the thread that called this method.
        /// </remarks>
        protected void ExecuteDetached(Func<Task> execute)
        {
            var context = SynchronizationContext.Current;
            try
            {
                execute().ContinueWith(task =>
                    {
                        if (context != null)
                        {
                            context.Post(_ => OnStaleException(task.Exception), null);
                        }
                        else
                        {
                            OnStaleException(task.Exception);
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception exception)
            {
                OnStaleException(exception);
            }
        }
    }

    /// <summary>
    /// A delegate based implementation of <see cref="ICommand"/> with no parameters.
    /// </summary>
    /// <remarks>
    /// <see cref="DelegateCommand"/> provides delegate based implementation
    /// with no parameter needed.
    /// When calling methods via <see cref="ICommand"/> interface,
    /// you need to pass null as parameters.
    /// </remarks>
    /// <seealso cref="DelegateCommand{T}"/>
    /// <seealso cref="DelegateCommand{T1,T2}"/>
    /// <seealso cref="DelegateCommand{T1, T2, T3}"/>
    /// <seealso cref="DelegateCommand{T1, T2, T3, T4}"/>
    public class DelegateCommand : DelegateCommandBase, ICommand
    {
        /// <summary>
        /// The Execute async delegate.
        /// </summary>
        private readonly Func<Task> _ExecuteAsyc;

        /// <summary>
        /// The CanExecute delegate, or null if always true.
        /// </summary>
        private readonly Func<bool> _CanExecute;

        /// <summary>
        /// Creates an ICommand object from two delegates.
        /// </summary>
        /// <param name="executeAsync">asynchronous delegate to perform when this command is executed.</param>
        /// <param name="canExecute">Returns whether this command can be executed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="executeAsync"/> is null.</exception>
        /// <remarks>
        /// <paramref name="canExecute"/> may be null if this command can always be executed.
        /// </remarks>
        public DelegateCommand(Func<Task> executeAsync, Func<bool> canExecute = null)
            : base(canExecute != null)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException("execute");
            }

            _ExecuteAsyc = executeAsync;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Execute this command without waiting for completion.
        /// </summary>
        /// <remarks>
        /// This method simply invokes the execute delegate passed to the constructor.
        /// The caller has no way to know the completion of the command.
        /// Any Exception raised by the delegate fires <see cref="StaleException"/> event.
        /// </remarks>
        public void Execute()
        {
            ExecuteDetached(_ExecuteAsyc);
        }

        /// <summary>
        /// Execute this command asynchronously.
        /// </summary>
        /// <returns>
        /// This method simply invokes the execute delegate passed to the constructor.
        /// You can await this method to know the completion of the command.
        /// </returns>
        public Task ExecuteAsync()
        {
            return _ExecuteAsyc();
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <returns>true if it can.</returns>
        /// <remarks>
        /// It invokes the <see cref="Func{TResult}"/> delegate passed to the constructor,
        /// or always returns true if the delegate was null.
        /// </remarks>
        public bool CanExecute()
        {
            return _CanExecute == null ? true : _CanExecute();
        }

        /// <summary>
        /// Executes this command detached.
        /// </summary>
        /// <param name="parameter">Must be a null.</param>
        /// <exception cref="ArgumentException"><paramref name="parameter"/> is not null.</exception>
        void ICommand.Execute(object parameter)
        {
            if (parameter != null) throw new ArgumentException("Must be a null.", "parameter");
            Execute();
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter">Must be a null.</param>
        /// <exception cref="ArgumentException"><paramref name="parameter"/> is not a null.</exception>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter != null) throw new ArgumentException("Must be a null.", "parameter");
            return CanExecute();
        }
    }

    /// <summary>
    /// A delegate based implementation of <see cref="ICommand"/> with a parameter.
    /// </summary>
    /// <typeparam name="T">Type of command parameter object.</typeparam>
    /// <remarks>
    /// <see cref="DelegateCommand{T}"/> provides delegate based implementation
    /// as well as a type-safe feature over the standard ICommand system.
    /// If you passed an object of type other than <typeparamref name="T"/> to either of the
    /// <see cref="ICommand"/> methods,
    /// an <see cref="Exception"/> is thrown.
    /// </remarks>
    /// <seealso cref="DelegateCommand"/>
    public class DelegateCommand<T> : DelegateCommandBase, ICommand
    {
        /// <summary>
        /// The Execute delegate.
        /// </summary>
        private readonly Func<T, Task> _ExecuteAsync;

        /// <summary>
        /// The CanExecute delegate, or null if always true.
        /// </summary>
        private readonly Func<T, bool> _CanExecute;

        /// <summary>
        /// Creates an ICommand object from two delegates.
        /// </summary>
        /// <param name="executeAsync">Action to perform when this command is executed.</param>
        /// <param name="canExecute">Returns whether this command can be executed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="executeAsync"/> is null.</exception>
        /// <remarks>
        /// <paramref name="canExecute"/> may be null if this command can always be executed.
        /// </remarks>
        public DelegateCommand(Func<T, Task> executeAsync, Func<T, bool> canExecute = null)
            : base(canExecute != null)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException("execute");
            }

            _ExecuteAsync = executeAsync;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">Any parameter defined by command semantics.</param>
        /// <remarks>
        /// It simply invokes the <see cref="Action{T}"/> delegate passed to the constructor.
        /// </remarks>
        public void Execute(T parameter)
        {
            ExecuteDetached(() => _ExecuteAsync(parameter));
        }

        public Task ExecuteAsync(T parameter)
        {
            return _ExecuteAsync(parameter);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter">Any parameter defined by command semantics.</param>
        /// <returns>true if it can.</returns>
        /// <remarks>
        /// It invokes the <see cref="Func{T, TResult}"/> delegate passed to the constructor,
        /// or always returns true if the delegate was null.
        /// </remarks>
        public bool CanExecute(T parameter)
        {
            return _CanExecute == null ? true : _CanExecute(parameter);
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">A parameter of type <typeparamref name="T"/></param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null and <typeparamref name="T"/> is not nullable.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type <typeparamref name="T"/>.</exception>
        void ICommand.Execute(object parameter)
        {
            Execute((T)parameter);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter">A parameter of type <typeparamref name="T"/></param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null and <typeparamref name="T"/> is not nullable.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type <typeparamref name="T"/>.</exception>
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }
    }

    /// <summary>
    /// A delegate based implementation of <see cref="ICommand"/> with two parameters.
    /// </summary>
    /// <typeparam name="T1">Type of first command parameter object.</typeparam>
    /// <typeparam name="T2">Type of second command parameter object.</typeparam>
    /// <remarks>
    /// <see cref="DelegateCommand{T1,T2}"/> provides delegate based implementation
    /// as well as a type-safe parameter marshaling feature over the standard ICommand system.
    /// The two parameters are passed as a single array of objects per ICommand methods.
    /// If you passed null or an object of type other than an array of objects
    /// to either of the <see cref="ICommand"/> methods,
    /// or the array has less than two elements,
    /// an <see cref="Exception"/> is thrown.
    /// Extra elements are silently ignored.
    /// </remarks>
    /// <seealso cref="DelegateCommand"/>
    public class DelegateCommand<T1, T2> : DelegateCommandBase, ICommand
    {
        /// <summary>
        /// The Execute delegate.
        /// </summary>
        private readonly Func<T1, T2, Task> _ExecuteAsync;

        /// <summary>
        /// The CanExecute delegate, or null if always true.
        /// </summary>
        private readonly Func<T1, T2, bool> _CanExecute;

        /// <summary>
        /// Creates an ICommand object from two delegates.
        /// </summary>
        /// <param name="executeAsync">Action to perform when this command is executed.</param>
        /// <param name="canExecute">Returns whether this command can be executed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="executeAsync"/> is null.</exception>
        /// <remarks>
        /// <paramref name="canExecute"/> may be null if this command can always be executed.
        /// </remarks>
        public DelegateCommand(Func<T1, T2, Task> executeAsync, Func<T1, T2, bool> canExecute = null)
            : base(canExecute != null)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException("execute");
            }

            _ExecuteAsync = executeAsync;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter1">First parameter defined by command semantics.</param>
        /// <param name="parameter2">Second parameter defined by command semantics.</param>
        /// <remarks>
        /// It simply invokes the <see cref="Action{T1, T2}"/> delegate passed to the constructor.
        /// </remarks>
        public void Execute(T1 parameter1, T2 parameter2)
        {
            ExecuteDetached(() => _ExecuteAsync(parameter1, parameter2));
        }

        public Task ExecuteAsync(T1 parameter1, T2 parameter2)
        {
            return _ExecuteAsync(parameter1, parameter2);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter1">First parameter defined by command semantics.</param>
        /// <param name="parameter2">Second parameter defined by command semantics.</param>
        /// <returns>true if it can.</returns>
        /// <remarks>
        /// It invokes the <see cref="Func{T1, T2, TResult}"/> delegate passed to the constructor,
        /// or always returns true if the delegate was null.
        /// </remarks>
        public bool CanExecute(T1 parameter1, T2 parameter2)
        {
            return _CanExecute == null ? true : _CanExecute(parameter1, parameter2);
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">A parameter object whose type is an array of objects with two or more elements.</param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type object array.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="parameter"/> has less than two elements.</exception>
        void ICommand.Execute(object parameter)
        {
            if (parameter == null) throw new NullReferenceException();
            var array = (object[])parameter;
            Execute((T1)array[0], (T2)array[1]);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter">A parameter object whose type is an array of objects with two or more elements.</param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type object array.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="parameter"/> has less than two elements.</exception>
        /// <returns>True if this command can execute.  False otherwise.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter == null) throw new NullReferenceException();
            var array = (object[])parameter;
            return CanExecute((T1)array[0], (T2)array[1]);
        }
    }

    /// <summary>
    /// A delegate based implementation of <see cref="ICommand"/> with two parameters.
    /// </summary>
    /// <typeparam name="T1">Type of first command parameter object.</typeparam>
    /// <typeparam name="T2">Type of second command parameter object.</typeparam>
    /// <typeparam name="T3">Type of third command parameter object.</typeparam>
    /// <remarks>
    /// <see cref="DelegateCommand{T1,T2,T3}"/> provides delegate based implementation
    /// as well as a type-safe parameter marshaling feature over the standard ICommand system.
    /// The three parameters are passed as a single array of objects per ICommand methods.
    /// If you passed null or an object of type other than an array of objects
    /// to either of the <see cref="ICommand"/> methods,
    /// or the array has less than three elements,
    /// an <see cref="Exception"/> is thrown.
    /// Extra elements are silently ignored.
    /// </remarks>
    /// <seealso cref="DelegateCommand"/>
    public class DelegateCommand<T1, T2, T3> : DelegateCommandBase, ICommand
    {
        /// <summary>
        /// The Execute delegate.
        /// </summary>
        private readonly Func<T1, T2, T3, Task> _ExecuteAsync;

        /// <summary>
        /// The CanExecute delegate, or null if always true.
        /// </summary>
        private readonly Func<T1, T2, T3, bool> _CanExecute;

        /// <summary>
        /// Creates an ICommand object from two delegates.
        /// </summary>
        /// <param name="executeAsync">Action to perform when this command is executed.</param>
        /// <param name="canExecute">Returns whether this command can be executed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="executeAsync"/> is null.</exception>
        /// <remarks>
        /// <paramref name="canExecute"/> may be null if this command can always be executed.
        /// </remarks>
        public DelegateCommand(Func<T1, T2, T3, Task> executeAsync, Func<T1, T2, T3, bool> canExecute = null)
            : base(canExecute != null)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException("execute");
            }

            _ExecuteAsync = executeAsync;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter1">First parameter defined by command semantics.</param>
        /// <param name="parameter2">Second parameter defined by command semantics.</param>
        /// <param name="parameter3">Third parameter defined by command semantics.</param>
        /// <remarks>
        /// It simply invokes the <see cref="Action{T1, T2, T3}"/> delegate passed to the constructor.
        /// </remarks>
        public void Execute(T1 parameter1, T2 parameter2, T3 parameter3)
        {
            ExecuteDetached(() => _ExecuteAsync(parameter1, parameter2, parameter3));
        }

        public Task ExecuteAsync(T1 parameter1, T2 parameter2, T3 parameter3)
        {
            return _ExecuteAsync(parameter1, parameter2, parameter3);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter1">First parameter defined by command semantics.</param>
        /// <param name="parameter2">Second parameter defined by command semantics.</param>
        /// <param name="parameter3">Third parameter defined by command semantics.</param>
        /// <returns>true if it can.</returns>
        /// <remarks>
        /// It invokes the <see cref="Func{T1, T2, T3, TResult}"/> delegate passed to the constructor,
        /// or always returns true if the delegate was null.
        /// </remarks>
        public bool CanExecute(T1 parameter1, T2 parameter2, T3 parameter3)
        {
            return _CanExecute == null ? true : _CanExecute(parameter1, parameter2, parameter3);
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">A parameter object whose type is an array of objects with three or more elements.</param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type object array.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="parameter"/> has less than two elements.</exception>
        void ICommand.Execute(object parameter)
        {
            if (parameter == null) throw new NullReferenceException();
            var array = (object[])parameter;
            Execute((T1)array[0], (T2)array[1], (T3)array[2]);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter">A parameter object whose type is an array of objects with two or more elements.</param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type object array.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="parameter"/> has less than two elements.</exception>
        /// <returns>True if this command can execute.  False otherwise.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter == null) throw new NullReferenceException();
            var array = (object[])parameter;
            return CanExecute((T1)array[0], (T2)array[1], (T3)array[2]);
        }
    }

    /// <summary>
    /// A delegate based implementation of <see cref="ICommand"/> with two parameters.
    /// </summary>
    /// <typeparam name="T1">Type of first command parameter object.</typeparam>
    /// <typeparam name="T2">Type of second command parameter object.</typeparam>
    /// <typeparam name="T3">Type of third command parameter object.</typeparam>
    /// <typeparam name="T4">Type of fourth command parameter object.</typeparam>
    /// <remarks>
    /// <see cref="DelegateCommand{T1,T2,T3,T4}"/> provides delegate based implementation
    /// as well as a type-safe parameter marshaling feature over the standard ICommand system.
    /// The four parameters are passed as a single array of objects per ICommand methods.
    /// If you passed null or an object of type other than an array of objects
    /// to either of the <see cref="ICommand"/> methods,
    /// or the array has less than four elements,
    /// an <see cref="Exception"/> is thrown.
    /// Extra elements are silently ignored.
    /// </remarks>
    /// <seealso cref="DelegateCommand"/>
    public class DelegateCommand<T1, T2, T3, T4> : DelegateCommandBase, ICommand
    {
        /// <summary>
        /// The Execute delegate.
        /// </summary>
        private readonly Func<T1, T2, T3, T4, Task> _ExecuteAsync;

        /// <summary>
        /// The CanExecute delegate, or null if always true.
        /// </summary>
        private readonly Func<T1, T2, T3, T4, bool> _CanExecute;

        /// <summary>
        /// Creates an ICommand object from two delegates.
        /// </summary>
        /// <param name="executeAsync">Action to perform when this command is executed.</param>
        /// <param name="canExecute">Returns whether this command can be executed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="executeAsync"/> is null.</exception>
        /// <remarks>
        /// <paramref name="canExecute"/> may be null if this command can always be executed.
        /// </remarks>
        public DelegateCommand(Func<T1, T2, T3, T4, Task> executeAsync, Func<T1, T2, T3, T4, bool> canExecute = null)
            : base(canExecute != null)
        {
            if (executeAsync == null)
            {
                throw new ArgumentNullException("execute");
            }

            _ExecuteAsync = executeAsync;
            _CanExecute = canExecute;
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter1">First parameter defined by command semantics.</param>
        /// <param name="parameter2">Second parameter defined by command semantics.</param>
        /// <param name="parameter3">Third parameter defined by command semantics.</param>
        /// <param name="parameter4">Fourth parameter defined by command semantics.</param>
        /// <remarks>
        /// It simply invokes the <see cref="Action{T1, T2, T3, T4}"/> delegate passed to the constructor.
        /// </remarks>
        public void Execute(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
        {
            ExecuteDetached(() => _ExecuteAsync(parameter1, parameter2, parameter3, parameter4));
        }

        public Task ExecuteAsync(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
        {
            return _ExecuteAsync(parameter1, parameter2, parameter3, parameter4);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter1">First parameter defined by command semantics.</param>
        /// <param name="parameter2">Second parameter defined by command semantics.</param>
        /// <param name="parameter3">Third parameter defined by command semantics.</param>
        /// <param name="parameter4">Third parameter defined by command semantics.</param>
        /// <returns>true if it can.</returns>
        /// <remarks>
        /// It invokes the <see cref="Func{T1, T2, T3, T4, TResult}"/> delegate passed to the constructor,
        /// or always returns true if the delegate was null.
        /// </remarks>
        public bool CanExecute(T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
        {
            return _CanExecute == null ? true : _CanExecute(parameter1, parameter2, parameter3, parameter4);
        }

        /// <summary>
        /// Executes this command.
        /// </summary>
        /// <param name="parameter">A parameter object whose type is an array of objects with three or more elements.</param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type object array.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="parameter"/> has less than two elements.</exception>
        void ICommand.Execute(object parameter)
        {
            if (parameter == null) throw new NullReferenceException();
            var array = (object[])parameter;
            Execute((T1)array[0], (T2)array[1], (T3)array[2], (T4)array[3]);
        }

        /// <summary>
        /// Tests whether this command can execute.
        /// </summary>
        /// <param name="parameter">A parameter object whose type is an array of objects with two or more elements.</param>
        /// <exception cref="NullReferenceException"><paramref name="parameter"/> is null.</exception>
        /// <exception cref="InvalidCastException"><paramref name="parameter"/> is not of type object array.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="parameter"/> has less than two elements.</exception>
        /// <returns>True if this command can execute.  False otherwise.</returns>
        bool ICommand.CanExecute(object parameter)
        {
            if (parameter == null) throw new NullReferenceException();
            var array = (object[])parameter;
            return CanExecute((T1)array[0], (T2)array[1], (T3)array[2], (T4)array[3]);
        }
    }
}
