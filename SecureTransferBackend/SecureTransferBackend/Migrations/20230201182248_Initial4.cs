using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecureTransferBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EncryptedSymmetricKey",
                table: "DecryptorKeys",
                type: "character varying(750)",
                maxLength: 750,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EncryptedSymmetricKey",
                table: "DecryptorKeys",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(750)",
                oldMaxLength: 750);
        }
    }
}
