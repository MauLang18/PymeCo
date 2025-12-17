# Proyecto Final – PymeCo

## Curso
**Programación Avanzada (SC-601)**

## Profesor
**Raúl Alexander Monge**

## Equipo 1 – Grupo de trabajo

### Integrantes
- María Fernanda Mata Halleslebens  
- Maurice Lang Bonilla  
- Matías Aguilar Vega  
- Isaac Navarro Bermúdez  

Maurice Lang Bonilla cumplió un rol de guía y apoyo técnico general para el equipo.  
El desarrollo del front-end y las vistas fue trabajado de forma colaborativa por todos los integrantes.

---

## Descripción del proyecto

PymeCo es un sistema web desarrollado bajo el patrón **ASP.NET Core MVC**, orientado a la gestión de productos, clientes, inventario y pedidos en un contexto tipo B2B.

El sistema implementa autenticación y autorización mediante **ASP.NET Core Identity**, cálculo automático de totales, administración de stock, exportación de información a Excel y un módulo de pedidos con control de estados.

La aplicación fue desarrollada siguiendo la arquitectura sugerida por el curso, separando responsabilidades en capas para facilitar el mantenimiento y la evolución del sistema.

### Tecnologías utilizadas
- ASP.NET Core MVC (.NET 9)
- Entity Framework Core (Code First)
- SQL Server
- Bootstrap 5
- jQuery y AJAX

---

## Enlace al sistema en producción

🔗 **https://pymeco.customcoder.com**

> El sistema se encuentra desplegado en hosting y accesible públicamente para su evaluación.

---

## Requisitos previos

Antes de ejecutar el proyecto en ambiente local, se requiere contar con:

- Windows 10 u 11
- Visual Studio 2022
- .NET 9 SDK
- SQL Server Express o SQL Server local
- (Opcional) SQL Server Management Studio para inspección de la base de datos

---

## Instalación del proyecto (Instalación Express)

El proyecto cumple con el requisito de instalación express solicitado en el curso:  
**clonar el repositorio → ejecutar migraciones → correr la aplicación**.

### 1. Clonar el repositorio


    git clone https://github.com/MauLang18/PymeCo.git


### 2.	Configurar la cadena de conexión en POS.Web/appsettings.Development.json o appsettings.json.
Utilice el siguiente formato y sustituir únicamente el nombre del servidor SQL de cada persona:

        "ConnectionStrings": {
        "Default": "Server=MI_SERVIDOR\SQLEXPRESS;Database=PymeBD;Trusted_Connection=True;TrustServerCertificate=True"
        },
        "Database": {
        "Provider": "SqlServer"
        }

El nombre de la base de datos debe permanecer como PymeBD, ya que se crea automáticamente mediante migraciones.

### 3.	Crear la base de datos

En Visual Studio, abrir:

1. Tools → NuGet Package Manager → Package Manager Console

2. Seleccionar **POS.Infrastructure** como proyecto predeterminado.

3. Ejecutar el comando que la base de datos PymeBD y todas las tablas requeridas:


*Update-Database -Context AppDbContext*


**4.	Ejecutar el proyecto**
   
Seleccionar **POS.Web** como proyecto de inicio, y ejecutar con F5.

---

## Roles del sistema

El sistema implementa control de acceso basado en roles:

### Administrador

- Acceso completo al sistema

- Gestión de usuarios, productos, clientes y pedidos

- Exportación de información

### Vendedor

- Gestión de productos y clientes

- Registro y consulta de pedidos

- Exportación de listados

### Cajero

- Registro y consulta de pedidos

- Consulta de productos y clientes

- Seguimiento de estados de pedidos


---


## Funcionalidades principales

### Productos:

⦁	CRUD completo

⦁	Imagen obligatoria al crear

⦁	Filtros y paginación

⦁	Validaciones de campos

⦁	Exportación a Excel


### Clientes:

⦁	CRUD completo

⦁	Validaciones y búsqueda por nombre o cédula

⦁	Exportación a Excel


### Pedidos:

⦁	Selección de cliente

⦁	Agregado de productos

⦁	Cálculo en vivo de subtotal, impuestos y total

⦁	Control de estados (Pendiente, Pagado, Enviado

⦁	Actualización automática de stock

⦁	Exportación a Excel


---


## Seguridad:

⦁	Identity con roles Admin, Ventas y Cajero

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


---


## Datos de prueba

El proyecto debe incluir datos mínimos para evaluación:

⦁	Productos: 10 a 20

⦁	Clientes: 5 a 10

⦁	Categorías: al menos 2

⦁	Stock y precios coherentes


Los datos pueden cargarse mediante seeding o ingresarse manualmente en la interfaz.
