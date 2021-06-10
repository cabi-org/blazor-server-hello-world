using System;
using Bunit;
using Cabi.KubernetesTechDemo.Web.Shared;
using Xunit;

namespace Cabi.KubernetesTechDemo.Web.Tests
{
    public class NavMenuShould
    {
        [Fact]
        public void HaveAHeaderOfTechDemo()
        {
            // Arrange: render the Counter.razor component
            using var ctx = new TestContext();
            var cut = ctx.RenderComponent<NavMenu>();

            // Act: find and click the <button> element to increment
            // the counter in the <p> element
            //cut.Find("button").Click();

            // Assert: first find the <p> element, then verify its content
            Assert.Equal("Tech Demo", cut.Find(".navbar-brand").InnerHtml);
        }
    }
}
