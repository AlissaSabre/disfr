using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace disfr.UI
{
    /// <summary>
    /// A WPF control whose content is loaded from a separate XAML file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This control is primarily intended to replace <see cref="Image"/>
    /// with a separate vector graphic serialized to a XAML file.
    /// That is, you can write a XAML code for your window as follows:
    /// <example>
    /// &lt;Button Command="ApplicationCommands.Save"&gt;
    ///     &lt;Button.Content&gt;
    ///         &lt;a:XamlControl Source="icons/Save.xaml" xmlns:a="clr-namespace:Alissa.Wpf" /&gt;
    ///     &lt;/Button.Content&gt;
    /// &lt;/Button&gt;
    /// </example>
    /// in place of:
    /// <example>
    /// &lt;Button Command="ApplicationCommands.Save"&gt;
    ///     &lt;Button.Content&gt;
    ///         &lt;Image Source="icons/Save.png" /&gt;
    ///     &lt;/Button.Content&gt;
    /// &lt;/Button&gt;
    /// </example>
    /// </para>
    /// <para>
    /// Note that there are several options how to encode a vector graphic in a XAML file.
    /// The current implementation is tested only with the ones created by Inkscape 0.48,
    /// whose root element is always a <see cref="Viewbox"/>.
    /// </para>
    /// </remarks>
    public class XamlControl : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(XamlControl),
                new FrameworkPropertyMetadata(OnSourceChanged));

        /// <summary>
        /// Gets or sets URI of XAML that defines the <see cref="Content"/> of this control.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A non-absolute URI is deemed a resource path, i.e., relative to <code>pack://Application,,,/</code>.
        /// This applies when an <see cref="Uri"/> object is assigned to this property.
        /// If a relative URI string is specified in a XAML attribute,
        /// it is usually completed before assigned to this property, referring to its base URI.
        /// </para>
        /// <para>
        /// The acceptable URI scheme/syntax depends on the Framework implementation of <see cref="WebRequest"/>.
        /// I'm not exactly sure what URIs are supported. :-) 
        /// </para>
        /// <para>
        /// All operations in this class are synchronous.
        /// Assigning a new URI to this <see cref="Source"/> dependent property
        /// causes loading of the content of the URI synchronously.
        /// It means that, under a typical WPF scenario,
        /// the UI thread will block until XAML data from the specified URI is read into memory.
        /// So, never specify a network download URI, e.g., <code>http://</code>.
        /// Otherwise, your application will stop responding.
        /// </para>
        /// </remarks>
        public Uri Source
        {
            get { return GetValue(SourceProperty) as Uri; }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            object element = null;
            if (e.NewValue != null)
            {
                var uri = (Uri)e.NewValue;
                if (uri.IsAbsoluteUri)
                {
                    // This can be a time-consuming blocking operation depending on the URI.
                    // We should use a separate thread to do it.  XXX.
                    using (var response = WebRequest.Create(uri).GetResponse())
                    {
                        var context = new ParserContext() { BaseUri = response.ResponseUri };
                        element = XamlReader.Load(response.GetResponseStream(), context);
                    }
                }
                else
                {
                    // LoadComponent is assumed always finishing lightning fast. :)
                    element = Application.LoadComponent(uri);
                }
            }
            d.SetValue(ContentProperty, element);
        }
    }
}
