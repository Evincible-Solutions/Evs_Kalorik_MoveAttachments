using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Activities;

namespace Evs_Kalorik_MoveAttachments
{
    public class Class1 : CodeActivity
    {
        protected override void Execute(CodeActivityContext Execution)
        {
            IWorkflowContext context = Execution.GetExtension<IWorkflowContext>();
            //ITracingService tracingService = (ITracingService)Execution.GetService(typeof(ITracingService));
            ITracingService tracingService = Execution.GetExtension<ITracingService>();
            //create iorganization service object
            tracingService.Trace("Tracing : Execute");
            IOrganizationServiceFactory serviceFactory = Execution.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.InitiatingUserId);

            try
            {
                //Entity cloneRecord = new Entity("evs_accountapplication");
                Guid PrimaryID = context.PrimaryEntityId;
                Entity accountApplication = service.Retrieve("evs_accountapplication", PrimaryID, new ColumnSet(true));
                Entity cloneRecord = service.Retrieve("evs_accountapplication", PrimaryID, new ColumnSet(true));
                //tracingService.Trace("Cloned Account" + clonedAccount);

                if (cloneRecord.Contains("evs_parentaccountapplication"))
                {
                    Guid parentReferenceId = cloneRecord.GetAttributeValue<EntityReference>("evs_parentaccountapplication").Id;
                    tracingService.Trace("Parent Ref" + parentReferenceId);


                    QueryExpression annotaionQuery = new QueryExpression("annotation");
                    annotaionQuery.Criteria.AddCondition("objectid", ConditionOperator.Equal, parentReferenceId);
                    annotaionQuery.ColumnSet = new ColumnSet(true);
                    var Notes = service.RetrieveMultiple(annotaionQuery);
                    //if (parentReference != null)

                    foreach (Entity note in Notes.Entities)
                    {
                        Entity clonedNote = new Entity("annotation");
                        clonedNote["objectid"] = new EntityReference("evs_accountapplication", PrimaryID);
                        clonedNote["objecttypecode"] = "evs_accountapplication";

                        foreach (var attribute in note.Attributes)
                        {
                            if (attribute.Key == "annotationid" || attribute.Key == "objectid" || attribute.Key == "objecttypecode")
                            {
                                continue;
                            }
                            else
                            {
                                clonedNote[attribute.Key] = attribute.Value;
                            }
                        }

                        Guid clonedNoteId = service.Create(clonedNote);
                        tracingService.Trace("Cloned Note Created: " + clonedNoteId);


                    }


                    //--- creating Credit Approval Application Entity ---//
                    Entity CreditApprovalApplication = new Entity("evs_creditapprovalapplication");

                    //Guid CreditApprovalApplicationId = service.Create(CreditApprovalApplication);

                    //check new_creditapproval approval for parent account 
                    if (accountApplication.Contains("new_creditapproval"))
                    {
                        EntityReference parentRef = accountApplication.GetAttributeValue<EntityReference>("new_creditapproval");
                        Guid parentId = parentRef.Id;

                        Entity parentRecord = service.Retrieve(parentRef.LogicalName, parentId, new ColumnSet(true));


                        if (cloneRecord.Contains("evs_name"))
                        {
                            CreditApprovalApplication["evs_name"] = "GCAA:100019";
                        }

                        if (cloneRecord.Contains("evs_name"))
                        {
                            CreditApprovalApplication["evs_clientname"] = cloneRecord.GetAttributeValue<string>("evs_name");
                        }

                        if (cloneRecord.Contains("evs_name"))
                        {
                            CreditApprovalApplication["evs_businessname"] = cloneRecord.GetAttributeValue<string>("evs_name");
                        }

                        //if (parentRecord.Contains("evs_streetaddress"))
                        //{
                        //    CreditApprovalApplication["evs_streetaddress"] = parentRecord.GetAttributeValue<string>("evs_streetaddress");
                        //}

                        //if (parentRecord.Contains("evs_city"))
                        //{
                        //    CreditApprovalApplication["evs_city"] = parentRecord.GetAttributeValue<string>("evs_city");
                        //}

                        //if (parentRecord.Contains("evs_state"))
                        //{
                        //    CreditApprovalApplication["evs_state"] = parentRecord.GetAttributeValue<string>("evs_state");
                        //}

                        //if (parentRecord.Contains("evs_zip"))
                        //{
                        //    CreditApprovalApplication["evs_zip"] = parentRecord.GetAttributeValue<string>("evs_zip");
                        //}

                        //if (parentRecord.Contains("evs_phone"))
                        //{
                        //    CreditApprovalApplication["evs_phone"] = parentRecord.GetAttributeValue<string>("evs_phone");
                        //}

                        //if (parentRecord.Contains("evs_dba"))
                        //{
                        //    CreditApprovalApplication["evs_dba"] = parentRecord.GetAttributeValue<string>("evs_dba");
                        //}

                        //if (parentRecord.Contains("evs_businessdata"))
                        //{
                        //    var business = accountApplication.GetAttributeValue<OptionSetValueCollection>("evs_businessdata");
                        //    CreditApprovalApplication["evs_businessdata"] = new OptionSetValueCollection(business);
                        //}

                        //if (parentRecord.Contains("new_creditamtrequested"))
                        //{
                           
                        //    int wholeNumberValue = parentRecord.GetAttributeValue<int>("new_creditamtrequested");
                        //    CreditApprovalApplication["new_creditamtrequested"] = wholeNumberValue;
                        //}

                        Guid CreditApprovalApplicationId = service.Create(CreditApprovalApplication);

                        EntityReference tradingEntityReference = new EntityReference("evs_creditapprovalapplication", CreditApprovalApplicationId);
                        cloneRecord["new_creditapproval"] = tradingEntityReference;
                        service.Update(cloneRecord);
                       
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
    }
}

