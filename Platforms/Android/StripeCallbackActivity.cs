using Android.App;
using Android.Content;
using Android.Content.PM;
using Microsoft.Maui.Authentication;

namespace NET_MAUI_App;

[Activity(NoHistory = true, Exported = true, LaunchMode = LaunchMode.SingleTop)]
[IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "rogerrestaurant", DataHost = "stripe-return")]
public class StripeCallbackActivity : WebAuthenticatorCallbackActivity
{
}
