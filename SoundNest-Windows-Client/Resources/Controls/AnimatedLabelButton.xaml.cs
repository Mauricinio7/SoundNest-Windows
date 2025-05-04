using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SoundNest_Windows_Client.Resources.Controls
{
    /// <summary>
    /// Lógica de interacción para AnimatedLabelButton.xaml
    /// </summary>
    public partial class AnimatedLabelButton : UserControl
    {
        public AnimatedLabelButton()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string), typeof(AnimatedLabelButton), new PropertyMetadata("Label"));

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        public static readonly DependencyProperty ForegroundColorProperty =
            DependencyProperty.Register("ForegroundColor", typeof(Brush), typeof(AnimatedLabelButton), new PropertyMetadata(Brushes.White));

        public Brush ForegroundColor
        {
            get { return (Brush)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty =
            DependencyProperty.Register("LabelFontSize", typeof(double), typeof(AnimatedLabelButton), new PropertyMetadata(16.0));

        public double LabelFontSize
        {
            get { return (double)GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(AnimatedLabelButton), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
    DependencyProperty.Register("CommandParameter", typeof(object), typeof(AnimatedLabelButton), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var animation = (Storyboard)FindResource("ClickAnimation");
            animation.Begin();

            if (ClickCommand != null && ClickCommand.CanExecute(CommandParameter))
            {
                ClickCommand.Execute(CommandParameter);
            }
        }
    }
}
