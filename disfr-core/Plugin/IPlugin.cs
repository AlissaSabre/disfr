using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace disfr.Plugin
{
    public interface IPlugin
    {
        string Name { get; }
    }

    public interface IReaderPlugin : IPlugin
    {
        IReader CreateReader();
    }

    public interface IWriterPlugin : IPlugin
    {
        IWriter CreateWriter();
    }
}
