using Elsys_FiskeApp.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
namespace Elsys_FiskeApp.View
{
    /// <summary>
    /// Interaction logic for SingleMerdView.xaml
    /// </summary>
    public partial class SingleMerdView : UserControl
    {
        public SingleMerdView()
        {

            InitializeComponent();
        }

        private void isInputValidNumber(object sender, TextCompositionEventArgs e)
        {
            if (e.Text != "." && !int.TryParse(e.Text, out _))
            {
                e.Handled = true; // if its a character, dont let it be valid input.
            }
            else if (e.Text == ".")
            {
                if (((TextBox)sender).Text.IndexOf(e.Text) > -1) // Indexof returns -1 if no instance is found.  
                {
                    e.Handled = true; // Thus, if it comes here, there is already an instance of ., and we shouldnt allow another to be made.
                }
            }
        }
    }
}
