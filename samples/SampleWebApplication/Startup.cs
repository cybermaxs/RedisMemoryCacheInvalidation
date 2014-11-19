using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SampleWebApplication.Startup))]
namespace SampleWebApplication
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
