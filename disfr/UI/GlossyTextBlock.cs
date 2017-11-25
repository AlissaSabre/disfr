using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace disfr.UI
{
    public class GlossyTextBlock : TextBlock
    {
        static GlossyTextBlock()
        {
            InitializeGlossMap();
        }

        public GlossyTextBlock()
        {
            TextWrapping = TextWrapping.Wrap;
        }

        public GlossyString GlossyText
        {
            get { return GetValue(GlossyTextProperty) as GlossyString; }
            set { SetValue(GlossyTextProperty, value); }
        }

        public static readonly DependencyProperty GlossyTextProperty =
            DependencyProperty.Register("GlossyText", typeof(GlossyString), typeof(GlossyTextBlock), new PropertyMetadata(GlossyString.Empty, OnGlossyTextChanged));

#if TRADITIONAL

        private static readonly TextDecorationCollection INSUL = new TextDecorationCollection() { new TextDecoration() { Location = TextDecorationLocation.Underline, Pen = new Pen(Brushes.Blue, 2) } };
        private static readonly TextDecorationCollection DELST = new TextDecorationCollection() { new TextDecoration() { Location = TextDecorationLocation.Strikethrough, Pen = new Pen(Brushes.Red, 2) } };

        private static readonly Brush INS = Brushes.Blue;
        private static readonly Brush DEL = Brushes.Red;
        private static readonly Brush TAG = Brushes.White;
        private static readonly Brush SYM = Brushes.White;

        private static readonly Brush TAGBG = Brushes.Gray;
        private static readonly Brush SYMBG = Brushes.LightGreen;

        public static void OnGlossyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBlock;
            var inlines = tb.Inlines;
            inlines.Clear();
            foreach (var p in ((GlossyString)e.NewValue).AsCollection())
            {
                switch (p.Gloss)
                {
                    default:
                        inlines.Add(new Run(p.Text));
                        break;
                    case Gloss.INS:
                        inlines.Add(new Run(p.Text) { Foreground = INS, TextDecorations = INSUL });
                        break;
                    case Gloss.DEL:
                        inlines.Add(new Run(p.Text) { Foreground = DEL, TextDecorations = DELST });
                        break;
                    case Gloss.TAG:
                        inlines.Add(new Run(p.Text) { Foreground = TAG, Background = TAGBG });
                        break;
                    case Gloss.TAG | Gloss.INS:
                        inlines.Add(new Run(p.Text) { Foreground = INS, Background = TAGBG, TextDecorations = INSUL });
                        break;
                    case Gloss.TAG | Gloss.DEL:
                        inlines.Add(new Run(p.Text) { Foreground = DEL, Background = TAGBG, TextDecorations = DELST });
                        break;
                    case Gloss.SYM:
                        inlines.Add(new Run(p.Text) { Background = SYMBG });
                        break;
                    case Gloss.SYM | Gloss.INS:
                        inlines.Add(new Run(p.Text) { Foreground = INS, Background = SYMBG, TextDecorations = INSUL });
                        break;
                    case Gloss.SYM | Gloss.DEL:
                        inlines.Add(new Run(p.Text) { Foreground = DEL, Background = SYMBG, TextDecorations = DELST });
                        break;

                }
            }
        }

#else
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
        }

        private static GlossEntry[] GlossMap;

        private static void InitializeGlossMap()
        {
            var map = new GlossEntry[Enum.GetValues(typeof(Gloss)).Cast<int>().Aggregate((x, y) => x | y) + 1];

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
            map[(int)(Gloss.SYM | Gloss.COM)] = new GlossEntry(SYM, COMBG, null);
            map[(int)(Gloss.SYM | Gloss.INS)] = new GlossEntry(SYM, INSBG, null);
            map[(int)(Gloss.SYM | Gloss.DEL)] = new GlossEntry(SYM, DELBG, null);

#if !DEBUG
            // Fill unused positions with default for safety.
            for (var i = 1; i < map.Length; i++)
            {
                if (map[i] == null) map[i] = map[(int)(Gloss.NOR | Gloss.COM)];
            }
#endif
            GlossMap = map;
        }

        public static void OnGlossyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBlock;
            var inlines = tb.Inlines;
            inlines.Clear();
            var gs = e.NewValue as GlossyString;
            if (!GlossyString.IsNullOrEmpty(gs))
            {
                foreach (var p in gs.AsCollection())
                {
                    var entry = GlossMap[(int)p.Gloss];
                    inlines.Add(new Run(p.Text) { Foreground = entry.Foreground, Background = entry.Background, TextDecorations = entry.Decorations });
                }
            }
        }

#endif

    }
}
