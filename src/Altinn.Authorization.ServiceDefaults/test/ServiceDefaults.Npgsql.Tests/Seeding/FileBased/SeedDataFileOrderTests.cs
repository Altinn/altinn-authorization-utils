using Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed.FileBased;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests.Seeding.FileBased;

public class SeedDataFileOrderTests
{
    [Fact]
    public void SeedDataFileOrder_OrdersCorrectly()
    {
        CheckOrder([
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.PrepareScript,
                DirectoryOrder = 0,
                FileOrder = 0,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.PrepareScript,
                DirectoryOrder = 0,
                FileOrder = 1,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.PrepareScript,
                DirectoryOrder = 1,
                FileOrder = 0,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.SeedScript,
                DirectoryOrder = 0,
                FileOrder = 0,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.SeedScript,
                DirectoryOrder = 0,
                FileOrder = 1,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.SeedScript,
                DirectoryOrder = 1,
                FileOrder = 0,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.FinalizeScript,
                DirectoryOrder = 0,
                FileOrder = 0,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.FinalizeScript,
                DirectoryOrder = 0,
                FileOrder = 1,
            },
            new SeedDataFileOrder
            {
                Type = SeedDataFileOrder.SeedDataFileType.FinalizeScript,
                DirectoryOrder = 1,
                FileOrder = 0,
            },
        ]);
    }

    private void CheckOrder(
        ReadOnlySpan<SeedDataFileOrder> orders)
    {
        for (var i = 0; i < orders.Length - 1; i++)
        {
            var current = orders[i];
            var next = orders[i + 1];

            Assert.True(current < next);
            Assert.True(next > current);
        }
    }
}
