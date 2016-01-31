using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(XmlToJSONConverterTest.Startup))]
namespace XmlToJSONConverterTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
