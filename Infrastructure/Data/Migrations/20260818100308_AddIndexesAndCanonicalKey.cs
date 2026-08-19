using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesAndCanonicalKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_UserId",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Message_Reads_UserId",
                table: "Message_Reads");

            migrationBuilder.DropIndex(
                name: "IX_Message_Reactions_UserId",
                table: "Message_Reactions");

            migrationBuilder.AddColumn<string>(
                name: "CanonicalKey",
                table: "Conversations",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participants_UserId_ConversationId",
                table: "Participants",
                columns: new[] { "UserId", "ConversationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_CreatedAt",
                table: "Messages",
                columns: new[] { "SenderId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_Reads_UserId_ReadAt",
                table: "Message_Reads",
                columns: new[] { "UserId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_Reactions_UserId_EmojiCode",
                table: "Message_Reactions",
                columns: new[] { "UserId", "EmojiCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CanonicalKey",
                table: "Conversations",
                column: "CanonicalKey",
                unique: true,
                filter: "[CanonicalKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Participants_UserId_ConversationId",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId_CreatedAt",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Message_Reads_UserId_ReadAt",
                table: "Message_Reads");

            migrationBuilder.DropIndex(
                name: "IX_Message_Reactions_UserId_EmojiCode",
                table: "Message_Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_CanonicalKey",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "CanonicalKey",
                table: "Conversations");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_UserId",
                table: "Participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_Reads_UserId",
                table: "Message_Reads",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_Reactions_UserId",
                table: "Message_Reactions",
                column: "UserId");
        }
    }
}
