using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RhDev.Customer.Component.App.Data.Migrations
{
    /// <inheritdoc />
    public partial class Workflow_Engine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_CreatedById",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_LastModifiedById",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CreatedById",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LastModifiedById",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastModifiedById",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "ApplicationUserApplicationUser",
                columns: table => new
                {
                    AuthoredUsersId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ModifiedUsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserApplicationUser", x => new { x.AuthoredUsersId, x.ModifiedUsersId });
                    table.ForeignKey(
                        name: "FK_ApplicationUserApplicationUser_AspNetUsers_AuthoredUsersId",
                        column: x => x.AuthoredUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserApplicationUser_AspNetUsers_ModifiedUsersId",
                        column: x => x.ModifiedUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    WorkflowDocumentIdentificator = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitionRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Payload = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Finished = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResultData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    TranDuration = table.Column<double>(type: "float", nullable: false),
                    RepeatCount = table.Column<double>(type: "float", nullable: false),
                    LastInitiatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Workflow = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TransitionType = table.Column<int>(type: "int", nullable: false),
                    DocumentReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitionRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowDocumentId = table.Column<int>(type: "int", nullable: false),
                    InstanceId = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    RunVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CurrentTaskGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Started = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Finished = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WorkflowState = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    WorkflowStateSystem = table.Column<bool>(type: "bit", nullable: false),
                    InitiatorId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkflowDefinition = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    LastSystemProcessing = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFailed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstances_WorkflowDocuments_WorkflowDocumentId",
                        column: x => x.WorkflowDocumentId,
                        principalTable: "WorkflowDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Event = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceHistory_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceSystemProcessingInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    Stamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Header = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceSystemProcessingInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceSystemProcessingInfo_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowProperties_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    AssigneeType = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResolvedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserData = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    SelectedTriggerCode = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    TaskRespondeType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTransitionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransitionId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromState = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ToState = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    BeginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Handler = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkflowInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTransitionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTransitionLogs_WorkflowInstances_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "WorkflowInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserApplicationUser_ModifiedUsersId",
                table: "ApplicationUserApplicationUser",
                column: "ModifiedUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceHistory_WorkflowInstanceId",
                table: "WorkflowInstanceHistory",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstances_WorkflowDocumentId",
                table: "WorkflowInstances",
                column: "WorkflowDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceSystemProcessingInfo_WorkflowInstanceId",
                table: "WorkflowInstanceSystemProcessingInfo",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowProperties_WorkflowInstanceId",
                table: "WorkflowProperties",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignedTo",
                table: "WorkflowTasks",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowInstanceId",
                table: "WorkflowTasks",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTransitionLogs_WorkflowInstanceId",
                table: "WorkflowTransitionLogs",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserApplicationUser");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceHistory");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceSystemProcessingInfo");

            migrationBuilder.DropTable(
                name: "WorkflowProperties");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "WorkflowTransitionLogs");

            migrationBuilder.DropTable(
                name: "WorkflowTransitionRequest");

            migrationBuilder.DropTable(
                name: "WorkflowInstances");

            migrationBuilder.DropTable(
                name: "WorkflowDocuments");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedById",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CreatedById",
                table: "AspNetUsers",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LastModifiedById",
                table: "AspNetUsers",
                column: "LastModifiedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_CreatedById",
                table: "AspNetUsers",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_LastModifiedById",
                table: "AspNetUsers",
                column: "LastModifiedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
