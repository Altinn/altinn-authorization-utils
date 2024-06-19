using Altinn.Authorization.ServiceDefaults.Utils;
using System.Net;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class IPNetworkUtilsTests
{
    [Theory]
    [InlineData("10.50.0.0/16", "10.50.0.0/16")]
    [InlineData("10.50.0.15/16", "10.50.0.0/16")]
    [InlineData("1.2.3.4/32", "1.2.3.4/32")]
    [InlineData("81b9:4153:6eb3:de45:bea5:fd85:0de4:02e3/64", "81b9:4153:6eb3:de45:0000:0000:0000:0000/64")]
    [InlineData("81b9:4153:6eb3:de45:bea5:fd85:0de4:02e3/32", "81b9:4153:0000:0000:0000:0000:0000:0000/32")]
    [InlineData("81b9:4153:6eb3:de45:bea5:fd85:0de4:02e3/96", "81b9:4153:6eb3:de45:bea5:fd85:0000:0000/96")]
    [InlineData("81b9:4153:6eb3:de45:bea5:fd85:0de4:02e3/112", "81b9:4153:6eb3:de45:bea5:fd85:0de4:0000/112")]
    [InlineData("81b9:4153:6eb3:de45:bea5:fd85:0de4:02e3/128", "81b9:4153:6eb3:de45:bea5:fd85:0de4:02e3/128")]
    public void TryParse(string cidr, string result)
    {
        Assert.True(IPNetworkUtils.TryParseIPNetwork(cidr, out var ipNetwork, out var address));
        var expected = IPNetwork.Parse(result);

        ipNetwork.Should().Be(expected);
        address.Should().Be(IPAddress.Parse(cidr.Split('/')[0]));
    }
}
