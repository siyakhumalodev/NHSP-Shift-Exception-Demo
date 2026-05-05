using System.ServiceModel;
using Azure.Core;
using Azure.Identity;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

var options = ParseArgs(args);
var environmentUrl = options.GetValueOrDefault("environment-url", "https://org1aa19a83.crm.dynamics.com");
var solutionUniqueName = options.GetValueOrDefault("solution-unique-name", "nhspshiftexceptiondemo");

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