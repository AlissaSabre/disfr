using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Plugin
{
    /// <summary>
    /// The base plugin interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an interface that all disfr plugins provide an implementation of.
    /// With this interface,
    /// <see cref="PluginManager"/> creates an instance of a plugin class
    /// and an instance of actual <see cref="disfr.Doc.IAssetReader"/>,
    /// for example, too.
    /// </para>
    /// <para>
    /// A plugin is often a <i>bridge</i> to an external program
    /// and requires that program to be available on the PC.
    /// There may be other circumstances that a plugin can't work property.
    /// We have two ways to handle such conditions.
    /// </para>
    /// <para>
    /// Under the older plugin API (up to disfr 0.4.0),
    /// the case was handled through throwing an <see cref="Exception"/>,
    /// either upon the instanciation of an <see cref="IPlugin"/>
    /// or upon the actual use of the plugin
    /// (e.g., through <see cref="disfr.Doc.IAssetReader.Read(string, int)"/>.)
    /// </para>
    /// <para>
    /// The newer plugin API provides <see cref="IPluginStatus"/> interface
    /// that all plugins may (and should) implement.
    /// A plugin that implements <see cref="IPluginStatus"/> may return null
    /// from <see cref="IReaderPlugin.CreateReader"/> or <see cref="IWriterPlugin.CreateWriter"/>,
    /// indicating the cause of the failure through <see cref="IPluginStatus.Status"/>.
    /// </para>
    /// </remarks>
    public interface IPlugin
    {
        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// A plugin that provides an <see cref="IReader"/>.
    /// </summary>
    public interface IReaderPlugin : IPlugin
    {
        /// <summary>
        /// Creates an <see cref="IReader"/> instance of this plugin.
        /// </summary>
        /// <returns>An instance of an <see cref="IReader"/>, such as of <see cref="disfr.Doc.IAssetReader"/>.</returns>
        IReader CreateReader();
    }

    /// <summary>
    /// A plugin that provides an <see cref="IWriter"/>.
    /// </summary>
    public interface IWriterPlugin : IPlugin
    {
        /// <summary>
        /// Creates an <see cref="IWriter"/> of this plugin.
        /// </summary>
        /// <returns>The <see cref="IWriter"/>.</returns>
        IWriter CreateWriter();
    }

    /// <summary>
    /// An optional interface that a plugin may provide.
    /// </summary>
    public interface IPluginStatus : IPlugin
    {
        /// <summary>
        /// Gets the status of this plugin.
        /// </summary>
        /// <value>
        /// A status text for end users, 
        /// or null if no specific status information is available.
        /// </value>
        /// <remarks>
        /// This is intended to tell a cause of failure upon initialization of a plugin.
        /// The returned status is assumed <i>static</i>,
        /// i.e., the returned text is always same
        /// once an <see cref="IPlugin"/> istance is created.
        /// You <i>could</i> report something upon a successful initialization,
        /// e.g., "working in 32bit" or "server not configured; falled back to local mode",
        /// but it is NOT for reporting some dynamic statuses
        /// such as memory consumption or operating performance.
        /// </remarks>
        string Status { get; }
    }
}
