using CommunityToolkit.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Models;

/// <summary>
/// Polymorphic field value record models.
/// </summary>
[ExcludeFromCodeCoverage]
public static class PolymorphicFieldValueRecords
{
    /// <summary>
    /// Variant types for the <see cref="Base"/> polymorphic field value record model.
    /// </summary>
    [StringEnumConverter(JsonKnownNamingPolicy.KebabCaseLower)]
    public enum VariantType
    {
        /// <summary>
        /// Variant for the left child of the base record.
        /// </summary>
        LeftChild,

        /// <summary>
        /// Variant for the right child of the base record.
        /// </summary>
        RightChild1,

        /// <summary>
        /// Second variant for the right child of the base record.
        /// </summary>
        RightChild2,

        /// <summary>
        /// Variant for the right grandchild of the base record.
        /// </summary>
        RightGrandChild,

        /// <summary>
        /// Variant for the concrete left child of the base record.
        /// </summary>
        ConcreteLeft,

        /// <summary>
        /// Variant for the concrete right child of the base record.
        /// </summary>
        ConcreteRight,
    }

    /// <summary>
    /// Base polymorphic field value record model.
    /// </summary>
    [PolymorphicFieldValueRecord(IsRoot = true)]
    [PolymorphicDerivedType(typeof(LeftChild), VariantType.LeftChild)]
    [PolymorphicDerivedType(typeof(RightChild), VariantType.RightChild1)]
    [PolymorphicDerivedType(typeof(RightChild), VariantType.RightChild2)]
    [PolymorphicDerivedType(typeof(RightGrandChild), VariantType.RightGrandChild)]
    [PolymorphicDerivedType(typeof(ConcreteLeft), VariantType.ConcreteLeft)]
    [PolymorphicDerivedType(typeof(ConcreteRight), VariantType.ConcreteRight)]
    public record Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Base"/> class.
        /// </summary>
        public Base(NonExhaustiveEnum<VariantType> type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets the type of the variant.
        /// </summary>
        [PolymorphicDiscriminatorProperty]
        public NonExhaustiveEnum<VariantType> Type { get; }

        /// <summary>
        /// A required property.
        /// </summary>
        public required string RequiredBaseProperty { get; init; }

        /// <summary>
        /// An optional property.
        /// </summary>
        public required FieldValue<string> OptionalBaseProperty { get; init; }
    }

    /// <summary>
    /// Left child of <see cref="Base"/>.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public record LeftChild()
        : Base(VariantType.LeftChild)
    {
        /// <summary>
        /// A required property for the left child.
        /// </summary>
        public required string RequiredLeftChildProperty { get; init; }

        /// <summary>
        /// An optional property for the left child.
        /// </summary>
        public required FieldValue<string> OptionalLeftChildProperty { get; init; }
    }

    /// <summary>
    /// Right child of <see cref="Base"/>.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public record RightChild
        : Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RightChild"/> class.
        /// </summary>
        public RightChild(NonExhaustiveEnum<VariantType> type)
            : base(type)
        {
        }

        /// <summary>
        /// Required property for the right child.
        /// </summary>
        public required string RequiredRightChildProperty { get; init; }

        /// <summary>
        /// Optional property for the right child.
        /// </summary>
        public required FieldValue<string> OptionalRightChildProperty { get; init; }
    }

    /// <summary>
    /// Grandchild of <see cref="RightChild"/>.
    /// </summary>
    /// <param name="type"></param>
    [PolymorphicFieldValueRecord]
    public record RightGrandChild(NonExhaustiveEnum<VariantType> type)
        : RightChild(type)
    {
        /// <summary>
        /// A required property for the right grandchild.
        /// </summary>
        public required string RequiredRightGrandChildProperty { get; init; }

        /// <summary>
        /// An optional property for the right grandchild.
        /// </summary>
        public required FieldValue<string> OptionalRightGrandChildProperty { get; init; }
    }

    /// <summary>
    /// An abstract middle in <see cref="Base"/>'s hierarchy.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public abstract record AbstractMiddle(NonExhaustiveEnum<VariantType> type)
        : Base(type)
    {
        /// <summary>
        /// A required abstract property for the middle record.
        /// </summary>
        public required string RequiredAbstractMiddleProperty { get; init; }

        /// <summary>
        /// An optional abstract property for the middle record.
        /// </summary>
        public required FieldValue<string> OptionalAbstractMiddleProperty { get; init; }
    }

    /// <summary>
    /// A concrete left child of <see cref="AbstractMiddle"/>.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public record ConcreteLeft()
        : AbstractMiddle(VariantType.ConcreteLeft)
    {
        /// <summary>
        /// A required property for the concrete left child.
        /// </summary>
        public required string RequiredConcreteLeftProperty { get; init; }

        /// <summary>
        /// An optional property for the concrete left child.
        /// </summary>
        public required FieldValue<string> OptionalConcreteLeftProperty { get; init; }
    }

    /// <summary>
    /// A concrete right child of <see cref="AbstractMiddle"/>.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public record ConcreteRight()
        : AbstractMiddle(VariantType.ConcreteRight)
    {
        /// <summary>
        /// A required property for the concrete right child.
        /// </summary>
        public required string RequiredConcreteRightProperty { get; init; }

        /// <summary>
        /// An optional property for the concrete right child.
        /// </summary>
        public required FieldValue<string> OptionalConcreteRightProperty { get; init; }
    }

    /// <summary>
    /// Represents a party type.
    /// </summary>
    [StringEnumConverter]
    public enum PartyType
    {
        /// <summary>
        /// Person party type.
        /// </summary>
        [JsonStringEnumMemberName("person")]
        Person = 1,

        /// <summary>
        /// Organization party type.
        /// </summary>
        [JsonStringEnumMemberName("organization")]
        Organization,

        /// <summary>
        /// Self-identified user party type.
        /// </summary>
        [JsonStringEnumMemberName("self-identified-user")]
        SelfIdentifiedUser,
    }

    /// <summary>
    /// A database record for a party.
    /// </summary>
    [PolymorphicFieldValueRecord(IsRoot = true)]
    [PolymorphicDerivedType(typeof(PersonRecord), PolymorphicFieldValueRecords.PartyType.Person)]
    [PolymorphicDerivedType(typeof(OrganizationRecord), PolymorphicFieldValueRecords.PartyType.Organization)]
    [PolymorphicDerivedType(typeof(SelfIdentifiedUserRecord), PolymorphicFieldValueRecords.PartyType.SelfIdentifiedUser)]
    public record PartyRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PartyRecord"/> class.
        /// </summary>
        [JsonConstructor]
        public PartyRecord(FieldValue<NonExhaustiveEnum<PartyType>> partyType)
        {
            PartyType = partyType;
        }

        public PartyRecord(NonExhaustiveEnum<PartyType> partyType)
            : this(FieldValue.Create(partyType))
        {
        }

        /// <summary>
        /// Gets the UUID of the party.
        /// </summary>
        public required FieldValue<Guid> PartyUuid { get; init; }

        /// <summary>
        /// Gets the ID of the party.
        /// </summary>
        public required FieldValue<uint> PartyId { get; init; }

        /// <summary>
        /// Gets the type of the party.
        /// </summary>
        [PolymorphicDiscriminatorProperty]
        public FieldValue<NonExhaustiveEnum<PartyType>> PartyType { get; private init; }

        /// <summary>
        /// Gets the display-name of the party.
        /// </summary>
        public required FieldValue<string> DisplayName { get; init; }

        /// <summary>
        /// Gets the person identifier of the party, or <see langword="null"/> if the party is not a person.
        /// </summary>
        public required FieldValue<string> PersonIdentifier { get; init; }

        /// <summary>
        /// Gets the organization identifier of the party, or <see langword="null"/> if the party is not an organization.
        /// </summary>
        public required FieldValue<string> OrganizationIdentifier { get; init; }

        /// <summary>
        /// Gets when the party was created in Altinn 3.
        /// </summary>
        public required FieldValue<DateTimeOffset> CreatedAt { get; init; }

        /// <summary>
        /// Gets when the party was last modified in Altinn 3.
        /// </summary>
        public required FieldValue<DateTimeOffset> ModifiedAt { get; init; }

        /// <summary>
        /// Gets user information for the party.
        /// </summary>
        public required FieldValue<PartyUserRecord> User { get; init; }

        /// <summary>
        /// Gets whether the party is deleted.
        /// </summary>
        public required FieldValue<bool> IsDeleted { get; init; }

        /// <summary>
        /// Gets the version ID of the party.
        /// </summary>
        public required FieldValue<ulong> VersionId { get; init; }
    }

    /// <summary>
    /// Represents a record for user-info of a party, with optional historical records.
    /// </summary>
    [FieldValueRecord]
    public sealed record PartyUserRecord
    {
        private readonly FieldValue<ImmutableValueArray<uint>> _userIds;

        /// <summary>
        /// Gets the user id.
        /// </summary>
        public FieldValue<uint> UserId
            => UserIds.Select(static ids => ids[0]);

        /// <summary>
        /// Gets the username of the party (if any).
        /// </summary>
        public FieldValue<string> Username { get; init; }

        /// <summary>
        /// Gets the (historical) user ids of the party.
        /// </summary>
        /// <remarks>
        /// Should never be empty, but can be null or unset. The first user id is the current one - the rest should be ordered in descending order.
        /// </remarks>
        public required FieldValue<ImmutableValueArray<uint>> UserIds
        {
            get => _userIds;
            init
            {
                if (value.HasValue)
                {
                    Guard.IsNotEmpty(value.Value.AsSpan(), nameof(value));
                }

                _userIds = value;
            }
        }
    }

    /// <summary>
    /// A database record for a person.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public sealed record PersonRecord
        : PartyRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRecord"/> class.
        /// </summary>
        public PersonRecord()
            : base(PolymorphicFieldValueRecords.PartyType.Person)
        {
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        public required FieldValue<string> FirstName { get; init; }

        /// <summary>
        /// Gets the (optional) middle name.
        /// </summary>
        public required FieldValue<string> MiddleName { get; init; }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        public required FieldValue<string> LastName { get; init; }

        /// <summary>
        /// Gets the short name.
        /// </summary>
        public required FieldValue<string> ShortName { get; init; }

        /// <summary>
        /// Gets the (optional) <see cref="StreetAddress"/> of the person.
        /// </summary>
        public required FieldValue<StreetAddress> Address { get; init; }

        /// <summary>
        /// Gets the (optional) <see cref="Parties.MailingAddress"/> of the person.
        /// </summary>
        public required FieldValue<MailingAddress> MailingAddress { get; init; }

        /// <summary>
        /// Gets the date of birth of the person.
        /// </summary>
        public required FieldValue<DateOnly> DateOfBirth { get; init; }

        /// <summary>
        /// Gets the (optional) date of death of the person.
        /// </summary>
        public required FieldValue<DateOnly> DateOfDeath { get; init; }
    }

    /// <summary>
    /// A database record for an organization.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public sealed record OrganizationRecord
        : PartyRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationRecord"/> class.
        /// </summary>
        public OrganizationRecord()
            : base(PolymorphicFieldValueRecords.PartyType.Organization)
        {
        }

        /// <summary>
        /// Gets the status of the organization.
        /// </summary>
        public required FieldValue<string> UnitStatus { get; init; }

        /// <summary>
        /// Gets the type of the organization.
        /// </summary>
        public required FieldValue<string> UnitType { get; init; }

        /// <summary>
        /// Gets the telephone number of the organization.
        /// </summary>
        public required FieldValue<string> TelephoneNumber { get; init; }

        /// <summary>
        /// Gets the mobile number of the organization.
        /// </summary>
        public required FieldValue<string> MobileNumber { get; init; }

        /// <summary>
        /// Gets the fax number of the organization.
        /// </summary>
        public required FieldValue<string> FaxNumber { get; init; }

        /// <summary>
        /// Gets the email address of the organization.
        /// </summary>
        public required FieldValue<string> EmailAddress { get; init; }

        /// <summary>
        /// Gets the internet address of the organization.
        /// </summary>
        public required FieldValue<string> InternetAddress { get; init; }

        /// <summary>
        /// Gets the mailing address of the organization.
        /// </summary>
        public required FieldValue<MailingAddress> MailingAddress { get; init; }

        /// <summary>
        /// Gets the business address of the organization.
        /// </summary>
        public required FieldValue<MailingAddress> BusinessAddress { get; init; }

        /// <summary>
        /// Gets the parent organization of the organization (if any).
        /// </summary>
        [JsonIgnore]
        public FieldValue<Guid> ParentOrganizationUuid { get; init; }
    }

    /// <summary>
    /// A database record for a self-identified user.
    /// </summary>
    [PolymorphicFieldValueRecord]
    public sealed record SelfIdentifiedUserRecord
        : PartyRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelfIdentifiedUserRecord"/> class.
        /// </summary>
        public SelfIdentifiedUserRecord()
            : base(PolymorphicFieldValueRecords.PartyType.SelfIdentifiedUser)
        {
        }
    }

    /// <summary>
    /// Represents a mailing address.
    /// </summary>
    public record MailingAddress
    {
        /// <summary>
        /// Gets the address part of the mailing address.
        /// </summary>
        public string? Address { get; init; }

        /// <summary>
        /// Gets the postal code of the mailing address.
        /// </summary>
        public string? PostalCode { get; init; }

        /// <summary>
        /// Gets the city of the mailing address.
        /// </summary>
        public string? City { get; init; }
    }

    /// <summary>
    /// Represents a street address.
    /// </summary>
    public record StreetAddress
    {
        /// <summary>
        /// Gets the municipal number of the street address.
        /// </summary>
        public string? MunicipalNumber { get; init; }

        /// <summary>
        /// Gets the municipal name of the street address.
        /// </summary>
        public string? MunicipalName { get; init; }

        /// <summary>
        /// Gets the street name of the street address.
        /// </summary>
        public string? StreetName { get; init; }

        /// <summary>
        /// Gets the house number of the street address.
        /// </summary>
        public string? HouseNumber { get; init; }

        /// <summary>
        /// Gets the house letter of the street address.
        /// </summary>
        public string? HouseLetter { get; init; }

        /// <summary>
        /// Gets the postal code of the mailing address.
        /// </summary>
        public string? PostalCode { get; init; }

        /// <summary>
        /// Gets the city of the mailing address.
        /// </summary>
        public string? City { get; init; }
    }
}
