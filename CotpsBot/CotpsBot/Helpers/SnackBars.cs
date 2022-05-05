using System;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Forms;


namespace CotpsBot.Helpers
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
            BackgroundColor = Color.FromHex("#35d65e");
            // BackgroundColor = Color.Green;
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
            BackgroundColor = Color.FromHex("#f45858");
            // BackgroundColor = Color.Red;
            Duration = TimeSpan.FromSeconds(3);
            CornerRadius = new Thickness(5);
            if (actions != null)
                Actions = actions;
        }
    }
    
    public class WarningSnackBar: SnackBarOptions
    {
        public WarningSnackBar(string message, SnackBarActionOptions[]? actions = null)
        {
            MessageOptions = new MessageOptions
            {
                Foreground = Color.White,
                Message = message,
                Padding = new Thickness(5)
            };
            BackgroundColor = Color.FromHex("#f45858");
            // BackgroundColor = Color.Red;
            Duration = TimeSpan.FromSeconds(3);
            CornerRadius = new Thickness(5);
            if (actions != null)
                Actions = actions;
        }
    }
}