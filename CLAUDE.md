# AcidoInMetal Trading Tracker — Contexto del proyecto

Este archivo le da contexto a Claude Code al arrancar. Léelo antes de tocar código.
El dueño del proyecto es Walter. Viene trabajando este proyecto en conversaciones
con Claude (claude.ai) y ahora suma Claude Code para trabajar directo sobre los
archivos. Segui el mismo estilo de trabajo que se describe abajo.

## Qué es esto

Aplicación de escritorio WPF (C# / .NET 10) para llevar un diario y análisis de
trading personal — "AcidoInMetal Trading Tracker". Estética visual: terminal de
trading estilo Bloomberg, dark mode, verde/rojo para P&L, acentos azules.

- IDE: Visual Studio 2026 (Community)
- DB: SQLite embebido vía `Microsoft.Data.Sqlite` (sin instalación para el usuario)
- Patrón: MVVM (`Models/`, `ViewModels/`, `Views/`, `Services/`)
- Walter tiene experiencia previa en Python y Java, pero es nuevo en C#. Explicá
  las cosas asumiendo ese nivel, sin sobre-explicar lo obvio para alguien con
  experiencia en programación en general.

## ⚠️ Namespace inconsistente — MUY IMPORTANTE

El proyecto tiene DOS namespaces raíz distintos conviviendo, por un tema histórico
de renombre de carpetas:

- `MainWindow` vive en el namespace **`AcidoInMetalTradingTracker`** (sin guion bajo)
- `Models/`, `ViewModels/`, `Views/`, `Services/` viven en
  **`Acidoinmetal_Trading_Tracker`** (con guion bajo, distinta capitalización)

Esto YA compila y funciona así. No lo "arregles" ni lo unifiques sin que Walter lo
pida explícitamente — es una decisión consciente para no romper todo lo que ya
anda. Cuando crees un archivo nuevo, fijate en qué carpeta va y usá el namespace
que corresponda a esa carpeta (ver ejemplos existentes al lado).

## Patrón de datos: todo se agrupa por FECHA

Concepto central del dominio: existe una "Sesión Operativa" por día
(`SesionOperativa.cs`, tabla manejada por `DatabaseService.cs`). Todas las
pantallas de carga de datos (Trader Status, Análisis Macro, y las que vengan)
cuelgan de esa misma sesión del día — no tienen su propia fecha suelta.

- Al abrir la app, se llama a algo como `ObtenerOCrearSesionPorFecha(DateTime.Now)`:
  si ya existe una sesión para la fecha de sistema, la reutiliza; si no, crea una
  nueva. Nunca se altera la fecha de una sesión desde ese flujo normal.
- Habrá en el futuro una pantalla aparte para editar sesiones de fechas
  anteriores — ahí sí se podrá elegir la fecha manualmente. No mezclar ese caso
  con el flujo normal de apertura de la app.
- Cualquier ViewModel nuevo que guarde datos debe recibir `DatabaseService` +
  `sesionId` en el constructor (mismo patrón que `TraderStatusViewModel` y
  `AnalisisMacroViewModel`), y el `DataContext` se asigna desde `MainWindow`,
  no desde el constructor de la View.

## Navegación actual

`MainWindow.xaml` usa **Visibility toggling** entre paneles (`PanelDashboard`,
`PanelTraderStatus`, `PanelAnalisisMacro`, etc.), no `ContentControl` todavía.
Migrar a `ContentControl` es una tarea pendiente conocida, pero no asumas que ya
se hizo — confirmá el estado real del archivo antes de tocar navegación.

## Pantallas — estado actual

- **Dashboard**: cards de resumen (Balance, P&L, operaciones abiertas, win rate)
  + grid de últimas operaciones. Tiene botones provisorios "Ir a X" que van a
  pantallas completas más adelante.
- **Trader Status**: 5 métricas con estrellas 1-10 (Descanso, Estado Anímico,
  Nivel de Stress, Nivel de Ansiedad, Cabina Estéril). Stress y Ansiedad son
  invertidas: `(11 - valor)`. Fórmula final: `((totalAportes - 5) / 45.0) * 100`.
  Indicador circular: rojo <30%, amarillo 30-59%, verde 60%+.
- **Análisis Macro**: pantalla nueva, dividida a la mitad — EURO (EUR/USD) a la
  izquierda, LIBRA (GBP/USD) a la derecha. Por ahora solo tiene los títulos
  grandes armados; los campos de carga de cada columna todavía no están
  definidos — se van a construir a continuación.
- **Operaciones, Gráficos, Configuración**: todavía no arrancadas.

## Cómo trabajar con Walter

- **Incremental y validado**: armar una pieza chica, que él la compile y confirme
  antes de seguir con la próxima. No tirar cambios grandes de una.
- **Explicaciones paso a paso**: Walter se describe como desorganizado con las
  herramientas — para pasos de UI en Visual Studio o comandos, sé explícito y
  ordenado, no asumas que sabe el atajo.
- **Confía en tu criterio**: prefiere que decidas vos y le entregues algo
  completo y listo para probar, en vez de preguntarle demasiadas opciones.
- **Git**: workflow ya establecido — después de cada cambio validado:
  ```bash
  cd "C:\AcidoInMetalTradingTracker"
  git add .
  git commit -m "mensaje descriptivo del cambio"
  git push origin master
  ```
- El repo tiene `.gitignore` (excluye `bin/`, `obj/`, `.vs/`, `*.user`) y
  `.gitattributes` (`* text=auto eol=crlf`) ya configurados — no los toques.

## Errores ya resueltos (para no repetir)

- `.csproj` corrupto por referencias duplicadas a `bin`/`obj`
- `Styles.xaml` duplicado (se dejó solo la versión en `Themes/`)
- Errores de namespace `mc:Ignorable`
- Clase `Operacion` definida dos veces
- Falta de la resource key `BgHover` → crasheaba en runtime
- `obj/` y `.vs/` estaban trackeados en Git por error — ya se sacaron del
  tracking, quedan solo ignorados vía `.gitignore`
