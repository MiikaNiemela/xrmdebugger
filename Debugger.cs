/**
 *    
 * 
 *    Copyright (C) 2015  Miika Niemelä
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace MiikaNiemela.XRMDebugger
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class Debugger
    {
        public static void debugContext(IPluginExecutionContext context, IOrganizationService service)
        {
            Entity debugLine = getDebugline(context, service);
            service.Create(debugLine);
        }
        private static int getOptionSetValue(string entityName, string attributeName, string optionsetText, IOrganizationService service)
        {
            int optionSetValue = 0;
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            RetrieveAttributeResponse retrieveAttributeResponse =
              (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            PicklistAttributeMetadata picklistAttributeMetadata = null;
            StatusAttributeMetadata statusAttributeMetadata = null;
            OptionSetMetadata optionsetMetadata = null;
            if (retrieveAttributeResponse.AttributeMetadata is PicklistAttributeMetadata)
            {
                picklistAttributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                optionsetMetadata = picklistAttributeMetadata.OptionSet;
            }
            else if (retrieveAttributeResponse.AttributeMetadata is StatusAttributeMetadata)
            {
                statusAttributeMetadata = (StatusAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                optionsetMetadata = statusAttributeMetadata.OptionSet;
            }
            else
            {
                return 0;
            }

            foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
            {
                if (optionMetadata.Label.UserLocalizedLabel.Label.ToLower() == optionsetText.ToLower())
                {
                    optionSetValue = optionMetadata.Value.Value;
                    return optionSetValue;
                }

            }
            return optionSetValue;
        }
        private static string getOptionSetText(string entityName, string attributeName, int optionsetValue, IOrganizationService service)
        {
            string optionsetText = string.Empty;
            RetrieveAttributeRequest retrieveAttributeRequest = new RetrieveAttributeRequest();
            retrieveAttributeRequest.EntityLogicalName = entityName;
            retrieveAttributeRequest.LogicalName = attributeName;
            retrieveAttributeRequest.RetrieveAsIfPublished = true;

            RetrieveAttributeResponse retrieveAttributeResponse =
              (RetrieveAttributeResponse)service.Execute(retrieveAttributeRequest);
            PicklistAttributeMetadata picklistAttributeMetadata = null;
            StatusAttributeMetadata statusAttributeMetadata = null;
            OptionSetMetadata optionsetMetadata = null;
            if (retrieveAttributeResponse.AttributeMetadata is PicklistAttributeMetadata)
            {
                picklistAttributeMetadata = (PicklistAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                optionsetMetadata = picklistAttributeMetadata.OptionSet;
            }
            else if (retrieveAttributeResponse.AttributeMetadata is StatusAttributeMetadata)
            {
                statusAttributeMetadata = (StatusAttributeMetadata)retrieveAttributeResponse.AttributeMetadata;
                optionsetMetadata = statusAttributeMetadata.OptionSet;
            }
            else
            {
                return "label not found";
            }

            foreach (OptionMetadata optionMetadata in optionsetMetadata.Options)
            {
                if (optionMetadata.Value == optionsetValue)
                {
                    optionsetText = optionMetadata.Label.UserLocalizedLabel.Label;
                    return optionsetText;
                }

            }
            return optionsetText;
        }
        private static Entity getDebugline(IPluginExecutionContext context, IOrganizationService service)
        {
            Entity debugLine = new Entity("mp_debugline");
            // add the following info, if available.

            debugLine.Attributes.Add("mp_calldepth", (int?)context.Depth);
            debugLine.Attributes.Add("mp_calltime", DateTime.Now);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                debugLine.Attributes.Add("mp_entityguid", ((Entity)context.InputParameters["Target"]).Id.ToString());
                debugLine.Attributes.Add("mp_entityname", ((Entity)context.InputParameters["Target"]).LogicalName);
            }
            debugLine.Attributes.Add("mp_messagename", context.MessageName);
            debugLine.Attributes.Add("mp_name", DateTime.Now.Ticks.ToString());
            if (context.ParentContext != null)
            {
                if (context.ParentContext.InputParameters.Contains("Target") && context.ParentContext.InputParameters["Target"] is Entity)
                    debugLine.Attributes.Add("mp_parententityname", ((Entity)context.ParentContext.InputParameters["Target"]).LogicalName);

                debugLine.Attributes.Add("mp_parentcontext", context.ParentContext.CorrelationId.ToString());

            }
            debugLine.Attributes.Add("mp_pipelinetoken", context.CorrelationId.ToString());
            debugLine.Attributes.Add("mp_primaryentityid", context.PrimaryEntityId.ToString());
            debugLine.Attributes.Add("mp_primaryentityname", context.PrimaryEntityName);
            debugLine.Attributes.Add("mp_requestid", context.RequestId.HasValue ? context.RequestId.Value.ToString() : "empty");
            debugLine.Attributes.Add("mp_stagenumber", (int?)context.Stage);
            string inputparameterdump = "owning Extension: " + context.OwningExtension.Name + Environment.NewLine;
            foreach (var attribute in context.InputParameters)
            {
                inputparameterdump += attribute.Key + ":" + attribute.Value + Environment.NewLine;

            }
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                foreach (var entityattribute in ((Entity)context.InputParameters["Target"]).Attributes)
                {
                    inputparameterdump += " Target - " + entityattribute.Key + " : ";
                    if (entityattribute.Value is EntityReference)
                    {
                        inputparameterdump += ((EntityReference)entityattribute.Value).LogicalName + " : " + ((EntityReference)entityattribute.Value).Name + " : " + ((EntityReference)entityattribute.Value).Id.ToString() + Environment.NewLine;
                    }
                    else if (entityattribute.Value is Money)
                    {
                        inputparameterdump += ((Money)entityattribute.Value).Value.ToString("F2") + Environment.NewLine;
                    }
                    else if (entityattribute.Value is OptionSetValue)
                    {
                        string ost = getOptionSetText(((Entity)context.InputParameters["Target"]).LogicalName, entityattribute.Key, ((OptionSetValue)entityattribute.Value).Value, service);
                        inputparameterdump += ((OptionSetValue)entityattribute.Value).Value + " / " + ost + Environment.NewLine;
                    }
                    else if (entityattribute.Value is ColumnSet)
                    {
                        string columnstring = "";
                        foreach (string column in ((ColumnSet)entityattribute.Value).Columns)
                        {
                            columnstring += column + ", ";
                        }
                        inputparameterdump += columnstring + Environment.NewLine;
                    }
                    else
                    {
                        inputparameterdump += entityattribute.Value + Environment.NewLine;
                    }
                }
            }
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
            {
                inputparameterdump += "Ent Ref = " + ((EntityReference)context.InputParameters["Target"]).LogicalName + " : " + ((EntityReference)context.InputParameters["Target"]).Name + " : " + ((EntityReference)context.InputParameters["Target"]).Id.ToString() + Environment.NewLine;
            }
            if (context.InputParameters.Contains("ColumnSet") && context.InputParameters["ColumnSet"] is ColumnSet)
            {
                string columnstring = "Columnset is: ";
                foreach (string column in ((ColumnSet)context.InputParameters["ColumnSet"]).Columns)
                {
                    columnstring += column + ", ";
                }
                inputparameterdump += columnstring + Environment.NewLine;
            }
            if (context.ParentContext != null)
            {
                IPluginExecutionContext abovecontext = context.ParentContext;
                inputparameterdump += "parent contexts are:" + Environment.NewLine;
                int depth = 1;
                while (abovecontext != null)
                {
                    inputparameterdump += depth.ToString() + " entity: " + abovecontext.PrimaryEntityName + ", owning ext: " + abovecontext.OwningExtension.Name + ", message: " + abovecontext.MessageName + ", id: " + abovecontext.CorrelationId + Environment.NewLine;
                    abovecontext = abovecontext.ParentContext;
                    depth++;
                }
            }
            debugLine.Attributes.Add("mp_inputparameterdump", inputparameterdump);
            return debugLine;
        }
    }
}
