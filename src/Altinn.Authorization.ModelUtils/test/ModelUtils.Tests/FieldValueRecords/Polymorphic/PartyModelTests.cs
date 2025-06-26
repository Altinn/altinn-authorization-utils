using Altinn.Authorization.ModelUtils.Tests.Utils;
using Altinn.Authorization.ModelUtils.Tests.Utils.Shouldly;
using CommunityToolkit.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Tests.FieldValueRecords.Polymorphic;

public class PartyModelTests
{
    private static DateTimeOffset ConstTime = new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Serialize_PartyRecord_AllUnset()
    {
        PartyRecord record = new PartyRecord(FieldValue.Unset)
        {
            PartyUuid = FieldValue.Unset,
            PartyId = FieldValue.Unset,
            DisplayName = FieldValue.Unset,
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = FieldValue.Unset,
            ModifiedAt = FieldValue.Unset,
            IsDeleted = FieldValue.Unset,
            User = FieldValue.Unset,
            VersionId = FieldValue.Unset,
        };

        record.ShouldJsonRoundTripAs(
            """{}""");
    }

    [Fact]
    public void Serialize_PartyRecord_TypeUnset()
    {
        PartyRecord record = new PartyRecord(FieldValue.Unset)
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = new PartyUserRecord
            {
                UserIds = ImmutableValueArray.Create(1U, 2U),
            },
            VersionId = 50,
        };

        record.ShouldJsonRoundTripAs(
            """
            {
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user": {
                    "userId": 1,
                    "userIds": [1, 2]
                },
                "versionId":50
            }
            """);
    }

    [Fact]
    public void Serialize_PartyRecord_TypeUnknown()
    {
        NonExhaustiveEnum<PartyType> unknown = Json.Deserialize<NonExhaustiveEnum<PartyType>>("\"unknown-value\"")!;

        PartyRecord record = new PartyRecord(unknown)
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = new PartyUserRecord
            {
                UserIds = ImmutableValueArray.Create(1U, 2U),
            },
            VersionId = 50,
        };

        record.ShouldJsonRoundTripAs(
            """
            {
                "partyType": "unknown-value",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user": {
                    "userId": 1,
                    "userIds": [1, 2]
                },
                "versionId":50
            }
            """);
    }

    [Fact]
    public void Serialize_PartyRecord_PersonType()
    {
        PartyRecord record = new PartyRecord(PartyType.Person)
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = "25871999336",
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = true,
            User = new PartyUserRecord
            {
                UserIds = FieldValue.Unset,
            },
            VersionId = 42,
        };

        var doc = record.ShouldJsonSerializeAs(
            """
            {
                "partyType": "person",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "personIdentifier":"25871999336",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":true,
                "user": {},
                "versionId":42
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(doc)!;
        deserialized.ShouldBeOfType<PersonRecord>();
    }

    [Fact]
    public void Serialize_PartyRecord_OrganizationType()
    {
        PartyRecord record = new PartyRecord(PartyType.Organization)
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = "123456785",
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = new PartyUserRecord
            {
                UserIds = FieldValue.Unset,
            },
            VersionId = 42,
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "organization",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "organizationIdentifier":"123456785",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user": {},
                "versionId":42
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<OrganizationRecord>();
    }

    [Fact]
    public void Serialize_PartyRecord_SIType()
    {
        PartyRecord record = new PartyRecord(PartyType.SelfIdentifiedUser)
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = "25871999336",
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = true,
            User = new PartyUserRecord
            {
                UserIds = ImmutableValueArray.Create(1U),
            },
            VersionId = 42,
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "self-identified-user",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "personIdentifier":"25871999336",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":true,
                "user": {
                    "userId": 1,
                    "userIds": [1]
                },
                "versionId":42
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<SelfIdentifiedUserRecord>();
    }

    [Fact]
    public void Serialize_PersonRecord_Empty()
    {
        PartyRecord record = new PersonRecord
        {
            PartyUuid = FieldValue.Unset,
            PartyId = FieldValue.Unset,
            DisplayName = FieldValue.Unset,
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = FieldValue.Unset,
            ModifiedAt = FieldValue.Unset,
            IsDeleted = FieldValue.Unset,
            User = FieldValue.Unset,
            VersionId = FieldValue.Unset,

            FirstName = FieldValue.Unset,
            MiddleName = FieldValue.Unset,
            LastName = FieldValue.Unset,
            ShortName = FieldValue.Unset,
            Address = FieldValue.Unset,
            MailingAddress = FieldValue.Unset,
            DateOfBirth = FieldValue.Unset,
            DateOfDeath = FieldValue.Unset,
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "person"
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<PersonRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_PersonRecord_OnlyPartyFields()
    {
        PartyRecord record = new PersonRecord
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = "25871999336",
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = new PartyUserRecord
            {
                UserIds = FieldValue.Unset,
            },
            VersionId = 42,

            FirstName = FieldValue.Unset,
            MiddleName = FieldValue.Unset,
            LastName = FieldValue.Unset,
            ShortName = FieldValue.Unset,
            Address = FieldValue.Unset,
            MailingAddress = FieldValue.Unset,
            DateOfBirth = FieldValue.Unset,
            DateOfDeath = FieldValue.Unset,
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "person",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "personIdentifier":"25871999336",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user": {},
                "versionId":42
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<PersonRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_PersonRecord_Full()
    {
        PartyRecord record = new PersonRecord
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = "25871999336",
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime + TimeSpan.FromDays(1),
            IsDeleted = true,
            User = new PartyUserRecord
            {
                UserIds = ImmutableValueArray.Create(1U),
            },
            VersionId = 42,

            FirstName = "First",
            MiddleName = null,
            LastName = "Last",
            ShortName = "Short",
            Address = new StreetAddress
            {
                MunicipalNumber = "1",
                MunicipalName = "2",
                StreetName = "3",
                HouseNumber = "4",
                HouseLetter = "5",
                PostalCode = "6",
                City = "7",
            },
            MailingAddress = new MailingAddress
            {
                Address = "Address",
                PostalCode = "PostalCode",
                City = "City",
            },
            DateOfBirth = new DateOnly(1900, 01, 01),
            DateOfDeath = new DateOnly(2000, 02, 02),
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "person",
                "partyUuid": "00000000-0000-0000-0000-000000000001",
                "partyId": 1,
                "displayName": "1",
                "personIdentifier": "25871999336",
                "createdAt": "2000-01-01T00:00:00+00:00",
                "modifiedAt": "2000-01-02T00:00:00+00:00",
                "isDeleted": true,
                "user": {
                    "userId": 1,
                    "userIds": [1]
                },
                "versionId": 42,
                "firstName": "First",
                "middleName": null,
                "lastName": "Last",
                "shortName": "Short",
                "address": {
                    "municipalNumber": "1",
                    "municipalName": "2",
                    "streetName": "3",
                    "houseNumber": "4",
                    "houseLetter": "5",
                    "postalCode": "6",
                    "city": "7"
                },
                "mailingAddress": {
                    "address": "Address",
                    "postalCode": "PostalCode",
                    "city": "City"
                },
                "dateOfBirth": "1900-01-01",
                "dateOfDeath": "2000-02-02"
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<PersonRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_OrganizationRecord_Empty()
    {
        PartyRecord record = new OrganizationRecord
        {
            PartyUuid = FieldValue.Unset,
            PartyId = FieldValue.Unset,
            DisplayName = FieldValue.Unset,
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = FieldValue.Unset,
            ModifiedAt = FieldValue.Unset,
            IsDeleted = FieldValue.Unset,
            User = FieldValue.Unset,
            VersionId = FieldValue.Unset,

            UnitStatus = FieldValue.Unset,
            UnitType = FieldValue.Unset,
            TelephoneNumber = FieldValue.Unset,
            MobileNumber = FieldValue.Unset,
            FaxNumber = FieldValue.Unset,
            EmailAddress = FieldValue.Unset,
            InternetAddress = FieldValue.Unset,
            MailingAddress = FieldValue.Unset,
            BusinessAddress = FieldValue.Unset,
        };
        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "organization"
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<OrganizationRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_OrganizationRecord_Full()
    {
        PartyRecord record = new OrganizationRecord
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = "123456785",
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = FieldValue.Null,
            VersionId = 42,

            UnitStatus = "status",
            UnitType = "type",
            TelephoneNumber = "telephone",
            MobileNumber = "mobile",
            FaxNumber = "fax",
            EmailAddress = "email",
            InternetAddress = "internet",
            MailingAddress = new MailingAddress
            {
                Address = "mailing address",
                PostalCode = "mailing postal",
                City = "mailing city",
            },
            BusinessAddress = new MailingAddress
            {
                Address = "business address",
                PostalCode = "business postal",
                City = "business city",
            },
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "organization",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "organizationIdentifier":"123456785",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user":null,
                "versionId":42,
                "unitStatus":"status",
                "unitType":"type",
                "telephoneNumber":"telephone",
                "mobileNumber":"mobile",
                "faxNumber":"fax",
                "emailAddress":"email",
                "internetAddress":"internet",
                "mailingAddress": {
                    "address": "mailing address",
                    "postalCode": "mailing postal",
                    "city": "mailing city"
                },
                "businessAddress": {
                    "address": "business address",
                    "postalCode": "business postal",
                    "city": "business city"
                }
            }
            """);

        var deserialized = Json.Deserialize<PartyRecord>(json);
        deserialized.ShouldBeOfType<OrganizationRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_OrganizationRecord_ParentOrganizationUuid_IsIgnored()
    {
        PartyRecord record = new OrganizationRecord
        {
            PartyUuid = FieldValue.Unset,
            PartyId = FieldValue.Unset,
            DisplayName = FieldValue.Unset,
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = FieldValue.Unset,
            ModifiedAt = FieldValue.Unset,
            IsDeleted = FieldValue.Unset,
            User = FieldValue.Unset,
            VersionId = FieldValue.Unset,

            UnitStatus = FieldValue.Unset,
            UnitType = FieldValue.Unset,
            TelephoneNumber = FieldValue.Unset,
            MobileNumber = FieldValue.Unset,
            FaxNumber = FieldValue.Unset,
            EmailAddress = FieldValue.Unset,
            InternetAddress = FieldValue.Unset,
            MailingAddress = FieldValue.Unset,
            BusinessAddress = FieldValue.Unset,

            ParentOrganizationUuid = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        };
        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "organization"
            }
            """);

        var json2 =
            """
            {
                "partyType": "organization",
                "parentOrganizationUuid": "00000000-0000-0000-0000-000000000002"
            }
            """;

        var deserialized = Json.Deserialize<PartyRecord>(json2);
        var org = deserialized.ShouldBeOfType<OrganizationRecord>();
        org.ParentOrganizationUuid.ShouldBeUnset();
    }

    [Fact]
    public void Serialize_SIRecord()
    {
        SelfIdentifiedUserRecord record = new SelfIdentifiedUserRecord
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = "25871999336",
            OrganizationIdentifier = FieldValue.Unset,
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = new PartyUserRecord
            {
                UserIds = ImmutableValueArray.Create(1U),
            },
            VersionId = 42,
        };

        using var json = Json.SerializeToDocument(record);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "partyType": "self-identified-user",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "personIdentifier":"25871999336",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user": {
                    "userId": 1,
                    "userIds": [1]
                },
                "versionId":42
            }
            """);

        var deserialized = Json.Deserialize<SelfIdentifiedUserRecord>(json);
        deserialized.ShouldBeOfType<SelfIdentifiedUserRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_RespectsOptions()
    {
        var options = JsonSerializerOptions.Default;

        PartyRecord record = new OrganizationRecord
        {
            PartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            PartyId = 1,
            DisplayName = "1",
            PersonIdentifier = FieldValue.Unset,
            OrganizationIdentifier = "123456785",
            CreatedAt = ConstTime,
            ModifiedAt = ConstTime,
            IsDeleted = false,
            User = FieldValue.Null,
            VersionId = 42,

            UnitStatus = "status",
            UnitType = "type",
            TelephoneNumber = "telephone",
            MobileNumber = "mobile",
            FaxNumber = "fax",
            EmailAddress = "email",
            InternetAddress = "internet",
            MailingAddress = new MailingAddress
            {
                Address = "mailing address",
                PostalCode = "mailing postal",
                City = "mailing city",
            },
            BusinessAddress = new MailingAddress
            {
                Address = "business address",
                PostalCode = "business postal",
                City = "business city",
            },
        };

        var json = JsonSerializer.SerializeToElement(record, options);
        json.ShouldBeStructurallyEquivalentTo(
            """
            {
                "PartyType": "organization",
                "PartyUuid":"00000000-0000-0000-0000-000000000001",
                "PartyId":1,
                "DisplayName":"1",
                "OrganizationIdentifier":"123456785",
                "CreatedAt":"2000-01-01T00:00:00+00:00",
                "ModifiedAt":"2000-01-01T00:00:00+00:00",
                "IsDeleted":false,
                "User":null,
                "VersionId":42,
                "UnitStatus":"status",
                "UnitType":"type",
                "TelephoneNumber":"telephone",
                "MobileNumber":"mobile",
                "FaxNumber":"fax",
                "EmailAddress":"email",
                "InternetAddress":"internet",
                "MailingAddress": {
                    "Address": "mailing address",
                    "PostalCode": "mailing postal",
                    "City": "mailing city"
                },
                "BusinessAddress": {
                    "Address": "business address",
                    "PostalCode": "business postal",
                    "City": "business city"
                }
            }
            """);

        var deserialized = JsonSerializer.Deserialize<PartyRecord>(json, options);
        deserialized.ShouldBeOfType<OrganizationRecord>();
        deserialized.ShouldBeEquivalentTo(record);
    }

    [Fact]
    public void Serialize_SupportsCaseInsensitive()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            PropertyNameCaseInsensitive = true,
        };

        var json =
            """
            {
                "pARTYtYPE": "organization",
                "DISPLAYNAME": "Test",
                "UNITTYPE": "type"
            }
            """;

        var deserialized = JsonSerializer.Deserialize<PartyRecord>(json, options);
        var org = deserialized.ShouldBeOfType<OrganizationRecord>();
        org.DisplayName.ShouldBe("Test");
        org.UnitType.ShouldBe("type");
    }

    [Fact]
    public void Deserialize_Subclasses()
    {
        Json.Deserialize<PersonRecord>("""{}""").ShouldNotBeNull();
        Json.Deserialize<OrganizationRecord>("""{}""").ShouldNotBeNull();
        Json.Deserialize<SelfIdentifiedUserRecord>("""{}""").ShouldNotBeNull();
    }

    [Fact]
    public void Unknown_PartyType()
    {
        var json =
            """
            {
                "partyType": "unknown-party-type",
                "partyUuid":"00000000-0000-0000-0000-000000000001",
                "partyId":1,
                "displayName":"1",
                "personIdentifier":"25871999336",
                "createdAt":"2000-01-01T00:00:00+00:00",
                "modifiedAt":"2000-01-01T00:00:00+00:00",
                "isDeleted":false,
                "user": {
                    "userId": 1,
                    "userIds": [1]
                },
                "versionId":42,
                "newField": "with-value",
                "otherNewField": { "prop": "value" }
            }
            """;

        var party = Json.Deserialize<PartyRecord>(json);
        party.ShouldBeOfType<PartyRecord>();
        party.PartyType.HasValue.ShouldBeTrue();
        party.PartyType.Value.IsUnknown.ShouldBeTrue();
        party.PartyType.Value.UnknownValue.ShouldBe("unknown-party-type");

        var serialized = Json.SerializeToDocument(party);
        serialized.ShouldBeStructurallyEquivalentTo(json);
    }

    // previous bug
    [Fact]
    public void Person_DateOfDeath_Null()
    {
        var json =
            """
            {
              "partyType": "person",
              "firstName": "Test",
              "middleName": "Mid",
              "lastName": "Testson",
              "shortName": "Testson Test Mid",
              "address": {
                "municipalNumber": "0001",
                "municipalName": "Test",
                "streetName": "Testveien",
                "houseNumber": "1",
                "houseLetter": null,
                "postalCode": "0001",
                "city": "Testby"
              },
              "mailingAddress": {
                "address": "Testveien 1",
                "postalCode": "0001",
                "city": "Testby"
              },
              "dateOfBirth": "1944-09-03",
              "dateOfDeath": null,
              "partyUuid": "88504754-b266-4a09-aca0-09a39f66601f",
              "partyId": 2,
              "displayName": "Testson Test Mid",
              "personIdentifier": "03094431319",
              "organizationIdentifier": null,
              "createdAt": "2000-01-01T00:00:00+00:00",
              "modifiedAt": "2000-01-01T00:00:00+00:00",
              "isDeleted": false,
              "versionId": 429
            }
            """;

        var party = Json.Deserialize<PartyRecord>(json);
        var person = party.ShouldBeOfType<PersonRecord>();
        person.DateOfDeath.ShouldBeNull();
    }

    #region Models

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
    [PolymorphicDerivedType(typeof(PersonRecord), PartyModelTests.PartyType.Person)]
    [PolymorphicDerivedType(typeof(OrganizationRecord), PartyModelTests.PartyType.Organization)]
    [PolymorphicDerivedType(typeof(SelfIdentifiedUserRecord), PartyModelTests.PartyType.SelfIdentifiedUser)]
    public record PartyRecord
    {
        [JsonExtensionData]
        private readonly JsonElement _extensionData;

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
            : base(PartyModelTests.PartyType.Person)
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
            : base(PartyModelTests.PartyType.Organization)
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
            : base(PartyModelTests.PartyType.SelfIdentifiedUser)
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

    #endregion
}
