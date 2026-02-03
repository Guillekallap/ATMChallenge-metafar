# ATM Challenge API - .NET 8

API REST para sistema de cajero autom�tico desarrollada con .NET 8, Entity Framework Core, MediatR, FluentValidation y autenticaci�n JWT. Implementa Clean Architecture con patr�n Repository, CQRS y principios SOLID.

## ?? Tabla de Contenidos

- [Requisitos Previos](#requisitos-previos)
- [Arquitectura y Dise�o](#arquitectura-y-dise�o)
- [Ejecuci�n con Docker](#ejecuci�n-con-docker)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Base de Datos](#base-de-datos)
- [Endpoints de la API](#endpoints-de-la-api)
- [Documentaci�n Swagger](#documentaci�n-swagger)
- [Colecci�n Postman](#colecci�n-postman)
- [Diagrama Entidad-Relaci�n](#diagrama-entidad-relaci�n)
- [Ejecuci�n Local sin Docker](#ejecuci�n-local-sin-docker)
- [Pruebas Unitarias](#pruebas-unitarias)
- [Tecnolog�as Utilizadas](#tecnolog�as-utilizadas)
- [Principios SOLID y Patrones](#principios-solid-y-patrones)

## ?? Requisitos Previos

- **Docker Desktop** (Windows / macOS / Linux) - [Descargar aqu�](https://www.docker.com/products/docker-desktop)
- **.NET 8 SDK** (solo para desarrollo local) - [Descargar aqu�](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Postman** (opcional, para probar la API) - [Descargar aqu�](https://www.postman.com/downloads/)

## ??? Arquitectura y Dise�o

### Clean Architecture

Este proyecto implementa **Clean Architecture** (tambi�n conocida como Arquitectura Limpia o Arquitectura de Capas Limpias), propuesta por Robert C. Martin (Uncle Bob).

#### �Por qu� Clean Architecture?

Se eligi� Clean Architecture sobre otras arquitecturas (Hexagonal, Onion) por las siguientes razones:

1. **Separaci�n Clara de Responsabilidades**
   - Cada capa tiene un prop�sito �nico y bien definido
   - F�cil de entender y mantener para nuevos desarrolladores
   - La estructura de carpetas refleja directamente la arquitectura

2. **Independencia del Framework**
   - La l�gica de negocio (Domain y Application) no depende de ASP.NET Core
   - Podr�a migrarse a otro framework sin cambiar la l�gica central
   - Los tests pueden ejecutarse sin infraestructura

3. **Testabilidad**
   - Cada capa puede testearse independientemente
   - Los handlers de MediatR son f�ciles de probar con mocks
   - No se requiere base de datos para tests unitarios

4. **Facilidad de Evoluci�n**
   - Nuevas features se agregan sin modificar c�digo existente (Open/Closed)
   - Cambiar de SQL Server a PostgreSQL solo afecta la capa Infrastructure
   - Agregar nuevos endpoints no requiere cambios en el dominio

5. **Est�ndar de la Industria**
   - Ampliamente adoptada en proyectos empresariales .NET
   - Excelente documentaci�n y comunidad
   - Compatible con DDD (Domain-Driven Design)

#### Capas de la Arquitectura

```
???????????????????????????????????????????????????????
?                  API (Presentation)                 ?
?  Controllers, Middleware, Program.cs                ?
?  Depende de: Application, Infrastructure            ?
???????????????????????????????????????????????????????
                          ?
???????????????????????????????????????????????????????
?            Application (Use Cases)                  ?
?  Handlers, Commands, Queries, DTOs, Validators      ?
?  Depende de: Domain                                 ?
???????????????????????????????????????????????????????
                          ?
???????????????????????????????????????????????????????
?              Domain (Business Logic)                ?
?  Entities, Enums, Business Rules                    ?
?  NO depende de nada (n�cleo puro)                   ?
???????????????????????????????????????????????????????
                          ?
???????????????????????????????????????????????????????
?        Infrastructure (Data & External)             ?
?  Repositories, DbContext, Migrations                ?
?  Depende de: Domain, Application (interfaces)       ?
???????????????????????????????????????????????????????
```

**Regla de Dependencia:** Las dependencias apuntan hacia adentro. El dominio no conoce nada sobre las capas externas.

### CQRS con MediatR

El proyecto implementa **CQRS (Command Query Responsibility Segregation)** usando **MediatR**:

#### Commands (Escritura)
- `LoginCommand`: Autentica al usuario
- `WithdrawCommand`: Realiza un retiro de dinero

#### Queries (Lectura)
- `GetBalanceQuery`: Obtiene el saldo de una cuenta
- `GetOperationsQuery`: Obtiene el historial paginado de operaciones

#### Ventajas de CQRS

? **Separaci�n de Responsabilidades:** Las operaciones de lectura y escritura est�n claramente separadas  
? **Optimizaci�n Independiente:** Queries y Commands pueden optimizarse por separado  
? **Escalabilidad:** F�cil de escalar lecturas y escrituras independientemente  
? **Claridad en el C�digo:** Cada handler tiene una sola responsabilidad  
? **Testabilidad:** Cada handler puede testearse de forma aislada

### Pipeline Behaviors (Cross-Cutting Concerns)

MediatR permite inyectar comportamientos transversales mediante **IPipelineBehavior**:

#### 1. ValidationBehavior
```csharp
// Valida autom�ticamente todos los requests antes de ejecutar el handler
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
```

**Responsabilidad:** Ejecutar validaciones de FluentValidation antes de que el request llegue al handler.

**Beneficios:**
- ? No hay que validar manualmente en cada handler
- ? C�digo DRY (Don't Repeat Yourself)
- ? Validaciones centralizadas y consistentes

#### 2. LoggingBehavior
```csharp
// Registra autom�ticamente entrada, salida y performance de todos los requests
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
```

**Responsabilidad:** Logging autom�tico de todas las operaciones y medici�n de performance.

**Beneficios:**
- ? Trazabilidad completa de operaciones
- ? Detecci�n de cuellos de botella
- ? No contamina la l�gica de negocio con c�digo de logging

#### Orden de Ejecuci�n del Pipeline

```
Request ? LoggingBehavior ? ValidationBehavior ? Handler ? Response
            ?                     ?                  ?
         (logs)            (valida FluentVal)  (l�gica negocio)
```

### Validators con FluentValidation

Cada Command/Query tiene su propio validador:

- `LoginCommandValidator`: Valida cardNumber y PIN
- `WithdrawCommandValidator`: Valida cardNumber y amount

**Ejemplo:**
```csharp
public class WithdrawCommandValidator : AbstractValidator<WithdrawCommand>
{
    public WithdrawCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Length(16);
            
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a 0");
    }
}
```

**Ventajas:**
- ? Validaciones expresivas y legibles
- ? Mensajes de error personalizables
- ? Validaciones complejas con poco c�digo
- ? Testeable independientemente

## ?? Ejecuci�n con Docker

La forma **m�s sencilla y recomendada** de ejecutar la aplicaci�n es usando Docker Compose:

### Paso 1: Clonar el repositorio

```bash
git clone <url-del-repositorio>
cd ATMChallenge-metafar
```

### Paso 2: Iniciar los contenedores

```bash
docker-compose up --build
```

Este comando:
- Descarga las im�genes de Docker necesarias (SQL Server 2022 y .NET 8)
- Construye la imagen de la API
- Inicia dos contenedores:
  - **sqlserver**: Base de datos SQL Server 2022 en el puerto 1433
  - **api**: API REST en el puerto 5000

### Paso 3: Verificar que todo est� funcionando

Una vez que los contenedores est�n en ejecuci�n, ver�s mensajes similares a:

```
api_1        | info: Microsoft.Hosting.Lifetime[14]
api_1        |       Now listening on: http://[::]:80
sqlserver_1  | SQL Server is now ready for client connections.
```

Abre tu navegador y ve a:
- **Swagger UI**: http://localhost:5000/swagger

### Detener los contenedores

Para detener la aplicaci�n, presiona `Ctrl+C` en la terminal donde ejecutaste `docker-compose up`, o ejecuta:

```bash
docker-compose down
```

### Persistencia de Datos

Los datos de la base de datos se persisten en un volumen de Docker llamado `sqlserverdata`. Esto significa que aunque detengas y vuelvas a iniciar los contenedores, tus datos permanecer�n intactos.

Para eliminar completamente los datos y empezar desde cero:

```bash
docker-compose down -v
```

## ?? Estructura del Proyecto

El proyecto sigue los principios de **Clean Architecture**:

```
ATMChallenge/
??? ATMChallenge.API/              # Capa de Presentaci�n
?   ??? Controllers/               # Controladores de la API
?   ??? Middleware/                # Middleware personalizado
?   ??? Program.cs                 # Punto de entrada
??? ATMChallenge.Application/      # Capa de Aplicaci�n
?   ??? Features/                  # Casos de uso (Commands/Queries)
?   ??? DTOs/                      # Objetos de transferencia de datos
?   ??? Validators/                # Validadores FluentValidation
?   ??? Interfaces/                # Interfaces de repositorios
??? ATMChallenge.Domain/           # Capa de Dominio
?   ??? Entities/                  # Entidades del dominio
??? ATMChallenge.Infrastructure/   # Capa de Infraestructura
?   ??? Persistence/               # Configuraci�n de EF Core
?   ??? Repositories/              # Implementaci�n de repositorios
?   ??? Migrations/                # Migraciones de base de datos
??? ATMChallenge.Tests/            # Pruebas Unitarias
??? ERD/                           # Diagrama Entidad-Relaci�n
??? postman/                       # Colecci�n de Postman
??? docker-compose.yml             # Orquestaci�n de contenedores
??? Dockerfile                     # Imagen de Docker para la API
```

## ?? Base de Datos

### SQL Server 2022

La aplicaci�n utiliza **SQL Server 2022** como motor de base de datos relacional.

### Migraciones

Las migraciones de Entity Framework Core se aplican autom�ticamente al iniciar la aplicaci�n (ver `Program.cs`).

### Datos Iniciales (Seed)

Al iniciar por primera vez, la base de datos se crea con los siguientes datos de prueba:

**Usuarios y Cuentas:**

| Usuario       | N�mero de Cuenta | Tarjeta          | PIN  | Saldo Inicial |
|---------------|------------------|------------------|------|---------------|
| Juan P�rez    | AR12345678       | 4000000000000001 | 1234 | $10,000.00    |
| Mar�a Garc�a  | AR87654321       | 4000000000000002 | 5678 | $25,000.00    |
| Carlos L�pez  | AR11223344       | 4000000000000003 | 9999 | $5,500.00     |
| Ana Mart�nez  | AR55667788       | 4000000000000004 | 1111 | $50,000.00    |
| Pedro Rodr�guez | AR99887766     | 4000000000000005 | 2222 | $15,750.00    |

**Operaciones hist�ricas:**
- Cada cuenta tiene entre 5 y 15 operaciones de retiro previas para probar la paginaci�n.

### Esquema de Base de Datos

Las tablas principales son:

- **Users**: Informaci�n de los usuarios
- **Accounts**: Cuentas bancarias
- **Cards**: Tarjetas asociadas a cuentas
- **Operations**: Historial de operaciones (retiros)

<img width="904" height="619" alt="ATMChallenge-ERD" src="https://github.com/user-attachments/assets/0f0f270a-a85b-4cd1-b316-212d47469d5c" />


## ?? Endpoints de la API

**URL Base**: `http://localhost:5000`

### 1. Login (Autenticaci�n)

Valida las credenciales y retorna un token JWT.

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "cardNumber": "4000000000000001",
  "pin": "1234"
}
```

**Response Exitosa (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Respuestas de Error:**
- `401 Unauthorized`: PIN inv�lido o tarjeta bloqueada
- `400 Bad Request`: Datos de entrada inv�lidos

**?? Importante:** Despu�s de 4 intentos fallidos de PIN, la tarjeta se bloquea autom�ticamente.

### 2. Consultar Saldo

Obtiene el saldo actual de la cuenta.

**Endpoint:** `GET /api/account/balance/{cardNumber}`

**Headers:**
```
Authorization: Bearer {token}
```

**Ejemplo:** `GET /api/account/balance/4000000000000001`

**Response Exitosa (200 OK):**
```json
{
  "userFullName": "Juan P�rez",
  "accountNumber": "AR12345678",
  "balance": 10000.00,
  "lastWithdrawal": "2026-01-15T14:30:00Z"
}
```

**Respuestas de Error:**
- `401 Unauthorized`: Token inv�lido o expirado
- `403 Forbidden`: El token no corresponde al n�mero de tarjeta
- `404 Not Found`: Tarjeta no encontrada

### 3. Realizar Retiro

Realiza una extracci�n de dinero.

**Endpoint:** `POST /api/withdraw`

**Headers:**
```
Authorization: Bearer {token}
```

**Request Body:**
```json
{
  "cardNumber": "4000000000000001",
  "amount": 500.00
}
```

**Response Exitosa (200 OK):**
```json
{
  "newBalance": 9500.00,
  "timestamp": "2026-02-02T15:45:00Z"
}
```

**Respuestas de Error:**
- `400 Bad Request`: Fondos insuficientes o monto inv�lido
- `401 Unauthorized`: Token inv�lido
- `403 Forbidden`: El token no corresponde al n�mero de tarjeta

**Validaciones:**
- El monto debe ser mayor a 0
- El monto no puede exceder el saldo disponible
- El monto debe ser un n�mero v�lido con hasta 2 decimales

### 4. Historial de Operaciones

Obtiene el historial paginado de operaciones.

**Endpoint:** `GET /api/operations/{cardNumber}?page={pageNumber}`

**Headers:**
```
Authorization: Bearer {token}
```

**Par�metros de Query:**
- `page` (opcional, default=1): N�mero de p�gina

**Ejemplo:** `GET /api/operations/4000000000000001?page=1`

**Response Exitosa (200 OK):**
```json
{
  "page": 1,
  "pageSize": 10,
  "totalItems": 25,
  "items": [
    {
      "type": "Withdrawal",
      "amount": 500.00,
      "timestamp": "2026-02-01T10:30:00Z"
    },
    {
      "type": "Withdrawal",
      "amount": 1000.00,
      "timestamp": "2026-01-28T14:15:00Z"
    }
  ]
}
```

**Respuestas de Error:**
- `401 Unauthorized`: Token inv�lido
- `403 Forbidden`: El token no corresponde al n�mero de tarjeta
- `404 Not Found`: Tarjeta no encontrada

## ?? Documentaci�n Swagger

La API incluye documentaci�n interactiva con **Swagger UI**.

**URL:** http://localhost:5000/swagger

### C�mo usar Swagger:

1. Abre http://localhost:5000/swagger en tu navegador
2. Primero, ejecuta el endpoint `POST /api/auth/login` con credenciales v�lidas
3. Copia el token de la respuesta
4. Haz clic en el bot�n **"Authorize"** (arriba a la derecha)
5. Ingresa: `Bearer {tu-token}` (ejemplo: `Bearer eyJhbGci...`)
6. Haz clic en **"Authorize"** y luego **"Close"**
7. Ahora puedes probar todos los endpoints protegidos

## ?? Colecci�n Postman

Se incluye una colecci�n completa de Postman en: **`postman/ATMChallenge.postman_collection.json`**

### Importar la colecci�n:

1. Abre Postman
2. Click en **"Import"** (arriba a la izquierda)
3. Selecciona el archivo `postman/ATMChallenge.postman_collection.json`
4. Click en **"Import"**

### Configurar variables:

La colecci�n incluye variables pre-configuradas:
- `baseUrl`: `http://localhost:5000`
- `jwt_token`: Se actualiza autom�ticamente despu�s del login

### Flujo de trabajo:

1. Ejecuta **"1. Login"** primero - esto guardar� el token autom�ticamente
2. Ejecuta cualquier otro endpoint (el token se incluye autom�ticamente)

### Casos de prueba incluidos:

- ? Login exitoso
- ? Login con PIN incorrecto
- ? Login con tarjeta bloqueada
- ? Consultar saldo
- ? Realizar retiro v�lido
- ? Retiro con fondos insuficientes
- ? Obtener operaciones (p�gina 1)
- ? Obtener operaciones (p�gina 2)

## ?? Diagrama Entidad-Relaci�n

El diagrama ERD se encuentra en: **`ERD/ATMChallenge-ERD.drawio`** y **`ERD/ATMChallenge-ERD.png`**

### Ver el diagrama:

- **Archivo PNG**: Abre directamente la imagen `ERD/ATMChallenge-ERD.png`
- **Archivo Draw.io**: Abre en https://app.diagrams.net/ o con la extensi�n de VS Code "Draw.io Integration"

### Entidades principales:

```
???????????       ????????????       ????????
?  Users  ????????? Accounts ????????? Cards?
???????????       ????????????       ????????
                        ?
                        ?
                  ??????????????
                  ? Operations ?
                  ??????????????
```

**Relaciones:**
- Un **User** tiene una **Account** (1:1)
- Una **Account** puede tener m�ltiples **Cards** (1:N)
- Una **Account** tiene m�ltiples **Operations** (1:N)

## ?? Ejecuci�n Local sin Docker

Si prefieres ejecutar la aplicaci�n sin Docker (para desarrollo):

### Requisitos:

1. **.NET 8 SDK** instalado
2. **SQL Server** (local o remoto) en ejecuci�n

### Pasos:

1. **Configurar la conexi�n a la base de datos**

Edita `ATMChallenge.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ATMChallengeDb;User Id=sa;Password=TuPassword;TrustServerCertificate=True;"
  }
}
```

2. **Restaurar paquetes NuGet**

```bash
dotnet restore
```

3. **Aplicar migraciones**

```bash
cd ATMChallenge.API
dotnet ef database update --project ../ATMChallenge.Infrastructure
```

4. **Ejecutar la aplicaci�n**

```bash
dotnet run
```

La API estar� disponible en:
- http://localhost:5000
- https://localhost:5001

## ?? Pruebas Unitarias

El proyecto incluye pruebas unitarias con **xUnit** en el proyecto `ATMChallenge.Tests`.

### Ejecutar todas las pruebas:

```bash
dotnet test
```

### Ejecutar con detalles:

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Cobertura de pruebas:

- ? LoginHandler: Validaci�n de credenciales y bloqueo de tarjeta
- ? WithdrawHandler: Validaci�n de retiros y actualizaci�n de saldo
- ? GetBalanceHandler: Consulta de saldo
- ? GetOperationsHandler: Paginaci�n de operaciones
- ? Validadores FluentValidation

## ??? Tecnolog�as Utilizadas

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core 8** - ORM
- **SQL Server 2022** - Base de datos

### Patrones y Arquitectura
- **Clean Architecture** - Separaci�n en 4 capas (Domain, Application, Infrastructure, API)
- **CQRS con MediatR** - Command Query Responsibility Segregation
- **MediatR 12.0** - Implementaci�n del patr�n Mediator
- **Pipeline Behaviors** - LoggingBehavior y ValidationBehavior
- **Repository Pattern** - Abstracci�n de acceso a datos (ICardRepository, IAccountRepository)
- **Dependency Injection** - Inversi�n de control nativa de .NET

### Seguridad
- **JWT Bearer Authentication** - Autenticaci�n basada en tokens
- **BCrypt.Net 4.0** - Hash seguro de PINs (factor de trabajo 11)
- **Authorization** - Validaci�n de que el token corresponda al n�mero de tarjeta

### Validaci�n y Documentaci�n
- **FluentValidation 11.6** - Validaci�n de modelos con sintaxis fluida
- **ValidationBehavior** - Validaci�n autom�tica en el pipeline de MediatR
- **Swagger/OpenAPI** - Documentaci�n interactiva de la API

### Testing
- **xUnit** - Framework de pruebas unitarias
- **Moq** - Librer�a de mocking para tests
- **InMemory Database** - Testing de integraci�n sin SQL Server

### DevOps
- **Docker** - Contenerizaci�n
- **Docker Compose** - Orquestaci�n de contenedores (API + SQL Server)

### Librer�as Principales

```xml
<PackageReference Include="MediatR" Version="12.0.1" />
<PackageReference Include="FluentValidation" Version="11.6.0" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="6.30.0" />
```

## ?? Seguridad

### Variables de Entorno

Las credenciales y claves sensibles se configuran mediante variables de entorno en `docker-compose.yml`:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=sqlserver,1433;...
  - Jwt__Key=ThisIsASecretKeyForJWT1234567890
  - Jwt__Issuer=ATMChallengeIssuer
  - Jwt__Audience=ATMChallengeAudience
```

### Hash de PINs

Los PINs se almacenan utilizando **BCrypt** con un factor de trabajo de 11, lo que hace pr�cticamente imposible revertir el hash.

### Tokens JWT

Los tokens JWT tienen una duraci�n de **60 minutos** (configurable en `appsettings.json`).

## ?? Notas Adicionales

### Bloqueo de Tarjetas

- Despu�s de **4 intentos fallidos** de PIN, la tarjeta se bloquea autom�ticamente
- Una tarjeta bloqueada no puede realizar ninguna operaci�n
- El campo `IsBlocked` en la entidad `Card` indica el estado

### Concurrencia

- La entidad `Account` incluye un campo `RowVersion` para manejo de concurrencia optimista
- Previene condiciones de carrera en retiros simult�neos

### Auditor�a

Las entidades incluyen campos de auditor�a:
- `CreatedAt`: Fecha de creaci�n
- `UpdatedAt`: Fecha de �ltima actualizaci�n
- `CreatedBy`: Usuario que cre� el registro (en Operations)

## ?? Principios SOLID y Patrones de Dise�o

Este proyecto sigue estrictamente los principios SOLID y utiliza m�ltiples patrones de dise�o para garantizar c�digo mantenible, escalable y de alta calidad.

### Principios SOLID Aplicados

#### 1. **S**ingle Responsibility Principle (SRP)

**Cada clase tiene una �nica responsabilidad:**

? **LoginHandler** - Solo maneja la autenticaci�n  
? **WithdrawHandler** - Solo maneja retiros  
? **GetBalanceHandler** - Solo obtiene saldos  
? **GetOperationsHandler** - Solo obtiene operaciones  
? **ValidationBehavior** - Solo valida requests  
? **LoggingBehavior** - Solo registra logs  

**Ejemplo:**
```csharp
// Cada handler tiene UNA sola responsabilidad
public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    // Solo se encarga de autenticar usuarios
    // No hace logging, no valida (eso es del Behavior)
}
```

#### 2. **O**pen/Closed Principle (OCP)

**Abierto para extensi�n, cerrado para modificaci�n:**

? Nuevos handlers se agregan sin modificar los existentes  
? Nuevos behaviors se inyectan en el pipeline sin cambiar c�digo  
? Nuevas validaciones se agregan sin tocar la l�gica de negocio  

**Ejemplo:**
```csharp
// Para agregar un nuevo comportamiento (ej: CacheBehavior),
// NO modificamos c�digo existente, solo registramos el nuevo behavior
builder.Services.AddMediatR(cfg => {
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});
```

#### 3. **L**iskov Substitution Principle (LSP)

**Las interfaces pueden sustituirse por sus implementaciones:**

? `ICardRepository` puede ser `CardRepository` o `InMemoryCardRepository` (tests)  
? `IAccountRepository` puede cambiar de implementaci�n sin afectar handlers  
? Los handlers solo dependen de abstracciones, no de implementaciones concretas  

**Ejemplo:**
```csharp
// El handler depende de la abstracci�n
public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly ICardRepository _cardRepository; // Abstracci�n
    
    // Funciona con cualquier implementaci�n que cumpla el contrato
    public LoginHandler(ICardRepository cardRepository) { }
}
```

#### 4. **I**nterface Segregation Principle (ISP)

**Interfaces espec�ficas en lugar de interfaces generales:**

? `ICardRepository` - Solo operaciones de tarjetas  
? `IAccountRepository` - Solo operaciones de cuentas  
? Cada repository expone �nicamente los m�todos necesarios  

**Ejemplo:**
```csharp
// Interfaces peque�as y espec�ficas
public interface ICardRepository
{
    Task<Card?> GetByCardNumberAsync(string cardNumber);
    Task UpdateAsync(Card card);
    // Solo m�todos relacionados con Cards
}

public interface IAccountRepository
{
    Task<Account?> GetByCardNumberAsync(string cardNumber);
    Task<List<Operation>> GetOperationsAsync(int accountId, int page, int pageSize);
    // Solo m�todos relacionados con Accounts
}
```

#### 5. **D**ependency Inversion Principle (DIP)

**Depender de abstracciones, no de implementaciones:**

? Handlers dependen de `ICardRepository`, no de `CardRepository`  
? La capa Application NO conoce Entity Framework  
? Inyecci�n de Dependencias configurada en `Program.cs`  

**Ejemplo:**
```csharp
// ? MAL: Dependencia directa de implementaci�n
public class LoginHandler
{
    private readonly CardRepository _cardRepository; // Implementaci�n concreta
}

// ? BIEN: Dependencia de abstracci�n
public class LoginHandler
{
    private readonly ICardRepository _cardRepository; // Abstracci�n
}
```

### Patrones de Dise�o Implementados

#### 1. Repository Pattern

**Ubicaci�n:** `ATMChallenge.Infrastructure/Repositories/`

**Prop�sito:** Abstraer el acceso a datos y desacoplar la l�gica de negocio de Entity Framework.

**Implementaci�n:**
```csharp
// Abstracci�n (Application layer)
public interface ICardRepository
{
    Task<Card?> GetByCardNumberAsync(string cardNumber);
    Task UpdateAsync(Card card);
}

// Implementaci�n (Infrastructure layer)
public class CardRepository : ICardRepository
{
    private readonly AppDbContext _context;
    
    public async Task<Card?> GetByCardNumberAsync(string cardNumber)
    {
        return await _context.Cards
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.CardNumber == cardNumber);
    }
}
```

**Beneficios:**
- ? F�cil de mockear en tests
- ? Cambiar ORM no afecta handlers
- ? Queries centralizadas y reutilizables

#### 2. CQRS (Command Query Responsibility Segregation)

**Ubicaci�n:** `ATMChallenge.Application/Features/`

**Prop�sito:** Separar operaciones de lectura y escritura.

**Commands (Escritura):**
- `LoginCommand` ? `LoginHandler`
- `WithdrawCommand` ? `WithdrawHandler`

**Queries (Lectura):**
- `GetBalanceQuery` ? `GetBalanceHandler`
- `GetOperationsQuery` ? `GetOperationsHandler`

**Beneficios:**
- ? Escalabilidad independiente
- ? C�digo m�s mantenible
- ? Optimizaci�n espec�fica por tipo de operaci�n

#### 3. Mediator Pattern (con MediatR)

**Ubicaci�n:** `Program.cs` + todos los handlers

**Prop�sito:** Desacoplar emisores y receptores de mensajes.

**Implementaci�n:**
```csharp
// Controller (emisor) no conoce al Handler (receptor)
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var command = new LoginCommand(request.CardNumber, request.Pin);
        var result = await _mediator.Send(command); // Env�a sin conocer el handler
        return Ok(result);
    }
}
