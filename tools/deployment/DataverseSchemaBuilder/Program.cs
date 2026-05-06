using System.ServiceModel;
using System.Globalization;
using Azure.Core;
using Azure.Identity;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

var options = ParseArgs(args);
var environmentUrl = options.GetValueOrDefault("environment-url", "https://org1aa19a83.crm.dynamics.com");
var solutionUniqueName = options.GetValueOrDefault("solution-unique-name", "nhspshiftexceptiondemo");
var importSampleData = bool.TryParse(options.GetValueOrDefault("import-sample-data", "false"), out var parsedImportSampleData) && parsedImportSampleData;
var samplesPath = options.GetValueOrDefault("samples-path", Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "..", "samples")));

Console.WriteLine($"Target environment: {environmentUrl}");
Console.WriteLine($"Target solution: {solutionUniqueName}");

var credential = new AzureCliCredential();
var serviceClient = new ServiceClient(
    new Uri(environmentUrl),
    async _ =>
    {
        var scope = $"{environmentUrl.TrimEnd('/')}/.default";

        var token = await credential.GetTokenAsync(new TokenRequestContext(new[] { scope }));
        return token.Token;
    },
    useUniqueInstance: true);

if (!serviceClient.IsReady)
{
    throw new InvalidOperationException($"Dataverse connection failed: {serviceClient.LastError}");
}

Console.WriteLine($"Connected: {serviceClient.ConnectedOrgFriendlyName}");

var schema = new SchemaBuilder(serviceClient, solutionUniqueName);
schema.Build();

Console.WriteLine("Dataverse schema build complete.");

if (importSampleData)
{
    var importer = new SampleDataImporter(serviceClient, samplesPath);
    importer.Import();
}

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var index = 0; index < args.Length; index++)
    {
        if (!args[index].StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = args[index][2..];
        if (index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            result[key] = args[index + 1];
            index++;
        }
    }

    return result;
}

internal sealed class SchemaBuilder
{
    private readonly IOrganizationService service;
    private readonly string solutionUniqueName;

    public SchemaBuilder(IOrganizationService service, string solutionUniqueName)
    {
        this.service = service;
        this.solutionUniqueName = solutionUniqueName;
    }

    public void Build()
    {
        CreateEntity("nhsp_Shift", "Shift", "Shifts", "nhsp_ShiftReference", "Shift ID", "Bank shift at a trust ward that requires workers.");
        CreateEntity("nhsp_Worker", "Worker", "Workers", "nhsp_WorkerReference", "Worker ID", "Bank worker available for shift assignment.");
        CreateEntity("nhsp_ShiftException", "Shift Exception", "Shift Exceptions", "nhsp_ExceptionReference", "Exception ID", "Exception raised against a shift that requires triage and resolution.");
        CreateEntity("nhsp_ExceptionAction", "Exception Action", "Exception Actions", "nhsp_ExceptionActionReference", "Exception Action ID", "Auditable action taken by a coordinator, manager, or flow.");
        CreateEntity("nhsp_TrustContact", "Trust Contact", "Trust Contacts", "nhsp_TrustContactReference", "Trust Contact ID", "Synthetic or configured routing for trust escalation notifications.");

        AddAttribute("nhsp_shift", Picklist("nhsp_Trust", "Trust", true, "Northshire NHS Trust", "Southvale NHS Trust"));
        AddAttribute("nhsp_shift", StringAttribute("nhsp_Ward", "Ward", true, 100));
        AddAttribute("nhsp_shift", Picklist("nhsp_Role", "Role", true, "Registered Nurse", "Healthcare Assistant"));
        AddAttribute("nhsp_shift", DateTime("nhsp_StartTime", "Start Time", true));
        AddAttribute("nhsp_shift", DateTime("nhsp_EndTime", "End Time", true));
        AddAttribute("nhsp_shift", Picklist("nhsp_Status", "Status", true, "Open", "Filled", "Cancelled"));
        AddAttribute("nhsp_shift", Integer("nhsp_RequiredWorkers", "Required Workers", true));
        AddAttribute("nhsp_shift", Integer("nhsp_FilledWorkers", "Filled Workers", true));

        AddAttribute("nhsp_worker", StringAttribute("nhsp_FullName", "Full Name", true, 150));
        AddAttribute("nhsp_worker", Picklist("nhsp_Role", "Role", true, "Registered Nurse", "Healthcare Assistant"));
        AddAttribute("nhsp_worker", Picklist("nhsp_ComplianceStatus", "Compliance Status", true, "Compliant", "Training Expired", "DBS Review Required"));
        AddAttribute("nhsp_worker", Picklist("nhsp_AvailabilityStatus", "Availability", true, "Available", "Unavailable"));
        AddAttribute("nhsp_worker", Picklist("nhsp_PreferredTrust", "Preferred Trust", false, "Northshire NHS Trust", "Southvale NHS Trust"));

        AddAttribute("nhsp_shiftexception", Picklist("nhsp_ExceptionType", "Exception Type", true, "Unfilled Shift", "Compliance Blocker", "Urgent Staffing Request"));
        AddAttribute("nhsp_shiftexception", Picklist("nhsp_Priority", "Priority", true, "Critical", "High", "Medium", "Low"));
        AddAttribute("nhsp_shiftexception", Picklist("nhsp_Status", "Status", true, "Open", "In Progress", "Resolved", "Closed"));
        AddAttribute("nhsp_shiftexception", DateTime("nhsp_CreatedTime", "Created Time", true));
        AddAttribute("nhsp_shiftexception", Picklist("nhsp_EscalationStatus", "Escalation Status", true, "Not Escalated", "Pending Trust Response", "Escalated"));
        AddAttribute("nhsp_shiftexception", Memo("nhsp_ResolutionNotes", "Resolution Notes", false));
        AddAttribute("nhsp_shiftexception", DateTime("nhsp_ResolvedTime", "Resolved Time", false));

        AddAttribute("nhsp_exceptionaction", Picklist("nhsp_ActionType", "Action Type", true, "Coordinator Notification", "Trust Escalation", "Flow Error", "Manual Update"));
        AddAttribute("nhsp_exceptionaction", Picklist("nhsp_ActionStatus", "Action Status", true, "Sent", "Skipped", "Failed", "Retried", "Completed"));
        AddAttribute("nhsp_exceptionaction", DateTime("nhsp_ActionTime", "Action Time", true));
        AddAttribute("nhsp_exceptionaction", StringAttribute("nhsp_ActionBy", "Action By", true, 150));
        AddAttribute("nhsp_exceptionaction", Memo("nhsp_Notes", "Notes", false));

        AddAttribute("nhsp_trustcontact", Picklist("nhsp_Trust", "Trust", true, "Northshire NHS Trust", "Southvale NHS Trust"));
        AddAttribute("nhsp_trustcontact", StringAttribute("nhsp_ContactName", "Contact Name", true, 150));
        AddAttribute("nhsp_trustcontact", StringAttribute("nhsp_ContactRole", "Contact Role", true, 100));
        AddAttribute("nhsp_trustcontact", StringAttribute("nhsp_ContactEmail", "Contact Email", true, 150, StringFormatName.Email));
        AddAttribute("nhsp_trustcontact", Picklist("nhsp_EscalationChannel", "Escalation Channel", true, "Email", "Teams", "Manual"));
        AddAttribute("nhsp_trustcontact", Boolean("nhsp_IsDemoContact", "Is Demo Contact", true));

        AddLookup("nhsp_Shift_ShiftException", "nhsp_shift", "nhsp_shiftexception", "nhsp_Shift", "Shift", true);
        AddLookup("nhsp_ShiftException_ExceptionAction", "nhsp_shiftexception", "nhsp_exceptionaction", "nhsp_ShiftException", "Shift Exception", true);
        AddLookup("nhsp_SystemUser_ShiftExceptionOwner", "systemuser", "nhsp_shiftexception", "nhsp_Owner", "Owner", true);

        PublishAll();
    }

    private void CreateEntity(string schemaName, string displayName, string collectionDisplayName, string primaryNameSchemaName, string primaryNameDisplayName, string description)
    {
        if (EntityExists(schemaName.ToLowerInvariant()))
        {
            Console.WriteLine($"Exists: table {schemaName}");
            return;
        }

        var request = new CreateEntityRequest
        {
            Entity = new EntityMetadata
            {
                SchemaName = schemaName,
                DisplayName = Label(displayName),
                DisplayCollectionName = Label(collectionDisplayName),
                Description = Label(description),
                OwnershipType = OwnershipTypes.UserOwned,
                IsActivity = false,
                HasActivities = false,
                HasNotes = false,
                IsAuditEnabled = new BooleanManagedProperty(true)
            },
            PrimaryAttribute = StringAttribute(primaryNameSchemaName, primaryNameDisplayName, true, 100),
            SolutionUniqueName = solutionUniqueName
        };

        service.Execute(request);
        Console.WriteLine($"Created: table {schemaName}");
    }

    private void AddAttribute(string entityLogicalName, AttributeMetadata attribute)
    {
        var logicalName = attribute.SchemaName!.ToLowerInvariant();
        if (AttributeExists(entityLogicalName, logicalName))
        {
            Console.WriteLine($"Exists: column {entityLogicalName}.{logicalName}");
            return;
        }

        var request = new CreateAttributeRequest
        {
            EntityName = entityLogicalName,
            Attribute = attribute,
            SolutionUniqueName = solutionUniqueName
        };

        service.Execute(request);
        Console.WriteLine($"Created: column {entityLogicalName}.{logicalName}");
    }

    private void AddLookup(string schemaName, string referencedEntity, string referencingEntity, string lookupSchemaName, string lookupDisplayName, bool required)
    {
        if (AttributeExists(referencingEntity, lookupSchemaName.ToLowerInvariant()))
        {
            Console.WriteLine($"Exists: relationship lookup {referencingEntity}.{lookupSchemaName.ToLowerInvariant()}");
            return;
        }

        var request = new CreateOneToManyRequest
        {
            OneToManyRelationship = new OneToManyRelationshipMetadata
            {
                SchemaName = schemaName,
                ReferencedEntity = referencedEntity,
                ReferencingEntity = referencingEntity,
                CascadeConfiguration = new CascadeConfiguration
                {
                    Assign = CascadeType.NoCascade,
                    Delete = CascadeType.Restrict,
                    Merge = CascadeType.NoCascade,
                    Reparent = CascadeType.NoCascade,
                    Share = CascadeType.NoCascade,
                    Unshare = CascadeType.NoCascade
                },
                AssociatedMenuConfiguration = new AssociatedMenuConfiguration
                {
                    Behavior = AssociatedMenuBehavior.UseLabel,
                    Group = AssociatedMenuGroup.Details,
                    Label = Label(lookupDisplayName),
                    Order = 10000
                }
            },
            Lookup = new LookupAttributeMetadata
            {
                SchemaName = lookupSchemaName,
                DisplayName = Label(lookupDisplayName),
                RequiredLevel = Required(required),
                IsAuditEnabled = new BooleanManagedProperty(true)
            },
            SolutionUniqueName = solutionUniqueName
        };

        service.Execute(request);
        Console.WriteLine($"Created: relationship {schemaName}");
    }

    private bool EntityExists(string logicalName)
    {
        try
        {
            service.Execute(new RetrieveEntityRequest
            {
                LogicalName = logicalName,
                EntityFilters = EntityFilters.Entity,
                RetrieveAsIfPublished = true
            });
            return true;
        }
        catch (FaultException<OrganizationServiceFault> ex) when (IsMissingMetadataFault(ex))
        {
            return false;
        }
    }

    private bool AttributeExists(string entityLogicalName, string attributeLogicalName)
    {
        try
        {
            service.Execute(new RetrieveAttributeRequest
            {
                EntityLogicalName = entityLogicalName,
                LogicalName = attributeLogicalName,
                RetrieveAsIfPublished = true
            });
            return true;
        }
        catch (FaultException<OrganizationServiceFault> ex) when (IsMissingMetadataFault(ex))
        {
            return false;
        }
    }

    private static bool IsMissingMetadataFault(FaultException<OrganizationServiceFault> ex)
    {
        var message = ex.Detail.Message ?? string.Empty;
        return message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Could not find", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not found", StringComparison.OrdinalIgnoreCase);
    }

    private static StringAttributeMetadata StringAttribute(string schemaName, string displayName, bool required, int maxLength, StringFormatName? format = null)
    {
        return new StringAttributeMetadata
        {
            SchemaName = schemaName,
            DisplayName = Label(displayName),
            RequiredLevel = Required(required),
            MaxLength = maxLength,
            FormatName = format ?? StringFormatName.Text,
            IsAuditEnabled = new BooleanManagedProperty(true)
        };
    }

    private static MemoAttributeMetadata Memo(string schemaName, string displayName, bool required)
    {
        return new MemoAttributeMetadata
        {
            SchemaName = schemaName,
            DisplayName = Label(displayName),
            RequiredLevel = Required(required),
            MaxLength = 2000,
            IsAuditEnabled = new BooleanManagedProperty(true)
        };
    }

    private static DateTimeAttributeMetadata DateTime(string schemaName, string displayName, bool required)
    {
        return new DateTimeAttributeMetadata
        {
            SchemaName = schemaName,
            DisplayName = Label(displayName),
            RequiredLevel = Required(required),
            DateTimeBehavior = DateTimeBehavior.UserLocal,
            Format = DateTimeFormat.DateAndTime,
            IsAuditEnabled = new BooleanManagedProperty(true)
        };
    }

    private static IntegerAttributeMetadata Integer(string schemaName, string displayName, bool required)
    {
        return new IntegerAttributeMetadata
        {
            SchemaName = schemaName,
            DisplayName = Label(displayName),
            RequiredLevel = Required(required),
            MinValue = 0,
            MaxValue = 100000,
            Format = IntegerFormat.None,
            IsAuditEnabled = new BooleanManagedProperty(true)
        };
    }

    private static PicklistAttributeMetadata Picklist(string schemaName, string displayName, bool required, params string[] options)
    {
        var optionSet = new OptionSetMetadata
        {
            IsGlobal = false,
            OptionSetType = OptionSetType.Picklist
        };

        var value = 100000000;
        foreach (var option in options)
        {
            optionSet.Options.Add(new OptionMetadata(Label(option), value++));
        }

        return new PicklistAttributeMetadata
        {
            SchemaName = schemaName,
            DisplayName = Label(displayName),
            RequiredLevel = Required(required),
            OptionSet = optionSet,
            IsAuditEnabled = new BooleanManagedProperty(true)
        };
    }

    private static BooleanAttributeMetadata Boolean(string schemaName, string displayName, bool required)
    {
        return new BooleanAttributeMetadata
        {
            SchemaName = schemaName,
            DisplayName = Label(displayName),
            RequiredLevel = Required(required),
            OptionSet = new BooleanOptionSetMetadata(
                new OptionMetadata(Label("Yes"), 1),
                new OptionMetadata(Label("No"), 0)),
            IsAuditEnabled = new BooleanManagedProperty(true)
        };
    }

    private void PublishAll()
    {
        service.Execute(new PublishAllXmlRequest());
        Console.WriteLine("Published: all customizations");
    }

    private static AttributeRequiredLevelManagedProperty Required(bool required)
    {
        return new AttributeRequiredLevelManagedProperty(required ? AttributeRequiredLevel.ApplicationRequired : AttributeRequiredLevel.None);
    }

    private static Label Label(string text)
    {
        return new Label(text, 1033);
    }
}

internal sealed class SampleDataImporter
{
    private readonly IOrganizationService service;
    private readonly string samplesPath;
    private readonly Guid currentUserId;

    public SampleDataImporter(IOrganizationService service, string samplesPath)
    {
        this.service = service;
        this.samplesPath = Path.GetFullPath(samplesPath);
        currentUserId = ((WhoAmIResponse)service.Execute(new WhoAmIRequest())).UserId;
    }

    public void Import()
    {
        Console.WriteLine($"Importing synthetic sample data from: {samplesPath}");

        var shifts = ImportShifts();
        ImportWorkers();
        ImportTrustContacts();
        var exceptions = ImportExceptions(shifts);
        ImportExceptionActions(exceptions);

        Console.WriteLine("Synthetic sample data import complete.");
    }

    private Dictionary<string, EntityReference> ImportShifts()
    {
        var references = new Dictionary<string, EntityReference>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in ReadCsv("sample-shifts.csv"))
        {
            var shiftId = RequiredValue(row, "ShiftId");
            var entity = new Entity("nhsp_shift")
            {
                ["nhsp_shiftreference"] = shiftId,
                ["nhsp_trust"] = Choice("trust", RequiredValue(row, "Trust")),
                ["nhsp_ward"] = RequiredValue(row, "Ward"),
                ["nhsp_role"] = Choice("role", RequiredValue(row, "Role")),
                ["nhsp_starttime"] = Date(RequiredValue(row, "StartTime")),
                ["nhsp_endtime"] = Date(RequiredValue(row, "EndTime")),
                ["nhsp_status"] = Choice("shift-status", RequiredValue(row, "Status")),
                ["nhsp_requiredworkers"] = int.Parse(RequiredValue(row, "RequiredWorkers"), CultureInfo.InvariantCulture),
                ["nhsp_filledworkers"] = int.Parse(RequiredValue(row, "FilledWorkers"), CultureInfo.InvariantCulture)
            };

            var reference = Save("nhsp_shift", "nhsp_shiftreference", shiftId, entity);
            references[shiftId] = reference;
        }

        return references;
    }

    private void ImportWorkers()
    {
        foreach (var row in ReadCsv("sample-workers.csv"))
        {
            var workerId = RequiredValue(row, "WorkerId");
            var entity = new Entity("nhsp_worker")
            {
                ["nhsp_workerreference"] = workerId,
                ["nhsp_fullname"] = RequiredValue(row, "FullName"),
                ["nhsp_role"] = Choice("role", RequiredValue(row, "Role")),
                ["nhsp_compliancestatus"] = Choice("compliance-status", RequiredValue(row, "ComplianceStatus")),
                ["nhsp_availabilitystatus"] = Choice("availability-status", RequiredValue(row, "AvailabilityStatus"))
            };

            var preferredTrust = Value(row, "PreferredTrust");
            if (!string.IsNullOrWhiteSpace(preferredTrust))
            {
                entity["nhsp_preferredtrust"] = Choice("trust", preferredTrust);
            }

            Save("nhsp_worker", "nhsp_workerreference", workerId, entity);
        }
    }

    private void ImportTrustContacts()
    {
        foreach (var row in ReadCsv("sample-trust-contacts.csv"))
        {
            var trustContactId = RequiredValue(row, "TrustContactId");
            var entity = new Entity("nhsp_trustcontact")
            {
                ["nhsp_trustcontactreference"] = trustContactId,
                ["nhsp_trust"] = Choice("trust", RequiredValue(row, "Trust")),
                ["nhsp_contactname"] = RequiredValue(row, "ContactName"),
                ["nhsp_contactrole"] = RequiredValue(row, "ContactRole"),
                ["nhsp_contactemail"] = RequiredValue(row, "ContactEmail"),
                ["nhsp_escalationchannel"] = Choice("escalation-channel", RequiredValue(row, "EscalationChannel")),
                ["nhsp_isdemocontact"] = bool.Parse(RequiredValue(row, "IsDemoContact"))
            };

            Save("nhsp_trustcontact", "nhsp_trustcontactreference", trustContactId, entity);
        }
    }

    private Dictionary<string, EntityReference> ImportExceptions(IReadOnlyDictionary<string, EntityReference> shifts)
    {
        var references = new Dictionary<string, EntityReference>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in ReadCsv("sample-exceptions.csv"))
        {
            var exceptionId = RequiredValue(row, "ExceptionId");
            var shiftId = RequiredValue(row, "ShiftId");
            if (!shifts.TryGetValue(shiftId, out var shiftReference))
            {
                throw new InvalidOperationException($"Exception {exceptionId} references missing synthetic shift {shiftId}.");
            }

            var entity = new Entity("nhsp_shiftexception")
            {
                ["nhsp_exceptionreference"] = exceptionId,
                ["nhsp_shift"] = shiftReference,
                ["nhsp_exceptiontype"] = Choice("exception-type", RequiredValue(row, "ExceptionType")),
                ["nhsp_priority"] = Choice("priority", RequiredValue(row, "Priority")),
                ["nhsp_status"] = Choice("exception-status", RequiredValue(row, "Status")),
                ["nhsp_createdtime"] = Date(RequiredValue(row, "CreatedTime")),
                ["nhsp_owner"] = new EntityReference("systemuser", currentUserId),
                ["nhsp_escalationstatus"] = Choice("escalation-status", RequiredValue(row, "EscalationStatus"))
            };

            var reference = Save("nhsp_shiftexception", "nhsp_exceptionreference", exceptionId, entity);
            references[exceptionId] = reference;
        }

        return references;
    }

    private void ImportExceptionActions(IReadOnlyDictionary<string, EntityReference> exceptions)
    {
        foreach (var row in ReadCsv("sample-exception-actions.csv"))
        {
            var actionId = RequiredValue(row, "ActionId");
            var exceptionId = RequiredValue(row, "ExceptionId");
            if (!exceptions.TryGetValue(exceptionId, out var exceptionReference))
            {
                throw new InvalidOperationException($"Action {actionId} references missing synthetic exception {exceptionId}.");
            }

            var entity = new Entity("nhsp_exceptionaction")
            {
                ["nhsp_exceptionactionreference"] = actionId,
                ["nhsp_shiftexception"] = exceptionReference,
                ["nhsp_actiontype"] = Choice("action-type", RequiredValue(row, "ActionType")),
                ["nhsp_actionstatus"] = Choice("action-status", RequiredValue(row, "ActionStatus")),
                ["nhsp_actiontime"] = Date(RequiredValue(row, "ActionTime")),
                ["nhsp_actionby"] = RequiredValue(row, "ActionBy"),
                ["nhsp_notes"] = Value(row, "Notes")
            };

            Save("nhsp_exceptionaction", "nhsp_exceptionactionreference", actionId, entity);
        }
    }

    private EntityReference Save(string entityLogicalName, string keyAttributeName, string keyValue, Entity entity)
    {
        var existing = FindByReference(entityLogicalName, keyAttributeName, keyValue);
        if (existing is null)
        {
            var id = service.Create(entity);
            Console.WriteLine($"Created: row {entityLogicalName} {keyValue}");
            return new EntityReference(entityLogicalName, id);
        }

        entity.Id = existing.Id;
        service.Update(entity);
        Console.WriteLine($"Updated: row {entityLogicalName} {keyValue}");
        return existing.ToEntityReference();
    }

    private Entity? FindByReference(string entityLogicalName, string keyAttributeName, string keyValue)
    {
        var query = new QueryExpression(entityLogicalName)
        {
            ColumnSet = new ColumnSet(keyAttributeName),
            TopCount = 1
        };
        query.Criteria.AddCondition(keyAttributeName, ConditionOperator.Equal, keyValue);

        return service.RetrieveMultiple(query).Entities.FirstOrDefault();
    }

    private IEnumerable<Dictionary<string, string>> ReadCsv(string fileName)
    {
        var path = Path.Combine(samplesPath, fileName);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Sample data file not found: {path}", path);
        }

        using var reader = new StreamReader(path);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            yield break;
        }

        var headers = ParseCsvLine(headerLine).ToArray();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = ParseCsvLine(line).ToArray();
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var index = 0; index < headers.Length; index++)
            {
                row[headers[index]] = index < values.Length ? values[index] : string.Empty;
            }

            yield return row;
        }
    }

    private static IEnumerable<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    current.Append('"');
                    index++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (character == ',' && !inQuotes)
            {
                values.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }

            current.Append(character);
        }

        values.Add(current.ToString().Trim());
        return values;
    }

    private static string RequiredValue(IReadOnlyDictionary<string, string> row, string key)
    {
        var value = Value(row, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Required sample data column '{key}' is missing or empty.");
        }

        return value;
    }

    private static string Value(IReadOnlyDictionary<string, string> row, string key)
    {
        return row.TryGetValue(key, out var value) ? value : string.Empty;
    }

    private static DateTime Date(string value)
    {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
    }

    private static OptionSetValue Choice(string optionSet, string label)
    {
        var value = optionSet switch
        {
            "trust" => label switch
            {
                "Northshire NHS Trust" => 100000000,
                "Southvale NHS Trust" => 100000001,
                _ => throw UnknownChoice(optionSet, label)
            },
            "role" => label switch
            {
                "Registered Nurse" => 100000000,
                "Healthcare Assistant" => 100000001,
                _ => throw UnknownChoice(optionSet, label)
            },
            "shift-status" => label switch
            {
                "Open" => 100000000,
                "Filled" => 100000001,
                "Cancelled" => 100000002,
                _ => throw UnknownChoice(optionSet, label)
            },
            "compliance-status" => label switch
            {
                "Compliant" => 100000000,
                "Training Expired" => 100000001,
                "DBS Review Required" => 100000002,
                _ => throw UnknownChoice(optionSet, label)
            },
            "availability-status" => label switch
            {
                "Available" => 100000000,
                "Unavailable" => 100000001,
                _ => throw UnknownChoice(optionSet, label)
            },
            "exception-type" => label switch
            {
                "Unfilled Shift" => 100000000,
                "Compliance Blocker" => 100000001,
                "Urgent Staffing Request" => 100000002,
                _ => throw UnknownChoice(optionSet, label)
            },
            "priority" => label switch
            {
                "Critical" => 100000000,
                "High" => 100000001,
                "Medium" => 100000002,
                "Low" => 100000003,
                _ => throw UnknownChoice(optionSet, label)
            },
            "exception-status" => label switch
            {
                "Open" => 100000000,
                "In Progress" => 100000001,
                "Resolved" => 100000002,
                "Closed" => 100000003,
                _ => throw UnknownChoice(optionSet, label)
            },
            "escalation-status" => label switch
            {
                "Not Escalated" => 100000000,
                "Pending Trust Response" => 100000001,
                "Escalated" => 100000002,
                _ => throw UnknownChoice(optionSet, label)
            },
            "action-type" => label switch
            {
                "Coordinator Notification" => 100000000,
                "Trust Escalation" => 100000001,
                "Flow Error" => 100000002,
                "Manual Update" => 100000003,
                _ => throw UnknownChoice(optionSet, label)
            },
            "action-status" => label switch
            {
                "Sent" => 100000000,
                "Skipped" => 100000001,
                "Failed" => 100000002,
                "Retried" => 100000003,
                "Completed" => 100000004,
                _ => throw UnknownChoice(optionSet, label)
            },
            "escalation-channel" => label switch
            {
                "Email" => 100000000,
                "Teams" => 100000001,
                "Manual" => 100000002,
                _ => throw UnknownChoice(optionSet, label)
            },
            _ => throw new InvalidOperationException($"Unknown option set mapping '{optionSet}'.")
        };

        return new OptionSetValue(value);
    }

    private static InvalidOperationException UnknownChoice(string optionSet, string label)
    {
        return new InvalidOperationException($"Unknown choice label '{label}' for option set '{optionSet}'.");
    }
}