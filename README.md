# VeriFactuWinLib

Biblioteca para Windows basada en el proyecto original **VeriFactu**,
diseñada para integrarse en aplicaciones que necesiten enviar registros
de facturación tanto a los servicios de la **AEAT** como a **servidores
privados** mediante el modo _NoVerifactu_.

Este fork introduce un conjunto de extensiones específicas
y para integraciones donde el sistema de
facturación no utilice la infraestructura VeriFactu estándar,
manteniendo la compatibilidad con la implementación original.

---

> [!IMPORTANT]
> Para comenzar es necesario configurar el directorio que utilizará la librería y establecer correctamente el certificado con el que se firmarán los envíos.
> Podemos cargar el certificado desde un archivo .pfx / .p12 guardado en el disco, o (en Windows) cargar un certificado del almacén de certificados de windows. La configuración del sistema esta accesible mediante la propiedad estática 'Current' del objeto `Settings'. En la siguiente tabla se describen los valores de configuración relacionados con el certificado a utilizar:

<br>
<br>

## Establecer en la configuración los valores para el uso del certificado

| Propiedad             | Descripción                                                                                                                                                                                                                      |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CertificatePath       | Ruta al archivo del certificado a utilizar.                                                                                                                                                                                      |
| CertificatePassword   | Password del certificado. Este valor sólo es necesario si tenemos establecido el valor para 'CertificatePath' y el certificado tiene clave de acceso. Sólo se utiliza en los certificados cargados desde el sistema de archivos. |
| CertificateSerial     | Número de serie del certificado a utilizar. Mediante este número de serie se selecciona del almacén de certificados de windows el certificado con el que realizar las comunicaciones.                                            |
| CertificateThumbprint | Hash o Huella digital del certificado a utilizar. Mediante esta huella digital se selecciona del almacén de certificados de windows el certificado con el que realizar las comunicaciones.                                       |

## 🚀 Características principales

- Compatibilidad con el flujo estándar VeriFactu.
- Modo **NoVerifactu** para enviar facturas a un servidor HTTP
  privado.
- Clase extendida `ApiNoVerifactu` para comunicación REST
  personalizada.
- Nuevos parámetros de configuración en `Settings.cs` para gestionar
  ambos modos.
- Biblioteca orientada a integración desde aplicaciones externas
  (GeminisWin VF).
- Totalmente basada en .NET para entornos Windows.

---

## 📦 Cambios principales respecto a VeriFactu original

Resumen del changelog:

- **Nueva clase `ApiNoVerifactu`** para integrar servicios REST
  alternativos no dependientes de AEAT.
- Modificaciones en `Wsd.cs` para incorporar `ApiNoVerifactu` en el
  método `Call`.
- Añadido `ProdNoVerifactu` en `VeriFactuEndPointPrefixes.cs` como
  endpoint para el almacenamiento privado.
- Nuevas propiedades en `Settings.cs`:
  - `VeriFactuMode`
  - `NoVeriFactuNif`
  - `NoVeriFactuKey`
  - `VeriFactuEndPointNoVeriFactuPrefix`
  - `NoVeriFactuToken`
  - `NoVeriFactuTokenTime`
- Ajustes requeridos en `_Path` y en el método `GetDefault()` para
  seleccionar el modo `verifactu` o `no_verifactu`.

Para más detalle consulta el archivo `CHANGELOG.md`.

---

## ⚙️ Instalación

### 1. Añadir la biblioteca al proyecto

Clona este repositorio y agrega la librería a tu solución .NET para
Windows.

    git clone https://github.com/SantiagoHH/VeriFactuWinLib

Puedes compilarla e incluirla como referencia en tu proyecto.

---

## 🔧 Configuración

La configuración general se define en `Settings.cs`.

### Seleccionar el modo de operación

```csharp
VeriFactuMode = "verifactu"       // Envío a AEAT
VeriFactuMode = "no_verifactu"    // Envío a servidor privado
```

### Ajustes obligatorios para modo NoVerifactu

```csharp
NoVeriFactuNif = "TU_NIF";
NoVeriFactuKey = "TU_CLAVE";
VeriFactuEndPointNoVeriFactuPrefix = "http://IP_SERVIDOR";
```

## 🧩 Ejemplo básico de uso - (El uso de la biblioteca es idéntico a la librería original, el sistema que se encargue de guardar las facturas emitidas debe responder con la misma estructura que la AEAT)

```csharp
Settings.Current.CertificatePath = @"C:\CERTIFICADO.pfx";
Settings.Current.CertificatePassword = "pass certificado";
Settings.Current.VeriFactuMode = "no_verifactu";
Settings.Current.NoVeriFactuNif = "A00000000";
Settings.Current.NoVeriFactuKey = "123456789";
Settings.Current.VeriFactuEndPointNoVeriFactuPrefix = "http://192.168.1.10/api";
Settings.Save();

// Creamos una instacia de la clase factura
var invoice = new Invoice("GIT-EJ-0002", new DateTime(2024, 11, 15), "B72877814")
{
    InvoiceType = TipoFactura.F1,
    SellerName = "WEFINZ GANDIA SL",
    BuyerID = "B44531218",
    BuyerName = "WEFINZ SOLUTIONS SL",
    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
    TaxItems = new List<TaxItem>() {
        new TaxItem()
        {
            TaxRate = 4,
            TaxBase = 10,
            TaxAmount = 0.4m
        },
        new TaxItem()
        {
            TaxRate = 21,
            TaxBase = 100,
            TaxAmount = 21
        }
    }
};

// Creamos la entrada de la factura
var invoiceEntry = new InvoiceEntry(invoice);

// Guardamos la factura (En este paso se envía la factura en modo verifactu a la AEAT o al servidor HTTP que almacena las facturas en el modo no_verifactu)
invoiceEntry.Save();

// Consultamos el estado
Debug.Print($"Respuesta de la api que se encarga de guardar las facturas emitidas debe simular la respuesta de la AEAT:\n{invoiceEntry.Status}");

if (invoiceEntry.Status == "Correcto")
{

    // Consultamos el CSV
    Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.CSV}");

}
else
{

    // Consultamos el error
    Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}");

}


---

## 🖥️ Requisitos


- Servidor HTTP propio para modo NoVerifactu (si se usa dicho modo)

---

## 📝 Licencia

Este proyecto se distribuye bajo **GNU Affero General Public License
v3.0 (AGPL-3.0)**.

Cualquier obra derivada debe mantenerse bajo la misma licencia, y debe
ofrecer el código fuente completo incluyendo las modificaciones
realizadas.

---

## 👤 Créditos y atribución

Este fork está basado en el proyecto original **VeriFactu**, propiedad
de sus autores originales bajo licencia AGPL-3.0.

### Modificaciones adicionales realizadas por:

**Santiago Nicolás Hernández Hernández**\
**Ingeniería de Desarrollo y Servicios de Canarias, S.L.**\
Sitio web: https://idssoft.net/

---

## 📄 Aviso

Este proyecto mantiene compatibilidad con la implementación original de
VeriFactu, pero añade funciones extendidas para **entornos privados**
bajo el modo _NoVerifactu_.\
Su uso es responsabilidad del integrador.
```
