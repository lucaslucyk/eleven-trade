using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;


namespace OutsideWorks.Helpers
{
    public class SuccessSnackBar: SnackBarOptions
    {
        public SuccessSnackBar(string message, SnackBarActionOptions[]? actions = null)
        {
            MessageOptions = new MessageOptions
            {
                Foreground = Color.White,
                Message = message,
                Padding = new Thickness(5)
            };

            BackgroundColor = Color.Green;
            Duration = TimeSpan.FromSeconds(3);
            CornerRadius = new Thickness(5);
            if (actions != null)
                Actions = actions;
        }
    }

    public class ErrorSnackBar: SnackBarOptions
    {
        public ErrorSnackBar(string message, SnackBarActionOptions[]? actions = null)
        {
            MessageOptions = new MessageOptions
            {
                Foreground = Color.White,
                Message = message,
                Padding = new Thickness(5)
            };

            BackgroundColor = Color.Red;
            Duration = TimeSpan.FromSeconds(3);
            CornerRadius = new Thickness(5);
            if (actions != null)
                Actions = actions;
        }
    }
}