using Altinn.Authorization.ModelUtils.AspNet;
using Altinn.Authorization.ModelUtils.EnumUtils;
using Altinn.Authorization.ModelUtils.Tests.Utils;
using Altinn.Authorization.TestUtils.AspNetCore;
using Altinn.Authorization.TestUtils.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Altinn.Authorization.ModelUtils.Tests.AspNet;

public class FlagsEnumTests
{
    private static FlagsEnumModel<PartyFieldIncludes> _model = FlagsEnumModel.Create<PartyFieldIncludes>();

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void Equals_Self(PartyFieldIncludes value)
    {
        FlagsEnum<PartyFieldIncludes> sut = value;
        sut.Equals(sut).ShouldBeTrue();
        sut.Equals(value).ShouldBeTrue();
        sut.GetHashCode().ShouldBe(value.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void Value(PartyFieldIncludes value)
    {
        FlagsEnum<PartyFieldIncludes> sut = value;
        sut.Value.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void RoundTrip(PartyFieldIncludes value)
    {
        FlagsEnum<PartyFieldIncludes> sut = value;
        
        var formatted = sut.ToString();
        formatted.ShouldBe(_model.Format(value));

        var parsed = FlagsEnum<PartyFieldIncludes>.Parse(formatted);
        parsed.Value.ShouldBe(value);

        parsed = FlagsEnum<PartyFieldIncludes>.Parse(formatted.AsSpan());
        parsed.Value.ShouldBe(value);

        FlagsEnum<PartyFieldIncludes>.TryParse(formatted, out parsed).ShouldBeTrue();
        parsed.Value.ShouldBe(value);

        FlagsEnum<PartyFieldIncludes>.TryParse(formatted.AsSpan(), out parsed).ShouldBeTrue();
        parsed.Value.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void JsonDeserialize_FromString(PartyFieldIncludes value)
    {
        FlagsEnum<PartyFieldIncludes> sut = value;

        var formatted = sut.ToString();
        var json = Json.SerializeToString(formatted);

        var parsed = Json.Deserialize<FlagsEnum<PartyFieldIncludes>>(json);
        parsed.Value.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public void JsonRoundTrip_AsArray(PartyFieldIncludes value)
    {
        FlagsEnum<PartyFieldIncludes> sut = value;

        using var doc = Json.SerializeToDocument(sut);
        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.Array);

        var parsed = Json.Deserialize<FlagsEnum<PartyFieldIncludes>>(doc);
        parsed.Value.ShouldBe(value);
    }

    [Theory]
    [MemberData(nameof(AllVariants))]
    public async Task QueryString_Single(PartyFieldIncludes value)
    {
        FlagsEnum<PartyFieldIncludes> sut = value;
        var formatted = sut.ToString();

        await using var client = await CreateClientAsync();
        using var response = await client.GetAsync($"/test/query?value={Uri.EscapeDataString(formatted)}", TestContext.Current.CancellationToken);

        await response.ShouldHaveSuccessStatusCode();
        var resturned = await response.ShouldHaveJsonContent<FlagsEnum<PartyFieldIncludes>>();
        resturned.Value.ShouldBe(value);
    }

    [Fact]
    public async Task QueryString_Multiple()
    {
        await using var client = await CreateClientAsync();
        using var response = await client.GetAsync($"/test/query?value=party&value=person&value=org.type", TestContext.Current.CancellationToken);

        await response.ShouldHaveSuccessStatusCode();
        var resturned = await response.ShouldHaveJsonContent<FlagsEnum<PartyFieldIncludes>>();
        resturned.Value.ShouldBe(PartyFieldIncludes.Party | PartyFieldIncludes.Person | PartyFieldIncludes.OrganizationUnitType);
    }

    public static TheoryData<PartyFieldIncludes> AllVariants => GetAllVariants();

    private static TheoryData<PartyFieldIncludes> GetAllVariants()
    {
        var set = new HashSet<PartyFieldIncludes>(Enum.GetValues<PartyFieldIncludes>());
        var all = default(PartyFieldIncludes);
        foreach (var flag in set)
        {
            all |= flag;
        }

        set.Add(all);

        return [.. set];
    }

    private Task<TestClient> CreateClientAsync()
    {
        return TestClient.CreateControllerClient<TestController>(
            configureMvc: builder =>
            {
                builder.AddAuthorizationModelUtilsBinders();
            });
    }

    [ApiController]
    [Route("test")]
    private class TestController
        : ControllerBase
    {
        [HttpGet("query")]
        public ActionResult<FlagsEnum<PartyFieldIncludes>> Get([FromQuery] FlagsEnum<PartyFieldIncludes> value)
        {
            return value;
        }
    }

    /// <summary>
    /// Fields to include when fetching a <see cref="PartyRecord"/>.
    /// </summary>
    [Flags]
    [StringEnumConverter]
    public enum PartyFieldIncludes
        : ulong
    {
        /// <summary>
        /// No extra information (default).
        /// </summary>
        [JsonStringEnumMemberName("none")]
        None = 0UL,

        /// <summary>
        /// The party UUID.
        /// </summary>
        [JsonStringEnumMemberName("uuid")]
        PartyUuid = 1UL << 0,

        /// <summary>
        /// The party ID.
        /// </summary>
        [JsonStringEnumMemberName("id")]
        PartyId = 1UL << 1,

        /// <summary>
        /// The party type.
        /// </summary>
        [JsonStringEnumMemberName("type")]
        PartyType = 1UL << 2,

        /// <summary>
        /// The party display-name.
        /// </summary>
        [JsonStringEnumMemberName("display-name")]
        PartyDisplayName = 1UL << 3,

        /// <summary>
        /// The person identifier of the party, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person-id")]
        PartyPersonIdentifier = 1UL << 4,

        /// <summary>
        /// The organization identifier of the party, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org-id")]
        PartyOrganizationIdentifier = 1UL << 5,

        /// <summary>
        /// The time when the party was created.
        /// </summary>
        [JsonStringEnumMemberName("created")]
        PartyCreatedAt = 1UL << 6,

        /// <summary>
        /// The time when the party was last modified.
        /// </summary>
        [JsonStringEnumMemberName("modified")]
        PartyModifiedAt = 1UL << 7,

        /// <summary>
        /// Whether the party is deleted.
        /// </summary>
        [JsonStringEnumMemberName("deleted")]
        PartyIsDeleted = 1UL << 8,

        /// <summary>
        /// When the party was deleted, if it is deleted.
        /// </summary>
        [JsonStringEnumMemberName("deleted-at")]
        PartyDeletedAt = 1UL << 9,

        /// <summary>
        /// The version ID of the party.
        /// </summary>
        [JsonStringEnumMemberName("version")]
        PartyVersionId = 1UL << 10,

        /// <summary>
        /// The UUID of the owner party, if any.
        /// </summary>
        [JsonStringEnumMemberName("owner-uuid")]
        PartyOwnerUuid = 1UL << 11,

        /// <summary>
        /// All party identifiers.
        /// </summary>
        [JsonStringEnumMemberName("identifiers")]
        Identifiers = PartyUuid | PartyId | PartyPersonIdentifier | PartyOrganizationIdentifier,

        /// <summary>
        /// All party fields.
        /// </summary>
        [JsonStringEnumMemberName("party")]
        Party = Identifiers | PartyType | PartyDisplayName | PartyCreatedAt | PartyModifiedAt | PartyIsDeleted | PartyDeletedAt | PartyVersionId | PartyOwnerUuid,

        /// <summary>
        /// The first name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.first-name")]
        PersonFirstName = 1UL << 12,

        /// <summary>
        /// The middle name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.middle-name")]
        PersonMiddleName = 1UL << 13,

        /// <summary>
        /// The last name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.last-name")]
        PersonLastName = 1UL << 14,

        /// <summary>
        /// The short name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.short-name")]
        PersonShortName = 1UL << 15,

        /// <summary>
        /// All person name fields.
        /// </summary>
        [JsonStringEnumMemberName("person.name")]
        PersonName = PersonFirstName | PersonMiddleName | PersonLastName | PersonShortName,

        /// <summary>
        /// The address of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.address")]
        PersonAddress = 1UL << 16,

        /// <summary>
        /// The mailing address of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.mailing-address")]
        PersonMailingAddress = 1UL << 17,

        /// <summary>
        /// The date of birth of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.date-of-birth")]
        PersonDateOfBirth = 1UL << 18,

        /// <summary>
        /// The date of death of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.date-of-death")]
        PersonDateOfDeath = 1UL << 19,

        /// <summary>
        /// All person fields.
        /// </summary>
        [JsonStringEnumMemberName("person")]
        Person = PersonFirstName | PersonMiddleName | PersonLastName | PersonShortName | PersonAddress | PersonMailingAddress | PersonDateOfBirth | PersonDateOfDeath,

        /// <summary>
        /// The organization unit status, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.status")]
        OrganizationUnitStatus = 1UL << 20,

        /// <summary>
        /// The organization unit type, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.type")]
        OrganizationUnitType = 1UL << 21,

        /// <summary>
        /// The organization telephone number, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.telephone")]
        OrganizationTelephoneNumber = 1UL << 22,

        /// <summary>
        /// The organization mobile number, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.mobile")]
        OrganizationMobileNumber = 1UL << 23,

        /// <summary>
        /// The organization fax number, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.fax")]
        OrganizationFaxNumber = 1UL << 24,

        /// <summary>
        /// The organization email address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.email")]
        OrganizationEmailAddress = 1UL << 25,

        /// <summary>
        /// The organization internet address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.internet")]
        OrganizationInternetAddress = 1UL << 26,

        /// <summary>
        /// The organization mailing address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.mailing-address")]
        OrganizationMailingAddress = 1UL << 27,

        /// <summary>
        /// The organization business address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.business-address")]
        OrganizationBusinessAddress = 1UL << 28,

        /// <summary>
        /// All organization fields.
        /// </summary>
        [JsonStringEnumMemberName("org")]
        Organization = OrganizationUnitStatus | OrganizationUnitType | OrganizationTelephoneNumber | OrganizationMobileNumber | OrganizationFaxNumber
            | OrganizationEmailAddress | OrganizationInternetAddress | OrganizationMailingAddress | OrganizationBusinessAddress,

        /// <summary>
        /// All system user fields.
        /// </summary>
        /// <remarks>
        /// This needs to be before <see cref="SystemUserType"/>, as long as <see cref="SystemUser"/> only consists of a single flag value.
        /// This is to get the correct to-string representation, used in queries and tests.
        /// </remarks>
        [JsonStringEnumMemberName("sysuser")]
        SystemUser = SystemUserType,

        /// <summary>
        /// The system user type, if the party is a system user.
        /// </summary>
        [JsonStringEnumMemberName("sysuser.type")]
        SystemUserType = 1UL << 29,

        /// <summary>
        /// Include subunits (if party is an organization).
        /// </summary>
        [JsonStringEnumMemberName("org.subunits")]
        SubUnits = 1UL << 30,

        /// <summary>
        /// The user id(s), if the party has an associated user.
        /// </summary>
        [JsonStringEnumMemberName("user.id")]
        UserId = 1UL << 31,

        /// <summary>
        /// The username, if the party has an associated user.
        /// </summary>
        [JsonStringEnumMemberName("user.name")]
        Username = 1UL << 32,

        /// <summary>
        /// All user fields.
        /// </summary>
        [JsonStringEnumMemberName("user")]
        User = UserId | Username,
    }
}
