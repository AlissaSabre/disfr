using Dragablz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace disfr.UI
{
    /// <summary>
    /// A variant <see cref="DefaultInterTabClient"/> that preserves DataContext of hosting windows.
    /// </summary>
    /// <example>
    /// &lt;dragablz:TabablzControl>
    ///     &lt;dragablz:TabablzControl.InterTabController>
    ///         &lt;dragablz:InterTabController InterTabClient="{x:Static local:InterTabClient.Instance}" />
    ///     &lt;/dragablz:TabablzControl.InterTabController>
    /// </example>
    public class InterTabClient : DefaultInterTabClient
    {
        public override INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            // We hold the only backend in DataContext on all MainWindow (host).
            // We need to copy it to the new window.
            var host = base.GetNewHost(interTabClient, partition, source);
            host.Container.DataContext = Window.GetWindow(source).DataContext;
            return host;
        }
    }
}
