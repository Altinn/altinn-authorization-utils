using Altinn.Authorization.ModelUtils.EnumUtils;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Tests.EnumUtils;

public class FlagsEnumModelTests
{
    [Fact]
    public void Requires_None_Variant()
    {
        Should.Throw<InvalidOperationException>(() => FlagsEnumModel.Create<InvalidMissingNone>())
            .Message.ShouldBe("Flags enum must have a default value called 'None'");
    }

    [Fact]
    public void Required_None_Variant_To_Be_Named_None()
    {
        Should.Throw<InvalidOperationException>(() => FlagsEnumModel.Create<InvalidWrongNameNone>())
            .Message.ShouldBe("Flags enum's default value must be called 'None'");
    }

    // Note: these exists because some "groups" consists of a single flag value.
    [Theory]
    [InlineData(PartyFieldIncludes.SystemUserType, "sysuser.type")]
    public void ParseOnly(PartyFieldIncludes value, string stringRepr)
    {
        var model = FlagsEnumModel.Create<PartyFieldIncludes>();

        model.TryParse(stringRepr, out var parsed).ShouldBeTrue();
        parsed.ShouldBe(value);
    }

    [Theory]
    [InlineData(PartyFieldIncludes.None, "")]
    [InlineData(PartyFieldIncludes.Party, "party")]
    [InlineData(PartyFieldIncludes.PartyUuid, "uuid")]
    [InlineData(PartyFieldIncludes.PartyId, "id")]
    [InlineData(PartyFieldIncludes.PartyType, "type")]
    [InlineData(PartyFieldIncludes.PartyDisplayName, "display-name")]
    [InlineData(PartyFieldIncludes.PartyPersonIdentifier, "person-id")]
    [InlineData(PartyFieldIncludes.PartyOrganizationIdentifier, "org-id")]
    [InlineData(PartyFieldIncludes.PartyCreatedAt, "created")]
    [InlineData(PartyFieldIncludes.PartyModifiedAt, "modified")]
    [InlineData(PartyFieldIncludes.PartyIsDeleted, "deleted")]
    [InlineData(PartyFieldIncludes.PartyVersionId, "version")]
    [InlineData(PartyFieldIncludes.Identifiers, "identifiers")]
    [InlineData(PartyFieldIncludes.PersonFirstName, "person.first-name")]
    [InlineData(PartyFieldIncludes.PersonMiddleName, "person.middle-name")]
    [InlineData(PartyFieldIncludes.PersonLastName, "person.last-name")]
    [InlineData(PartyFieldIncludes.PersonShortName, "person.short-name")]
    [InlineData(PartyFieldIncludes.PersonName, "person.name")]
    [InlineData(PartyFieldIncludes.PersonAddress, "person.address")]
    [InlineData(PartyFieldIncludes.PersonMailingAddress, "person.mailing-address")]
    [InlineData(PartyFieldIncludes.PersonDateOfBirth, "person.date-of-birth")]
    [InlineData(PartyFieldIncludes.PersonDateOfDeath, "person.date-of-death")]
    [InlineData(PartyFieldIncludes.Person, "person")]
    [InlineData(PartyFieldIncludes.OrganizationUnitStatus, "org.status")]
    [InlineData(PartyFieldIncludes.OrganizationUnitType, "org.type")]
    [InlineData(PartyFieldIncludes.OrganizationTelephoneNumber, "org.telephone")]
    [InlineData(PartyFieldIncludes.OrganizationMobileNumber, "org.mobile")]
    [InlineData(PartyFieldIncludes.OrganizationFaxNumber, "org.fax")]
    [InlineData(PartyFieldIncludes.OrganizationEmailAddress, "org.email")]
    [InlineData(PartyFieldIncludes.OrganizationInternetAddress, "org.internet")]
    [InlineData(PartyFieldIncludes.OrganizationMailingAddress, "org.mailing-address")]
    [InlineData(PartyFieldIncludes.OrganizationBusinessAddress, "org.business-address")]
    [InlineData(PartyFieldIncludes.Organization, "org")]
    [InlineData(PartyFieldIncludes.SubUnits, "org.subunits")]
    [InlineData(PartyFieldIncludes.UserId, "user.id")]
    [InlineData(PartyFieldIncludes.Username, "user.name")]
    [InlineData(PartyFieldIncludes.User, "user")]
    [InlineData(PartyFieldIncludes.SystemUser, "sysuser")]
    [InlineData(PartyFieldIncludes.PartyId | PartyFieldIncludes.PartyUuid, "uuid,id")]
    [InlineData(PartyFieldIncludes.Person | PartyFieldIncludes.Party, "party,person")]
    public void StringRoundTripping(PartyFieldIncludes value, string stringRepr)
    {
        var model = FlagsEnumModel.Create<PartyFieldIncludes>();

        model.Format(value).ShouldBe(stringRepr);
        model.TryParse(stringRepr, out var parsed).ShouldBeTrue();
        parsed.ShouldBe(value);
    }

    [Fact]
    public void Default_Uses_KebabCase()
    {
        var model = FlagsEnumModel.Create<DefaultEnum>();

        model.Items.ShouldNotBeEmpty();
        model.Items.Length.ShouldBe(4); // does not include None
        model.Items.ShouldContain(item => item.Value == DefaultEnum.FirstFlag && item.Name == "first-flag");
        model.Items.ShouldContain(item => item.Value == DefaultEnum.SecondFlag && item.Name == "second-flag");
        model.Items.ShouldContain(item => item.Value == DefaultEnum.ThirdFlag && item.Name == "third-flag");
        model.Items.ShouldContain(item => item.Value == DefaultEnum.FourthFlag && item.Name == "fourth-flag");
    }

    [Fact]
    public void With_Custom_Enum_Converter()
    {
        var model = FlagsEnumModel.Create<DefaultEnum>(JsonKnownNamingPolicy.SnakeCaseLower);

        model.Items.ShouldNotBeEmpty();
        model.Items.Length.ShouldBe(4); // does not include None
        model.Items.ShouldContain(item => item.Value == DefaultEnum.FirstFlag && item.Name == "first_flag");
        model.Items.ShouldContain(item => item.Value == DefaultEnum.SecondFlag && item.Name == "second_flag");
        model.Items.ShouldContain(item => item.Value == DefaultEnum.ThirdFlag && item.Name == "third_flag");
        model.Items.ShouldContain(item => item.Value == DefaultEnum.FourthFlag && item.Name == "fourth_flag");
    }

    [Flags]
    public enum InvalidWrongNameNone
    {
        NotNone = 0,
        FirstFlag = 1 << 0,
        SecondFlag = 1 << 1,
        ThirdFlag = 1 << 2,
        FourthFlag = 1 << 3,
    }

    [Flags]
    public enum InvalidMissingNone
    {
        FirstFlag = 1 << 0,
        SecondFlag = 1 << 1,
        ThirdFlag = 1 << 2,
        FourthFlag = 1 << 3,
    }

    [Flags]
    public enum DefaultEnum
    {
        None = 0,
        FirstFlag = 1 << 0,
        SecondFlag = 1 << 1,
        ThirdFlag = 1 << 2,
        FourthFlag = 1 << 3,
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
        /// The version ID of the party.
        /// </summary>
        [JsonStringEnumMemberName("version")]
        PartyVersionId = 1UL << 9,

        /// <summary>
        /// The UUID of the owner party, if any.
        /// </summary>
        PartyOwnerUuid = 1UL << 10,

        /// <summary>
        /// All party identifiers.
        /// </summary>
        [JsonStringEnumMemberName("identifiers")]
        Identifiers = PartyUuid | PartyId | PartyPersonIdentifier | PartyOrganizationIdentifier,

        /// <summary>
        /// All party fields.
        /// </summary>
        [JsonStringEnumMemberName("party")]
        Party = Identifiers | PartyType | PartyDisplayName | PartyCreatedAt | PartyModifiedAt | PartyIsDeleted | PartyVersionId | PartyOwnerUuid,

        /// <summary>
        /// The first name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.first-name")]
        PersonFirstName = 1UL << 11,

        /// <summary>
        /// The middle name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.middle-name")]
        PersonMiddleName = 1UL << 12,

        /// <summary>
        /// The last name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.last-name")]
        PersonLastName = 1UL << 13,

        /// <summary>
        /// The short name of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.short-name")]
        PersonShortName = 1UL << 14,

        /// <summary>
        /// All person name fields.
        /// </summary>
        [JsonStringEnumMemberName("person.name")]
        PersonName = PersonFirstName | PersonMiddleName | PersonLastName | PersonShortName,

        /// <summary>
        /// The address of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.address")]
        PersonAddress = 1UL << 15,

        /// <summary>
        /// The mailing address of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.mailing-address")]
        PersonMailingAddress = 1UL << 16,

        /// <summary>
        /// The date of birth of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.date-of-birth")]
        PersonDateOfBirth = 1UL << 17,

        /// <summary>
        /// The date of death of the person, if the party is a person.
        /// </summary>
        [JsonStringEnumMemberName("person.date-of-death")]
        PersonDateOfDeath = 1UL << 18,

        /// <summary>
        /// All person fields.
        /// </summary>
        [JsonStringEnumMemberName("person")]
        Person = PersonFirstName | PersonMiddleName | PersonLastName | PersonShortName | PersonAddress | PersonMailingAddress | PersonDateOfBirth | PersonDateOfDeath,

        /// <summary>
        /// The organization unit status, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.status")]
        OrganizationUnitStatus = 1UL << 19,

        /// <summary>
        /// The organization unit type, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.type")]
        OrganizationUnitType = 1UL << 20,

        /// <summary>
        /// The organization telephone number, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.telephone")]
        OrganizationTelephoneNumber = 1UL << 21,

        /// <summary>
        /// The organization mobile number, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.mobile")]
        OrganizationMobileNumber = 1UL << 22,

        /// <summary>
        /// The organization fax number, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.fax")]
        OrganizationFaxNumber = 1UL << 23,

        /// <summary>
        /// The organization email address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.email")]
        OrganizationEmailAddress = 1UL << 24,

        /// <summary>
        /// The organization internet address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.internet")]
        OrganizationInternetAddress = 1UL << 25,

        /// <summary>
        /// The organization mailing address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.mailing-address")]
        OrganizationMailingAddress = 1UL << 26,

        /// <summary>
        /// The organization business address, if the party is an organization.
        /// </summary>
        [JsonStringEnumMemberName("org.business-address")]
        OrganizationBusinessAddress = 1UL << 27,

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
        SystemUserType = 1UL << 28,

        /// <summary>
        /// Include subunits (if party is an organization).
        /// </summary>
        [JsonStringEnumMemberName("org.subunits")]
        SubUnits = 1UL << 29,

        /// <summary>
        /// The user id(s), if the party has an associated user.
        /// </summary>
        [JsonStringEnumMemberName("user.id")]
        UserId = 1UL << 30,

        /// <summary>
        /// The username, if the party has an associated user.
        /// </summary>
        [JsonStringEnumMemberName("user.name")]
        Username = 1UL << 31,

        /// <summary>
        /// All user fields.
        /// </summary>
        [JsonStringEnumMemberName("user")]
        User = UserId | Username,
    }
}
