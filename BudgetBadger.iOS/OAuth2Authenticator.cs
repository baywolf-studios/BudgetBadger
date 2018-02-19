using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetBadger.Core.Authentication;
using BudgetBadger.Models;
using Foundation;
using SafariServices;
using UIKit;

namespace BudgetBadger.iOS
{
    public class OAuth2Authenticator : IOAuth2Authenticator
    {
        SFSafariViewController _safariViewController { get; set; }
        SFAuthenticationSession _authenticationSession { get; set; }
        NSObject _redirectNotificationObserver { get; set; }
        TaskCompletionSource<Uri> _taskCompletion { get; set; }

        public OAuth2Authenticator()
        {
        }

        public async Task<Uri> AuthenticateAsync(string url, string callbackUrlScheme)
        {
            var authorizeUrl = new NSUrl(url);
            _taskCompletion = new TaskCompletionSource<Uri>();
            if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0))
            {
                _authenticationSession = new SFAuthenticationSession(authorizeUrl,
                                                                    callbackUrlScheme,
                                                                    (callbackUrl, error) =>
                {
                    if (error != null)
                    {
                        _taskCompletion.SetResult(new Uri(callbackUrl.AbsoluteString));
                    }
                    else
                    {
                        _taskCompletion.SetException(new Exception(error.Description));
                    }
                });

                _authenticationSession.Start();
                return await _taskCompletion.Task;
            }
            else
            {
                _safariViewController = new SFSafariViewController(authorizeUrl);
                _safariViewController.Delegate = new AuthenticatorSFSafariViewControllerDelegate(_taskCompletion);
                // show safari controller
                var vc = GetVisibleViewController();

                if (_safariViewController.PopoverPresentationController != null)
                {
                    _safariViewController.PopoverPresentationController.SourceView = vc.View;
                }

                AuthMessenger.Subscribe += AuthMessenger_Subscribe;
                await vc.PresentViewControllerAsync(_safariViewController, true);

                Uri result;
                try
                {
                    result = await _taskCompletion.Task;
                }
                finally
                {
                    AuthMessenger.Subscribe -= AuthMessenger_Subscribe;
                }

                return result;
            }
        }

        void AuthMessenger_Subscribe(object sender, Uri authUrl)
        {
            _safariViewController?.DismissViewController(true, null);
            if (authUrl == null)
            {
                _taskCompletion.SetCanceled();
            }
            else
            {
                _taskCompletion.SetResult(authUrl);
            }
        }

        UIViewController GetVisibleViewController()
        {
            UIViewController viewController = null;
            var window = UIApplication.SharedApplication.KeyWindow;


            if (window != null && window.WindowLevel == UIWindowLevel.Normal)
                viewController = window.RootViewController;

            if (viewController == null)
            {
                window = UIApplication.SharedApplication.Windows.OrderByDescending(w => w.WindowLevel).FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);
                if (window == null)
                {
                    throw new InvalidOperationException("Could not find current view controller");
                }
                else
                {
                    viewController = window.RootViewController;
                }
            }

            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }


            return viewController;
        }
    }

    public class AuthenticatorSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
    {
        readonly TaskCompletionSource<Uri> _taskCompletionSource;

        public AuthenticatorSFSafariViewControllerDelegate(TaskCompletionSource<Uri> taskCompletionSource)
        {
            _taskCompletionSource = taskCompletionSource;
        }

        public override void DidFinish(SFSafariViewController controller)
        {
            controller?.DismissViewController(true, null);
            _taskCompletionSource.SetCanceled();
        }
    }
}
