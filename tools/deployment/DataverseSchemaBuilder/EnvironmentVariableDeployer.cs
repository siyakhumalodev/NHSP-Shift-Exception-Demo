using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

internal sealed class EnvironmentVariableDeployer
{
    private const int TextType = 100000000;
    private const int NumberType = 100000001;
    private const int BooleanType = 100000002;
    private const int EnvironmentVariableDefinitionComponentType = 380;
    private const int EnvironmentVariableValueComponentType = 381;

    private readonly IOrganizationService service;
    private readonly string solutionUniqueName;

    public EnvironmentVariableDeployer(IOrganizationService service, string solutionUniqueName)
    {
        this.service = service;
        this.solutionUniqueName = solutionUniqueName;
    }

    public void Deploy()
    {
        Console.WriteLine("Deploying environment variables...");

        Upsert("nhsp_SupportMailbox", "NHSP Support Mailbox", "Demo-safe mailbox used for flow failure and support notifications.", TextType, "nhsp-demo-support@example.invalid");
        Upsert("nhsp_DefaultTrustContactEmail", "NHSP Default Trust Contact Email", "Demo-safe fallback recipient for trust escalation routing.", TextType, "trust-contact-demo@example.invalid");
        Upsert("nhsp_CriticalEscalationDelayMinutes", "NHSP Critical Escalation Delay Minutes", "Delay before rechecking Critical shift exceptions in the demo environment.", NumberType, "5");
        Upsert("nhsp_HighEscalationDelayMinutes", "NHSP High Escalation Delay Minutes", "Delay before rechecking High shift exceptions in the demo environment.", NumberType, "10");
        Upsert("nhsp_AppDeepLinkUrl", "NHSP App Deep Link URL", "Link included in notifications after the canvas app is published.", TextType, "https://make.powerapps.com/environments/08941cf4-52e5-e907-9f93-85a5627deb0e/apps");
        Upsert("nhsp_EnableExternalNotifications", "NHSP Enable External Notifications", "Controls whether external trust contact notifications can be sent. Keep false for the demo unless explicitly approved.", BooleanType, "false");

        Console.WriteLine("Environment variable deployment complete.");
    }

    private void Upsert(string schemaName, string displayName, string description, int type, string value)
    {
        var definition = FindDefinition(schemaName);
        Guid definitionId;

        if (definition is null)
        {
            var entity = new Entity("environmentvariabledefinition")
            {
                ["schemaname"] = schemaName,
                ["displayname"] = displayName,
                ["description"] = description,
                ["type"] = new OptionSetValue(type),
                ["defaultvalue"] = value
            };

            definitionId = service.Create(entity);
            Console.WriteLine($"Created: environment variable definition {schemaName}");
        }
        else
        {
            definitionId = definition.Id;
            var entity = new Entity("environmentvariabledefinition", definitionId)
            {
                ["displayname"] = displayName,
                ["description"] = description,
                ["type"] = new OptionSetValue(type),
                ["defaultvalue"] = value
            };

            service.Update(entity);
            Console.WriteLine($"Updated: environment variable definition {schemaName}");
        }

        AddToSolution(definitionId, EnvironmentVariableDefinitionComponentType, "environment variable definition");
        UpsertValue(definitionId, schemaName, value);
    }

    private Entity? FindDefinition(string schemaName)
    {
        var query = new QueryExpression("environmentvariabledefinition")
        {
            ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname"),
            TopCount = 1
        };
        query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, schemaName);

        return service.RetrieveMultiple(query).Entities.FirstOrDefault();
    }

    private void UpsertValue(Guid definitionId, string schemaName, string value)
    {
        var existing = FindValue(definitionId);
        if (existing is null)
        {
            var entity = new Entity("environmentvariablevalue")
            {
                ["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", definitionId),
                ["value"] = value
            };

            var valueId = service.Create(entity);
            AddToSolution(valueId, EnvironmentVariableValueComponentType, "environment variable value");
            Console.WriteLine($"Created: environment variable value {schemaName} = {value}");
            return;
        }

        var update = new Entity("environmentvariablevalue", existing.Id)
        {
            ["value"] = value
        };

        service.Update(update);
        AddToSolution(existing.Id, EnvironmentVariableValueComponentType, "environment variable value");
        Console.WriteLine($"Updated: environment variable value {schemaName} = {value}");
    }

    private Entity? FindValue(Guid definitionId)
    {
        var query = new QueryExpression("environmentvariablevalue")
        {
            ColumnSet = new ColumnSet("environmentvariablevalueid", "value"),
            TopCount = 1
        };
        query.Criteria.AddCondition("environmentvariabledefinitionid", ConditionOperator.Equal, definitionId);

        return service.RetrieveMultiple(query).Entities.FirstOrDefault();
    }

    private void AddToSolution(Guid componentId, int componentType, string label)
    {
        try
        {
            service.Execute(new AddSolutionComponentRequest
            {
                ComponentId = componentId,
                ComponentType = componentType,
                SolutionUniqueName = solutionUniqueName,
                AddRequiredComponents = true,
                DoNotIncludeSubcomponents = false
            });

            Console.WriteLine($"Added to solution: {label} {componentId}");
        }
        catch (FaultException<OrganizationServiceFault> ex) when (IsAlreadyInSolution(ex))
        {
            Console.WriteLine($"Exists in solution: {label} {componentId}");
        }
    }

    private static bool IsAlreadyInSolution(FaultException<OrganizationServiceFault> ex)
    {
        var message = ex.Detail.Message ?? string.Empty;
        return message.Contains("already", StringComparison.OrdinalIgnoreCase)
            || message.Contains("exists", StringComparison.OrdinalIgnoreCase);
    }
}
