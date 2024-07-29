# Web.Common
Common library to build solutions based on .net web stack

## Custom Workflow Engine
Set of class libraries to build custom workflow based solution. 
Workflow can be applicable to any kind of persistent ready metadata entities like EF core entities to save them in DB.
Simple example definition file can look like this:

```json
{
  "Version": "1.0",
  "VersionComment": "Document Base workflow test",
  "StateDefinitions": [
    {
      "Title": "Přijatý",
      "Code": "Received",
      "IsStart": true
    },
    {
      "Title": "Ke zpracování",
      "Code": "ForProcessor"
    },
    {
      "Title": "Zaúčtovaný",
      "Code": "Processed",
      "IsEnd": true
    }
  ],
  "UserTransitions": [
    {
      "Code": "ForProcessor",
      "CompletionMail": {
        "Text": {
          "Operand": {
            "Type": "Computed",
            "Operator": "Format",
            "Operands": [
              {
                "Text": "Vaše <b>vyjádření<\/b> již není potřeba, úkol dokončil uživatel {0}."
              },
              {
                "DataType": "User",
                "Format": "DisplayName",
                "Fetcher": "CurrentWorkflowUserId"
              }
            ]
          }
        },
        "Subject": {
          "Operand": { "Text": "Ukol - Workflow" }
        }
      },
      "Transitions": [
        {
          "State": "Processed",
          "StateManagementTrigger": {
            "Name": "Zpracovat",
            "Order": 1,
            "Parameters": [
              {
                "Required": true,
                "PropertyName": "Approver",
                "DisplayName": "Uživatel",
                "Type": "User"
              }
            ],
            "History": [
              {
                "Entry": {
                  "Operand": {
                    "Type": "Computed",
                    "Operator": "Format",
                    "Operands": [
                      {
                        "Text": "Zpracováno dne : {0}"
                      },
                      {
                        "Fetcher": "CurrentDate"
                      }
                    ]
                  }
                },
                "Message": {
                  "Operand": { "Text": "Komentář zpracovatele" }
                }
              }
            ]
          },
          "AdditionalActions": [
            {
              "Id": "UpdateMetadata",
              "TypeName": "RhDev.Common.Workflow.Impl.Actions.Definition.StateManagementUpdateMetadataAction",
              "Parameters": [
                {
                  "Name": "DocumentMiningHint",
                  "Operand": { "Text": "Zpracováno" }
                },
                {
                  "Name": "WorkflowAssignedTo",
                  "Operand": { "DataType": "Null" }
                }
              ]
            }
          ]
        }
      ]
    }
  ],
  "SystemTransitions": [
    {
      "Code": "Received",
      "Transitions": [
        {
          "State": "ForProcessor",
          "Condition": {
            "Or": [
              {
                "Operator": "Less",
                "Operands": [
                  {
                    "Type": "Metadata",
                    "Text": "Id"
                  },
                  { "Text": "1000" }
                ]
              },
              {
                "Operator": "Greater",
                "Operands": [
                  {
                    "Type": "Metadata",
                    "Text": "Id"
                  },
                  {
                    "Text": "0"
                  }
                ]
              }
            ]
          },
          "Task": {
            "GroupExtract": true,
            "TaskRespondeType": "FirstWin",
            "Assignee": [
              {
                "DataType": "User",
                "Text": "Editor"
              }
            ]
          },
          "StateManagementTrigger": {
            "Parameters": [
              {
                "PropertyName": "Datum",
                "Type": "DateTime",
                "Required": true
              }
            ],
            "History": [
              {
                "Entry": {
                  "Operand": {
                    "Type": "Computed",
                    "Operator": "Format",
                    "Operands": [
                      {
                        "Text": "Zpracováno dne : {0}"
                      },
                      {
                        "Fetcher": "CurrentDate"
                      }
                    ]
                  }
                }
              }
            ],
            "Message": {
              "Operand": {
                "Type": "Computed",
                "Operator": "Format",
                "Operands": [
                  { "Text": "Komentář : {1}, {2}, {3}, {10}, {4}, {5} --- {5}, {6} ;;;; {1}" },
                  {
                    "Fetcher": "CurrentDate"
                  },
                  {
                    "Type": "Metadata",
                    "Text": "CreatedById",
                    "Format": "email"
                  },
                  {
                    "Type": "Metadata",
                    "Text": "Id"
                  },
                  {
                    "Type": "Metadata",
                    "Text": "LastModified"
                  },
                  {
                    "Type": "Client",
                    "Text": "Datum"
                  }
                ]
              }
            }
          },
          "AdditionalActions": [
            {
              "Id": "UpdateMetadata1",
              "TypeName": "RhDev.Common.Workflow.Impl.Actions.Definition.StateManagementUpdateMetadataAction",
              "Parameters": [
                {
                  "Name": "MiningScore",
                  "Operand": {
                    "Type": "Computed",
                    "Operator": "Add",
                    "Operands": [
                      { "Text": "10" },
                      { "Text": "25" }
                    ]
                  }
                },
                {
                  "Name": "LastModified",
                  "Operand": {
                    "Fetcher": "CurrentDate"
                  }
                },
                {
                  "Name": "WorkflowAssignedTo",
                  "Operand": {
                    "DataType": "User",
                    "Text": "Editor"
                  }
                },
                {
                  "Name": "DocumentMiningHint",
                  "Operand": {
                    "Type": "Computed",
                    "Operator": "Format",
                    "Operands": [
                      { "Text": "{0} ..." },
                      {
                        "Type": "Workflow",
                        "Format": "Name",
                        "Text": "SuperValue"
                      }
                    ]
                  }
                },
                {
                  "Name": "MinedSuccessfully",
                  "Operand": {
                    "Type": "Computed",
                    "Operator": "Neg",
                    "Operands": [
                      {
                        "Type": "Metadata",
                        "Text": "MinedSuccessfully"
                      }
                    ]
                  }
                }
              ]
            }
          ]
        }
      ]
    }
  ]
}

```
