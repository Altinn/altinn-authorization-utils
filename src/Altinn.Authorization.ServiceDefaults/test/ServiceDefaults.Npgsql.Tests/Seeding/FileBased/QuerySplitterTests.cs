using Altinn.Authorization.ServiceDefaults.Npgsql.Seeding.FileBased;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests.Seeding.FileBased;

public class QuerySplitterTests
{
    [Fact]
    public void SplitQueries_Empty()
    {
        CheckSplitter(
            "",
            []);
    }

    [Fact]
    public void SplitQueries_Single()
    {
        CheckSplitter(
            "SELECT * FROM table;",
            ["SELECT * FROM table"]);
    }

    [Fact]
    public void SplitQueries_Multiple()
    {
        CheckSplitter(
            """
            SELECT * FROM table1;
            SELECT * FROM table2;
            """,
            [
                "SELECT * FROM table1", 
                "SELECT * FROM table2"
            ]);
    }

    [Fact]
    public void SplitQueries_Multiline()
    {
        CheckSplitter(
            """
            SELECT * FROM table1
            WHERE foo = 'bar';
            SELECT * FROM table2
            WHERE foo = 'bar';
            """,
            [
                "SELECT * FROM table1\nWHERE foo = 'bar'",
                "SELECT * FROM table2\nWHERE foo = 'bar'"
            ]);
    }

    [Fact]
    public void SplitQueries_Multiline_WithComments()
    {
        CheckSplitter(
            """
            SELECT * FROM table1 -- line1
            WHERE foo = 'bar'; -- line2
            SELECT * FROM table2 -- line3
            WHERE foo = 'bar'; -- line4
            """,
            [
                "SELECT * FROM table1\nWHERE foo = 'bar'",
                "SELECT * FROM table2\nWHERE foo = 'bar'"
            ]);
    }

    private static void CheckSplitter(
        string query,
        List<string> expected)
    {
        List<string> actual = new(expected.Count);

        foreach (var split in new QuerySplitter(query))
        {
            actual.Add(split);
        }

        actual.Should().BeEquivalentTo(expected);
    }
}
