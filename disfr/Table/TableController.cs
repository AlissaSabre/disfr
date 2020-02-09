using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using disfr.Doc;

namespace disfr.UI
{
    /// <summary>
    /// Represents a single table data and its states for presentation.
    /// </summary>
    /// <remarks>
    /// An instance of this class will be set to <see cref="System.Windows.FrameworkElement.DataContext"/> of <see cref="TableView"/>.
    /// You can think this class is something like a View Model in MVVM pattern,
    /// though it doesn't follow the MVVM doctrine.
    /// In particular this class doesn't implement <see cref="INotifyPropertyChanged"/>.
    /// (And doing so would be disasterous.)
    /// </remarks>
    public class TableController : ITableController
    {
        /// <summary>
        /// Creates an unloaded instance of TableController.
        /// </summary>
        /// <param name="renderer">A pair renderer to render the rows in this table.</param>
        /// <remarks>
        /// Use <see cref="LoadBilingualAssets"/> to create a usable instance.
        /// </remarks>
        protected TableController(PairRenderer renderer, IAssetBundle bundle)
        {
            DelegateCommandHelper.GetHelp(this);

            Renderer = renderer;
            Bundle = bundle;

            // The following are the default settings whose values are different from default(T).
            // BTW, the default settings should be user configurable. FIXME.
            TagShowing = TagShowing.Disp;
            ShowSpecials = true;

            UpdateFilter();
        }

        /// <summary>
        /// Use by UI components for their own pruposes.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// User friendly name of this table.
        /// </summary>
        public string Name { get; private set; }

        #region Rows and related properties

        private readonly RowDataCollection _Rows = new RowDataCollection();

        /// <summary>
        /// Gets filtered rows of this table.
        /// </summary>
        /// <value>
        /// A <see cref="RowDataCollection"/> object.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property is intended to be bound by an <see cref="System.Windows.Controls.ItemCollection.ItemsSource"/>.
        /// The returned object implements <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// The returned collection is filtered.
        /// That is, some rows may be excluded by settings of <see cref="ShowAll"/> and <see cref="ContentsFilter"/>.
        /// Use <see cref="AllRows"/> if you need the full set of rows.
        /// </para>
        /// </remarks>
        public IEnumerable<IRowData> Rows { get { return _Rows; } }

        /// <summary>
        /// Gets unfiltered rows of this table.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Rows"/>, this property returns unfiltered set of rows.
        /// The returned object is not suitable for data binding.
        /// </remarks>
        public IEnumerable<IRowData> AllRows { get { return _Rows.Rows; } }

        private List<AdditionalPropertiesInfo> _AdditionalProps;

        public IEnumerable<AdditionalPropertiesInfo> AdditionalProps { get { return _AdditionalProps; } }

        #endregion

        private readonly PairRenderer Renderer;

        private readonly IAssetBundle Bundle;

        /// <summary>
        /// Load a set of <see cref="IAsset"/>s from a bilingual file into a new TableController.
        /// </summary>
        /// <param name="bundle">Asset bundle that contains entries to show on this table.</param>
        /// <returns>
        /// A newly created and loaded <see cref="ITableController"/> instance.
        /// </returns>
        public static ITableController LoadBilingualAssets(IAssetBundle bundle)
        {
            var renderer = new PairRenderer();
            var instance = new TableController(renderer, bundle);
            instance.ReloadBilingualAssets();
            return instance;
        }

        private void ReloadBilingualAssets()
        {
            var assets = Bundle.Assets.ToArray();
            ReloadBilingualRowData(assets, a => a.TransPairs);
            Name = Bundle.Name;

            // Take care of Alt.
            AltLoader = null;
            AltOriginsLoader = null;
            if (assets.Any(a => a.AltPairs?.Any() == true))
            {
                AltLoader = delegate(string[] origins)
                {
                    var alt_assets = Bundle.Assets.ToArray();
                    var alt_name = string.Format("Alt TM {0}", Name);
                    var alt_bundle = new SimpleAssetBundle(alt_assets, alt_name);
                    var alt_instance = new TableController(Renderer.Clone(), alt_bundle);
                    alt_instance.ReloadBilingualRowData(alt_assets, CreateFilteredAltPairsGetter(origins));
                    alt_instance.Name = alt_name;
                    return alt_instance;
                };

                AltOriginsLoader = delegate ()
                {
                    var origins = new HashSet<string>();
                    foreach (var asset in Bundle.Assets)
                    {
                        var origin = asset.Properties.ToList().FindIndex(prop => prop.Key == "origin");
                        if (origin >= 0) 
                        {
                            origins.UnionWith(asset.AltPairs.Select(pair => pair[origin]));
                        }
                    }
                    origins.Remove(null); // XXX
                    return origins.AsEnumerable();
                };
            }
        }

        private static Func<IAsset, IEnumerable<ITransPair>> CreateFilteredAltPairsGetter(string[] origins)
        {
            if (origins == null)
            {
                return asset => asset.AltPairs;
            }
            else
            {
                return delegate (IAsset asset)
                {
                    var origin_index = asset.Properties.ToList().FindIndex(prop => prop.Key == "origin");
                    return asset.AltPairs.Where(pair => Array.IndexOf(origins, pair[origin_index]) >= 0);
                };
            }
        }

        public event EventHandler AdditionalPropsChanged;

        private void ReloadBilingualRowData(IAsset[] assets, Func<IAsset, IEnumerable<ITransPair>> get_pairs)
        {
            var props = _AdditionalProps ?? new List<AdditionalPropertiesInfo>();
            var props_indexes = new Dictionary<string, int>();
            for (int i = 0; i < props.Count; i++) props_indexes[props[i].Key] = i;
            foreach (var prop in assets.SelectMany(a => a.Properties))
            {
                int index;
                if (!props_indexes.TryGetValue(prop.Key, out index))
                {
                    index = props_indexes[prop.Key] = props.Count;
                    props.Add(new AdditionalPropertiesInfo(index, prop.Key, prop.Visible));
                }
                else if (prop.Visible && !props[index].Visible)
                {
                    props[index] = new AdditionalPropertiesInfo(index, prop.Key, prop.Visible);
                }
            }

            var rows = new List<IRowData>();
            var seq = 0;
            var serial = 0;
            foreach (var asset in assets)
            {
                var mapper = new int[props.Count];
                for (int i = 0; i < mapper.Length; i++) mapper[i] = int.MaxValue;
                for (int j = 0; j < asset.Properties.Count; j++)
                {
                    mapper[props_indexes[asset.Properties[j].Key]] = j;
                }

                var ad = new AssetData()
                {
                    LongAssetName = asset.Original,
                    BaseSerial = serial,
                    SourceLang = asset.SourceLang,
                    TargetLang = asset.TargetLang,
                    PropMapper = mapper,
                };

                foreach (var pair in get_pairs(asset))
                {
                    rows.Add(new BilingualRowData(Renderer, ad, pair, seq++));
                    if (pair.Serial > 0) serial++;
                }
            }

            _AdditionalProps = props;
            AdditionalPropsChanged?.Invoke(this, EventArgs.Empty);
            _Rows.Rows = rows;
            _Rows.Reset();
        }

        public DelegateCommand RefreshCommand { get; private set; }

        private void RefreshCommand_Execute()
        {
            Task.Run(() =>
            {
                Bundle.Refresh();
            }).ContinueWith(worker =>
            {
                if (worker.IsFaulted)
                {
                    var e = worker.Exception;
                    Dispatcher.FromThread(Thread.CurrentThread)?.BeginInvoke((Action)delegate { throw e; });
                }
                ReloadBilingualAssets();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool RefreshCommand_CanExecute()
        {
            return Bundle.CanRefresh;
        }

        /// <summary>
        /// Defines how inline tags included in the contents of this table will be presented.
        /// </summary>
        public TagShowing TagShowing
        {
            get { return Renderer.ShowTag; }
            set { Renderer.ShowTag = value; _Rows.Reset(); }
        }

        public object InlineStringRenderMode
        {
            get { return Renderer.InlineStringRenderingMode; }
        }

        /// <summary>
        /// Defines how the serial numbers of the rows are presented in this table.
        /// </summary>
        /// <value>
        /// If true, the serial numbers are local to each asset, i.e., 
        /// the first row in an asset will have the serial number 1.
        /// If false, the serial numbers are contiguous throughout the table, i.e., 
        /// the first row in an asset will have a serial number that is one larget than the last row of the previous asset.
        /// </value>
        public bool ShowLocalSerial
        {
            get { return Renderer.ShowLocalSerial; }
            set { Renderer.ShowLocalSerial = value; _Rows.Reset(); }
        }

        /// <summary>
        /// Deifnes how the asset names are presented in this table.
        /// </summary>
        /// <value>
        /// If true, an asset name is shown as a full pathname.
        /// If false, an asset name is shown as the base filename. 
        /// </value>
        public bool ShowLongAssetName
        {
            get { return Renderer.ShowLongAssetName; }
            set { Renderer.ShowLongAssetName = value;  _Rows.Reset(); }
        }

        public bool ShowSpecials
        {
            get { return Renderer.ShowSpecials; }
            set { Renderer.ShowSpecials = value;  _Rows.Reset(); }
        }

        private bool _ShowAll;

        /// <summary>
        /// Defines whether those segments not for translation are included in the table presentation.
        /// </summary>
        /// <value>
        /// If true, all segments including those not for translation are presented.
        /// If false, only segments that are for translation are presented.
        /// </value>
        public bool ShowAll
        {
            get { return _ShowAll; }
            set { _ShowAll = value; UpdateFilter(); }
        }

        private Func<IRowData, bool> _ContentsFilter;

        public Func<IRowData, bool> ContentsFilter
        {
            get { return _ContentsFilter; }
            set { _ContentsFilter = value; UpdateFilter(); }
        }

        private void UpdateFilter()
        {
            if (_ShowAll && _ContentsFilter == null)
            {
                _Rows.Filter = null;
            }
            else if (_ShowAll && _ContentsFilter != null)
            {
                _Rows.Filter = _ContentsFilter;
            }
            else if (!_ShowAll && _ContentsFilter == null)
            {
                _Rows.Filter = r => !r.Hidden;
            }
            else if (!_ShowAll && _ContentsFilter != null)
            {
                _Rows.Filter = r => !r.Hidden && _ContentsFilter(r);
            }
            _Rows.Reset();
        }

        private Func<string[], ITableController> AltLoader = null;

        public ITableController LoadAltAssets(string[] origins)
        {
            return AltLoader?.Invoke(origins);
        }

        private Func<IEnumerable<string>> AltOriginsLoader = null;

        public IEnumerable<string> AltAssetOrigins { get { return AltOriginsLoader?.Invoke(); } }

        public bool HasAltAssets { get { return AltLoader != null; } }

        /// <summary>
        /// A collection wrapper of <see cref="IRowData"/> that supports 
        /// our own filtering and <see cref="INotifyCollectionChanged"/>.
        /// </summary>
        public class RowDataCollection : IEnumerable<IRowData>, INotifyCollectionChanged
        {
            /// <summary>
            /// Implements <see cref="INotifyCollectionChanged.CollectionChanged"/>.
            /// </summary>
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            /// <summary>
            /// Notifies <see cref="INotifyCollectionChanged"/> clients that this collection is reset.
            /// </summary>
            public void Reset()
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

            /// <summary>
            /// Underlying collection.
            /// </summary>
            public List<IRowData> Rows { get; set; }

            /// <summary>
            /// Callback to filter rows to present.
            /// </summary>
            /// <value>
            /// If this is set to a delegate and it returned true for a row, the row is presented to user.
            /// If it returned false, the row is hidden.
            /// If this is set to null, all rows are presented.
            /// </value>
            public Func<IRowData, bool> Filter { get; set; }

            /// <summary>
            /// Implements <see cref="IEnumerable{T}.GetEnumerator"/>.
            /// </summary>
            /// <returns>An enumerator that enumerates the filtered set of rows.</returns>
            public IEnumerator<IRowData> GetEnumerator()
            {
                if (Filter == null)
                {
                    return Rows.GetEnumerator();
                }
                else
                {
                    return Rows.Where(Filter).GetEnumerator();
                }
            }

            /// <summary>
            /// Implements <see cref="IEnumerable.GetEnumerator"/>.
            /// </summary>
            /// <returns>An enumerator that enumerates the filtered set of rows.</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
