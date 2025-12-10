**Proyecto Final – PymeCo.**

**Curso: Programación Avanzada (SC-601)**

**Profesor: Raul Alexander Monge**

**Equipo 1 – Grupo de trabajo**

*Integrantes:*

  ⦁	María Fernanda Mata Halleslebens
  
  ⦁	Maurice Lang Bonilla
  
  ⦁	Matías Aguilar Vega
  
  ⦁	Isaac Navarro Bermúdez

Maurice Lang Bonilla cumplió un rol de guía y apoyo técnico general para el equipo.
El desarrollo del front-end y vistas fue trabajado entre todos los integrantes.


*Descripción del proyecto*

PymeCo es un sistema web MVC para la gestión de productos, clientes, inventario y pedidos tipo B2B.
Implementa autenticación y autorización mediante ASP.NET Core Identity, incluye cálculo automático de totales, búsqueda inteligente de productos (autosuggest), administración de stock y módulo de pedidos con detalle.

La aplicación sigue la arquitectura sugerida por el curso:

  ⦁	ASP.NET Core MVC (C#)
  
  ⦁	Entity Framework Core con Code First
  
  ⦁	SQL Server
  
  ⦁	Bootstrap 5
  
  ⦁	jQuery y AJAX para búsqueda y cálculo en vivo

*Enlace al sistema en producción*

[http://fidelitaspos.runasp.net/](http://fidelitaspos.runasp.net) 


*Requisitos previos*

Antes de ejecutar el proyecto, el entorno debe contar con:

⦁	Windows 10/11

⦁	Visual Studio 2022

⦁	.NET 9 SDK

⦁	SQL Server Express o SQL Server local

⦁	(Opcional) SQL Server Management Studio para inspeccionar la base de datos


*Instalación del proyecto*

Este proyecto está configurado para cumplir con la instalación express requerida por el curso: clonar el repositorio, ejecutar las migraciones y correr la aplicación.

Pasos:

**1.	Clonar el repositorio**

git clone https://github.com/MauLang18/PymeCo.git

Abrir la solución **PymeCo.sln** en Visual Studio.

**2.	Configurar la cadena de conexión en POS.Web/appsettings.Development.json o appsettings.json.**
Utilizar el siguiente formato y sustituir únicamente el nombre del servidor SQL de cada persona:

        "ConnectionStrings": {
        "Default": "Server=MI_SERVIDOR\SQLEXPRESS;Database=PymeBD;Trusted_Connection=True;TrustServerCertificate=True"
        },
        "Database": {
        "Provider": "SqlServer"
        }

El nombre de la base de datos debe permanecer como PymeBD, ya que se crea automáticamente mediante migraciones.

**3.	Crear la base de datos**

En Visual Studio, abrir:

Tools → NuGet Package Manager → Package Manager Console

Seleccionar **POS.Infrastructure** como proyecto predeterminado.

Ejecutar el comando que la base de datos PymeBD y todas las tablas requeridas:


    Update-Database -Context AppDbContext

**4.	Ejecutar el proyecto**
   
Seleccionar **POS.Web** como proyecto de inicio, y ejecutar con F5.

*Usuarios de prueba*

Los roles implementados son Admin, Ventas y Operaciones.

Se utilizan usuarios de prueba opcionales siguiendo el estándar del curso:


admin@demo.local -> / Passw0rd!

ventas@demo.local -> / Passw0rd!

ops@demo.local -> / Passw0rd!


Si el proyecto incluye seeding, estos usuarios estarán disponibles al ejecutar la aplicación.
Si no, pueden crearse manualmente desde la interfaz de registro y luego asignarse roles usando la base de datos.

*Funcionalidades principales*

**Productos:**

⦁	CRUD completo

⦁	Imagen obligatoria al crear

⦁	Filtros y paginación

⦁	Validaciones de campos


**Clientes:**

⦁	CRUD

⦁	Validaciones y búsqueda por nombre o cédula


**Pedidos:**

⦁	Selección de cliente

⦁	Autosuggest AJAX para productos

⦁	Cálculo en vivo de subtotal, impuestos y total

⦁	Disminución automática de stock al confirmar

⦁	Registro de totales para auditoría

⦁	Consulta de detalle del pedido


**Seguridad:**

⦁	Identity con roles Admin, Ventas y Operaciones

⦁	Controladores y acciones protegidas con [Authorize]

⦁	Acceso restringido por rol según funcionalidad


**API:**
Endpoints para AJAX:

⦁	/api/productos/buscar

⦁	/api/pedidos/calcular


**Manejo de errores:**

⦁	Vistas personalizadas 404 y 500

⦁	Manejo básico de excepciones

⦁	Logging con Serilog


*Estructura de la solución*

POS.Web: Controladores MVC, vistas, autenticación Identity, configuración general.

POS.Infrastructure: DbContext, Migrations, repositorios y configuración de EF Core.

POS.Domain: Entidades del dominio (Producto, Cliente, Pedido, etc.).

POS.Application: Servicios, lógica de negocio y casos de uso.


*Datos de prueba*

El proyecto debe incluir datos mínimos para evaluación:

⦁	Productos: 10 a 20

⦁	Clientes: 5 a 10

⦁	Categorías: al menos 2

⦁	Stock y precios coherentes


Los datos pueden cargarse mediante seeding o ingresarse manualmente en la interfaz.
