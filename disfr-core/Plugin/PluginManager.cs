using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Plugin
{
    /// <summary>
    /// A base interface for classes to read a bilingual file.
    /// </summary>
    public interface IReader
    {
    }

    /// <summary>
    /// A base interface for classes to write a bilingual file out.
    /// </summary>
    public interface IWriter
    {
    }

    /// <summary>
    /// Loads all available plugins and provides them to the application.
    /// </summary>
    /// <remarks>
    /// This class has no public constructor.
    /// Use <see cref="Current"/> to get its instance.
    /// </remarks>
    public class PluginManager
    {
        private static PluginManager _Current = null;

        public static PluginManager Current { get { return _Current ?? (_Current = new PluginManager()); } }

        private PluginManager()
        {
            var reader_plugins = new List<IReader>();
            var writer_plugins = new List<IWriter>();
            var plugin_names = new List<string>();

            // I have no plan to register this program into GAC, 
            // so the plugins should be on a same folder as disfr.exe, anyway.
            var plugin_folder = Path.GetDirectoryName(this.GetType().Assembly.Location);
            foreach (var dll in Directory.EnumerateFiles(plugin_folder, "disfr.*.dll"))
            {
                try
                {
                    var version = FileVersionInfo.GetVersionInfo(dll);
                    var assembly = Assembly.LoadFile(dll);
                    foreach (var type in assembly.ExportedTypes.Where(t => typeof(IPlugin).IsAssignableFrom(t)))
                    {
                        var plugin = assembly.CreateInstance(type.ToString());
                        if (plugin is IReaderPlugin)
                        {
                            reader_plugins.Add(((IReaderPlugin)plugin).CreateReader());
                        }
                        if (plugin is IWriterPlugin)
                        {
                            writer_plugins.Add(((IWriterPlugin)plugin).CreateWriter());
                        }
                        var status = (plugin as IPluginStatus)?.Status;
                        var format = (status == null) ? "{0} {1}" : "{0} {1} - {2}";
                        plugin_names.Add(string.Format(format, ((IPlugin)plugin).Name, version.FileVersion, status));
                    }
                }
                catch (Exception e)
                {
                    // Just ignore.
                }
            }

            Readers = reader_plugins.ToArray();
            Writers = writer_plugins.ToArray();
            PluginNames = plugin_names.AsReadOnly();
        }

        public IReader[] Readers { get; private set; }

        public IWriter[] Writers { get; private set; }

        public IEnumerable<string> PluginNames { get; private set; }
    }
}
