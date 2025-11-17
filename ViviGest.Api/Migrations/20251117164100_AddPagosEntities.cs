using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViviGest.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPagosEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conjunto",
                columns: table => new
                {
                    IdConjunto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conjunto", x => x.IdConjunto);
                });

            migrationBuilder.CreateTable(
                name: "EstadoCorrespondencia",
                columns: table => new
                {
                    IdEstadoCorrespondencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoCorrespondencia", x => x.IdEstadoCorrespondencia);
                });

            migrationBuilder.CreateTable(
                name: "MetodoPago",
                columns: table => new
                {
                    IdMetodoPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodoPago", x => x.IdMetodoPago);
                });

            migrationBuilder.CreateTable(
                name: "PeriodoPago",
                columns: table => new
                {
                    IdPeriodoPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Periodo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodoPago", x => x.IdPeriodoPago);
                });

            migrationBuilder.CreateTable(
                name: "Persona",
                columns: table => new
                {
                    IdPersona = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTipoDocumento = table.Column<int>(type: "int", nullable: false),
                    NumeroDocumento = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombres = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persona", x => x.IdPersona);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "TipoCorrespondencia",
                columns: table => new
                {
                    IdTipoCorrespondencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoCorrespondencia", x => x.IdTipoCorrespondencia);
                });

            migrationBuilder.CreateTable(
                name: "TipoRelacionAutorizado",
                columns: table => new
                {
                    IdTipoRelacionAutorizado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoRelacionAutorizado", x => x.IdTipoRelacionAutorizado);
                });

            migrationBuilder.CreateTable(
                name: "Torre",
                columns: table => new
                {
                    IdTorre = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdConjunto = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Torre", x => x.IdTorre);
                    table.ForeignKey(
                        name: "FK_Torre_Conjunto_IdConjunto",
                        column: x => x.IdConjunto,
                        principalTable: "Conjunto",
                        principalColumn: "IdConjunto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPersona = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContrasenaHash = table.Column<byte[]>(type: "varbinary(256)", nullable: false),
                    ContrasenaSalt = table.Column<byte[]>(type: "varbinary(128)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuario_Persona_IdPersona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitante",
                columns: table => new
                {
                    IdVisitante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPersona = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitante", x => x.IdVisitante);
                    table.ForeignKey(
                        name: "FK_Visitante_Persona_IdPersona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Unidad",
                columns: table => new
                {
                    IdUnidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTorre = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AreaM2 = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidad", x => x.IdUnidad);
                    table.ForeignKey(
                        name: "FK_Unidad_Torre_IdTorre",
                        column: x => x.IdTorre,
                        principalTable: "Torre",
                        principalColumn: "IdTorre",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PersonaAutorizada",
                columns: table => new
                {
                    IdPersonaAutorizada = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUsuarioResidente = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPersona = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTipoRelacionAutorizado = table.Column<int>(type: "int", nullable: false),
                    PlacaVehiculo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonaAutorizada", x => x.IdPersonaAutorizada);
                    table.ForeignKey(
                        name: "FK_PersonaAutorizada_Persona_IdPersona",
                        column: x => x.IdPersona,
                        principalTable: "Persona",
                        principalColumn: "IdPersona",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonaAutorizada_TipoRelacionAutorizado_IdTipoRelacionAutorizado",
                        column: x => x.IdTipoRelacionAutorizado,
                        principalTable: "TipoRelacionAutorizado",
                        principalColumn: "IdTipoRelacionAutorizado",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonaAutorizada_Usuario_IdUsuarioResidente",
                        column: x => x.IdUsuarioResidente,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRol",
                columns: table => new
                {
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRol", x => new { x.IdUsuario, x.IdRol });
                    table.ForeignKey(
                        name: "FK_UsuarioRol_Rol_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Rol",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRol_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargoCuenta",
                columns: table => new
                {
                    IdCargoCuenta = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUnidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPeriodoPago = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargoCuenta", x => x.IdCargoCuenta);
                    table.ForeignKey(
                        name: "FK_CargoCuenta_PeriodoPago_IdPeriodoPago",
                        column: x => x.IdPeriodoPago,
                        principalTable: "PeriodoPago",
                        principalColumn: "IdPeriodoPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargoCuenta_Unidad_IdUnidad",
                        column: x => x.IdUnidad,
                        principalTable: "Unidad",
                        principalColumn: "IdUnidad",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Correspondencia",
                columns: table => new
                {
                    IdCorrespondencia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUnidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTipoCorrespondencia = table.Column<int>(type: "int", nullable: false),
                    Remitente = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaRecepcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdEstadoCorrespondencia = table.Column<int>(type: "int", nullable: false),
                    FechaNotificado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaEntregado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntregadoA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdUsuarioRegistro = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correspondencia", x => x.IdCorrespondencia);
                    table.ForeignKey(
                        name: "FK_Correspondencia_EstadoCorrespondencia_IdEstadoCorrespondencia",
                        column: x => x.IdEstadoCorrespondencia,
                        principalTable: "EstadoCorrespondencia",
                        principalColumn: "IdEstadoCorrespondencia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Correspondencia_TipoCorrespondencia_IdTipoCorrespondencia",
                        column: x => x.IdTipoCorrespondencia,
                        principalTable: "TipoCorrespondencia",
                        principalColumn: "IdTipoCorrespondencia",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Correspondencia_Unidad_IdUnidad",
                        column: x => x.IdUnidad,
                        principalTable: "Unidad",
                        principalColumn: "IdUnidad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Correspondencia_Usuario_IdUsuarioRegistro",
                        column: x => x.IdUsuarioRegistro,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pago",
                columns: table => new
                {
                    IdPago = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUnidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPeriodoPago = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IdMetodoPago = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdUsuarioRegistro = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pago", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pago_MetodoPago_IdMetodoPago",
                        column: x => x.IdMetodoPago,
                        principalTable: "MetodoPago",
                        principalColumn: "IdMetodoPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pago_PeriodoPago_IdPeriodoPago",
                        column: x => x.IdPeriodoPago,
                        principalTable: "PeriodoPago",
                        principalColumn: "IdPeriodoPago",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pago_Unidad_IdUnidad",
                        column: x => x.IdUnidad,
                        principalTable: "Unidad",
                        principalColumn: "IdUnidad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pago_Usuario_IdUsuarioRegistro",
                        column: x => x.IdUsuarioRegistro,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Residencia",
                columns: table => new
                {
                    IdResidencia = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUsuario = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUnidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Residencia", x => x.IdResidencia);
                    table.ForeignKey(
                        name: "FK_Residencia_Unidad_IdUnidad",
                        column: x => x.IdUnidad,
                        principalTable: "Unidad",
                        principalColumn: "IdUnidad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Residencia_Usuario_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visita",
                columns: table => new
                {
                    IdVisita = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdVisitante = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUnidad = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotaDestino = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlacaVehiculo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdUsuarioRegistro = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visita", x => x.IdVisita);
                    table.ForeignKey(
                        name: "FK_Visita_Unidad_IdUnidad",
                        column: x => x.IdUnidad,
                        principalTable: "Unidad",
                        principalColumn: "IdUnidad",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visita_Usuario_IdUsuarioRegistro",
                        column: x => x.IdUsuarioRegistro,
                        principalTable: "Usuario",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visita_Visitante_IdVisitante",
                        column: x => x.IdVisitante,
                        principalTable: "Visitante",
                        principalColumn: "IdVisitante",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargoCuenta_IdPeriodoPago",
                table: "CargoCuenta",
                column: "IdPeriodoPago");

            migrationBuilder.CreateIndex(
                name: "IX_CargoCuenta_IdUnidad",
                table: "CargoCuenta",
                column: "IdUnidad");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondencia_IdEstadoCorrespondencia",
                table: "Correspondencia",
                column: "IdEstadoCorrespondencia");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondencia_IdTipoCorrespondencia",
                table: "Correspondencia",
                column: "IdTipoCorrespondencia");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondencia_IdUnidad_FechaRecepcion",
                table: "Correspondencia",
                columns: new[] { "IdUnidad", "FechaRecepcion" });

            migrationBuilder.CreateIndex(
                name: "IX_Correspondencia_IdUsuarioRegistro",
                table: "Correspondencia",
                column: "IdUsuarioRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_IdMetodoPago",
                table: "Pago",
                column: "IdMetodoPago");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_IdPeriodoPago",
                table: "Pago",
                column: "IdPeriodoPago");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_IdUnidad",
                table: "Pago",
                column: "IdUnidad");

            migrationBuilder.CreateIndex(
                name: "IX_Pago_IdUsuarioRegistro",
                table: "Pago",
                column: "IdUsuarioRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_Persona_CorreoElectronico",
                table: "Persona",
                column: "CorreoElectronico",
                unique: true,
                filter: "[CorreoElectronico] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PersonaAutorizada_IdPersona",
                table: "PersonaAutorizada",
                column: "IdPersona");

            migrationBuilder.CreateIndex(
                name: "IX_PersonaAutorizada_IdTipoRelacionAutorizado",
                table: "PersonaAutorizada",
                column: "IdTipoRelacionAutorizado");

            migrationBuilder.CreateIndex(
                name: "IX_PersonaAutorizada_IdUsuarioResidente_IdPersona",
                table: "PersonaAutorizada",
                columns: new[] { "IdUsuarioResidente", "IdPersona" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Residencia_IdUnidad",
                table: "Residencia",
                column: "IdUnidad");

            migrationBuilder.CreateIndex(
                name: "IX_Residencia_IdUsuario_IdUnidad_FechaInicio",
                table: "Residencia",
                columns: new[] { "IdUsuario", "IdUnidad", "FechaInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Torre_IdConjunto_Nombre",
                table: "Torre",
                columns: new[] { "IdConjunto", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidad_IdTorre",
                table: "Unidad",
                column: "IdTorre");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_IdPersona",
                table: "Usuario",
                column: "IdPersona",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRol_IdRol",
                table: "UsuarioRol",
                column: "IdRol");

            migrationBuilder.CreateIndex(
                name: "IX_Visita_IdUnidad_FechaRegistro",
                table: "Visita",
                columns: new[] { "IdUnidad", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "IX_Visita_IdUsuarioRegistro",
                table: "Visita",
                column: "IdUsuarioRegistro");

            migrationBuilder.CreateIndex(
                name: "IX_Visita_IdVisitante",
                table: "Visita",
                column: "IdVisitante");

            migrationBuilder.CreateIndex(
                name: "IX_Visitante_IdPersona",
                table: "Visitante",
                column: "IdPersona",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargoCuenta");

            migrationBuilder.DropTable(
                name: "Correspondencia");

            migrationBuilder.DropTable(
                name: "Pago");

            migrationBuilder.DropTable(
                name: "PersonaAutorizada");

            migrationBuilder.DropTable(
                name: "Residencia");

            migrationBuilder.DropTable(
                name: "UsuarioRol");

            migrationBuilder.DropTable(
                name: "Visita");

            migrationBuilder.DropTable(
                name: "EstadoCorrespondencia");

            migrationBuilder.DropTable(
                name: "TipoCorrespondencia");

            migrationBuilder.DropTable(
                name: "MetodoPago");

            migrationBuilder.DropTable(
                name: "PeriodoPago");

            migrationBuilder.DropTable(
                name: "TipoRelacionAutorizado");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Unidad");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Visitante");

            migrationBuilder.DropTable(
                name: "Torre");

            migrationBuilder.DropTable(
                name: "Persona");

            migrationBuilder.DropTable(
                name: "Conjunto");
        }
    }
}
