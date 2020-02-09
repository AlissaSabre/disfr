using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace disfr.UI
{
    public class GlossyTextBlock : FlowDocumentScrollViewer
    {
        static GlossyTextBlock()
        {
            InitializeGlossMap();
        }

        public GlossyTextBlock()
        {
            IsHitTestVisible = false;

            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            Paragraph = new Paragraph();
            Document = new FlowDocument(Paragraph);
            Document.PagePadding = new Thickness(0);
            Document.SetBinding(FlowDocument.FontFamilyProperty, new Binding() { Source = this, Path = new PropertyPath(FontFamilyProperty), Mode = BindingMode.OneWay });
            Document.SetBinding(FlowDocument.FontSizeProperty, new Binding() { Source = this, Path = new PropertyPath(FontSizeProperty), Mode = BindingMode.OneWay });
            Document.SetBinding(FlowDocument.TextAlignmentProperty, new Binding() { Source = this, Path = new PropertyPath(FlowDirectionProperty), Mode = BindingMode.OneWay, Converter = new FlowDirectionToTextAlignmentConverter(), ConverterParameter = "Leading" });
        }

        private readonly Paragraph Paragraph;

        public GlossyString GlossyText
        {
            get { return GetValue(GlossyTextProperty) as GlossyString; }
            set { SetValue(GlossyTextProperty, value); }
        }

        public static readonly DependencyProperty GlossyTextProperty =
            DependencyProperty.Register("GlossyText", typeof(GlossyString), typeof(GlossyTextBlock), new PropertyMetadata(GlossyString.Empty, OnGlossyTextChanged));

        private class GlossEntry
        {
            public Brush Foreground;
            public Brush Background;
            public TextDecorationCollection Decorations;

            private static TextDecorationCollection None = new TextDecorationCollection();

            public GlossEntry(Brush foreground, Brush background, TextDecorationCollection decorations)
            {
                Foreground = foreground;
                Background = background;
                Decorations = decorations ?? None;
            }

            public static readonly GlossEntry IGNORE = new GlossEntry(null, null, null);

            public static readonly GlossEntry ERROR = new GlossEntry(Brushes.Green, Brushes.Red, null);
        }


        private static GlossEntry[] GlossMap;

        private static void InitializeGlossMap()
        {
            var map = new GlossEntry[Enum.GetValues(typeof(Gloss)).Cast<int>().Aggregate((x, y) => x | y) + 1];

            // This is primarity for debugging.
            for (var i = 0; i < map.Length; i++)
            {
                map[i] = GlossEntry.ERROR;
            }

            Brush NOR = Brushes.Black;
            Brush TAG = Brushes.Purple;
            Brush SYM = Brushes.Gray;

            Brush COMBG = Brushes.Transparent;
            Brush INSBG = Brushes.LightBlue;
            Brush DELBG = Brushes.LightPink;

            map[(int)(Gloss.NOR | Gloss.COM)] = new GlossEntry(NOR, COMBG, null);
            map[(int)(Gloss.NOR | Gloss.INS)] = new GlossEntry(NOR, INSBG, null);
            map[(int)(Gloss.NOR | Gloss.DEL)] = new GlossEntry(NOR, DELBG, null);
            map[(int)(Gloss.TAG | Gloss.COM)] = new GlossEntry(TAG, COMBG, null);
            map[(int)(Gloss.TAG | Gloss.INS)] = new GlossEntry(TAG, INSBG, null);
            map[(int)(Gloss.TAG | Gloss.DEL)] = new GlossEntry(TAG, DELBG, null);
            map[(int)(Gloss.SYM | Gloss.COM)] = GlossEntry.IGNORE;
            map[(int)(Gloss.SYM | Gloss.INS)] = GlossEntry.IGNORE;
            map[(int)(Gloss.SYM | Gloss.DEL)] = GlossEntry.IGNORE;
            map[(int)(Gloss.ALT | Gloss.COM)] = new GlossEntry(SYM, COMBG, null);
            map[(int)(Gloss.ALT | Gloss.INS)] = new GlossEntry(SYM, INSBG, null);
            map[(int)(Gloss.ALT | Gloss.DEL)] = new GlossEntry(SYM, DELBG, null);

            GlossMap = map;
        }

        /// <summary>
        /// Given a Pair in a GlossyString, returns a corresponding Run.
        /// </summary>
        /// <param name="pair">A pair.</param>
        /// <returns>A newly created Run.</returns>
        /// <see cref="GlossyString"/>
        /// <see cref="Run"/>
        public static Run GlossyPairToRun(GlossyString.Pair pair)
        {
            var entry = GlossMap[(int)pair.Gloss];
            if (entry == GlossEntry.IGNORE)
            {
                return null;
            }
            else
            {
                return new Run(pair.Text) { Foreground = entry.Foreground, Background = entry.Background, TextDecorations = entry.Decorations };
            }
        }

        public static void OnGlossyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as GlossyTextBlock;
            var inlines = control.Paragraph.Inlines;
            inlines.Clear();
            var gs = e.NewValue as GlossyString;
            if (!GlossyString.IsNullOrEmpty(gs))
            {
                foreach (var run in gs.AsCollection().Select(GlossyPairToRun).Where(r => r != null))
                {
                    inlines.Add(run);
                }
            }
        }
    }

    /// <summary>
    /// Provides a flow direction neutral way to specify left/right text alignment. 
    /// </summary>
    /// <remarks>
    /// A conversion parameter is mandatory, which is either "Leading" or "Trailing".
    /// If it is "Leading", the text alignment is to the side that the text starts flowing from.
    /// If it is "Trailing", the text alignment is to the side that the text flowing toward.
    /// </remarks>
    /// <example>
    /// &lt;TextBox TextAlignment="{Binding FlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}, ConverterParameter=Trailing}" /&gt; 
    /// </example>
    [ValueConversion(typeof(FlowDirection), typeof(TextAlignment))]
    public class FlowDirectionToTextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FlowDirection)
            {
                var direction = (FlowDirection)value;
                switch (parameter as string)
                {
                    case "Leading":
                        return (direction == FlowDirection.LeftToRight) ? TextAlignment.Left : TextAlignment.Right;
                    case "Trailing":
                        return (direction == FlowDirection.LeftToRight) ? TextAlignment.Right : TextAlignment.Left;
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(string.Format("{0} supports OneWay conversion only", GetType()));
        }
    }
}
