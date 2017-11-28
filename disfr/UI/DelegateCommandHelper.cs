using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace disfr.UI
{
    /// <summary>
    /// A class to provide a static method to instanciate DelegateCommand automatically.
    /// </summary>
    /// <example>
    /// class MyViewModel
    /// {
    ///     public DelegateCommand&lt;string> SampleCommand { get; private set; }
    /// 
    ///     private void SampleCommand_Execute(string arg)
    ///     {
    ///         DoSomethingUseful(arg);
    ///     }
    /// 
    ///     private bool SampleCommand_CanExecute(string arg)
    ///     {
    ///         return CheckReallyUseful(arg);
    ///     }
    /// 
    ///     public MyViewModel()
    ///     {
    ///         DelegateCommandHelper.GetHelp(this);
    ///     }
    /// }
    /// </example>
    /// <remarks>
    /// <para>
    /// <see cref="DelegateCommandHelper"/> relys on a set of conventions:
    /// The property for a command should have a public getter and a private setter, and its name should end with "Command".
    /// Execute handler method must have a name "*_Execute".
    /// CanExecute handler method must have a name "*_CanExecute" if any.
    /// (You can omit *_CanExecute to indicate the command is always can-execute.)
    /// The corresponding methods must have compatible signature.
    /// You must not overload (the names of) handler methods.
    /// </para>
    /// <para>
    /// <see cref="DelegateCommandHelper.GetHelp(object)"/> doesn't touch already initialized properties,
    /// even if it follows the above conventions.
    /// So, you can initilize some command properties that require special handling
    /// before calling <see cref="DelegateCommandHelper.GetHelp(object)"/>
    /// and keep the automatic initialization off from those special command properties.
    /// </para>
    /// <para>
    /// Yes, more ordinary way of detecting properties is to use attrbiutes.
    /// I just don't like it. :-)
    /// </para>
    /// </remarks>
    /// <seealso cref="DelegateCommand"/>
    /// <seealso cref="DelegateCommand{T}"/>
    /// <seealso cref="DelegateCommand{T1, T2}"/>
    /// <seealso cref="DelegateCommand{T1, T2, T3}"/>
    /// <seealso cref="DelegateCommand{T1, T2, T3, T4}"/>
    ///
    public static class DelegateCommandHelper
    {
        private const string COMMAND_SUFFIX = "Command";

        private const string EXECUTE_SUFFIX = "_Execute";

        private const string CANEXECUTE_SUFFIX = "_CanExecute";

        /// <summary>
        /// Initializes all uninitialized DelegateCommand properties of <paramref name="obj"/>. 
        /// </summary>
        /// <param name="obj">An object with DelegateCommand properties to initialize.</param>
        /// <exception cref="DelegateCommandHelperException">Something is wrong.</exception>
        public static void GetHelp(object obj)
        {
            var type = obj.GetType();
            foreach (var property in type.GetProperties())
            {
                // Check the name convention and type.
                if (property.IsSpecialName ||
                    !property.Name.EndsWith(COMMAND_SUFFIX) ||
                    !typeof(DelegateCommandBase).IsAssignableFrom(property.PropertyType))
                {
                    // This is not my business. :-)
                    continue;
                }

                // Check that it has a public getter and a private setter.
                if (property.GetGetMethod() == null ||
                    property.GetSetMethod() != null || property.GetSetMethod(true) == null)
                {
                    // This is not my business, either.
                    // Should we throw an Exception, instead?
                    continue;
                }

                // Check whether it has a valid value already.
                if (property.GetValue(obj) != null)
                {
                    // We don't touch those commands initialized otherways.
                    continue;
                }

                // A sanity check.
                if (property.PropertyType == typeof(DelegateCommandBase))
                {
                    Throw("A command property {0}.{1} has type {2}, which is inappropriate.",
                        type.FullName, property.Name, typeof(DelegateCommandBase).FullName);
                }

                var ctors = property.PropertyType.GetConstructors();
                if (ctors?.Length != 1)
                {
                    Throw("{0} must have exactly one constructor.", property.PropertyType.FullName);
                }
                var ctor_params = ctors[0].GetParameters();
                if (ctor_params.Length != 2 ||
                    !typeof(Delegate).IsAssignableFrom(ctor_params[0].ParameterType) ||
                    !typeof(Delegate).IsAssignableFrom(ctor_params[1].ParameterType))
                {
                    Throw("Constructor of {0} has an illegal signature.", property.PropertyType.FullName);
                }

                var execute_name = property.Name + EXECUTE_SUFFIX;
                var execute_delegate_type = ctor_params[0].ParameterType;
                var execute_delegate = GetDelegate(obj, execute_name, execute_delegate_type);
                if (execute_delegate == null)
                {
                    Throw("Method {0}.{1} is not found.", type.FullName, execute_name);
                }

                var can_execute_name = property.Name + CANEXECUTE_SUFFIX;
                var can_execute_delegate_type = ctor_params[1].ParameterType;
                var can_execute_delegate = GetDelegate(obj, can_execute_name, can_execute_delegate_type);

                var command = ctors[0].Invoke(new object[] { execute_delegate, can_execute_delegate });
                property.SetValue(obj, command);
            }
        }

        /// <summary>
        /// Creates and returns a delegate via refrection.
        /// </summary>
        /// <param name="obj">An object which is bound to the resulting delegate.</param>
        /// <param name="name">Name of the method in <paramref name="obj"/> that the resulting delegate represents.</param>
        /// <param name="delegate_type">Type of the resulting delegate.</param>
        /// <returns>A delegate if a suitable method is found in <paramref name="obj"/>.  null if no method of the specified name exists.</returns>
        /// <remarks>
        /// If a method of the specified name is found in <paramref name="obj"/> but the method is not suitable, an <see cref="Exception"/> is thrown.
        /// This method explicitly prohibits the use of an overloaded method.
        /// </remarks>
        private static Delegate GetDelegate(object obj, string name, Type delegate_type)
        {
            var obj_type = obj.GetType();

            MethodInfo method = null;
            try
            {
                method = obj_type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
            catch (AmbiguousMatchException e)
            {
                Throw(e, "Method {0}.{1} is overloaded; it is unsuitable.", obj_type.FullName, name);
            }
            if (method == null) return null;

            Delegate result = null;
            try
            {
                result = Delegate.CreateDelegate(delegate_type, obj, method);
            }
            catch (Exception e)
            {
                Throw(e, "Signature and/or return type of method {0}.{1} is incompatible with {2}.", obj_type.FullName, name, delegate_type.FullName);
            }

            return result;
        }

        /// <summary>
        /// Formats a message and throws an exception with it.
        /// </summary>
        /// <param name="format">Format string as in <see cref="string.Format(string, object[])"/>.</param>
        /// <param name="args">Arguments for the message.</param>
        private static void Throw(string format, params object[] args)
        {
            throw new DelegateCommandHelperException(string.Format(format, args));
        }

        /// <summary>
        /// Formats a message and trhows an exception with it with an inner exception.
        /// </summary>
        /// <param name="e">The inner exception.</param>
        /// <param name="format">Format string as in <see cref="string.Format(string, object[])"/>.</param>
        /// <param name="args">Arguments for the message.</param>
        private static void Throw(Exception e, string format, params object[] args)
        {
            throw new DelegateCommandHelperException(string.Format(format, args), e);
        }
    }

    /// <summary>
    /// An Exception to indicate the <see cref="DelegateCommandHelper.GetHelp(object)"/> has detected an error.
    /// </summary>
    public class DelegateCommandHelperException : Exception
    {
        /// <summary>
        /// Create an instance.
        /// </summary>
        public DelegateCommandHelperException() { }

        /// <summary>
        /// Create an instance with a message.
        /// </summary>
        /// <param name="message">A message.</param>
        public DelegateCommandHelperException(string message) : base(message) { }

        /// <summary>
        /// Create an instance with a message and an inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="e"></param>
        public DelegateCommandHelperException(string message, Exception e) : base(message, e) { }
    }
}
