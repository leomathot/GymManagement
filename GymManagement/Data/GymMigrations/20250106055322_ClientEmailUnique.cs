﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Data.GymMigrations
{
    /// <inheritdoc />
    public partial class ClientEmailUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                table: "Clients");
        }
    }
}
