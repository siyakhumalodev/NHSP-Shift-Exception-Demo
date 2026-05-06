using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

internal sealed class CloudFlowDeployer
{
    private const int ModernFlowCategory = 5;
    private const int DefinitionType = 1;
    private const int WorkflowComponentType = 29;

    private readonly IOrganizationService service;
    private readonly string solutionUniqueName;

    public CloudFlowDeployer(IOrganizationService service, string solutionUniqueName)
    {
        this.service = service;
        this.solutionUniqueName = solutionUniqueName;
    }

    public Guid Deploy(string flowName, string description, string clientDataPath)
    {
        var resolvedClientDataPath = Path.GetFullPath(clientDataPath);
        if (!File.Exists(resolvedClientDataPath))
        {
            throw new FileNotFoundException($"Flow clientdata file not found: {resolvedClientDataPath}", resolvedClientDataPath);
        }

        var clientData = File.ReadAllText(resolvedClientDataPath);
        var existing = FindFlow(flowName);
        Guid workflowId;

        if (existing is null)
        {
            var workflow = new Entity("workflow")
            {
                ["category"] = new OptionSetValue(ModernFlowCategory),
                ["name"] = flowName,
                ["type"] = new OptionSetValue(DefinitionType),
                ["description"] = description,
                ["primaryentity"] = "none",
                ["clientdata"] = clientData
            };

            workflowId = service.Create(workflow);
            Console.WriteLine($"Created: cloud flow {flowName} ({workflowId})");
        }
        else
        {
            workflowId = existing.Id;
            TryTurnOff(workflowId);

            var workflow = new Entity("workflow", workflowId)
            {
                ["description"] = description,
                ["clientdata"] = clientData
            };

            service.Update(workflow);
            Console.WriteLine($"Updated: cloud flow {flowName} ({workflowId})");
        }

        AddToSolution(workflowId);
        return workflowId;
    }

    private Entity? FindFlow(string flowName)
    {
        var query = new QueryExpression("workflow")
        {
            ColumnSet = new ColumnSet("workflowid", "name", "category", "statecode", "type"),
            TopCount = 1
        };
        query.Criteria.AddCondition("name", ConditionOperator.Equal, flowName);
        query.Criteria.AddCondition("category", ConditionOperator.Equal, ModernFlowCategory);
        query.Criteria.AddCondition("type", ConditionOperator.Equal, DefinitionType);

        return service.RetrieveMultiple(query).Entities.FirstOrDefault();
    }

    private void TryTurnOff(Guid workflowId)
    {
        try
        {
            var workflow = new Entity("workflow", workflowId)
            {
                ["statecode"] = new OptionSetValue(0)
            };

            service.Update(workflow);
        }
        catch (FaultException<OrganizationServiceFault> ex)
        {
            Console.WriteLine($"Warning: could not set existing flow to draft/off before update: {ex.Detail.Message}");
        }
    }

    private void AddToSolution(Guid workflowId)
    {
        try
        {
            service.Execute(new AddSolutionComponentRequest
            {
                ComponentId = workflowId,
                ComponentType = WorkflowComponentType,
                SolutionUniqueName = solutionUniqueName,
                AddRequiredComponents = true,
                DoNotIncludeSubcomponents = false
            });

            Console.WriteLine($"Added to solution: workflow component {workflowId}");
        }
        catch (FaultException<OrganizationServiceFault> ex) when (IsAlreadyInSolution(ex))
        {
            Console.WriteLine($"Exists in solution: workflow component {workflowId}");
        }
    }

    private static bool IsAlreadyInSolution(FaultException<OrganizationServiceFault> ex)
    {
        var message = ex.Detail.Message ?? string.Empty;
        return message.Contains("already", StringComparison.OrdinalIgnoreCase)
            || message.Contains("exists", StringComparison.OrdinalIgnoreCase);
    }
}
