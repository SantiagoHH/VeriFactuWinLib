# Changelog

## [Unreleased] - 2025-10-29

### Añadido

- **`NetCore/Src/Net/Rest/ApiNoVerifactu.cs`**  
  Nueva clase para la integración con servicios REST no VeriFactu.  
  Permite realizar peticiones HTTP con una compatibilidad ampliada para entornos NoVerifactu.

### Modificado

- **`NetCore/Src/Net/Wsd.cs`**

  - Se añadió `using VeriFactu.Net.Rest;`.
  - Se integró `ApiNoVerifactu` dentro del método `Call`.
  - Modificaciones internas de lógica y organización del código.

- **`NetCore/Src/VeriFactuEndPointPrefixes.cs`**

  - Se añadió `ProdNoVerifactu`, utilizado como endpoint IP al que `ApiNoVerifactu` enviará las facturas para su almacenamiento posterior.
  - Se añadió `TestNoVerifactu`, utilizado como endpoint IP al que `ApiNoVerifactu` enviará las facturas para su almacenamiento posterior en modo test.

- **`NetCore/Src/Config/Settings.cs`**  
  Agregadas nuevas propiedades para gestionar el modo NoVerifactu:

  - `VeriFactuMode`
  - `NoVeriFactuNif`
  - `NoVeriFactuKey`
  - `VeriFactuEndPointNoVeriFactuPrefix`
  - `NoVeriFactuToken`
  - `NoVeriFactuTokenTime`

  **Valores que deben ajustarse antes de usar (`Settings.cs`):**

  1. `static readonly string _Path = "path_verifactu_setting";`

     - Define el directorio donde se guardan los archivos generados. (Línea 80)

  2. `public string VeriFactuMode { get; set; }`

     - Debe configurarse en `internal static Settings GetDefault()`
       - `VeriFactuMode = "no_verifactu"`
       - o `VeriFactuMode = "verifactu"`  
         (Línea 234)

  3. Para el modo `no_verifactu` deben configurarse:
     - `NoVeriFactuNif`
     - `NoVeriFactuKey`  
       (Líneas 235 y 236)

### Notas

- Esta versión mantiene la compatibilidad con la implementación original de VeriFactu.
- Para utilizar el modo **NoVerifactu**, basta con disponer de un servidor HTTP que gestione el almacenamiento de las facturas.
- Todos los cambios cumplen con la licencia **GNU Affero General Public License v3.0 (AGPL-3.0)**.
