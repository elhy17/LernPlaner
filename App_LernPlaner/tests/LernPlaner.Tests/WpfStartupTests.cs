using System.Threading;
using Xunit;

namespace LernPlaner.Tests;
public class WpfStartupTests
{
    [Fact]
    public void ExistingLoginWindowAndApplicationResourcesLoadOnStaThread()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var app = new App_LernPlaner.App();
                app.InitializeComponent();
                var login = new App_LernPlaner.Views.LoginPage();
                Assert.NotNull(login.FindName("txtUsername"));
                Assert.NotNull(login.FindName("txtPassword"));
                login.Close();
                app.Shutdown();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(15)), "WPF initialization timed out.");
        Assert.Null(failure);
    }
}
