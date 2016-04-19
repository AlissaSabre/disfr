using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace disfr.UI
{
    /// <summary>
    /// A <see cref="MenuItem"/> that acts like a <see cref="System.Windows.Controls.RadioButton"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This control appears to users just as a radio button accessed via a menu.
    /// It's API is somewhat different from <see cref="System.Windows.Controls.RadioButton"/>, though.
    /// This control manages the value controlled by a set of multiple <see cref="RadioMenuItem"/>
    /// through two way data binding.
    /// </para>
    /// </remarks>
    /// <example>
    /// &lt;MenuItem Header="Casing">
    ///     &lt;local:RadioMenuItem Header="Lower" Value="{x:Static CharacterCasing.Lower}" Source="{Binding CharacterCasing, ElementName=textBox1}"/>
    ///     &lt;local:RadioMenuItem Header="Upper" Value="{x:Static CharacterCasing.Upper}" Source="{Binding CharacterCasing, ElementName=textBox1}"/>
    ///     &lt;local:RadioMenuItem Header="Normal" Value="{x:Static CharacterCasing.Normal}" Source="{Binding CharacterCasing, ElementName=textBox1}"/>
    /// &lt;/MenuItem>
    /// </example>
    public class RadioMenuItem : MenuItem
    {
        private readonly RadioButton RadioButton = new RadioButton() { IsHitTestVisible = false };

        /// <summary>
        /// Creates an instance.
        /// </summary>
        public RadioMenuItem()
        {
            Icon = RadioButton;
            IsCheckable = false;
        }

        protected override void OnClick()
        {
            base.OnClick();
            if (Value != null) Source = Value;
        }

        /// <summary>
        /// Identifies <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(RadioMenuItem),
                new FrameworkPropertyMetadata(OnMyPropertiesChanged));

        /// <summary>
        /// Gets or sets the value that this <see cref="RadioMenuItem"/> represents.
        /// </summary>
        /// <value>
        /// Any value other than null.
        /// </value>
        /// <seealso cref="ValueProperty"/>
        /// <remarks>
        /// <para>
        /// The radio button on this menu item appears checked if the value of this <see cref="Value"/> equals to the value of <see cref="Source"/>.
        /// When this menu item is clicked, the source that is bound two-way to <see cref="Source"/> is set to the value of this <see cref="Value"/>.
        /// </para>
        /// <para>
        /// Note that null is not considered a valid value for <see cref="Value"/>.
        /// If set to null, nothing happens when this menu item is clicked (i.e., the source is not set to null by clicking),
        /// and its radio button never appears checked even if the value of <see cref="Source"/> is also null.
        /// </para>
        /// <para>
        /// This property is of type <see cref="Object"/>.
        /// When used in XAML, you can't rely on the ordinary attribute type conversion mechanism of XAML processor.
        /// That means you can't just write a member name of an Enumeration value.
        /// You need to use <c>{x:Static }</c> extension (or other means) to specify it.
        /// For example, if you want to specify a <see cref="Visibility"/> value,
        /// <c>&lt;local:RadioMenuItem Value="Collapsed" ... /></c> doesn't work.
        /// You need to write <c>&lt;local:RadioMenuItem Value="{x:Static Visibility.Collapsed}" ... /></c>.
        /// </para>
        /// </remarks>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(RadioMenuItem),
                new FrameworkPropertyMetadata(OnMyPropertiesChanged) { BindsTwoWayByDefault = true });

        /// <summary>
        /// Gets or sets the source value of this <see cref="RadioMenuItem"/>.
        /// </summary>
        /// <value>
        /// Any value other than null.
        /// </value>
        /// <seealso cref="SourceProperty"/>
        /// <remarks>
        /// <para>
        /// This is a dependency property that is intended to be bound two-way to another property.
        /// For the purpose, the default binding mode of this property is two-way.
        /// </para>
        /// <para>
        /// The radio button on this menu item appears checked if the value of <see cref="Value"/> equals to the value of this <see cref="Source"/>.
        /// When this menu item is clicked, the source that is bound two-way to this <see cref="Source"/> is set to the value of <see cref="Value"/>.
        /// Having multiple <see cref="RadioMenuItem"/> whose <see cref="Source"/> bound to a same property effectively makes a value selector of that property.
        /// </para>
        /// <para>
        /// Note that null is not considered a valid value for <see cref="Source"/>.
        /// If the value becomes null, this menu item's radio button portion never appears checked even if the value of <see cref="Value"/> is also null.
        /// </para>
        /// </remarks>
        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// A shared OnValuePropertyChanged and OnSourcePropertyChanged callback.
        /// </summary>
        /// <param name="d">A <see cref="RadioMenuItem"/> object.</param>
        /// <param name="e">Not used.</param>
        private static void OnMyPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var m = d as RadioMenuItem;
            m.RadioButton.IsChecked = m.Value?.Equals(m.Source);
        }
    }
}
