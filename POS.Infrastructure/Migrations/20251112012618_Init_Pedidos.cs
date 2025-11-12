using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace POS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init_Pedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rol",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EstadoUsuario = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Activo"),
                    RolId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_RolId",
                        column: x => x.RolId,
                        principalSchema: "dbo",
                        principalTable: "Rol",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pedido",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Impuestos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    EstadoPedido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pendiente")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_Cliente_ClienteId",
                        column: x => x.ClienteId,
                        principalSchema: "dbo",
                        principalTable: "Cliente",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pedido_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "dbo",
                        principalTable: "Usuario",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PedidoDetalle",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DescuentoPorc = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    ImpuestoPorc = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 13m),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoDetalle_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalSchema: "dbo",
                        principalTable: "Pedido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoDetalle_Producto_ProductoId",
                        column: x => x.ProductoId,
                        principalSchema: "dbo",
                        principalTable: "Producto",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Rol",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Vendedor" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_ClienteId",
                schema: "dbo",
                table: "Pedido",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Estado",
                schema: "dbo",
                table: "Pedido",
                column: "EstadoPedido");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_Fecha",
                schema: "dbo",
                table: "Pedido",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_UsuarioId",
                schema: "dbo",
                table: "Pedido",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalle_PedidoId",
                schema: "dbo",
                table: "PedidoDetalle",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoDetalle_ProductoId",
                schema: "dbo",
                table: "PedidoDetalle",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "UQ_Rol_Nombre",
                schema: "dbo",
                table: "Rol",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_RolId",
                schema: "dbo",
                table: "Usuario",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PedidoDetalle",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Pedido",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Usuario",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Rol",
                schema: "dbo");
        }
    }
}
