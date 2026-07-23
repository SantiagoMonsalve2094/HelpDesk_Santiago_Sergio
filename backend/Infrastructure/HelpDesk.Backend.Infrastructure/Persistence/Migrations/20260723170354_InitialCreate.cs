using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "support_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticket_number_sequences",
                columns: table => new
                {
                    year = table.Column<int>(type: "int", nullable: false),
                    last_value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_number_sequences", x => x.year);
                    table.CheckConstraint("ck_ticket_number_sequences_last_value", "[last_value] BETWEEN 0 AND 999999");
                    table.CheckConstraint("ck_ticket_number_sequences_year", "[year] BETWEEN 2000 AND 9999");
                });

            migrationBuilder.CreateTable(
                name: "sla_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    response_time_ticks = table.Column<long>(type: "bigint", nullable: false),
                    support_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sla_policies", x => x.id);
                    table.CheckConstraint("ck_sla_policies_response_time", "[response_time_ticks] > 0");
                    table.ForeignKey(
                        name: "FK_sla_policies_support_categories_support_category_id",
                        column: x => x.support_category_id,
                        principalTable: "support_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    technician_max_active_tickets = table.Column<int>(type: "int", nullable: true),
                    supervisor_support_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("ck_users_role_profile", "([role] = 'Technician' AND [technician_max_active_tickets] IS NOT NULL AND [supervisor_support_category_id] IS NULL) OR ([role] = 'Supervisor' AND [technician_max_active_tickets] IS NULL AND [supervisor_support_category_id] IS NOT NULL) OR ([role] IN ('User', 'SuperAdmin') AND [technician_max_active_tickets] IS NULL AND [supervisor_support_category_id] IS NULL)");
                    table.CheckConstraint("ck_users_technician_capacity", "[technician_max_active_tickets] IS NULL OR [technician_max_active_tickets] > 0");
                    table.ForeignKey(
                        name: "FK_users_support_categories_supervisor_support_category_id",
                        column: x => x.supervisor_support_category_id,
                        principalTable: "support_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "technician_categories",
                columns: table => new
                {
                    support_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    technician_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_technician_categories", x => new { x.technician_user_id, x.support_category_id });
                    table.ForeignKey(
                        name: "FK_technician_categories_support_categories_support_category_id",
                        column: x => x.support_category_id,
                        principalTable: "support_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_technician_categories_users_technician_user_id",
                        column: x => x.technician_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ticket_number = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    support_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    current_technician_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    resolved_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    closed_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tickets", x => x.id);
                    table.ForeignKey(
                        name: "FK_tickets_support_categories_support_category_id",
                        column: x => x.support_category_id,
                        principalTable: "support_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tickets_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tickets_users_current_technician_user_id",
                        column: x => x.current_technician_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ticket_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    technician_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ticket_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_assignments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_assignments_users_assigned_by_user_id",
                        column: x => x.assigned_by_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ticket_assignments_users_technician_user_id",
                        column: x => x.technician_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ticket_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    type = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    satisfies_resolution_requirement = table.Column<bool>(type: "bit", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_comments_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_comments_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ticket_sla_cycles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    trigger = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    support_category_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    duration_ticks = table.Column<long>(type: "bigint", nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    deadline_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    responded_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    breached_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    responsible_technician_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ticket_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_sla_cycles", x => x.id);
                    table.CheckConstraint("ck_ticket_sla_cycles_duration", "[duration_ticks] > 0");
                    table.ForeignKey(
                        name: "FK_ticket_sla_cycles_support_categories_support_category_id",
                        column: x => x.support_category_id,
                        principalTable: "support_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ticket_sla_cycles_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_sla_cycles_users_responsible_technician_user_id",
                        column: x => x.responsible_technician_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ticket_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    previous_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    new_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changed_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_automatic = table.Column<bool>(type: "bit", nullable: false),
                    changed_at_utc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ticket_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_ticket_status_history_tickets_ticket_id",
                        column: x => x.ticket_id,
                        principalTable: "tickets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticket_status_history_users_changed_by_user_id",
                        column: x => x.changed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ux_sla_policies_category_priority",
                table: "sla_policies",
                columns: new[] { "support_category_id", "priority" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_support_categories_name",
                table: "support_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_technician_categories_support_category_id",
                table: "technician_categories",
                column: "support_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_assignments_assigned_by_user_id",
                table: "ticket_assignments",
                column: "assigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_assignments_technician_user_id",
                table: "ticket_assignments",
                column: "technician_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_assignments_ticket_assigned_at_utc",
                table: "ticket_assignments",
                columns: new[] { "ticket_id", "assigned_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_ticket_comments_author_user_id",
                table: "ticket_comments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_comments_ticket_created_at_utc",
                table: "ticket_comments",
                columns: new[] { "ticket_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_sla_cycles_outcome_deadline_at_utc",
                table: "ticket_sla_cycles",
                columns: new[] { "outcome", "deadline_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_sla_cycles_responsible_technician_user_id",
                table: "ticket_sla_cycles",
                column: "responsible_technician_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ticket_sla_cycles_support_category_id",
                table: "ticket_sla_cycles",
                column: "support_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_sla_cycles_ticket_started_at_utc",
                table: "ticket_sla_cycles",
                columns: new[] { "ticket_id", "started_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_ticket_status_history_changed_by_user_id",
                table: "ticket_status_history",
                column: "changed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_status_history_ticket_changed_at_utc",
                table: "ticket_status_history",
                columns: new[] { "ticket_id", "changed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_creator_user_id",
                table: "tickets",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_current_technician_user_id",
                table: "tickets",
                column: "current_technician_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tickets_status_created_at_utc",
                table: "tickets",
                columns: new[] { "status", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_tickets_support_category_id",
                table: "tickets",
                column: "support_category_id");

            migrationBuilder.CreateIndex(
                name: "ux_tickets_ticket_number",
                table: "tickets",
                column: "ticket_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role_is_active",
                table: "users",
                columns: new[] { "role", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_users_supervisor_support_category_id",
                table: "users",
                column: "supervisor_support_category_id");

            migrationBuilder.CreateIndex(
                name: "ux_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sla_policies");

            migrationBuilder.DropTable(
                name: "technician_categories");

            migrationBuilder.DropTable(
                name: "ticket_assignments");

            migrationBuilder.DropTable(
                name: "ticket_comments");

            migrationBuilder.DropTable(
                name: "ticket_number_sequences");

            migrationBuilder.DropTable(
                name: "ticket_sla_cycles");

            migrationBuilder.DropTable(
                name: "ticket_status_history");

            migrationBuilder.DropTable(
                name: "tickets");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "support_categories");
        }
    }
}
