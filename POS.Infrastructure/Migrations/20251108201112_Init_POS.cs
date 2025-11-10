using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_POS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "dbo");

            migrationBuilder.CreateTable(
                name: "Cliente",
                schema: "dbo",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Cedula = table.Column<string>(
                        type: "nvarchar(40)",
                        maxLength: 40,
                        nullable: true
                    ),
                    Correo = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: true
                    ),
                    Telefono = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: true
                    ),
                    Direccion = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    FechaRegistro = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "GETDATE()"
                    ),
                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true,
                        defaultValueSql: "GETDATE()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Producto",
                schema: "dbo",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(
                        type: "nvarchar(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(
                        type: "decimal(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    ImpuestoPorc = table.Column<decimal>(
                        type: "decimal(5,2)",
                        precision: 5,
                        scale: 2,
                        nullable: false
                    ),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    ImagenUrl = table.Column<string>(
                        type: "nvarchar(512)",
                        maxLength: 512,
                        nullable: true
                    ),
                    EstadoProducto = table.Column<string>(
                        type: "nvarchar(10)",
                        maxLength: 10,
                        nullable: false
                    ),
                    FechaRegistro = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false,
                        defaultValueSql: "GETDATE()"
                    ),
                    FechaModificacion = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true,
                        defaultValueSql: "GETDATE()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producto", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_Cedula",
                schema: "dbo",
                table: "Cliente",
                column: "Cedula"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Producto_Nombre",
                schema: "dbo",
                table: "Producto",
                column: "Nombre"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Cliente", schema: "dbo");

            migrationBuilder.DropTable(name: "Producto", schema: "dbo");
        }
    }
}
